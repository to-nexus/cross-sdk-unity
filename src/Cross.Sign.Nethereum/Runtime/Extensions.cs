using System;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Cross.Core.Common.Logging;
using Cross.Core.Common.Model.Errors;
using Cross.Sign.Interfaces;
using Cross.Sign.Nethereum.Model;

namespace Cross.Sign.Nethereum
{
    public static class Extensions
    {
        /// <summary>
        ///     Switches the Ethereum chain of the wallet to the specified chain. The task will complete after `chainChanged` event is received.
        /// </summary>
        /// <param name="signClient">Cross Sign client</param>
        /// <param name="ethereumChain">Ethereum chain to switch to</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="ethereumChain"/> is null</exception>
        [Obsolete("This extension will be removed in the future versions. Cross CrossSdk will handle chain switching in MetaMask internally.")]
        public static async Task SwitchEthereumChainAsync(this ISignClient signClient, EthereumChain ethereumChain)
        {
            if (ethereumChain == null)
                throw new ArgumentNullException(nameof(ethereumChain));
            
            var caip2ChainId = $"eip155:{ethereumChain.chainIdDecimal}";
            if (!signClient.AddressProvider.DefaultSession.Namespaces.TryGetValue("eip155", out var @namespace)
                || !@namespace.Chains.Contains(caip2ChainId))
            {
                try
                {
                    // Try to switch chain. This will only work if the chain is already added to the MetaMask
                    var data = new WalletSwitchEthereumChain(ethereumChain.chainIdHex);
                    await signClient.Request<WalletSwitchEthereumChain, string>(data);
                }
                catch (CrossNetworkException e)
                {
                    try
                    {
                        var metaMaskError = JsonConvert.DeserializeObject<MetaMaskError>(e.Message);
                        if (metaMaskError is { Code: 4001 }) // If user rejected
                            throw;

                        CrossLogger.LogError($"[MetaMask Error] {metaMaskError.Message}");
                    }
                    catch (Exception)
                    {
                        // If requested chain is not added to the MetaMask, it returns an error that can't be deserialized
                    }

                    // If the chain is not added to the MetaMask, add it
                    // MetaMask will also prompt the user to switch to the new chain
                    var request = new WalletAddEthereumChain(ethereumChain);
                    await signClient.Request<WalletAddEthereumChain, string>(request);
                }
            }
        }
    }
}