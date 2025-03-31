using Newtonsoft.Json;

namespace Cross.AppKit.Unity.Model
{
    public class GetWalletsResponse
    {
        [JsonProperty("count")] public int Count { get; set; }

        [JsonProperty("data")] public Wallet[] Data { get; set; }
    }
}