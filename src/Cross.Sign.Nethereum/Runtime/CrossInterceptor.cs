using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nethereum.JsonRpc.Client;
using Nethereum.RPC;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.RPC.HostWallet;
using Cross.Sign.Nethereum.Model;
using Cross.Sign.Models;
using UnityEngine;

namespace Cross.Sign.Nethereum
{
    public class CrossInterceptor : RequestInterceptor
    {
        private readonly CrossSignService _crossSignService;

        public readonly HashSet<string> SignMethods = new()
        {
            ApiMethods.eth_sendTransaction.ToString(),
            ApiMethods.personal_sign.ToString(),
            ApiMethods.eth_signTypedData_v4.ToString(),
            ApiMethods.wallet_switchEthereumChain.ToString(),
            ApiMethods.wallet_addEthereumChain.ToString()
        };

        public CrossInterceptor(CrossSignService crossSignService)
        {
            _crossSignService = crossSignService;
        }

        public override async Task<object> InterceptSendRequestAsync<T>(
            Func<RpcRequest, string, Task<T>> interceptedSendRequestAsync,
            RpcRequest request,
            string route = null)
        {
            if (!SignMethods.Contains(request.Method))
            {
                return await base
                    .InterceptSendRequestAsync(interceptedSendRequestAsync, request, route)
                    .ConfigureAwait(false);
            }

            if (!_crossSignService.IsWalletConnected)
                throw new InvalidOperationException("[CrossInterceptor] Wallet is not connected");

            if (_crossSignService.IsMethodSupported(request.Method))
            {
                if (request.Method == ApiMethods.eth_sendTransaction.ToString())
                {
                    return await _crossSignService.SendTransactionAsync((TransactionInput)request.RawParameters[0]);
                }

                if (request.Method == ApiMethods.personal_sign.ToString())
                {
                    return await _crossSignService.PersonalSignAsync((string)request.RawParameters[0], (string)request.RawParameters[1]);
                }

                if (request.Method == ApiMethods.eth_signTypedData_v4.ToString())
                {
                    if (request.RawParameters.Length == 1)
                        return await _crossSignService.EthSignTypedDataV4Async((string)request.RawParameters[0]);

                    return await _crossSignService.EthSignTypedDataV4Async((string)request.RawParameters[1], (string)request.RawParameters[0]);
                }

                if (request.Method == ApiMethods.wallet_switchEthereumChain.ToString())
                {
                    return await _crossSignService.WalletSwitchEthereumChainAsync((SwitchEthereumChainParameter)request.RawParameters[0]);
                }

                if (request.Method == ApiMethods.wallet_addEthereumChain.ToString())
                {
                    return await _crossSignService.WalletAddEthereumChainAsync((AddEthereumChainParameter)request.RawParameters[0]);
                }

                throw new NotImplementedException();
            }

            return await base
                .InterceptSendRequestAsync(interceptedSendRequestAsync, request, route)
                .ConfigureAwait(false);
        }

        public override async Task<object> InterceptSendRequestAsync<T>(
            Func<string, string, object[], Task<T>> interceptedSendRequestAsync,
            string method,
            string route = null,
            params object[] paramList)
        {
            if (!SignMethods.Contains(method))
            {
                return await base
                    .InterceptSendRequestAsync(interceptedSendRequestAsync, method, route, paramList)
                    .ConfigureAwait(false);
            }

            if (!_crossSignService.IsWalletConnected)
                throw new InvalidOperationException("[CrossInterceptor] Wallet is not connected");

            if (_crossSignService.IsMethodSupported(method))
            {
                if (method == ApiMethods.eth_sendTransaction.ToString())
                {
                    var customData = (paramList.Length == 2) ? (CustomData)paramList[1] : null;
                    return await _crossSignService.SendTransactionAsync((TransactionInput)paramList[0], customData);
                }

                if (method == ApiMethods.personal_sign.ToString())
                {
                    var customData = (paramList.Length == 3) ? (CustomData)paramList[2] : null;

                    return await _crossSignService.PersonalSignAsync((string)paramList[0], (string)paramList[1], customData);
                }

                if (method == ApiMethods.eth_signTypedData_v4.ToString())
                {
                    if (paramList.Length == 1)
                        return await _crossSignService.EthSignTypedDataV4Async((string)paramList[0]);

                    return await _crossSignService.EthSignTypedDataV4Async((string)paramList[1], (string)paramList[0]);
                }

                if (method == ApiMethods.wallet_switchEthereumChain.ToString())
                {
                    try
                    {
                        return await _crossSignService.WalletSwitchEthereumChainAsync((SwitchEthereumChain)paramList[0]);
                    }
                    catch (InvalidCastException)
                    {
                        return await _crossSignService.WalletSwitchEthereumChainAsync((SwitchEthereumChainParameter)paramList[0]);
                    }
                }

                if (method == ApiMethods.wallet_addEthereumChain.ToString())
                {
                    try
                    {
                        return await _crossSignService.WalletAddEthereumChainAsync((EthereumChain)paramList[0]);
                    }
                    catch (InvalidCastException)
                    {
                        return await _crossSignService.WalletAddEthereumChainAsync((AddEthereumChainParameter)paramList[0]);
                    }
                }


                throw new NotImplementedException();
            }

            return await base
                .InterceptSendRequestAsync(interceptedSendRequestAsync, method, route, paramList)
                .ConfigureAwait(false);
        }
    }
}