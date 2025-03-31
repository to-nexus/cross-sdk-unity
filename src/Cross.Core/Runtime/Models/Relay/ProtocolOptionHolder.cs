using System;
using Newtonsoft.Json;

namespace Cross.Core.Models.Relay
{
    /// <summary>
    ///     An abstract class that simply holds ProtocolOptions under the Relay property
    /// </summary>
    [Serializable]
    public abstract class ProtocolOptionHolder
    {
        /// <summary>
        ///     The relay protocol options to use for this event
        /// </summary>
        [JsonProperty("relay")]
        public ProtocolOptions Relay;
    }
}