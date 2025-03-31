using System.Collections.Generic;
using Cross.Core.Common.Utils;
using Cross.Core.Network.Models;
using Cross.Sign.Interfaces;

namespace Cross.Sign.Models.Engine.Methods
{
    /// <summary>
    ///     A class that represents the request wc_sessionPing. Used to ping a session
    /// </summary>
    [RpcMethod("wc_sessionPing")]
    [RpcRequestOptions(Clock.THIRTY_SECONDS, 1114)]
    [RpcResponseOptions(Clock.THIRTY_SECONDS, 1115)]
    public class SessionPing : Dictionary<string, object>, IWcMethod
    {
    }
}