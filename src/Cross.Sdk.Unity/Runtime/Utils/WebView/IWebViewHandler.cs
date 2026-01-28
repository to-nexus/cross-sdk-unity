using System;

namespace Cross.Sdk.Unity.WebView
{
    public interface IWebViewHandler
    {
        event Action<string> OnMessageReceived;
        event Action OnLoaded;
        event Action<string> OnError;

        void Open(string url, string initialData = null);
        void SendToWeb(string data);
        void Close();
    }
}
