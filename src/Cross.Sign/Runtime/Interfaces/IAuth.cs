using System.Threading.Tasks;
using Cross.Core.Interfaces;
using Cross.Sign.Models;

namespace Cross.Sign.Interfaces
{
    public interface IAuth
    {
        public Task Init();

        public IStore<string, AuthKey> Keys { get; }

        public IStore<string, AuthPairing> Pairings { get; }

        public IStore<long, AuthPendingRequest> PendingRequests { get; }
    }
}