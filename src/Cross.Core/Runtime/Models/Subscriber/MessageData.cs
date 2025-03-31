using Newtonsoft.Json;

namespace Cross.Core.Models.Subscriber
{
    /// <summary>
    ///     The data for a specific message, containing the message and the topic the message
    ///     came from
    /// </summary>
    public class MessageData
    {
        /// <summary>
        ///     The topic the message came from
        /// </summary>
        [JsonProperty("topic")]
        public string Topic;

        /// <summary>
        ///     The message as a string
        /// </summary>
        [JsonProperty("message")]
        public string Message { get; set; }
    }
}