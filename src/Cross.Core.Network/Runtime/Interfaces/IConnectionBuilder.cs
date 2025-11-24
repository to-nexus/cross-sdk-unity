using System.Threading.Tasks;

namespace Cross.Core.Network.Interfaces
{
    public interface IConnectionBuilder
    {
        Task<IJsonRpcConnection> CreateConnection(string url, string context = null);
    }
}