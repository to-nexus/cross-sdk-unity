using Newtonsoft.Json;
using Cross.Core.Models.Relay;

namespace Cross.Core.Models.Subscriber
{
    /// <summary>
    ///     Represents a subscription that's pending
    /// </summary>
    public class PendingSubscription : SubscribeOptions
    {
        /// <summary>
        ///     The topic that will be subscribed to
        /// </summary>
        [JsonProperty("topic")]
        public string Topic;
    }
}