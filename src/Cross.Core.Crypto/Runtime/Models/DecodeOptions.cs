using Newtonsoft.Json;

namespace Cross.Core.Crypto.Models
{
    /// <summary>
    ///     Class representing options for decoding data
    /// </summary>
    public class DecodeOptions
    {
        /// <summary>
        ///     The public key that received this encoded message
        /// </summary>
        [JsonProperty("receiverPublicKey")]
        public string ReceiverPublicKey;
    }
}