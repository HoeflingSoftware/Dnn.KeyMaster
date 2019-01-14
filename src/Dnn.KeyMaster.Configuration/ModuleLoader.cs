using System;
using System.Linq;
using System.Reflection;

namespace Dnn.KeyMaster.Configuration
{
    internal static class ModuleLoader
    {
        internal const string Default = "Dnn.KeyMaster.Configuration.AzureKeyVault";
        internal static T LoadImplementation<T>(string assemblyName)
        {
            var assembly = Assembly.Load(assemblyName);
            var type = assembly
                .GetTypes()
                .FirstOrDefault(x => x.GetInterface(typeof(T).Name) != null);

            if (type == null)
            {
                throw new Exception("Unable to load Key Master Configuration Module");
            }

            return (T)Activator.CreateInstance(type);
        }
    }
}
