using System;
using System.Threading;
using System.Threading.Tasks;
using Cross.Core.Common;
using Cross.Core.Common.Model.Relay;
using Cross.Core.Models.Relay;
using Cross.Core.Network;

namespace Cross.Core.Interfaces
{
    /// <summary>
    ///     The Relayer module handles the interaction with the WalletConnect relayer server.
    ///     Each Relayer module uses a Publisher, Subscriber and a JsonRPCProvider.
    /// </summary>
    public interface IRelayer : IModule
    {
        /// <summary>
        /// </summary>
        public const string Protocol = RelayConstants.Protocol;

        /// <summary>
        /// </summary>
        public const int Version = RelayConstants.Version;

        /// <summary>
        ///     The ICore module that is using this Relayer module
        /// </summary>
        public ICoreClient CoreClient { get; }

        bool TransportExplicitlyClosed { get; }

        //TODO Add logger

        /// <summary>
        ///     The ISubscriber module that this Relayer module is using
        /// </summary>
        public ISubscriber Subscriber { get; }

        /// <summary>
        ///     The IPublisher module that this Relayer module is using
        /// </summary>
        public IPublisher Publisher { get; }

        /// <summary>
        ///     The IMessageTracker module that this Relayer module is using
        /// </summary>
        public IMessageTracker Messages { get; }

        /// <summary>
        ///     The IJsonRpcProvider module that this Relayer module is using
        /// </summary>
        public IJsonRpcProvider Provider { get; }

        /// <summary>
        ///     How long the <see cref="IRelayer" /> should wait before throwing a <see cref="TimeoutException" /> during
        ///     the connection phase. If this field is null, then the timeout will be infinite.
        /// </summary>
        public TimeSpan? ConnectionTimeout { get; set; }

        /// <summary>
        ///     Whether this Relayer is connected to the WalletConnect relay server
        /// </summary>
        public bool Connected { get; }

        /// <summary>
        ///     Whether this Relayer is currently connecting to the WalletConnect relay server
        /// </summary>
        public bool Connecting { get; }

        event EventHandler OnConnected;

        event EventHandler OnDisconnected;

        event EventHandler<Exception> OnErrored;

        event EventHandler<MessageEvent> OnMessageReceived;

        event EventHandler OnTransportClosed;

        event EventHandler OnConnectionStalled;

        /// <summary>
        ///     Initialize this Relayer module. This will initialize all sub-modules
        ///     and connect the backing IJsonRpcProvider.
        /// </summary>
        public Task Init();

        /// <summary>
        ///     Publish a message to this Relayer in the given topic (optionally) specifying
        ///     PublishOptions.
        /// </summary>
        /// <param name="topic">The topic to publish the message in</param>
        /// <param name="message">The message to publish</param>
        /// <param name="opts">(Optional) Publish options to specify TTL and tag</param>
        public Task Publish(string topic, string message, PublishOptions opts = null);

        /// <summary>
        ///     Subscribe to a given topic optionally specifying Subscribe options
        /// </summary>
        /// <param name="topic">The topic to subscribe to</param>
        /// <param name="opts">(Optional) Subscribe options that specify protocol options</param>
        /// <returns></returns>
        public Task<string> Subscribe(string topic, SubscribeOptions opts = null);

        /// <summary>
        ///     Unsubscribe to a given topic optionally specify unsubscribe options
        /// </summary>
        /// <param name="topic">Tbe topic to unsubscribe to</param>
        /// <param name="opts">(Optional) Unsubscribe options specifying protocol options</param>
        /// <returns></returns>
        public Task Unsubscribe(string topic, UnsubscribeOptions opts = null);

        /// <summary>
        ///     Send a Json RPC request with a parameter field of type T, and decode a response with the type of TR.
        /// </summary>
        /// <param name="request">The json rpc request to send</param>
        /// <param name="context">The current context</param>
        /// <typeparam name="T">The type of the parameter field in the json rpc request</typeparam>
        /// <typeparam name="TR">The type of the parameter field in the json rpc response</typeparam>
        /// <returns>The decoded response for the request</returns>
        Task<TR> Request<T, TR>(IRequestArguments<T> request, object context = null);

        public Task TransportClose();

        public Task TransportOpen(string relayUrl = null);

        public Task RestartTransport(string relayUrl = null, CancellationToken cancellationToken = default);

        internal void TriggerConnectionStalled();
    }
}