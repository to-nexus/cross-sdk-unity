using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Cross.Core.Common.Utils;
using System.Collections.Generic;
using Cross.Sign.Models;
using Cross.Core.Models;

namespace Cross.Sign.Nethereum.Model
{
    public class Transaction
    {
        [JsonProperty("from")]
        public string From { get; set; }

        [JsonProperty("to")]
        public string To { get; set; }

        [JsonProperty("gas", NullValueHandling = NullValueHandling.Ignore)]
        public string Gas { get; set; }

        [JsonProperty("gasPrice", NullValueHandling = NullValueHandling.Ignore)]
        public string GasPrice { get; set; }

        [JsonProperty("maxFeePerGas", NullValueHandling = NullValueHandling.Ignore)]
        public string MaxFeePerGas { get; set; }

        [JsonProperty("maxPriorityFeePerGas", NullValueHandling = NullValueHandling.Ignore)]
        public string MaxPriorityFeePerGas { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }

        [JsonProperty("data", NullValueHandling = NullValueHandling.Ignore)]
        public string Data { get; set; }

        [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
        public string Type { get; set; }

        [Preserve]
        public Transaction()
        {
        }
    }
}