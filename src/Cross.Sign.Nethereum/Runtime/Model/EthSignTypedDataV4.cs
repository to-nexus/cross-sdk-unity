using System.Collections.Generic;
using Cross.Core.Common.Utils;
using Cross.Core.Network.Models;
using Cross.Core.Models;
using Nethereum.ABI.EIP712;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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

        [JsonProperty("primaryType")]
        public string PrimaryType { get; set; }

        [JsonProperty("message")]
        public object Message { get; set; }

        public EthSignTypedDataV4(string data)
        {
            // Parse JSON string directly to preserve object structure
            // instead of using TypedDataRawJsonConversion which converts domain to array format
            var jsonObject = JObject.Parse(data);
            
            Domain = jsonObject["domain"]?.ToObject<object>();
            PrimaryType = jsonObject["primaryType"]?.ToString();
            Message = jsonObject["message"]?.ToObject<object>();
            
            // Filter out EIP712Domain from types (wallets don't expect it)
            var typesObject = jsonObject["types"] as JObject;
            if (typesObject != null)
            {
                // Remove EIP712Domain if present
                typesObject.Remove("EIP712Domain");
                Types = typesObject.ToObject<object>();
            }
            else
            {
                Types = jsonObject["types"]?.ToObject<object>();
            }
        }

        [Preserve]
        public EthSignTypedDataV4()
        {
        }
    }
}