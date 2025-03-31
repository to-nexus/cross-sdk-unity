using System;
using Newtonsoft.Json;

namespace Cross.Core.Models.Relay
{
    /// <summary>
    ///     The parameters for publishing a message to the relay server under a specific topic
    /// </summary>
    [Serializable]
    public class RelayPublishRequest
    {
        /// <summary>
        ///     The message to publish to the relay server
        /// </summary>
        [JsonProperty("message")]
        public string Message;

        /// <summary>
        ///     A tag for the message to identify it
        /// </summary>
        [JsonProperty("tag")] public long Tag;

        /// <summary>
        ///     The topic to publish the message under
        /// </summary>
        [JsonProperty("topic")]
        public string Topic;

        /// <summary>
        ///     Time To Live. How long the message will remain on the relay server without being
        ///     consumed (by a subscriber to the topic) before it's deleted
        /// </summary>
        [JsonProperty("ttl")] public long TTL;
    }
}