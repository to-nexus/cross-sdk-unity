using System.Threading.Tasks;
using Cross.Core.Common;
using Cross.Core.Crypto.Interfaces;
using Cross.Core.Models;
using Cross.Core.Models.Verify;
using Cross.Core.Storage.Interfaces;

namespace Cross.Core.Interfaces
{
    /// <summary>
    ///     Represents the Core module and all fields the Core module will have
    /// </summary>
    public interface ICoreClient : IModule
    {
        /// <summary>
        ///     The Protocol string this Core module will use
        /// </summary>
        public const string Protocol = "wc";

        /// <summary>
        ///     The Protocol version this Core module will use
        /// </summary>
        public const int Version = 2;

        /// <summary>
        ///     The Relay URL this Core module will use
        /// </summary>
        public string RelayUrl { get; }

        /// <summary>
        ///     The project id this Core module will use
        /// </summary>
        public string ProjectId { get; }

        /// <summary>
        ///     The CoreOptions this Core module was initialized with
        /// </summary>
        public CoreOptions Options { get; }

        /// <summary>
        ///     The HeartBeat module this Core module is using. Acts as a consistent interval used for timing
        /// </summary>
        public IHeartBeat HeartBeat { get; }

        /// <summary>
        ///     The Crypto module this Core module is using. Keeps track of keypairs and executing cryptographic
        ///     functions
        /// </summary>
        public ICrypto Crypto { get; }

        /// <summary>
        ///     The Relayer module this Core module is using. Network layer that acts as relay between wallet / dapp
        /// </summary>
        public IRelayer Relayer { get; }

        /// <summary>
        ///     The Storage module this Core module is using. Used to store persistant state information between
        ///     SDK executions
        /// </summary>
        public IKeyValueStorage Storage { get; }

        /// <summary>
        ///     The <see cref="ITypedMessageHandler" /> module this Core module is using. Use this for handling
        ///     custom message types (request or response) and for sending messages (request, responses or errors)
        /// </summary>
        public ITypedMessageHandler MessageHandler { get; }

        /// <summary>
        ///     The <see cref="IExpirer" /> module this Sign Client is using to track expiration dates
        /// </summary>
        IExpirer Expirer { get; }

        /// <summary>
        ///     The <see cref="IJsonRpcHistoryFactory" /> factory this Core module is using. Used for storing
        ///     JSON RPC request and responses of various types T, TR
        /// </summary>
        IJsonRpcHistoryFactory History { get; }

        /// <summary>
        ///     The <see cref="IPairing" /> module this Core module is using. Used for pairing two peers
        ///     with each other and keeping track of pairing state
        /// </summary>
        IPairing Pairing { get; }

        Verifier Verify { get; }

        /// <summary>
        ///     Start the Core module, which will initialize all modules the Core module uses
        /// </summary>
        public Task Start();
    }
}