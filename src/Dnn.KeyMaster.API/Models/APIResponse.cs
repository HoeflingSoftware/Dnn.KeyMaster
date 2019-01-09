using Newtonsoft.Json;

namespace Dnn.KeyMaster.API.Models
{
    [JsonObject]
    public class APIResponse
    {
        [JsonProperty("isSuccessful")]
        public bool IsSuccessful { get; set; }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }
    }

    [JsonObject]
    public class APIResponse<T> : APIResponse
    {
        [JsonProperty("result")]
        public T Result { get; set; }        
    }
}
