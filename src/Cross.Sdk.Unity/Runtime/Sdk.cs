using System;
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
}