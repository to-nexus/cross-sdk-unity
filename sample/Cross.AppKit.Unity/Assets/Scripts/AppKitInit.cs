using Cross.AppKit.Unity;
using Cross.AppKit.Unity.Model;
using Cross.Core.Common.Logging;
using Skibitsky.Unity;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityLogger = Cross.Sign.Unity.UnityLogger;

#if !UNITY_WEBGL
using mixpanel;
#endif

namespace Sample
{
    public class AppKitInit : MonoBehaviour
    {
        [SerializeField] private SceneReference _menuScene;

        private async void Start()
        {
            // Set up Cross logger to collect logs from AppKit
            CrossLogger.Instance = new UnityLogger();

            // AppKit configuration
            var appKitConfig = new AppKitConfig
            {
                // Project ID from cross test
                projectId = "ef21cf313a63dbf63f2e9e04f3614029",
                metadata = new Metadata(
                    "AppKit Unity",
                    "AppKit Unity Sample",
                    "https://to.nexus",
                    "https://raw.githubusercontent.com/reown-com/reown-dotnet/main/media/appkit-icon.png",
                    new RedirectData
                    {
                        // Used by native wallets to redirect back to the app after approving requests
                        Native = "appkit-sample-unity://"
                    }
                )
            };

            Debug.Log("[AppKit Init] Initializing AppKit...");

            await AppKit.InitializeAsync(
                appKitConfig
            );
            
#if !UNITY_WEBGL
            // The Mixpanel is used by the sample project to collect telemetry
            var clientId = await AppKit.Instance.SignClient.CoreClient.Crypto.GetClientId();
            Mixpanel.Identify(clientId);
#endif

            Debug.Log($"[AppKit Init] AppKit initialized. Loading menu scene...");
            SceneManager.LoadScene(_menuScene);
        }
    }
}