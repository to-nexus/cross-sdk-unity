using System.Collections.Generic;
using Cross.Core.Common.Utils;
using Cross.Core.Network.Models;
using Cross.Sign.Interfaces;

namespace Cross.Sign.Models.Engine.Methods
{
    /// <summary>
    ///     A class that represents the request wc_sessionExtend. Used to extend a session
    /// </summary>
    [RpcMethod("wc_sessionExtend")]
    [RpcRequestOptions(Clock.ONE_DAY, 1106)]
    [RpcResponseOptions(Clock.ONE_DAY, 1107)]
    public class SessionExtend : Dictionary<string, object>, IWcMethod
    {
    }
}