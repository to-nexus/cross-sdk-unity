using System.Collections.Generic;
using Cross.Core.Common.Utils;
using Cross.Core.Network.Models;

namespace Cross.Sign.Nethereum.Model
{
    [RpcMethod("eth_sendRawTransaction")]
    [RpcRequestOptions(Clock.ONE_MINUTE, 99996)]
    public class EthSendRawTransaction : List<string>
    {
        public EthSendRawTransaction(string transaction) : base(new[] { transaction })
        {
        }

        [Preserve]
        public EthSendRawTransaction()
        {
        }
    }
}