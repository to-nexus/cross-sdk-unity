using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Cross.Sdk.Unity.Http
{
    public class PulseApiHeaderDecorator : CrossSdkApiHeaderDecorator
    {
        protected override Task<HttpResponseContext> SendAsyncCore(HttpRequestContext requestContext, CancellationToken cancellationToken, Func<HttpRequestContext, CancellationToken, Task<HttpResponseContext>> next)
        {
            requestContext.RequestHeaders["x-sdk-platform"] = Application.isMobilePlatform ? "mobile" : "desktop";

            return base.SendAsyncCore(requestContext, cancellationToken, next);
        }
    }
}