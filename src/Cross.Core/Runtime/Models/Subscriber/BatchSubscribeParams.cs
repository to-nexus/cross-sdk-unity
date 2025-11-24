using Newtonsoft.Json;

namespace Cross.Core.Models.Subscriber
{
    public class BatchSubscribeParams
    {
        [JsonProperty("topics")]
        public string[] Topics;
    }
}