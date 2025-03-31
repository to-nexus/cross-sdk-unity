using Newtonsoft.Json;
using Cross.Core.Network.Models;

namespace Cross.Sign.Models.Engine
{
    /// <summary>
    ///     A class that represents parameters for rejecting a session proposal. Contains the id
    ///     of the session proposal to reject and the <see cref="Error" /> reason the session
    ///     proposal was rejected
    /// </summary>
    public class RejectParams
    {
        /// <summary>
        ///     The id of the session proposal to reject
        /// </summary>
        [JsonProperty("id")]
        public long Id;

        /// <summary>
        ///     The reason the session proposal was rejected, as an <see cref="Error" />
        /// </summary>
        [JsonProperty("reason")]
        public Error Reason;
    }
}