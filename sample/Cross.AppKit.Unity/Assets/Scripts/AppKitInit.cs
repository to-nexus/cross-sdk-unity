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

            // The very basic configuration of SIWE
            var siweConfig = new SiweConfig
            {
                GetMessageParams = () => new SiweMessageParams
                {
                    Domain = "example.com",
                    Uri = "https://example.com/login"
                },
                SignOutOnChainChange = false
            };

            // Subscribe to SIWE events
            siweConfig.SignInSuccess += _ => Debug.Log("[Dapp] SIWE Sign In Success!");
            siweConfig.SignOutSuccess += () => Debug.Log("[Dapp] SIWE Sign Out Success!");

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
                ),
                customWallets = GetCustomWallets(),
                // On mobile show 5 wallets on the Connect view (the first AppKit modal screen)
                connectViewWalletsCountMobile = 5,
                // Assign the SIWE configuration created above. Can be null if SIWE is not used.
                siweConfig = siweConfig
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

        /// <summary>
        ///     This method returns a list of Cross sample wallets on iOS and Android.
        ///     These wallets are used for testing and are not included in the default list of wallets returned by AppKit's REST API.
        ///     On other platforms, this method returns null, so only the default list of wallets is used.
        /// </summary>
        private Wallet[] GetCustomWallets()
        {
            return new[]
            {
                new Wallet
                {
                    Name = "Cross Wallet",
                    ImageUrl = "https://raw.githubusercontent.com/reown-com/reown-dotnet/refs/heads/main/media/walletkit-icon.png",
                    MobileLink = "cross-wallet://"
                }
            };

// #if UNITY_IOS && !UNITY_EDITOR
//             return new[]
//             {
//                 new Wallet
//                 {
//                     Name = "Swift Wallet",
//                     ImageUrl = "https://raw.githubusercontent.com/reown-com/reown-dotnet/refs/heads/main/media/walletkit-icon.png",
//                     MobileLink = "walletapp://"
//                 },
//                 new Wallet
//                 {
//                     Name = "React Native Wallet",
//                     ImageUrl = "https://raw.githubusercontent.com/reown-com/reown-dotnet/refs/heads/main/media/walletkit-icon.png",
//                     MobileLink = "rn-web3wallet://"
//                 },
//                 new Wallet
//                 {
//                     Name = "Flutter Wallet Prod",
//                     ImageUrl = "https://raw.githubusercontent.com/reown-com/reown-dotnet/refs/heads/main/media/walletkit-icon.png",
//                     MobileLink = "wcflutterwallet://"
//                 }
//             };
// #endif

// #if UNITY_ANDROID && !UNITY_EDITOR
//             return new[]
//             {
//                 new Wallet
//                 {
//                     Name = "Kotlin Wallet",
//                     ImageUrl = "https://raw.githubusercontent.com/reown-com/reown-dotnet/refs/heads/main/media/walletkit-icon.png",
//                     MobileLink = "kotlin-web3wallet://"
//                 },
//                 new Wallet
//                 {
//                     Name = "React Native Wallet",
//                     ImageUrl = "https://raw.githubusercontent.com/reown-com/reown-dotnet/refs/heads/main/media/walletkit-icon.png",
//                     MobileLink = "rn-web3wallet://"
//                 },
//                 new Wallet
//                 {
//                     Name = "Flutter Wallet Prod",
//                     ImageUrl = "https://raw.githubusercontent.com/reown-com/reown-dotnet/refs/heads/main/media/walletkit-icon.png",
//                     MobileLink = "wcflutterwallet://"
//                 }
//             };
// #endif
            return null;
        }
    }
}