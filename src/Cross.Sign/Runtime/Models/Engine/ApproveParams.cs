using System.Collections.Generic;
using Newtonsoft.Json;

namespace Cross.Sign.Models.Engine
{
    /// <summary>
    ///     A class that represents parameters to approve a session. Includes the id of the session proposal,
    ///     the enabled namespaces and what relay protocol will be used in the session
    /// </summary>
    public class ApproveParams
    {
        /// <summary>
        ///     The id of the session proposal to approve
        /// </summary>
        [JsonProperty("id")]
        public long Id;

        /// <summary>
        ///     The enabled namespaces in this session
        /// </summary>
        [JsonProperty("namespaces")]
        public Namespaces Namespaces;

        /// <summary>
        ///     The relay protocol that will be used in this session (as a protocol string)
        /// </summary>
        [JsonProperty("relayProtocol")]
        public string RelayProtocol;

        /// <summary>
        ///     Custom session properties for this approval
        /// </summary>
        [JsonProperty("sessionProperties")]
        public Dictionary<string, string> SessionProperties;
    }
}