using Newtonsoft.Json;

namespace Dnn.KeyMaster.API.Models
{
    [JsonObject]
    public class Status
    {
        [JsonProperty("isEnabled")]
        public bool IsEnabled { get; set; }
    }
}
