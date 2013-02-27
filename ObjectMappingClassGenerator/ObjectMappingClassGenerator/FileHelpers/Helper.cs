using System.IO;

namespace ObjectMappingClassGenerator.FileHelpers
{
    public class Helper
    {
        public static void SaveResult(string result, string name)
        {
            File.WriteAllText(@"C:\PocCodes\ClassMapperPOC\ClassMapperPOC\ClassMapperPOC\Mapper\GeneratedFile\" + name, result);
        }
    }
}
