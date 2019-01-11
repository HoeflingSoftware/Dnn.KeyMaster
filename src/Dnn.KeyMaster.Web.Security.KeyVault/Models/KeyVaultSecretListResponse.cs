using Newtonsoft.Json;
using System.Collections.Generic;

namespace Dnn.KeyMaster.Web.Security.KeyVault.Models
{
    [JsonObject]
    internal class KeyVaultSecretListResponse
    {
        [JsonProperty("value")]
        public IEnumerable<KeyVaultSecretListItem> Secrets { get; set; }
    }
}
