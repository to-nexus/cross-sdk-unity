using System;
using System.Collections.Generic;
using System.IO;
using Cross.Core.Common.Logging;
using Cross.Core.Models.Publisher;
using Cross.Sign.Interfaces;
using Cross.Sign.Models;
using Cross.Sign.Models.Engine.Events;
using Cross.Sign.Unity.Utils;
using UnityEngine;

namespace Cross.Sign.Unity
{
    public class Linker : IDisposable
    {
        private readonly SignClientUnity _signClient;

        protected bool disposed;

        public Linker(SignClientUnity signClient)
        {
            _signClient = signClient;

            RegisterEventListeners();
        }

        private void RegisterEventListeners()
        {
            _signClient.SessionRequestSentUnity += SessionRequestSentHandler;
        }

        public static void OpenSessionProposalDeepLink(string uri, string nativeRedirect)
        {
            if (string.IsNullOrWhiteSpace(uri))
                throw new ArgumentException("[Linker] Uri cannot be empty.");

#if UNITY_EDITOR && (UNITY_IOS || UNITY_ANDROID)
            // In editor we cannot open _mobile_ deep links, so we just log the uri
            Debug.Log($"[Linker] Requested to open mobile deep link. The uri: {uri}");
#else

            if (string.IsNullOrWhiteSpace(nativeRedirect))
                throw new Exception(
                    $"[Linker] No link found for {Application.platform} platform.");

            var url = BuildConnectionDeepLink(nativeRedirect, uri);

            CrossLogger.Log($"[Linker] Opening URL {url}");

            Application.OpenURL(url);
#endif
        }

        private void SessionRequestSentHandler(object _, SessionRequestEvent e)
        {
            var session = _signClient.Session.Get(e.Topic);
            OpenSessionRequestDeepLink(session, e.Id);
        }

        public static void OpenSessionRequestDeepLink(Session session, long requestId)
        {
            if (session == null)
                throw new ArgumentNullException(nameof(session));

            if (session.Peer.Metadata == null)
                return;

#if UNITY_STANDALONE
            // Skip desktop deep link if connected via QR Code or WebApp
            // These connections use WalletConnect protocol to push notifications to mobile/web wallets
            // Opening desktop deep link would show an alert when no desktop wallet app is installed
            var connectionMethod = PlayerPrefs.GetString("RE_CONNECTION_METHOD", "");
            if (connectionMethod == "qrcode" || connectionMethod == "webapp")
            {
                CrossLogger.Log($"[Linker] Connection method is '{connectionMethod}'. Skipping desktop deep link to avoid alert.");
                return;
            }
#endif

            var redirectNative = session.Peer.Metadata.Redirect?.Native;
            string deeplink;

            if (string.IsNullOrWhiteSpace(redirectNative))
            {
                if (!TryGetRecentWalletDeepLink(out deeplink))
                    return;

                Debug.LogWarning(
                    $"[Linker] No redirect found for {session.Peer.Metadata.Name}. Using deep link from the Recent Wallet."
                );
            }
            else
            {
                deeplink = redirectNative;
                CrossLogger.Log($"[Linker] Open native deep link: {deeplink}");
            }

            if (!deeplink.Contains("://"))
            {
                deeplink = deeplink.Replace("/", "").Replace(":", "");
                deeplink = $"{deeplink}://";
            }

            if (!deeplink.EndsWith("wc"))
                deeplink = Path.Combine(deeplink, "wc");

            deeplink = $"{deeplink}?requestId={requestId}&sessionTopic={session.Topic}";

            Debug.Log($"[Linker] Opening URL {deeplink}");
            Application.OpenURL(deeplink);
        }

        public static string BuildConnectionDeepLink(string appLink, string wcUri)
        {
            if (string.IsNullOrWhiteSpace(wcUri))
                throw new ArgumentException("[Linker] Uri cannot be empty.");

            if (string.IsNullOrWhiteSpace(appLink))
                throw new ArgumentException("[Linker] Native link cannot be empty.");

            var safeAppUrl = appLink;
            if (!safeAppUrl.Contains("://"))
            {
                safeAppUrl = safeAppUrl.Replace("/", "").Replace(":", "");
                safeAppUrl = $"{safeAppUrl}://";
            }

            if (!safeAppUrl.EndsWith('/'))
                safeAppUrl = $"{safeAppUrl}/";

            var encodedWcUrl = Uri.EscapeDataString(wcUri);

            return $"{safeAppUrl}wc?uri={encodedWcUrl}";
        }

        public static bool CanOpenURL(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return false;

            try
            {
#if !UNITY_EDITOR && UNITY_IOS
                return _CanOpenURL(url);
#elif !UNITY_EDITOR && UNITY_ANDROID 
                using var urlCheckerClass = new AndroidJavaClass("com.nexus.cross.sign.unity.Linker");
                using var unityPlayerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                using var currentContext = unityPlayerClass.GetStatic<AndroidJavaObject>("currentActivity");
                var result = urlCheckerClass.CallStatic<bool>("canOpenURL", currentContext, url);
                return result;
#endif
            }
            catch (Exception e)
            {
                CrossLogger.LogError($"[Linker] Exception for url {url}: {e.Message}");
            }

            return false;
        }

#if !UNITY_EDITOR && UNITY_IOS
        [System.Runtime.InteropServices.DllImport("__Internal")]
        public static extern bool _CanOpenURL(string url);
#endif

        private static bool TryGetRecentWalletDeepLink(out string deeplink)
        {
            deeplink = null;

            deeplink = PlayerPrefs.GetString("RE_RECENT_WALLET_DEEPLINK");

            if (string.IsNullOrWhiteSpace(deeplink))
                return false;

            return deeplink != null;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed) return;

            if (disposing)
                _signClient.SessionRequestSent -= SessionRequestSentHandler;

            disposed = true;
        }
    }
}