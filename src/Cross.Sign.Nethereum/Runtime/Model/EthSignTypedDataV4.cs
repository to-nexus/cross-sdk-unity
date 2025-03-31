using System.Collections.Generic;
using Cross.Core.Common.Utils;
using Cross.Core.Network.Models;

namespace Cross.Sign.Nethereum.Model
{
    [RpcMethod("eth_signTypedData_v4")]
    [RpcRequestOptions(Clock.ONE_MINUTE, 99999)]
    public class EthSignTypedDataV4 : List<string>
    {
        public EthSignTypedDataV4(string account, string data) : base(new[] { account, data })
        {
        }

        [Preserve]
        public EthSignTypedDataV4()
        {
        }
    }
}