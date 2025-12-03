using System;
using Cross.Sdk.Unity.Model;

namespace Cross.Sdk.Unity
{
    [Serializable]
    public class CrossSdkConfig
    {
        public string[] includedWalletIds;
        public string[] excludedWalletIds;

        public ushort connectViewWalletsCountMobile = 3;
        public ushort connectViewWalletsCountDesktop = 2;

        public bool enableAnalytics = false;    // do not log analytics data as we don't have it on backend

        public bool enableEmail = true; // Currently supported only in WebGL
        public bool enableOnramp = true; // Currently supported only in WebGL
        public bool enableCoinbaseWallet = true; // Currently supported only in WebGL

        public SiweConfig siweConfig = null;    // no use of siwe

        public Chain[] supportedChains =
        {
            ChainConstants.Chains.CrossMainnet,
            ChainConstants.Chains.CrossTestnet,
            // ChainConstants.Chains.Ethereum,
            // ChainConstants.Chains.EthereumSepolia,
            ChainConstants.Chains.BscMainnet,
            ChainConstants.Chains.BscTestnet,
            // ChainConstants.Chains.KaiaMainnet,
            // ChainConstants.Chains.KaiaTestnet,
            ChainConstants.Chains.Ronin,
            ChainConstants.Chains.RoninSaigon
        };

        public Wallet[] customWallets = GetCustomWallets();

        public Metadata metadata;
        public string projectId;

        public CrossSdkConfig()
        {
            // Exclude Coinbase Wallet on native platforms because it doesn't use WalletConnect protocol
            // On WebGL the official Coinbase Wallet library is used
#if !UNITY_WEBGL
            ExcludeCoinbaseWallet();
#endif
        }
        
        public CrossSdkConfig(string projectId, Metadata metadata) : this()
        {
            this.projectId = projectId;
            this.metadata = metadata;
        }

#if !UNITY_WEBGL
        private void ExcludeCoinbaseWallet()
        {
            const string cbWalletId = "fd20dc426fb37566d803205b19bbc1d4096b248ac04548e3cfb6b3a38bd033aa";

            if (excludedWalletIds == null || excludedWalletIds.Length == 0)
            {
                excludedWalletIds = new[] { cbWalletId };
            }
            else if (Array.IndexOf(excludedWalletIds, cbWalletId) == -1)
            {
                var newWalletIds = new string[excludedWalletIds.Length + 1];
                excludedWalletIds.CopyTo(newWalletIds, 0);
                newWalletIds[^1] = cbWalletId;
                excludedWalletIds = newWalletIds;
            }
        }
#endif
        private static Wallet[] GetCustomWallets()
        {
            return new[]
            {
                new Wallet
                {
                    Id = "cross_wallet",
                    Name = "Cross Wallet",
                    ImageUrl = "https://contents.crosstoken.io/img/CROSSx_AppIcon.png",
                    MobileLink = "crossx://",
                    DesktopLink = "crossx://",
                    AppStore = "https://apps.apple.com/us/app/crossx-games/id6741250674",
                    PlayStore = "https://play.google.com/store/apps/details?id=com.nexus.crosswallet"
                }
            };
        }
    }

    public class Metadata
    {
        public readonly string Name;
        public readonly string Description;
        public readonly string Url;
        public readonly string IconUrl;
        public readonly RedirectData Redirect;

        public Metadata(string name, string description, string url, string iconUrl, RedirectData redirect = null)
        {
            Name = name;
            Description = description;
            Url = url;
            IconUrl = iconUrl;
            Redirect = redirect ?? new RedirectData();
        }

        public static implicit operator Core.Metadata(Metadata metadata)
        {
            return new Core.Metadata
            {
                Name = metadata.Name,
                Description = metadata.Description,
                Url = metadata.Url,
                Icons = new[] { metadata.IconUrl },
                Redirect = new Core.Models.RedirectData
                {
                    Native = metadata.Redirect.Native,
                    Universal = metadata.Redirect.Universal
                }
            };
        }
    }
    
    public class RedirectData
    {
        public string Native = string.Empty;
        public string Universal = string.Empty;
    }
}