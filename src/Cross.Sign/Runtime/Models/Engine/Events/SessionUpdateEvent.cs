using Newtonsoft.Json;
using Cross.Sign.Models.Engine.Methods;

namespace Cross.Sign.Models.Engine.Events
{
    /// <summary>
    ///     An event that is emitted when a session is updated.
    /// </summary>
    public class SessionUpdateEvent : SessionEvent
    {
        /// <summary>
        ///     The wc_sessionUpdate request that triggered this event
        /// </summary>
        [JsonProperty("params")]
        public SessionUpdate Params;
    }
}