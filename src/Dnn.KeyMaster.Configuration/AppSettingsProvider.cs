using System.Collections.Specialized;

namespace Dnn.KeyMaster.Configuration
{
    public class AppSettingsProvider : NameValueCollection
    {
        private static readonly object _lockobject = new object();
        private static AppSettingsProvider _instance;
        public static AppSettingsProvider Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lockobject)
                    {
                        if (_instance == null)
                        {
                            var appsettings = ModuleLoader.LoadImplementation<IKeyMasterAppSettings>(ModuleLoader.Default);
                            _instance = new AppSettingsProvider(appsettings);
                        }
                    }
                }

                return _instance;
            }
        }

        public IKeyMasterAppSettings KeyMaster { get; }
        private AppSettingsProvider(IKeyMasterAppSettings keymaster)
            : base(keymaster.GetAppSettings())
        {
            KeyMaster = keymaster;
        }
    }
}
