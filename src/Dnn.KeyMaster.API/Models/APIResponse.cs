using Newtonsoft.Json;

namespace Dnn.KeyMaster.API.Models
{
    [JsonObject]
    public class APIResponse<T>
    {
        [JsonProperty("result")]
        public T Result { get; set; }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
