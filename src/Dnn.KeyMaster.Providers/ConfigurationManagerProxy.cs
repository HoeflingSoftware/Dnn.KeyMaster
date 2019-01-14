using Dnn.KeyMaster.Configuration;
using System.Collections.Specialized;
using System.Configuration.Internal;

namespace Dnn.KeyMaster.Providers
{
    internal sealed class ConfigurationManagerProxy : IInternalConfigSystem
    {
        readonly IInternalConfigSystem baseconf;

        public ConfigurationManagerProxy(IInternalConfigSystem baseconf)
        {
            this.baseconf = baseconf;
        }

        object appsettings;
        public object GetSection(string configKey)
        {
            if (configKey == "appSettings" && this.appsettings != null) return this.appsettings;
            object section = baseconf.GetSection(configKey);

            if (configKey == "appSettings" && section is NameValueCollection)
            {
                section = this.appsettings = AppSettingsProvider.Instance;
            }
            return section;
        }

        public void RefreshConfig(string sectionName)
        {
            if (sectionName == "appSettings") appsettings = null;
            baseconf.RefreshConfig(sectionName);
        }

        public bool SupportsUserConfig
        {
            get { return baseconf.SupportsUserConfig; }
        }
    }
}
