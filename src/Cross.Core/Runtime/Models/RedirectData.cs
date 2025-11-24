using System;
using Newtonsoft.Json;

namespace Cross.Core.Models
{
    [Serializable]
    public class RedirectData
    {
        [JsonProperty("native")] public string Native;

        [JsonProperty("universal")] public string Universal;
    }
}