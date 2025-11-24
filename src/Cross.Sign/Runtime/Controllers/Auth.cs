using System.Threading.Tasks;
using Cross.Core.Interfaces;
using Cross.Sign.Interfaces;
using Cross.Sign.Models;

namespace Cross.Sign.Controllers
{
    public class Auth : IAuth
    {
        public Auth(ICoreClient coreClient)
        {
            Keys = new AuthKeyStore(coreClient);
            Pairings = new AuthPairingTopics(coreClient);
            PendingRequests = new AuthPendingRequests(coreClient);
        }

        public Task Init()
        {
            return Task.WhenAll(
                Keys.Init(),
                Pairings.Init(),
                PendingRequests.Init()
            );
        }

        public IStore<string, AuthKey> Keys { get; }
        public IStore<string, AuthPairing> Pairings { get; }
        public IStore<long, AuthPendingRequest> PendingRequests { get; }
    }
}