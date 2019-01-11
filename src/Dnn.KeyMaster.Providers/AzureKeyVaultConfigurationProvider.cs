using System;
using System.Configuration;
using System.Configuration.Internal;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Dnn.KeyMaster.Providers
{
    public class AzureKeyVaultConfigurationProvider
    {
        internal static void Initialize()
        {
            var configManager = ConfigurationManager.AppSettings;

            var configSystem = typeof(ConfigurationManager).GetField("s_configSystem", BindingFlags.Static | BindingFlags.NonPublic);
            configSystem.SetValue(null, new ConfigurationManagerProxy((IInternalConfigSystem)configSystem.GetValue(null)));

            var dnnConfigGetSetting = typeof(DotNetNuke.Common.Utilities.Config).GetMethod("GetSetting", BindingFlags.Public | BindingFlags.Static);
            var proxyGetSetting = typeof(AzureKeyVaultConfigurationProvider).GetMethod("GetSetting", BindingFlags.Static | BindingFlags.NonPublic);

            RuntimeHelpers.PrepareMethod(dnnConfigGetSetting.MethodHandle);
            RuntimeHelpers.PrepareMethod(proxyGetSetting.MethodHandle);

            unsafe
            {
                if (IntPtr.Size == 4)
                {
                    int* injection = (int*)proxyGetSetting.MethodHandle.Value.ToPointer() + 2;
                    int* target = (int*)dnnConfigGetSetting.MethodHandle.Value.ToPointer() + 2;


                    *target = *injection;
                }
                else
                {
                    long* injection = (long*)proxyGetSetting.MethodHandle.Value.ToPointer() + 1;
                    long* target = (long*)dnnConfigGetSetting.MethodHandle.Value.ToPointer() + 1;

                    *target = *injection;
                }
            }
        }

        private static string GetSetting(string setting)
        {
            return ConfigurationManager.AppSettings[setting];
        }
    }
}
