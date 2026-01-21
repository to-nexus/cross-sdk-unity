using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Cross.Sdk.Unity.Model.BlockchainApi;

namespace Cross.Sdk.Unity
{
    public class NetworkControllerCore : NetworkController
    {
        protected override async Task InitializeAsyncCore(IEnumerable<Chain> supportedChains)
        {
            // 1. API로 최신 체인 정보 가져오기 시도
            var apiResponse = await FetchChainInfoAsync();
            
            // 2. API 성공 시 merge, 실패 시 기본 체인 사용
            IEnumerable<Chain> finalChains;
            if (apiResponse != null && apiResponse.Code == 200 && apiResponse.Data != null)
            {
                finalChains = MergeChains(supportedChains, apiResponse.Data);
            }
            else
            {
                finalChains = supportedChains;
            }

            Chains = new ReadOnlyDictionary<string, Chain>(finalChains.ToDictionary(c => c.ChainId, c => c));
            ActiveChain = null;
        }

        private async Task<ChainApiResponse> FetchChainInfoAsync()
        {
            try
            {
                var response = await CrossSdk.BlockchainApiController.FetchChainInfoAsync();
                
                if (response != null && response.Code == 200)
                {
                    return response;
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[NetworkController] Failed to fetch chain info: {ex.Message}");
            }
            
            return null;
        }

        private IEnumerable<Chain> MergeChains(IEnumerable<Chain> defaultChains, EthChainInfo[] apiChains)
        {
            // 기존 체인을 ChainReference(체인 ID 숫자)로 Dictionary 생성
            var chainDict = defaultChains.ToDictionary(c => c.ChainReference, c => c);
            
            // API 데이터로 덮어쓰기 + 새 체인 추가
            foreach (var apiChain in apiChains)
            {
                var chainReference = apiChain.ChainId.ToString();
                var mappedChain = MapApiChainToChain(apiChain);
                chainDict[chainReference] = mappedChain;
            }
            
            return chainDict.Values;
        }

        private Chain MapApiChainToChain(EthChainInfo apiChain)
        {
            return new Chain(
                ChainConstants.Namespaces.Evm,
                apiChain.ChainId.ToString(),
                apiChain.Name,
                new Currency(apiChain.CurrencyName, apiChain.CurrencySymbol, apiChain.CurrencyDecimals),
                new BlockExplorer("Explorer", apiChain.ExplorerUrl),
                apiChain.Rpc,
                apiChain.Testnet,
                GetChainImageUrl(apiChain.ChainId)
            );
        }

        private string GetChainImageUrl(int chainId)
        {
            // 기존 하드코딩된 체인의 이미지 URL 사용
            var existingChain = ChainConstants.Chains.All.FirstOrDefault(c => c.ChainReference == chainId.ToString());
            if (existingChain != null && !string.IsNullOrWhiteSpace(existingChain.ImageUrl))
            {
                return existingChain.ImageUrl;
            }

            // 기본 이미지 URL 생성 (API에서 제공하지 않는 경우)
            return $"https://dev-imgproxy-api.crosstoken.io/rs:fit:512:512:1/plain/https://contents.crosstoken.io/wallet/chain/images/{chainId}.svg@png";
        }

        protected override async Task ChangeActiveChainAsyncCore(Chain chain)
        {
            if (CrossSdk.ConnectorController.IsAccountConnected)
            {
                // Request connector to change active chain.
                // If connector approves the change, it will trigger the ChainChanged event.
                await CrossSdk.ConnectorController.ChangeActiveChainAsync(chain);

                var previousChain = ActiveChain;
                ActiveChain = chain;
                OnChainChanged(new ChainChangedEventArgs(previousChain, chain));
            }
            else
            {
                ActiveChain = chain;
            }

            CrossSdk.EventsController.SendEvent(new Event
            {
                name = "SWITCH_NETWORK",
                properties = new Dictionary<string, object>
                {
                    { "network", chain.ChainId }
                }
            });
        }

        protected override void ConnectorChainChangedHandlerCore(object sender, Connector.ChainChangedEventArgs e)
        {
            if (ActiveChain?.ChainId == e.ChainId)
                return;
            
            var chain = Chains.GetValueOrDefault(e.ChainId);

            var previousChain = ActiveChain;
            ActiveChain = chain;
            OnChainChanged(new ChainChangedEventArgs(previousChain, chain));
        }

        protected override async void ConnectorAccountConnectedHandlerCore(object sender, Connector.AccountConnectedEventArgs e)
        {
            var accounts = await e.GetAccounts();
            var previousChain = ActiveChain;

            if (ActiveChain == null)
            {
                var defaultAccount = await e.GetAccount();

                if (Chains.TryGetValue(defaultAccount.ChainId, out var defaultAccountChain))
                {
                    ActiveChain = defaultAccountChain;
                    OnChainChanged(new ChainChangedEventArgs(previousChain, defaultAccountChain));
                    return;
                }

                var account = Array.Find(accounts, a => Chains.ContainsKey(a.ChainId));
                if (account == default)
                {
                    ActiveChain = null;
                    OnChainChanged(new ChainChangedEventArgs(previousChain, null));
                    return;
                }

                var chain = Chains[account.ChainId];

                ActiveChain = chain;
                OnChainChanged(new ChainChangedEventArgs(previousChain, chain));
            }
            else
            {
                var defaultAccount = await e.GetAccount();
                if (defaultAccount.ChainId == ActiveChain.ChainId)
                    return;

                if (Array.Exists(accounts, a => a.ChainId == ActiveChain.ChainId))
                {
                    await ChangeActiveChainAsync(ActiveChain);
                    return;
                }

                var account = Array.Find(accounts, a => Chains.ContainsKey(a.ChainId));
                if (account == default)
                {
                    ActiveChain = null;
                    OnChainChanged(new ChainChangedEventArgs(previousChain, null));
                }
                else
                {
                    var chain = Chains[account.ChainId];

                    ActiveChain = chain;
                    OnChainChanged(new ChainChangedEventArgs(previousChain, chain));
                }
            }
        }
    }
}