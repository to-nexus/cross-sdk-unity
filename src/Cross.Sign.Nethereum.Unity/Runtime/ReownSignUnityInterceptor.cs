using System;
using System.Threading.Tasks;
using Nethereum.JsonRpc.Client;
using Cross.Core.Common.Logging;
using Cross.Sign.Unity;

namespace Cross.Sign.Nethereum.Unity
{
    public class CrossSignUnityInterceptor : RequestInterceptor
    {
        public readonly CrossInterceptor CrossInterceptor;
        public readonly SignClientUnity SignClient;

        public CrossSignUnityInterceptor(SignClientUnity signClient, CrossInterceptor crossInterceptor)
        {
            SignClient = signClient;
            CrossInterceptor = crossInterceptor;
        }

        public CrossSignUnityInterceptor(SignClientUnity signClient)
        {
            SignClient = signClient;
            CrossInterceptor = new CrossInterceptor(new CrossSignServiceCore(SignClient));
        }

        public override Task<object> InterceptSendRequestAsync<T>(
            Func<RpcRequest, string, Task<T>> interceptedSendRequestAsync,
            RpcRequest request,
            string route = null)
        {
            return CrossInterceptor.InterceptSendRequestAsync(interceptedSendRequestAsync, request, route);
        }

        public override Task<object> InterceptSendRequestAsync<T>(
            Func<string, string, object[], Task<T>> interceptedSendRequestAsync,
            string method,
            string route = null,
            params object[] paramList)
        {
            return CrossInterceptor.InterceptSendRequestAsync(interceptedSendRequestAsync, method, route, paramList);
        }
    }
}