using Newtonsoft.Json;

namespace Cross.Core.Models.Eth
{
    public class EthCall
    {
        [JsonProperty("data")] public string Data;
        [JsonProperty("to")] public string To;
    }
}