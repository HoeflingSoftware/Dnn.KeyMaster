using Newtonsoft.Json;
using System.Net;
using System.Net.Http;
using System.Text;

namespace Dnn.KeyMaster.API.Models
{
    [JsonObject]
    public class PersonaBarResponse
    {
        [JsonProperty("Success")]
        public bool Success { get; set; }

        public HttpResponseMessage ToHttpResponseMessage(HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            return new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(this.ToJson(), Encoding.UTF8, "application/json")
            };
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }
    }

    [JsonObject]
    public class PersonaBarResponse<T> : PersonaBarResponse
    {
        [JsonProperty("Result")]
        public T Result { get; set; }        
    }
}
