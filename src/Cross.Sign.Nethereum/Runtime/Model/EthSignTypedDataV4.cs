using System.Collections.Generic;
using Cross.Core.Common.Utils;
using Cross.Core.Network.Models;
using Cross.Core.Models;
using Nethereum.ABI.EIP712;
using Newtonsoft.Json;

namespace Cross.Sign.Nethereum.Model
{
    [RpcMethod("eth_signTypedData_v4")]
    [RpcRequestOptions(Clock.ONE_MINUTE, 99999)]
    public class EthSignTypedDataV4
    {
        [JsonProperty("domain")]
        public object Domain { get; set; }

        [JsonProperty("types")]
        public object Types { get; set; }

        [JsonProperty("message")]
        public object Message { get; set; }

        public EthSignTypedDataV4(string data)
        {
            var typedDataRaw = TypedDataRawJsonConversion.DeserialiseJsonToRawTypedData(data);
            Domain = typedDataRaw.DomainRawValues;
            Types = typedDataRaw.Types;
            Message = typedDataRaw.Message;
        }

        [Preserve]
        public EthSignTypedDataV4()
        {
        }
    }
}