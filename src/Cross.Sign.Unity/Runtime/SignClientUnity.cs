using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Cross.Core.Common.Logging;
using Cross.Core.Common.Model.Errors;
using Cross.Core.Crypto;
using Cross.Core.Storage;
using Cross.Core.Storage.Interfaces;
using Cross.Sign.Models;
using Cross.Sign.Models.Engine.Events;
using Cross.Sign.Unity.Utils;
using UnityEngine;

namespace Cross.Sign.Unity
{
    public class SignClientUnity : SignClient
    {
        public Linker Linker { get; }

        private bool _disposed;

        // --- Unity Events (Main Thread) ---
        public event EventHandler<Session> SessionConnectedUnity;
        public event EventHandler<Session> SessionUpdatedUnity;
        public event EventHandler SessionDisconnectedUnity;
        public event EventHandler<SessionRequestEvent> SessionRequestSentUnity;

        private SignClientUnity(SignClientOptions options) : base(options)
        {
            Linker = new Linker(this);

            SessionConnected += SessionConnectedHandler;
            SessionUpdateRequest += SessionUpdatedHandler;
            SessionDeleted += SessionDeletedHandler;
            SessionRequestSent += SessionRequestSentHandler;
        }

        public static async Task<SignClientUnity> Create(SignClientOptions options)
        {
            if (options.Storage == null)
            {
                var storage = await BuildUnityStorage();
                options.Storage = storage;
                options.KeyChain ??= new KeyChain(storage);
            }

            options.RelayUrlBuilder ??= new UnityRelayUrlBuilder();
            options.ConnectionBuilder ??= new ConnectionBuilderUnity();

            var sign = new SignClientUnity(options);
            await sign.Initialize();
            return sign;
        }

        private static async Task<IKeyValueStorage> BuildUnityStorage()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            var currentSyncContext = System.Threading.SynchronizationContext.Current;
            if (currentSyncContext.GetType().FullName != "UnityEngine.UnitySynchronizationContext")
                throw new System.Exception(
                    $"[Cross.Sign.Unity] SynchronizationContext is not of type UnityEngine.UnitySynchronizationContext. Current type is <i>{currentSyncContext.GetType().FullName}</i>. When targeting WebGL, Make sure to initialize SignClient from the main thread.");

            var playerPrefsStorage = new PlayerPrefsStorage(currentSyncContext);
            await playerPrefsStorage.Init();

            return playerPrefsStorage;
#endif

            var path = $"{Application.persistentDataPath}/Cross/storage.json";
            CrossLogger.Log($"[Cross.Sign.Unity] Using storage path <i>{path}</i>");

            var storage = new FileSystemStorage(path);

            try
            {
                await storage.Init();
            }
            catch (JsonException)
            {
                Debug.LogError($"[Cross.Sign.Unity] Failed to deserialize storage. Deleting it and creating a new one at <i>{path}</i>");
                await storage.Clear();
                await storage.Init();
            }

            return storage;
        }

        public async Task<bool> TryResumeSessionAsync()
        {
            await AddressProvider.LoadDefaultsAsync();

            if (AddressProvider.DefaultSession == null || string.IsNullOrWhiteSpace(AddressProvider.DefaultSession.Topic))
                return false;

            try
            {
                await Extend(AddressProvider.DefaultSession.Topic);
            }
            catch (KeyNotFoundException)
            {
                AddressProvider.DefaultSession = null;
                return false;
            }
            catch (CrossNetworkException)
            {
                AddressProvider.DefaultSession = null;
                return false;
            }

            return true;
        }

        private void SessionConnectedHandler(object sender, Session session)
        {
            UnitySyncContext.Context.Post(_ => { SessionConnectedUnity?.Invoke(this, session); }, null);
        }

        private void SessionUpdatedHandler(object sender, SessionEvent sessionEvent)
        {
            var sessionStruct = Session.Values.First(s => s.Topic == sessionEvent.Topic);
            UnitySyncContext.Context.Post(_ => { SessionUpdatedUnity?.Invoke(this, sessionStruct); }, null);
        }

        private void SessionDeletedHandler(object sender, SessionEvent _)
        {
            UnitySyncContext.Context.Post(_ => { SessionDisconnectedUnity?.Invoke(this, EventArgs.Empty); }, null);
        }

        private void SessionRequestSentHandler(object sender, SessionRequestEvent e)
        {
            UnitySyncContext.Context.Post(_ => { SessionRequestSentUnity?.Invoke(this, e); }, null);
        }

        protected override void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                Linker.Dispose();
            }

            base.Dispose(disposing);
            _disposed = true;
        }
    }
}