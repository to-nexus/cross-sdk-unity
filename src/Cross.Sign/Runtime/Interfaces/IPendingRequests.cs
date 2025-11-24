using Cross.Core.Interfaces;
using Cross.Sign.Models;

namespace Cross.Sign.Interfaces
{
    public interface IPendingRequests : IStore<long, PendingRequestStruct>
    {
    }
}