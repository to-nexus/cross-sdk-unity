using Newtonsoft.Json;
using Cross.Core.Network.Models;

namespace Cross.Core.Models.Subscriber
{
    /// <summary>
    ///     Represents a deleted subscription.
    /// </summary>
    public class DeletedSubscription : ActiveSubscription
    {
        /// <summary>
        ///     The reason why the subscription was deleted
        /// </summary>
        [JsonProperty("reason")]
        public Error Reason;
    }
}