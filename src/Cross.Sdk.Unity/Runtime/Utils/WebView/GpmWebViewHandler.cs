using System;
using System.Collections.Generic;
using Gpm.WebView;
using UnityEngine;

namespace Cross.Sdk.Unity.WebView
{
    /// <summary>
    /// GpmWebView 라이브러리(v2.2.0)를 사용하는 핸들러 구현체.
    /// </summary>
    public class GpmWebViewHandler : MonoBehaviour, IWebViewHandler
    {
        public event Action<string> OnMessageReceived;
        public event Action OnLoaded;
        public event Action<string> OnError;

        public void Open(string url, string initialData = null)
        {
            Debug.Log($"<color=green>[SDK-WebView-Gpm]</color> Opening URL: {url}");

            // GPM WebView 2.x 설정 방식
            var configuration = new GpmWebViewRequest.Configuration
            {
                style = GpmWebViewStyle.FULLSCREEN,
                orientation = GpmOrientation.UNSPECIFIED,
                isNavigationBarVisible = true,
                navigationBarColor = "#FFFFFF",
                title = "Wallet Login",
                isClearCache = false,
                isClearCookie = false
            };

            // 커스텀 스킴 리스트 (웹에서 Unity로 메시지를 보낼 때 사용)
            // 예: window.location.href = "gpmwebview://MESSAGE_DATA"
            List<string> schemeList = new List<string> { "gpmwebview" };

            GpmWebView.ShowUrl(url, configuration, (type, data, error) =>
            {
                switch (type)
                {
                    case GpmWebViewCallback.CallbackType.Open:
                        OnLoaded?.Invoke();
                        if (!string.IsNullOrEmpty(initialData))
                        {
                            SendToWeb(initialData);
                        }
                        break;
                    // 2.x 버전에서는 Scheme 호출을 통해 메시지를 수신합니다.
                    case GpmWebViewCallback.CallbackType.Scheme:
                        OnMessageReceived?.Invoke(data);
                        break;
                    case GpmWebViewCallback.CallbackType.Close:
                        Debug.Log("<color=green>[SDK-WebView-Gpm]</color> WebView Closed");
                        break;
                }

                if (error != null)
                {
                    OnError?.Invoke(error.ToString());
                }
            }, schemeList);
        }

        public void SendToWeb(string data)
        {
            // 웹사이트의 window.receiveFromUnity(data) 함수 호출
            GpmWebView.ExecuteJavaScript($"window.receiveFromUnity('{data}')");
        }

        public void Close()
        {
            GpmWebView.Close();
        }
    }
}
