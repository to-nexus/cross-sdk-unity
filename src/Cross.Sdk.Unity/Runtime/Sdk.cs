using System;
using System.Threading;
using System.Threading.Tasks;
using Cross.Core.Common.Utils;
using Cross.Sign.Models;
using Cross.Sign.Unity;
using Nethereum.Contracts.Standards.ERC20.TokenList;
using UnityEngine;

namespace Cross.Sdk.Unity
{
    public abstract class CrossSdk : MonoBehaviour
    {
        [VersionMarker]
        public const string Version = "sdk-unity-v1.0.0";
        
        public static CrossSdk Instance { get; protected set; }

        public static ModalController ModalController { get; protected set; }
        public static AccountController AccountController { get; protected set; }
        public static ConnectorController ConnectorController { get; protected set; }
        public static ApiController ApiController { get; protected set; }
        public static BlockchainApiController BlockchainApiController { get; protected set; }
        public static NotificationController NotificationController { get; protected set; }
        public static NetworkController NetworkController { get; protected set; }
        public static EventsController EventsController { get; protected set; }
        public static SiweController SiweController { get; protected set; }

        public static EvmService Evm { get; protected set; }

        public static CrossSdkConfig Config { get; private set; }
        
        public SignClientUnity SignClient { get; protected set; }
        
        public static bool IsInitialized { get; private set; }

        public static bool IsAccountConnected
        {
            get => ConnectorController.IsAccountConnected;
        }

        public static bool IsModalOpen
        {
            get => ModalController.IsOpen;
        }

        public static event EventHandler<InitializeEventArgs> Initialized;

        public static event EventHandler<Connector.AccountConnectedEventArgs> AccountConnected
        {
            add => ConnectorController.AccountConnected += value;
            remove => ConnectorController.AccountConnected -= value;
        }

        public static event EventHandler<Connector.AccountDisconnectedEventArgs> AccountDisconnected
        {
            add => ConnectorController.AccountDisconnected += value;
            remove => ConnectorController.AccountDisconnected -= value;
        }

        public static event EventHandler<Connector.AccountChangedEventArgs> AccountChanged
        {
            add => ConnectorController.AccountChanged += value;
            remove => ConnectorController.AccountChanged -= value;
        }

        public static event EventHandler<NetworkController.ChainChangedEventArgs> ChainChanged
        {
            add => NetworkController.ChainChanged += value;
            remove => NetworkController.ChainChanged -= value;
        }

        public static async Task InitializeAsync(CrossSdkConfig config)
        {
            if (Instance == null)
                throw new Exception("Instance not set");
            if (IsInitialized)
                throw new Exception("Already initialized"); // TODO: use custom ex type

            Config = config ?? throw new ArgumentNullException(nameof(config));

            await Instance.InitializeAsyncCore();

            IsInitialized = true;
            Initialized?.Invoke(null, new InitializeEventArgs());
        }

        public static void OpenModal(ViewType viewType = ViewType.None)
        {
            if (!IsInitialized)
                throw new Exception("CrossSdk not initialized"); // TODO: use custom ex type

            Instance.OpenModalCore(viewType);
        }

        public static void Connect() {
            if (IsModalOpen)
                return;

            OpenModal();
        }

        public static void ConnectWithWallet(string walletId)
        {
            if (IsModalOpen)
                return;

            if (!IsInitialized)
                throw new Exception("CrossSdk not initialized");

            Instance.ConnectWithWalletCore(walletId);
        }

        /// <summary>
        ///     Opens the modal to connect a wallet and performs SIWE authentication.
        ///     This method will always prompt for SIWE, regardless of the <see cref="SiweConfig.Required" /> setting.
        /// </summary>
        /// <returns>A task that represents the authentication operation with the result.</returns>
        /// <exception cref="InvalidOperationException">Thrown when SIWE is not configured.</exception>
        public static async Task<AuthenticationResult> Authenticate()
        {
            if (IsModalOpen)
                return new AuthenticationResult { Authenticated = false };

            if (!IsInitialized)
                throw new Exception("CrossSdk not initialized");

            // Check if SIWE is configured
            if (Config.siweConfig == null)
            {
                throw new InvalidOperationException(
                    "SIWE is not configured. Cannot authenticate without SiweConfig.\n" +
                    "Please configure CrossSdkConfig.siweConfig before calling Authenticate(), " +
                    "or use Connect()/ConnectWithWallet() for regular connection without authentication."
                );
            }

            // Temporarily enable and set SIWE as required for this connection
            var originalEnabled = Config.siweConfig.Enabled;
            var originalRequired = Config.siweConfig.Required;
            var originalGetRequired = Config.siweConfig.GetRequired;
            
            Config.siweConfig.Enabled = true;
            Config.siweConfig.Required = true;
            Config.siweConfig.GetRequired = () => true;

            try
            {
                // Wait for connection to complete with timeout
                var tcs = new TaskCompletionSource<bool>();
                var cts = new CancellationTokenSource();
                
                void OnConnected(object sender, Connector.AccountConnectedEventArgs e)
                {
                    tcs.TrySetResult(true);
                }
                
                void OnDisconnected(object sender, Connector.AccountDisconnectedEventArgs e)
                {
                    tcs.TrySetResult(false);
                }
                
                // Subscribe to events BEFORE opening modal to avoid race condition
                AccountConnected += OnConnected;
                AccountDisconnected += OnDisconnected;
                
                OpenModal();
                
                try
                {
                    // Wait for either connection completion or timeout (4.5 minutes)
                    // Timeout is longer than URI refresh interval (4 minutes) to allow for URI renewal
                    var completedTask = await Task.WhenAny(
                        tcs.Task,
                        Task.Delay(270000, cts.Token) // 270 seconds = 4.5 minutes
                    );
                    
                    if (completedTask == tcs.Task)
                    {
                        // Connection completed (success or failure)
                        var connected = await tcs.Task;
                        
                        if (!connected)
                        {
                            NotificationController.Notify(
                                NotificationType.Error,
                                "Connection failed. Please try again."
                            );
                            return new AuthenticationResult { Authenticated = false };
                        }
                    }
                    else
                    {
                        // Timeout occurred
                        Debug.LogWarning("[CrossSdk] Authentication timeout after 4.5 minutes");
                        NotificationController.Notify(
                            NotificationType.Error,
                            "Connection timeout. Please try again."
                        );
                        return new AuthenticationResult { Authenticated = false };
                    }
                }
                finally
                {
                    cts.Cancel(); // Cancel the timeout task
                    AccountConnected -= OnConnected;
                    AccountDisconnected -= OnDisconnected;
                }
                
                // Check if SIWE session exists
                if (SiweController.TryLoadSiweSessionFromStorage(out var session))
                {
                    return new AuthenticationResult
                    {
                        Authenticated = true,
                        Session = session
                    };
                }
                
                return new AuthenticationResult { Authenticated = false };
            }
            finally
            {
                // Restore original settings
                if (Config.siweConfig != null)
                {
                    Config.siweConfig.Enabled = originalEnabled;
                    Config.siweConfig.Required = originalRequired;
                    Config.siweConfig.GetRequired = originalGetRequired;
                }
            }
        }

        /// <summary>
        ///     Connects to a specific wallet and performs SIWE authentication.
        ///     This method will always prompt for SIWE, regardless of the <see cref="SiweConfig.Required" /> setting.
        /// </summary>
        /// <param name="walletId">The wallet ID to connect to (e.g., "cross_wallet").</param>
        /// <returns>A task that represents the authentication operation with the result.</returns>
        /// <exception cref="InvalidOperationException">Thrown when SIWE is not configured.</exception>
        public static async Task<AuthenticationResult> AuthenticateWithWallet(string walletId)
        {
            if (IsModalOpen)
                return new AuthenticationResult { Authenticated = false };

            if (!IsInitialized)
                throw new Exception("CrossSdk not initialized");

            // Check if SIWE is configured
            if (Config.siweConfig == null)
            {
                throw new InvalidOperationException(
                    "SIWE is not configured. Cannot authenticate without SiweConfig.\n" +
                    "Please configure CrossSdkConfig.siweConfig before calling AuthenticateWithWallet(), " +
                    "or use Connect()/ConnectWithWallet() for regular connection without authentication."
                );
            }

            // Temporarily enable and set SIWE as required for this connection
            var originalEnabled = Config.siweConfig.Enabled;
            var originalRequired = Config.siweConfig.Required;
            var originalGetRequired = Config.siweConfig.GetRequired;
            
            Config.siweConfig.Enabled = true;
            Config.siweConfig.Required = true;
            Config.siweConfig.GetRequired = () => true;

            try
            {
                // Wait for connection to complete with timeout
                var tcs = new TaskCompletionSource<bool>();
                var cts = new CancellationTokenSource();
                
                void OnConnected(object sender, Connector.AccountConnectedEventArgs e)
                {
                    tcs.TrySetResult(true);
                }
                
                void OnDisconnected(object sender, Connector.AccountDisconnectedEventArgs e)
                {
                    tcs.TrySetResult(false);
                }
                
                // Subscribe to events BEFORE initiating connection to avoid race condition
                AccountConnected += OnConnected;
                AccountDisconnected += OnDisconnected;
                
                Instance.ConnectWithWalletCore(walletId);
                
                try
                {
                    // Wait for either connection completion or timeout (4.5 minutes)
                    // Timeout is longer than URI refresh interval (4 minutes) to allow for URI renewal
                    var completedTask = await Task.WhenAny(
                        tcs.Task,
                        Task.Delay(270000, cts.Token) // 270 seconds = 4.5 minutes
                    );
                    
                    if (completedTask == tcs.Task)
                    {
                        // Connection completed (success or failure)
                        var connected = await tcs.Task;
                        
                        if (!connected)
                        {
                            NotificationController.Notify(
                                NotificationType.Error,
                                "Connection failed. Please try again."
                            );
                            return new AuthenticationResult { Authenticated = false };
                        }
                    }
                    else
                    {
                        // Timeout occurred
                        Debug.LogWarning("[CrossSdk] Authentication timeout after 4.5 minutes");
                        NotificationController.Notify(
                            NotificationType.Error,
                            "Connection timeout. Please try again."
                        );
                        return new AuthenticationResult { Authenticated = false };
                    }
                }
                finally
                {
                    cts.Cancel(); // Cancel the timeout task
                    AccountConnected -= OnConnected;
                    AccountDisconnected -= OnDisconnected;
                }
                
                // Check if SIWE session exists
                if (SiweController.TryLoadSiweSessionFromStorage(out var session))
                {
                    return new AuthenticationResult
                    {
                        Authenticated = true,
                        Session = session
                    };
                }
                
                return new AuthenticationResult { Authenticated = false };
            }
            finally
            {
                // Restore original settings
                if (Config.siweConfig != null)
                {
                    Config.siweConfig.Enabled = originalEnabled;
                    Config.siweConfig.Required = originalRequired;
                    Config.siweConfig.GetRequired = originalGetRequired;
                }
            }
        }

        public static void CloseModal()
        {
            if (!IsModalOpen)
                return;

            Instance.CloseModalCore();
        }

        public static Task<Account> GetAccountAsync()
        {
            return ConnectorController.GetAccountAsync();
        }

        public static async Task UpdateBalance()
        {
            await AccountController.UpdateBalance();
        }

        public static Token[] GetTokens()
        {
            return AccountController.Tokens;
        }

        public static Task DisconnectAsync()
        {
            if (!IsInitialized)
                throw new Exception("CrossSdk not initialized"); // TODO: use custom ex type

            if (!IsAccountConnected)
                throw new Exception("No account connected"); // TODO: use custom ex type

            return Instance.DisconnectAsyncCore();
        }

        protected abstract Task InitializeAsyncCore();

        protected abstract void OpenModalCore(ViewType viewType = ViewType.None);

        protected abstract void CloseModalCore();

        protected abstract Task DisconnectAsyncCore();

        protected abstract void ConnectWithWalletCore(string walletId);

        public class InitializeEventArgs : EventArgs
        {
            [Preserve]
            public InitializeEventArgs()
            {
            }
        }
    }

    /// <summary>
    ///     Result of authentication (Connect + SIWE) operation.
    /// </summary>
    public class AuthenticationResult
    {
        /// <summary>
        ///     True if SIWE authentication was completed successfully.
        /// </summary>
        public bool Authenticated { get; set; }

        /// <summary>
        ///     The SIWE session, if authentication was successful.
        /// </summary>
        public SiweSession Session { get; set; }
    }
}