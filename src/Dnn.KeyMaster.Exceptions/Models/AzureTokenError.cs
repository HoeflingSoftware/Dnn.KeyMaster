using Newtonsoft.Json;
using System.Collections.Generic;

namespace Dnn.KeyMaster.Exceptions.Models
{
    [JsonObject]
    public class AzureTokenError
    {
        [JsonProperty("error")]
        public string Error { get; set; }

        [JsonProperty("error_description")]
        public string Description { get; set; }

        [JsonProperty("error_codes")]
        public IEnumerable<string> ErrorCodes { get; set; }

        [JsonProperty("timestamp")]
        public string Timestamp { get; set; }

        [JsonProperty("trace_id")]
        public string TraceId { get; set; }

        [JsonProperty("correlation_id")]
        public string CorrelationId { get; set; }
    }
}
