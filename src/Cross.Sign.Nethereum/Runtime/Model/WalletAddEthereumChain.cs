using System.Collections.Generic;
using Cross.Core.Common.Utils;
using Cross.Core.Network.Models;

namespace Cross.Sign.Nethereum.Model
{
    [RpcMethod("wallet_addEthereumChain")]
    [RpcRequestOptions(Clock.ONE_MINUTE, 99990)]
    public class WalletAddEthereumChain : List<object>
    {
        public WalletAddEthereumChain(EthereumChain chain) : base(new[] { chain })
        {
        }

        [Preserve]
        public WalletAddEthereumChain()
        {
        }
    }
}