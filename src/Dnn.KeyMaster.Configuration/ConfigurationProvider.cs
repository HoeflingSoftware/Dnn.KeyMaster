namespace Dnn.KeyMaster.Configuration
{
    public class ConfigurationProvider
    {
        private static readonly object _lockobject = new object();
        private static ConfigurationProvider _instance;
        public static ConfigurationProvider Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lockobject)
                    {
                        if (_instance == null)
                        {
                            var configuration = ModuleLoader.LoadImplementation<IKeyMasterConfiguration>(ModuleLoader.Default);
                            _instance = new ConfigurationProvider(configuration);
                        }
                    }
                }

                return _instance;
            }
        }

        public IKeyMasterConfiguration Configuration { get; }
        public ConfigurationProvider(IKeyMasterConfiguration configuration)
        {
            Configuration = configuration;
            Configuration.Initialize();
        }

        string this[string key]
        {
            get
            {
                return Configuration.Secrets[key];
            }
            set
            {
                Configuration.Secrets[key] = value;
                Configuration.SaveOrUpdate();
            }
        }
    }
}