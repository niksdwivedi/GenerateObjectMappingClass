using RazorEngine;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace ObjectMappingClassGenerator.Mapper
{
    public class ObjectMapper
    {
        protected IList<PropertyMatch> PropertyMatches = new List<PropertyMatch>();

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
                                                (sourceProp, destinationProp, sourceProp.PropertyType,
                                                 destinationProp.PropertyType));
                    }
                }
            }
        }

        public virtual object Apply(object source)
        {
            var sourceType = source.GetType();
            var destinationType = sourceType.Name + "Generated";
            var destination = GetInstance(destinationType);

            var sb = new StringBuilder();
            sb.Append("using System;\n");
            sb.Append("using System.Collections.Generic;\n");
            sb.Append("using System.Linq;\n");
            sb.Append("using System.Web;\n");
            sb.Append("@Model.Namespace\n");

            sb.Append("namespace ClassMapperPOC.Mapper");
            sb.Append("{");
            sb.Append("\n\npublic class @Model.ClassName\n");
            sb.Append("     {\n");
            sb.Append("         public @Model.PropertyClassGenerated VersionableViewModelMapper (@Model.PropertyClass viewModel)\n");
            sb.Append("         {\n");
            sb.Append("          return new @Model.PropertyClassGenerated \n");
            sb.Append("             { \n");

            foreach (var item in PropertyMatches)
            {
                sb.Append(item.DestinationProperty.Name + "=viewModel." + item.SourceProperty.Name + ",\n");
            }
            sb.Append("             }; \n");
            sb.Append(" 	    } \n");
            sb.Append("     } \n");
            sb.Append("}");

            var index = sb.ToString().LastIndexOf(',');
            sb.Remove(index, 1);

            var lstTemplateModel = new List<TemplateModel>
                                                       {
                                                           new TemplateModel()
                                                               {
                                                                   Namespace = "using ClassMapperPOC;",
                                                                   PropertyClass = sourceType.Name,
                                                                   PropertyClassGenerated = destinationType,
                                                                   ClassName=sourceType.Name+"Mapping"
                                                               }
                                                       };

            var result = string.Empty;
            foreach (var model in lstTemplateModel)
            {
                result = Razor.Parse<TemplateModel>(Convert.ToString(sb), model);
            }

            //Utilities.SaveResult(result, "Mapping.cs");

            //if (source == null)
            //    throw new ArgumentNullException("Exception");

            //foreach (var propertyMatch in PropertyMatches.Where(x => x.SourceProperty.DeclaringType == sourceType))
            //{
            //    var sourceVal = propertyMatch.SourceProperty.GetValue(source, null);
            //    propertyMatch.DestinationProperty.SetValue(destination,
            //                                               propertyMatch.SourcePropertyType !=
            //                                               propertyMatch.DestinationPropertyType
            //                                                   ? Apply(sourceVal)
            //                                                   : sourceVal, null);
            //}

            return destination;
        }

        public object GetInstance(string className)
        {
            //ToDo :: Work out how to get assembly name as the view models are in different assembly.
            var assemblyReference = Assembly.GetCallingAssembly();
            var classType =
                assemblyReference.GetType(string.Format("{0}.{1}", assemblyReference.GetName().Name, className));
            return Activator.CreateInstance(classType);
        }
    }

    public class PropertyMatch
    {
        public PropertyInfo SourceProperty;
        public PropertyInfo DestinationProperty;
        public Type SourcePropertyType;
        public Type DestinationPropertyType;

        public PropertyMatch(PropertyInfo sourceProp, PropertyInfo destinationProp, Type sourcePropertyType,
                             Type destinationPropertyType)
        {
            SourceProperty = sourceProp;
            DestinationProperty = destinationProp;
            SourcePropertyType = sourcePropertyType;
            DestinationPropertyType = destinationPropertyType;
        }
    }

    public class TemplateModel
    {
        public string PropertyClassGenerated { get; set; }

        public string PropertyClass { get; set; }

        public string Namespace { get; set; }

        public string ClassName { get; set; }
    }
}