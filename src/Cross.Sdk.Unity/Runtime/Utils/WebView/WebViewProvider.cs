using UnityEngine;

namespace Cross.Sdk.Unity.WebView
{
    public static class WebViewProvider
    {
        public enum WebViewType
        {
            Mock,
            Gpm,
            Vuplex
        }

        public static WebViewType CurrentType = WebViewType.Gpm;

        public static IWebViewHandler GetHandler(MonoBehaviour owner)
        {
            switch (CurrentType)
            {
                case WebViewType.Gpm:
                    // GpmWebViewHandler 클래스가 존재하므로 이제 에러가 나지 않습니다.
                    var gpm = owner.gameObject.GetComponent<GpmWebViewHandler>();
                    if (gpm == null) gpm = owner.gameObject.AddComponent<GpmWebViewHandler>();
                    return gpm;
                case WebViewType.Vuplex:
                    // Vuplex는 별도 구현체 리턴
                    // return new VuplexWebViewHandler(); 
                    return new MockWebViewHandler();
                default:
                    return new MockWebViewHandler();
            }
        }
    }
}
