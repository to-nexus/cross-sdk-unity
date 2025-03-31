using Cross.Core.Interfaces;
using Cross.Sign.Constants;

namespace Cross.Sign.Models
{
    public class AuthKey : IKeyHolder<string>
    {
        public string Key
        {
            get => AuthConstants.AuthPublicKeyName;
        }

        public readonly string ResponseTopic;
        public readonly string PublicKey;

        public AuthKey(string responseTopic, string publicKey)
        {
            ResponseTopic = responseTopic;
            PublicKey = publicKey;
        }
    }
}