﻿using System;
using System.Threading.Tasks;
using Cross.Core.Common;
using Cross.Core.Models.Pairing;

namespace Cross.Core.Interfaces
{
    /// <summary>
    ///     The interface for a module that handles pairing two peers and storing related data
    /// </summary>
    public interface IPairing : IModule
    {
        /// <summary>
        ///     Get the <see cref="IStore{TKey,TValue}" /> module that is handling the storage of
        ///     <see cref="PairingStruct" />
        /// </summary>
        IPairingStore Store { get; }

        /// <summary>
        ///     Get all active and inactive pairings
        /// </summary>
        PairingStruct[] Pairings { get; }

        /// <summary>
        ///     The <see cref="ICoreClient" /> module using this module instance
        /// </summary>
        ICoreClient CoreClient { get; }

        event EventHandler<PairingCreatedEventArgs> PairingCreated;
        
        event EventHandler<PairingEvent> PairingExpired;

        event EventHandler<PairingEvent> PairingPinged;

        event EventHandler<PairingEvent> PairingDeleted;

        /// <summary>
        ///     Initialize this pairing module. This will restore all active / inactive pairings
        ///     from storage
        /// </summary>
        Task Init();

        /// <summary>
        ///     Pair with a peer using the given uri. The uri must be in the correct
        ///     format otherwise an exception will be thrown. You may (optionally) pair
        ///     without activating the pairing. By default the pairing will be activated before
        ///     it is returned
        /// </summary>
        /// <param name="uri">The URI to pair with</param>
        /// <returns>The pairing data that can be used to pair with the peer</returns>
        Task<PairingStruct> Pair(string uri, bool activatePairing = true);

        /// <summary>
        ///     Create a new pairing at the given pairing topic
        /// </summary>
        /// <returns>
        ///     A new instance of <see cref="CreatePairingData" /> that includes the pairing topic and
        ///     uri
        /// </returns>
        Task<CreatePairingData> Create(string[] methods = null);

        /// <summary>
        ///     Activate a previously created pairing at the given topic
        /// </summary>
        /// <param name="topic">The topic of the pairing to activate</param>
        Task Activate(string topic);

        /// <summary>
        ///     Subscribe to method requests
        /// </summary>
        /// <param name="methods">The methods to register and subscribe</param>
        Task Register(string[] methods);

        /// <summary>
        ///     Update the expiration of an existing pairing at the given topic
        /// </summary>
        /// <param name="topic">The topic of the pairing to update</param>
        /// <param name="expiration">The new expiration date as a unix timestamp (seconds)</param>
        /// <returns></returns>
        Task UpdateExpiry(string topic, long expiration);

        /// <summary>
        ///     Update the metadata of an existing pairing at the given topic
        /// </summary>
        /// <param name="topic">The topic of the pairing to update</param>
        /// <param name="metadata">The new metadata</param>
        Task UpdateMetadata(string topic, Metadata metadata);

        /// <summary>
        ///     Ping an existing pairing at the given topic
        /// </summary>
        /// <param name="topic">The topic of the pairing to ping</param>
        Task Ping(string topic);

        /// <summary>
        ///     Disconnect an existing pairing at the given topic
        /// </summary>
        /// <param name="topic">The topic of the pairing to disconnect</param>
        Task Disconnect(string topic);

        /// <summary>
        ///     Parse a session proposal URI and return all information in the URI in a
        ///     new <see cref="UriParameters" /> object
        /// </summary>
        /// <param name="uri">The uri to parse</param>
        /// <returns>
        ///     A new <see cref="UriParameters" /> object that contains all data
        ///     parsed from the given uri
        /// </returns>
        UriParameters ParseUri(string uri);
        
        /// <summary>
        ///     Get the expected methods for a given pairing topic
        /// </summary>
        public bool TryGetExpectedMethods(string topic, out string[] methods);
    }
}