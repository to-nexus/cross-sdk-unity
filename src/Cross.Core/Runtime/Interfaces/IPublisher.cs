using System;
using System.Threading.Tasks;
using Cross.Core.Common;
using Cross.Core.Models.Publisher;
using Cross.Core.Models.Relay;

namespace Cross.Core.Interfaces
{
    /// <summary>
    ///     An interface for the Publisher module. The Publisher module is responsible for sending messages to the
    ///     WalletConnect relay server.
    /// </summary>
    public interface IPublisher : IModule
    {
        /// <summary>
        ///     The IRelayer instance this publisher is using to publish messages
        /// </summary>
        public IRelayer Relayer { get; }

        event EventHandler<PublishParams> OnPublishedMessage;

        /// <summary>
        ///     Publish a new message to the relayer. This will be sent to the peer that is connected to the relayer.
        /// </summary>
        /// <param name="topic">The topic to publish the message in</param>
        /// <param name="message">The message to publish</param>
        /// <param name="opts">(optional) PublishOptions specifying TTL the Tag.</param>
        /// <returns></returns>
        public Task Publish(string topic, string message, PublishOptions opts = null);
    }
}