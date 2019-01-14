using System.Collections.Specialized;
using System.Threading.Tasks;

namespace Dnn.KeyMaster.Configuration
{
    public interface IKeyMasterAppSettings
    {
        NameValueCollection GetAppSettings();
        string GetSecret(string key);
        Task<bool> DeleteSecretAsync(string key);
        Task<bool> CreateOrUpdateAsync(string key, string value);
        string GetConnectionString();
    }
}
