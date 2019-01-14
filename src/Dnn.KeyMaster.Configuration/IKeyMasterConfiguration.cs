using System.Collections.Generic;
using System.Collections.Specialized;

namespace Dnn.KeyMaster.Configuration
{
    public interface IKeyMasterConfiguration
    {
        NameValueCollection Secrets { get; }
        void SaveOrUpdate();
        void Initialize();
    }
}
