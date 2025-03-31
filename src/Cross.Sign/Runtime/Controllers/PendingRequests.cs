using Cross.Core.Controllers;
using Cross.Core.Interfaces;
using Cross.Sign.Interfaces;
using Cross.Sign.Models;

namespace Cross.Sign.Controllers
{
    public class PendingRequests : Store<long, PendingRequestStruct>, IPendingRequests
    {
        public PendingRequests(ICoreClient coreClient) : base(coreClient, "request", SignClient.StoragePrefix)
        {
        }
    }
}