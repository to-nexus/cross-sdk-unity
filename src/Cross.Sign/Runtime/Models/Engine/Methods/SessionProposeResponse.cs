using Newtonsoft.Json;
using Cross.Core.Common.Utils;
using Cross.Core.Models.Relay;
using Cross.Core.Network.Models;

namespace Cross.Sign.Models.Engine.Methods
{
    /// <summary>
    ///     A class that represents the response to wc_sessionPropose. Used to approve a session proposal
    /// </summary>
    [RpcResponseOptions(Clock.FIVE_MINUTES, 1101)]
    public class SessionProposeResponse
    {
        /// <summary>
        ///     The protocol options that should be used in this session
        /// </summary>
        [JsonProperty("relay")]
        public ProtocolOptions Relay;

        /// <summary>
        ///     The public key of the responder to this session proposal
        /// </summary>
        [JsonProperty("responderPublicKey")]
        public string ResponderPublicKey;
    }
}