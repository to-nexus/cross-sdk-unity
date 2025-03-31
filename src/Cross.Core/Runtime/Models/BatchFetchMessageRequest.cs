using Newtonsoft.Json;

namespace Cross.Core.Models
{
    public class BatchFetchMessageRequest
    {
        [JsonProperty("topics")]
        public string[] Topics;
    }
}