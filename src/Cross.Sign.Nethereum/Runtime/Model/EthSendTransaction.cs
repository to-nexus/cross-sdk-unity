using Cross.Core.Common.Utils;
using Cross.Core.Network.Models;
using Newtonsoft.Json;

namespace Cross.Sign.Nethereum.Model
{
    [RpcMethod("eth_sendTransaction")]
    [RpcRequestOptions(Clock.ONE_MINUTE, 99997)]
    public class EthSendTransaction : Transaction
    {
        public EthSendTransaction(Transaction transaction)
        {
            From = transaction.From;
            To = transaction.To;
            Gas = transaction.Gas;
            GasPrice = transaction.GasPrice;
            Value = transaction.Value;
            Data = transaction.Data;
            Type = transaction.Type;
        }

        [Preserve]
        public EthSendTransaction()
        {
        }
    }
}
