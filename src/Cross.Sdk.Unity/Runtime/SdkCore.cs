using System;
using System.Threading.Tasks;
using Cross.Sdk.Unity.Model;
using Cross.Sdk.Unity.Utils;
using Cross.Sign.Models;
using Cross.Sign.Unity;
using UnityEngine;

namespace Cross.Sdk.Unity
{
    public class CrossSdkCore : CrossSdk
    {
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Debug.LogError("[CrossSdk] Instance already exists. Destroying...");
                Destroy(gameObject);
            }
        }

        protected override async Task InitializeAsyncCore()
        {
#if !UNITY_WEBGL || UNITY_EDITOR
            await CreateSignClient();
#endif

            ModalController = CreateModalController();
            AccountController = new AccountController();
            ConnectorController = new ConnectorController();
            ApiController = new ApiController();
            BlockchainApiController = new BlockchainApiController();
            NotificationController = new NotificationController();
            NetworkController = new NetworkControllerCore();
            EventsController = new EventsController();
            SiweController = new SiweController();

#if UNITY_WEBGL && !UNITY_EDITOR
            Evm = new WagmiEvmService();
#else
            Evm = new NethereumEvmService();
#endif

            await Task.WhenAll(
                BlockchainApiController.InitializeAsync(SignClient),
                ConnectorController.InitializeAsync(Config, SignClient),
                ModalController.InitializeAsync(),
                EventsController.InitializeAsync(Config, ApiController),
                NetworkController.InitializeAsync(ConnectorController, Config.supportedChains),
                AccountController.InitializeAsync(ConnectorController, NetworkController, BlockchainApiController)
            );

            await Evm.InitializeAsync(SignClient);

            ConnectorController.AccountConnected += AccountConnectedHandler;
            ConnectorController.AccountDisconnected += AccountDisconnectedHandler;
            
            EventsController.SendEvent(new Event
            {
                name = "MODAL_LOADED"
            });
        }

        protected override void OpenModalCore(ViewType viewType = ViewType.None)
        {
            if (viewType == ViewType.None)
            {
                ModalController.Open(IsAccountConnected ? ViewType.Account : ViewType.Connect);
            }
            else
            {
                if (IsAccountConnected && viewType == ViewType.Connect)
                    // TODO: use custom exception type
                    throw new Exception("Trying to open Connect view when account is already connected.");
                ModalController.Open(viewType);
            }
        }

        protected override void CloseModalCore()
        {
            ModalController.Close();
        }

        protected override Task DisconnectAsyncCore()
        {
            return ConnectorController.DisconnectAsync();
        }

        protected override void ConnectWithWalletCore(string walletId)
        {
            // Find wallet by ID in custom wallets
            var wallet = FindWalletById(walletId);
            if (wallet != null)
            {
                // Set the wallet as last viewed and open wallet view directly
                WalletUtils.SetLastViewedWallet(wallet);
                ModalController.Open(ViewType.Wallet);
            }
            else
            {
                Debug.LogError($"[CrossSdk] Wallet with ID '{walletId}' not found");
                // Fallback to normal connect flow
                OpenModalCore();
            }
        }

        private Wallet FindWalletById(string walletId)
        {
            // Check custom wallets
            if (Config.customWallets != null)
            {
                foreach (var wallet in Config.customWallets)
                {
                    if (wallet.Id == walletId)
                        return wallet;
                }
            }

            return null;
        }

        protected virtual ModalController CreateModalController()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            return new Cross.Sdk.Unity.WebGl.ModalControllerWebGl();
#else
            return new ModalControllerUtk();
#endif
        }

        private async Task CreateSignClient()
        {
            SignClient = await SignClientUnity.Create(new SignClientOptions
            {
                Name = Config.metadata.Name,
                ProjectId = Config.projectId,
                Metadata = Config.metadata
            });
        }

        private static void AccountConnectedHandler(object sender, Connector.AccountConnectedEventArgs e)
        {
            if (WalletUtils.TryGetLastViewedWallet(out var lastViewedWallet))
                WalletUtils.SetRecentWallet(lastViewedWallet);

            // If SIWE is not enabled, close modal immediately
            if (!SiweController.IsEnabled)
            {
                CloseModal();
                return;
            }

            // If SIWE is enabled but not required (optional), close modal
            // The signature request will only be triggered if explicitly requested
            if (!SiweController.Config.IsRequired())
            {
                CloseModal();
            }
            
            // If SIWE is enabled and required, keep modal open and wait for signature request
        }

        private static void AccountDisconnectedHandler(object sender, Connector.AccountDisconnectedEventArgs e)
        {
            CloseModal();
        }
    }
}