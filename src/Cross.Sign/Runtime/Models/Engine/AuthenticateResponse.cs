#nullable enable

using Cross.Core.Common.Utils;
using Cross.Core.Network.Models;
using Cross.Sign.Models.Cacao;

namespace Cross.Sign.Models.Engine
{
    [RpcResponseOptions(Clock.ONE_MINUTE, 1117)]
    public class AuthenticateResponse
    {
        public CacaoObject[]? Cacaos;

        public Participant Responder;
    }
}