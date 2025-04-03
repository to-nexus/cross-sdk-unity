using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Cross.Sdk.Unity.Http
{
    public class CrossSdkApiHeaderDecorator : HttpClientDecorator
    {
        protected override Task<HttpResponseContext> SendAsyncCore(HttpRequestContext requestContext, CancellationToken cancellationToken, Func<HttpRequestContext, CancellationToken, Task<HttpResponseContext>> next)
        {
            requestContext.RequestHeaders["x-project-id"] = CrossSdk.Config.projectId;
            requestContext.RequestHeaders["x-sdk-type"] = "cross-sdk";
            requestContext.RequestHeaders["x-sdk-version"] = CrossSdk.Version;

            var origin = Application.identifier;
            if (!string.IsNullOrWhiteSpace(origin))
            {
                requestContext.RequestHeaders["origin"] = origin;
            }            

            return next(requestContext, cancellationToken);
        }
    }
}