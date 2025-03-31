using Newtonsoft.Json;

namespace Cross.Core.Crypto.Models
{
    /// <summary>
    ///     Iridium JWT data that is encoded when signing JWT tokens
    /// </summary>
    public class IridiumJWTData
    {
        /// <summary>
        ///     The Iridium JWT header data
        /// </summary>
        [JsonProperty("header")]
        public IridiumJWTHeader Header;

        /// <summary>
        ///     The Iridium JWT payload
        /// </summary>
        [JsonProperty("payload")]
        public IridiumJWTPayload Payload;
    }
}