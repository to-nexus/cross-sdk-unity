using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Cross.Sdk.Unity.WebView
{
    public class MockWebViewHandler : IWebViewHandler
    {
        public event Action<string> OnMessageReceived;
        public event Action OnLoaded;
        public event Action<string> OnError;

        public void Open(string url, string initialData = null)
        {
            Debug.Log($"<color=orange>[SDK-WebView-Mock]</color> Opening: {url}");
            SimulateProcess(initialData);
        }

        public void SendToWeb(string data)
        {
            Debug.Log($"<color=orange>[SDK-WebView-Mock]</color> Data Sent: {data}");
        }

        public void Close() => Debug.Log("<color=orange>[SDK-WebView-Mock]</color> Closed");

        private async void SimulateProcess(string initialData)
        {
            await Task.Delay(1000);
            OnLoaded?.Invoke();
            
            if (!string.IsNullOrEmpty(initialData))
                Debug.Log($"<color=orange>[SDK-WebView-Mock]</color> Processing Initial Data...");

            await Task.Delay(2000);
            string mockResponse = "{\"status\": \"success\", \"address\": \"0x3A0b00acB94f25ee85f33C4FA190804aaB08e2ba\"}";
            OnMessageReceived?.Invoke(mockResponse);
        }
    }
}
