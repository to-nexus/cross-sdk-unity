using Newtonsoft.Json;

namespace Cross.Core.Crypto.Models
{
    public class IridiumJWTDecoded : IridiumJWTSigned
    {
        [JsonProperty("data")]
        public byte[] Data;
    }
}