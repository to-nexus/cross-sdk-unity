using Cross.Core.Controllers;
using Cross.Core.Interfaces;
using Cross.Sign.Constants;
using Cross.Sign.Models;

namespace Cross.Sign.Controllers
{
    public class AuthPendingRequests : Store<long, AuthPendingRequest>
    {
        public AuthPendingRequests(ICoreClient coreClient) : base(coreClient, AuthConstants.AuthPendingRequestContext, AuthConstants.AuthStoragePrefix)
        {
        }
    }
}