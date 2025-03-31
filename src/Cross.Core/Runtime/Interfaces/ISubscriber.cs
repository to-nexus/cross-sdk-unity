using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Cross.Core.Common;
using Cross.Core.Models.Relay;
using Cross.Core.Models.Subscriber;

namespace Cross.Core.Interfaces
{
    /// <summary>
    ///     An interface representing the Subscriber module. This module handles both subscribing to events as well as keeping track
    ///     of active and pending subscriptions. It will also resubscribe to topics if
    ///     the backing Relayer connection disconnects
    /// </summary>
    public interface ISubscriber : IModule
    {
        /// <summary>
        ///     A dictionary of active subscriptions where the key is the id of the Subscription
        /// </summary>
        public IReadOnlyDictionary<string, ActiveSubscription> Subscriptions { get; }

        /// <summary>
        ///     A subscription mapping of Topics => Subscription ids
        /// </summary>
        public ISubscriberMap TopicMap { get; }

        /// <summary>
        ///     The number of active subscriptions
        /// </summary>
        public int Length { get; }

        /// <summary>
        ///     An array of active subscription Ids
        /// </summary>
        public string[] Ids { get; }

        /// <summary>
        ///     An array of active Subscriptions
        /// </summary>
        public ActiveSubscription[] Values { get; }

        /// <summary>
        ///     An array of topics that are currently subscribed
        /// </summary>
        public string[] Topics { get; }

        event EventHandler Sync;
        event EventHandler Resubscribed;
        event EventHandler<ActiveSubscription> Created;
        event EventHandler<DeletedSubscription> Deleted;

        /// <summary>
        ///     Initialize this Subscriber, which will restore + resubscribe to all active subscriptions found
        ///     in storage
        /// </summary>
        public Task Init();

        /// <summary>
        ///     Subscribe to a new topic with (optional) SubscribeOptions
        /// </summary>
        /// <param name="topic">The topic to subscribe to</param>
        /// <param name="opts">Options to determine the protocol to use for subscribing</param>
        /// <returns>The subscription id</returns>
        public Task<string> Subscribe(string topic, SubscribeOptions opts = null);

        /// <summary>
        ///     Unsubscribe to a given topic with optional UnsubscribeOptions
        /// </summary>
        /// <param name="topic">The topic to unsubscribe from</param>
        /// <param name="opts">The options to specify the subscription id as well as protocol options</param>
        public Task Unsubscribe(string topic, UnsubscribeOptions opts = null);

        /// <summary>
        ///     Determines whether the given topic is subscribed or not
        /// </summary>
        /// <param name="topic">The topic to check</param>
        /// ///
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>Return true if the topic is subscribed, false otherwise</returns>
        public Task<bool> IsSubscribed(string topic, CancellationToken cancellationToken = default);
    }
}