using System;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Cross.Sign.Models;
using Cross.Sign.Models.Engine;
using Cross.Sign.Models.Engine.Methods;
using Cross.Sign.Nethereum;
using Cross.Sign.Nethereum.Model;
using Cross.Sign.Unity;
using UnityEngine;

namespace Cross.Sdk.Unity
{
    public class WalletConnectConnector : Connector
    {
        private ConnectionProposal _connectionProposal;
        private SignClientUnity _signClient;

        public WalletConnectConnector()
        {
            ImageId = "ef1a1fcf-7fe8-4d69-bd6d-fda1345b4400";
            Type = ConnectorType.WalletConnect;
        }

        public SignClientUnity SignClient
        {
            get => _signClient;
        }

        protected override Task InitializeAsyncCore(CrossSdkConfig config, SignClientUnity signClient)
        {
            _signClient = signClient;
            DappSupportedChains = config.supportedChains;

            _signClient.SubscribeToSessionEvent("chainChanged", ActiveChainIdChangedHandler);
            
            _signClient.SessionUpdatedUnity += ActiveSessionChangedHandler;
            _signClient.SessionDisconnectedUnity += SessionDeletedHandler;

            return Task.CompletedTask;
        }

        private void ActiveSessionChangedHandler(object sender, Session session)
        {
            if (session == null)
                return;

            var currentAccount = GetCurrentAccount();
            OnAccountChanged(new AccountChangedEventArgs(currentAccount));
        }

        private async void ActiveChainIdChangedHandler(object sender, SessionEvent<JToken> sessionEvent)
        {
            if (sessionEvent.ChainId == "eip155:0")
                return;

            // Wait for the session to be updated before changing the default chain id
            await Task.Delay(TimeSpan.FromSeconds(1));

            await _signClient.AddressProvider.SetDefaultChainIdAsync(sessionEvent.ChainId);

            OnChainChanged(new ChainChangedEventArgs(sessionEvent.ChainId));
            OnAccountChanged(new AccountChangedEventArgs(GetCurrentAccount()));
        }

        private async void SessionDeletedHandler(object sender, EventArgs e)
        {
            if (!IsAccountConnected)
                return;
            
            IsAccountConnected = false;
            OnAccountDisconnected(AccountDisconnectedEventArgs.Empty);
        }

        protected override async Task<bool> TryResumeSessionAsyncCore()
        {
            var isResumed = await _signClient.TryResumeSessionAsync();

            if (isResumed && CrossSdk.SiweController.IsEnabled)
            {
                var siweSessionJson = PlayerPrefs.GetString(SiweController.SessionPlayerPrefsKey);

                // If no siwe session is found, request signature
                if (string.IsNullOrWhiteSpace(siweSessionJson))
                {
                    Debug.Log("[WalletConnectConnector] No Siwe session found. Requesting signature.");
                    OnSignatureRequested();
                    return true;
                }

                var account = await GetAccountAsyncCore();
                var siweSession = JsonConvert.DeserializeObject<SiweSession>(siweSessionJson);
                
                var addressesMatch = string.Equals(siweSession.EthAddress, account.Address, StringComparison.InvariantCultureIgnoreCase);
                var chainsMatch = siweSession.EthChainIds.Contains(Core.Utils.ExtractChainReference(account.ChainId));

                // If siwe session found, but it doesn't match the sign session, request signature (i.e. new siwe session)
                if (!addressesMatch || !chainsMatch)
                {
                    OnSignatureRequested();
                    return true;
                }

                return true;
            }

            return isResumed;
        }

        protected override ConnectionProposal ConnectCore()
        {
            if (_connectionProposal is { IsConnected: false })
                return _connectionProposal;

            var activeChain = CrossSdk.NetworkController.ActiveChain;
            var sortedChains = activeChain != null ? DappSupportedChains.OrderByDescending(chainEntry => chainEntry.ChainId == activeChain.ChainId) : DappSupportedChains;
            var connectOptions = new ConnectOptions
            {
                OptionalNamespaces = sortedChains
                    .GroupBy(chainEntry => chainEntry.ChainNamespace)
                    .ToDictionary(
                        group => group.Key,
                        group => new ProposedNamespace
                        {
                            Methods = new[]
                            {
                                "eth_accounts",
                                "eth_requestAccounts",
                                "eth_sendRawTransaction",
                                "eth_sign",
                                "eth_signTransaction",
                                "eth_signTypedData",
                                "eth_signTypedData_v3",
                                "eth_signTypedData_v4",
                                "eth_sendTransaction",
                                "personal_sign",
                                "wallet_switchEthereumChain",
                                "wallet_addEthereumChain",
                                "wallet_getPermissions",
                                "wallet_requestPermissions",
                                "wallet_registerOnboarding",
                                "wallet_watchAsset",
                                "wallet_scanQRCode"
                            },
                            Chains = group.Select(chainEntry => chainEntry.ChainId).ToArray(),
                            Events = new[]
                            {
                                "chainChanged",
                                "accountsChanged",
                                "message",
                                "disconnect",
                                "connect"
                            }
                        }
                    )
            };
            _connectionProposal = new WalletConnectConnectionProposal(this, _signClient, connectOptions, CrossSdk.SiweController);
            return _connectionProposal;
        }

        protected override async Task DisconnectAsyncCore()
        {
            try
            {
                await _signClient.Disconnect();
            }
            catch (Exception)
            {
                CrossSdk.EventsController.SendEvent(new Event
                {
                    name = "DISCONNECT_ERROR"
                });
                throw;
            }
        }

        protected override async Task ChangeActiveChainAsyncCore(Chain chain)
        {
            if (ActiveSessionSupportsMethod("wallet_switchEthereumChain") && !ActiveSessionIncludesChain(chain.ChainId))
            {
                var ethereumChain = new EthereumChain(
                    chain.ChainReference,
                    chain.Name,
                    chain.NativeCurrency,
                    new[]
                    {
                        chain.RpcUrl
                    },
                    new[]
                    {
                        chain.BlockExplorer.url
                    }
                );
                await _signClient.SwitchEthereumChainAsync(ethereumChain);
            }
            else
            {
                if (!ActiveSessionIncludesChain(chain.ChainId))
                    throw new Exception("Chain is not supported"); // TODO: use custom ex type

                await _signClient.AddressProvider.SetDefaultChainIdAsync(chain.ChainId);
                OnChainChanged(new ChainChangedEventArgs(chain.ChainId));
                OnAccountChanged(new AccountChangedEventArgs(GetCurrentAccount()));
            }
        }

        protected override Task<Account> GetAccountAsyncCore()
        {
            return Task.FromResult(GetCurrentAccount());
        }

        protected override Task<Account[]> GetAccountsAsyncCore()
        {
            var caipAddresses = _signClient.AddressProvider.AllAccounts();
            return Task.FromResult(caipAddresses.Select(caip25Address => new Account(caip25Address.Address, caip25Address.ChainId)).ToArray());
        }

        private Account GetCurrentAccount()
        {
            var caipAddress = _signClient.AddressProvider.CurrentAccount();
            return new Account(caipAddress.Address, caipAddress.ChainId);
        }

        private bool ActiveSessionSupportsMethod(string method)
        {
            var @namespace = _signClient.AddressProvider.DefaultNamespace;
            var activeSession = _signClient.AddressProvider.DefaultSession;
            return activeSession.Namespaces[@namespace].Methods.Contains(method);
        }

        private bool ActiveSessionIncludesChain(string chainId)
        {
            var @namespace = _signClient.AddressProvider.DefaultNamespace;
            var activeSession = _signClient.AddressProvider.DefaultSession;
            var activeNamespace = activeSession.Namespaces[@namespace];

            var chainsOk = activeNamespace.TryGetChains(out var approvedChains);
            return chainsOk && approvedChains.Contains(chainId);
        }
    }
}