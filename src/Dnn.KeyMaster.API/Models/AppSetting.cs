using Newtonsoft.Json;

namespace Dnn.KeyMaster.API.Models
{
    [JsonObject]
    public class AppSetting
    {
        [JsonProperty("Key")]
        public string Key { get; set; }

        [JsonProperty("Value")]
        public string Value { get; set; }
    }
}
