using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Cross.Sign.Models
{
    public class CustomData
    {
        [JsonProperty("metadata")]
        public object Metadata { get; set; }

        [JsonExtensionData]
        private IDictionary<string, JToken> _additionalProperties = new Dictionary<string, JToken>();

        public object this[string key]
        {
            get => _additionalProperties.ContainsKey(key) ? _additionalProperties[key] : null;
            set => _additionalProperties[key] = JToken.FromObject(value);
        }

        public IDictionary<string, JToken> AdditionalProperties => _additionalProperties;
    }
} 