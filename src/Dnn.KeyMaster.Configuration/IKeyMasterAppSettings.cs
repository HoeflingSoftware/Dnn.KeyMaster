﻿using System.Collections.Specialized;
using System.Threading.Tasks;

namespace Dnn.KeyMaster.Configuration
{
    public interface IKeyMasterAppSettings
    {
        NameValueCollection GetAppSettings();
        string GetSecret(string key);
        bool DeleteSecret(string key, bool updateAppsettings = true, NameValueCollection secrets = null);
        bool CreateOrUpdate(string key, string value, bool updateAppsettings = true, NameValueCollection secrets = null);
        string GetConnectionString(NameValueCollection secrets = null);
    }
}
