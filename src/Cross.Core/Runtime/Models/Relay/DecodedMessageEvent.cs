using Newtonsoft.Json;
using Cross.Core.Network.Models;

namespace Cross.Core.Models.Relay
{
    /// <summary>
    ///     Represents a <see cref="MessageEvent" /> that has been deserialized into a <see cref="JsonRpcPayload" />
    /// </summary>
    public class DecodedMessageEvent : MessageEvent
    {
        /// <summary>
        ///     The deserialized payload that was decoded from the Message property
        /// </summary>
        [JsonProperty("payload")]
        public JsonRpcPayload Payload;
    }
}