using System.Threading.Tasks;
using Cross.Core.Network.Interfaces;

namespace Cross.Core.Network.Websocket
{
    public class WebsocketConnectionBuilder : IConnectionBuilder
    {
        public Task<IJsonRpcConnection> CreateConnection(string url, string context = null)
        {
            return Task.FromResult<IJsonRpcConnection>(new WebsocketConnection(url, context));
        }
    }
}