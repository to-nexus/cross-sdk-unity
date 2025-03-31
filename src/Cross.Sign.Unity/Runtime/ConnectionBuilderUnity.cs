using System.Threading.Tasks;
using Cross.Core.Network;
using Cross.Core.Network.Interfaces;

namespace Cross.Sign.Unity
{
    public class ConnectionBuilderUnity : IConnectionBuilder
    {
        public Task<IJsonRpcConnection> CreateConnection(string url, string context = null)
        {
            return Task.FromResult<IJsonRpcConnection>(new WebSocketConnectionUnity(url));
        }
    }
}