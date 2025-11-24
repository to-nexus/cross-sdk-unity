using Cross.Core.Interfaces;

namespace Cross.Sign.Models
{
    public class AuthPairing : IKeyHolder<string>
    {
        public readonly string ResponseTopic;
        public readonly string PairingTopic;

        public string Key
        {
            get => ResponseTopic;
        }

        public AuthPairing(string responseTopic, string pairingTopic)
        {
            ResponseTopic = responseTopic;
            PairingTopic = pairingTopic;
        }
    }
}