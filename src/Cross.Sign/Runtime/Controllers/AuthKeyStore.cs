using Cross.Core.Controllers;
using Cross.Core.Interfaces;
using Cross.Sign.Constants;
using Cross.Sign.Models;

namespace Cross.Sign.Controllers
{
    public class AuthKeyStore : Store<string, AuthKey>
    {
        public AuthKeyStore(ICoreClient coreClient) : base(coreClient, AuthConstants.AuthKeysContext, AuthConstants.AuthStoragePrefix)
        {
        }
    }
}