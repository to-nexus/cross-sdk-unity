using Newtonsoft.Json;
using Cross.Core.Network;

namespace Cross.Core.Models.History
{
    /// <summary>
    ///     A class representing a pending JSON RPC request event.
    /// </summary>
    /// <typeparam name="T">The type of request</typeparam>
    public class RequestEvent<T>
    {
        /// <summary>
        ///     The chainId this request is intended for
        /// </summary>
        [JsonProperty("chainId")]
        public string ChainId;

        /// <summary>
        ///     The request parameters sent
        /// </summary>
        [JsonProperty("request")]
        public IRequestArguments<T> Request;

        /// <summary>
        ///     The topic the request was sent in
        /// </summary>
        [JsonProperty("topic")]
        public string Topic;

        /// <summary>
        ///     A helper function to create a new RequestEvent from a <see cref="JsonRpcRecord{T,R}" />
        /// </summary>
        /// <param name="pending">The pending <see cref="JsonRpcRecord{T,R}" /></param>
        /// <typeparam name="TR">The response type expected for this request</typeparam>
        /// <returns>A new RequestEvent based on the given <see cref="JsonRpcRecord{T,R}" /></returns>
        public static RequestEvent<T> FromPending<TR>(JsonRpcRecord<T, TR> pending)
        {
            return new RequestEvent<T>
            {
                Topic = pending.Topic,
                Request = pending.Request,
                ChainId = pending.ChainId
            };
        }
    }
}