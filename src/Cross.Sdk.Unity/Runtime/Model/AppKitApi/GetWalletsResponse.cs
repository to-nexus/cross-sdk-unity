using Newtonsoft.Json;

namespace Cross.Sdk.Unity.Model
{
    public class GetWalletsResponse
    {
        [JsonProperty("count")] public int Count { get; set; }

        [JsonProperty("data")] public Wallet[] Data { get; set; }
    }
}