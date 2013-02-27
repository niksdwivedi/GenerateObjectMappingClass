using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

public class SimpleObjectMapper
{
    protected IList<PropertyMatch> PropertyMatches = new List<PropertyMatch>();

    private string _defaultMappedObjectName = "mappedObject";

    public void Mapper<TSource, TDestination>()
        where TSource : new()
        where TDestination : new()
    {
        var sourceProps = typeof(TSource).GetProperties();
        var destinationProps = typeof(TDestination).GetProperties();

        foreach (var sourceProp in sourceProps)
        {
            foreach (var destinationProp in destinationProps)
            {
                if ((sourceProp.Name.Equals(destinationProp.Name,
                                            StringComparison.InvariantCultureIgnoreCase)
                     ))
                {
                    PropertyMatches.Add(new PropertyMatch
                                            (sourceProp, destinationProp, sourceProp.PropertyType, destinationProp.PropertyType));
                }
            }
        }
    }

    public virtual object Apply(object source)
    {
        var sourceType = source.GetType();
        var destinationType = sourceType.Name + "V2";
        var destination = GetInstance(destinationType);

        if (source == null)
            throw new ArgumentNullException("Exception");

        foreach (var propertyMatch in PropertyMatches.Where(x => x.SourceProperty.DeclaringType == sourceType))
        {
            var sourceVal = propertyMatch.SourceProperty.GetValue(source, null);
            propertyMatch.DestinationProperty.SetValue(destination,
                                                       propertyMatch.SourcePropertyType !=
                                                       propertyMatch.DestinationPropertyType
                                                           ? Apply(sourceVal)
                                                           : sourceVal, null);

        }

        return destination;
    }

    public object GetInstance(string className)
    {
        //ToDo :: Work out how to get assembly name as the view models are in different assembly.
        var assemblyReference = Assembly.GetCallingAssembly();
        var classType = assemblyReference.GetType(string.Format("{0}.{1}", assemblyReference.GetName().Name, className));
        return Activator.CreateInstance(classType);
    }
}

public class PropertyMatch
{
    public PropertyInfo SourceProperty;
    public PropertyInfo DestinationProperty;
    public Type SourcePropertyType;
    public Type DestinationPropertyType;

    public PropertyMatch(PropertyInfo sourceProp, PropertyInfo destinationProp, Type sourcePropertyType, Type destinationPropertyType)
    {
        SourceProperty = sourceProp;
        DestinationProperty = destinationProp;
        SourcePropertyType = sourcePropertyType;
        DestinationPropertyType = destinationPropertyType;
    }
}