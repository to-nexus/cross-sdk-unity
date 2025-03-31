using Newtonsoft.Json;
using Cross.Core.Common.Utils;
using Cross.Core.Network.Models;
using Cross.Sign.Interfaces;

namespace Cross.Sign.Models.Engine.Methods
{
    /// <summary>
    ///     A class that represents the request wc_sessionUpdate. Used to update the <see cref="Namespaces" /> enabled
    ///     in this session
    /// </summary>
    [RpcMethod("wc_sessionUpdate")]
    [RpcRequestOptions(Clock.ONE_DAY, 1104)]
    [RpcResponseOptions(Clock.ONE_DAY, 1105)]
    public class SessionUpdate : IWcMethod
    {
        /// <summary>
        ///     The updated namespaces that are enabled for this session
        /// </summary>
        [JsonProperty("namespaces")]
        public Namespaces Namespaces;
    }
}