using Cross.Core.Controllers;
using Cross.Core.Interfaces;
using Cross.Sign.Constants;
using Cross.Sign.Models;

namespace Cross.Sign.Controllers
{
    public class AuthPairingTopics : Store<string, AuthPairing>
    {
        public AuthPairingTopics(ICoreClient coreClient) : base(coreClient, AuthConstants.AuthPairingTopicContext, AuthConstants.AuthStoragePrefix)
        {
        }
    }
}