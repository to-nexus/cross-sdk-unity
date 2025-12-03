using System;
using System.Collections.Generic;
using UnityEngine;

namespace Cross.Sdk.Unity
{
    [Serializable]
    public class Chain
    {
        public virtual string Name { get; }

        // https://github.com/wevm/viem/blob/main/src/chains/index.ts
        [Obsolete("The ViemName property will be removed")]
        public virtual string ViemName { get; }
        public virtual Currency NativeCurrency { get; }
        public virtual BlockExplorer BlockExplorer { get; }
        public virtual string RpcUrl { get; }
        public virtual bool IsTestnet { get; }
        public virtual string ImageUrl { get; }

        // --- CAIP-2
        public virtual string ChainNamespace { get; }
        public virtual string ChainReference { get; }

        public virtual string ChainId
        {
            get => $"{ChainNamespace}:{ChainReference}";
        }
        // ---

        public Chain(
            string chainNamespace,
            string chainReference,
            string name,
            Currency nativeCurrency,
            BlockExplorer blockExplorer,
            string rpcUrl,
            bool isTestnet,
            string imageUrl,
            string viemName = null)
        {
            ChainNamespace = chainNamespace;
            ChainReference = chainReference;
            Name = name;
            NativeCurrency = nativeCurrency;
            BlockExplorer = blockExplorer;
            RpcUrl = rpcUrl;
            IsTestnet = isTestnet;
            ImageUrl = imageUrl;
            ViemName = viemName;

            if (!string.IsNullOrWhiteSpace(viemName))
            {
                Debug.LogWarning($"The ViemName property is deprecated and will be removed in the future. You don't need to set <i>{viemName}</i> for the chain <b>{name}</b> in the `Chain` constructor.");
            }
        }
    }

    [Serializable]
    public readonly struct Currency
    {
        public readonly string name;
        public readonly string symbol;
        public readonly int decimals;

        public Currency(string name, string symbol, int decimals)
        {
            this.name = name;
            this.symbol = symbol;
            this.decimals = decimals;
        }

        public static implicit operator Cross.Sign.Nethereum.Model.Currency(Currency currency)
        {
            return new Cross.Sign.Nethereum.Model.Currency(currency.name, currency.symbol, currency.decimals);
        }
    }

    [Serializable]
    public readonly struct BlockExplorer
    {
        public readonly string name;
        public readonly string url;

        public BlockExplorer(string name, string url)
        {
            this.name = name;
            this.url = url;
        }
    }

    public static class ChainConstants
    {
        internal const string ChainImageUrl = "https://api.web3modal.com/public/getAssetImage";

        public static class Namespaces
        {
            public const string Evm = "eip155";
            public const string Algorand = "algorand";
            public const string Solana = "sol";
        }

        public static class References
        {
            public const string Ethereum = "1";
            public const string EthereumSepolia = "11155111";
            public const string BscMainnet = "56";
            public const string BscTestnet = "97";
            public const string KaiaMainnet = "8217";
            public const string KaiaTestnet = "1001";
            public const string Ronin = "2020";
            public const string RoninSaigon = "2021";
            public const string CrossTestnet = "612044";
            public const string CrossMainnet = "612055";
        }

        public static class Chains
        {
            public static readonly Chain CrossTestnet = new(
                Namespaces.Evm,
                References.CrossTestnet,
                "CROSS Testnet",
                new Currency("CROSS", "CROSS", 18),
                new BlockExplorer("Blockscout", "https://testnet.crossscan.io/"),
                "https://testnet.crosstoken.io:22001",
                true,
                "https://dev-imgproxy-api.crosstoken.io/rs:fit:512:512:1/plain/https://contents.crosstoken.io/wallet/token/images/CROSSx.svg@png"
            );

            public static readonly Chain CrossMainnet = new(
                Namespaces.Evm,
                References.CrossMainnet,
                "CROSS Mainnet",
                new Currency("CROSS", "CROSS", 18),
                new BlockExplorer("Blockscout", "https://www.crossscan.io"),
                "https://mainnet.crosstoken.io:22001",
                false,
                "https://dev-imgproxy-api.crosstoken.io/rs:fit:512:512:1/plain/https://contents.crosstoken.io/wallet/token/images/CROSSx.svg@png"
            );

            public static readonly Chain Ethereum = new(
                Namespaces.Evm,
                References.Ethereum,
                "Ether Mainnet",
                new Currency("Ether", "ETH", 18),
                new BlockExplorer("Ether scan", "https://etherscan.io/"),
                "https://eth-mainnet.crosstoken.io/fad29a23391f6d6e8fb41fb8eecbcca82343b378",
                false,
                "https://dev-imgproxy-api.crosstoken.io/rs:fit:512:512:1/plain/https://contents.crosstoken.io/wallet/token/images/ETH.svg@png"
            );

            public static readonly Chain EthereumSepolia = new(
                Namespaces.Evm,
                References.EthereumSepolia,
                "Ether Testnet (Sepolia)",
                new Currency("Sepolia", "ETH", 18),
                new BlockExplorer("Ether Sepolia scan", "https://sepolia.etherscan.io/"),
                "https://sepolia.crosstoken.io/8de52516c154dce8cc2ceaae39d657a1e1e74d2f",
                true,
                "https://dev-imgproxy-api.crosstoken.io/rs:fit:512:512:1/plain/https://contents.crosstoken.io/wallet/token/images/ETH.svg@png"
            );

            public static readonly Chain BscMainnet = new(
                Namespaces.Evm,
                References.BscMainnet,
                "BSC Mainnet",
                new Currency("BNB", "BNB", 18),
                new BlockExplorer("BscScan", "https://bscscan.com"),
                "https://bsc-mainnet.crosstoken.io/2272489872e4f1475ff25d57ce93b51989f933c7",
                false,
                "https://dev-imgproxy-api.crosstoken.io/rs:fit:512:512:1/plain/https://contents.crosstoken.io/wallet/token/images/BNB.svg@png"
            );

            public static readonly Chain BscTestnet = new(
                Namespaces.Evm,
                References.BscTestnet,
                "BSC Testnet",
                new Currency("BNB", "tBNB", 18),
                new BlockExplorer("BscScan", "https://testnet.bscscan.com"),
                "https://bsc-testnet.crosstoken.io/110ea3628b77f244e5dbab16790d81bba874b962",
                true,
                "https://dev-imgproxy-api.crosstoken.io/rs:fit:512:512:1/plain/https://contents.crosstoken.io/wallet/token/images/BNB.svg@png"
            );

            public static readonly Chain KaiaMainnet = new(
                Namespaces.Evm,
                References.KaiaMainnet,
                "Kaia Mainnet",
                new Currency("KAIA", "KAIA", 18),
                new BlockExplorer("Kaia Scan", "https://kaiascan.io/"),
                "https://kaia-mainnet-ext.crosstoken.io/815b8a6e389b34a4f82cfd1e501692dee2f4e8f5",
                false,
                "https://dev-imgproxy-api.crosstoken.io/rs:fit:512:512:1/plain/https://contents.crosstoken.io/wallet/token/images/KAIA.svg@png"
            );

            public static readonly Chain KaiaTestnet = new(
                Namespaces.Evm,
                References.KaiaTestnet,
                "Kaia Testnet (Kairos)",
                new Currency("KAIA", "tKAIA", 18),
                new BlockExplorer("Kairos Scan", "https://kairos.kaiascan.io/"),
                "https://kaia-testnet.crosstoken.io/fda0d5a47e2d0768e9329444295a3f0681fff365",
                true,
                "https://dev-imgproxy-api.crosstoken.io/rs:fit:512:512:1/plain/https://contents.crosstoken.io/wallet/token/images/KAIA.svg@png"
            );

            public static readonly Chain Ronin = new(
                Namespaces.Evm,
                References.Ronin,
                "Ronin Mainnet",
                new Currency("RON", "RON", 18),
                new BlockExplorer("RoninScan", "https://app.roninchain.com/"),
                "https://ronin-mainnet.cross-api.in:8545",
                false,
                "https://dev-imgproxy-api.crosstoken.io/rs:fit:512:512:1/plain/https://contents.crosstoken.io/wallet/token/images/RON.svg@png"
            );

            public static readonly Chain RoninSaigon = new(
                Namespaces.Evm,
                References.RoninSaigon,
                "Ronin Testnet",
                new Currency("tRON", "tRON", 18),
                new BlockExplorer("SaigonScan", "https://saigon-app.roninchain.com"),
                "https://ronin-testnet.cross-api.in:8545",
                true,
                "https://dev-imgproxy-api.crosstoken.io/rs:fit:512:512:1/plain/https://contents.crosstoken.io/wallet/token/images/RON.svg@png"
            );

            public static readonly IReadOnlyCollection<Chain> All = new HashSet<Chain>
            {
                CrossMainnet,
                CrossTestnet,
                Ethereum,
                EthereumSepolia,
                BscMainnet,
                BscTestnet,
                KaiaMainnet,
                KaiaTestnet,
                Ronin,
                RoninSaigon
            };
        }
    }
}