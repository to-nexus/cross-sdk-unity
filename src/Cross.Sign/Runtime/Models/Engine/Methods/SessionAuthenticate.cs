using Newtonsoft.Json;
using Cross.Core.Common.Utils;
using Cross.Core.Network.Models;
using Cross.Sign.Interfaces;

namespace Cross.Sign.Models.Engine.Methods
{
    [RpcMethod("wc_sessionAuthenticate")]
    [RpcRequestOptions(Clock.ONE_DAY, 1116)]
    [RpcResponseOptions(Clock.ONE_DAY, 1117)]
    public class SessionAuthenticate : IWcMethod
    {
        [JsonProperty("requester")]
        public Participant Requester { get; set; }

        [JsonProperty("authPayload")]
        public AuthPayloadParams Payload { get; set; }

        [JsonProperty("expiryTimestamp")]
        public long ExpiryTimestamp { get; set; }
    }
}