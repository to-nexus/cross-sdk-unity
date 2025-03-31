using System.Threading.Tasks;

namespace Cross.Sign.Models.Engine
{
    /// <summary>
    ///     A class that representing a pending session proposal. Includes a URI that can be given to a
    ///     wallet app out-of-band and an Approval Task that can be awaited.
    /// </summary>
    public class ConnectedData
    {
        /// <summary>
        ///     A class that representing a pending session proposal. Includes a URI that can be given to a
        ///     wallet app out-of-band and an Approval Task that can be awaited.
        /// </summary>
        public ConnectedData(string uri, string pairingTopic, Task<Session> approval)
        {
            Uri = uri;
            PairingTopic = pairingTopic;
            Approval = approval;
        }

        /// <summary>
        ///     The URI that can be used to retrieve the submitted session proposal. This should be shared
        ///     SECURELY out-of-band to a wallet supporting the SDK.
        /// </summary>
        public string Uri { get; private set; }

        /// <summary>
        ///     Pairing is a topic encrypted by a symmetric key shared through a URI between two clients with
        ///     the sole purpose of communicating session proposals.
        /// </summary>
        public string PairingTopic { get; private set; }

        /// <summary>
        ///     A task that will resolve to an approved session. If the session proposal is rejected, then this
        ///     task will throw an exception.
        /// </summary>
        public Task<Session> Approval { get; private set; }
    }
}