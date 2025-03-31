using Newtonsoft.Json;

namespace Cross.Core.Models.Expirer
{
    /// <summary>
    ///     A class that represents a specific expiration date for a specific target.
    /// </summary>
    public class Expiration
    {
        /// <summary>
        ///     The expiration date, as a unix timestamp (seconds)
        /// </summary>
        [JsonProperty("expiry")] public long Expiry;

        /// <summary>
        ///     The target this expiration is for
        ///     The format for the Target string can be either
        ///     * id:123
        ///     * topic:my_topic_string
        /// </summary>
        [JsonProperty("target")] public string Target;
    }
}