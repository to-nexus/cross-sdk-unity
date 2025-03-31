using System.Collections.Generic;
using Cross.Core.Common.Utils;
using Cross.Core.Network.Models;

namespace Cross.Sign.Nethereum.Model
{
    [RpcMethod("wallet_switchEthereumChain")]
    [RpcRequestOptions(Clock.ONE_MINUTE, 99990)]
    public class WalletSwitchEthereumChain : List<object>
    {
        public WalletSwitchEthereumChain(string chainId) : base(new[] { new { chainId } })
        {
        }

        [Preserve]
        public WalletSwitchEthereumChain()
        {
        }
    }
}