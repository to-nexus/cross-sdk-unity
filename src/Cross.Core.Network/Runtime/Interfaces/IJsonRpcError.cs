using Cross.Core.Network.Models;

namespace Cross.Core.Network
{
    /// <summary>
    ///     A JSON RPC response that may include an error
    /// </summary>
    public interface IJsonRpcError : IJsonRpcPayload
    {
        /// <summary>
        ///     The error for this JSON RPC response or null if no error is present
        /// </summary>
        Error Error { get; }
    }
}