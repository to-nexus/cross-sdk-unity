using System;
using Newtonsoft.Json;

namespace Cross.Sdk.Unity.Model.BlockchainApi
{
    [Serializable]
    public sealed class ChainApiResponse
    {
        [JsonProperty("code")]
        public int Code { get; }
        
        [JsonProperty("message")]
        public string Message { get; }
        
        [JsonProperty("data")]
        public EthChainInfo[] Data { get; }

        [JsonConstructor]
        public ChainApiResponse(int code, string message, EthChainInfo[] data)
        {
            Code = code;
            Message = message;
            Data = data;
        }
    }

    [Serializable]
    public sealed class EthChainInfo
    {
        [JsonProperty("chain")]
        public string Chain { get; }
        
        [JsonProperty("chain_id")]
        public int ChainId { get; }
        
        [JsonProperty("currency_decimals")]
        public int CurrencyDecimals { get; }
        
        [JsonProperty("currency_name")]
        public string CurrencyName { get; }
        
        [JsonProperty("currency_symbol")]
        public string CurrencySymbol { get; }
        
        [JsonProperty("explorer_url")]
        public string ExplorerUrl { get; }
        
        [JsonProperty("info_url")]
        public string InfoUrl { get; }
        
        [JsonProperty("name")]
        public string Name { get; }
        
        [JsonProperty("network_id")]
        public int NetworkId { get; }
        
        [JsonProperty("rpc")]
        public string Rpc { get; }
        
        [JsonProperty("short_name")]
        public string ShortName { get; }
        
        [JsonProperty("testnet")]
        public bool Testnet { get; }

        [JsonConstructor]
        public EthChainInfo(
            string chain,
            int chainId,
            int currencyDecimals,
            string currencyName,
            string currencySymbol,
            string explorerUrl,
            string infoUrl,
            string name,
            int networkId,
            string rpc,
            string shortName,
            bool testnet)
        {
            Chain = chain;
            ChainId = chainId;
            CurrencyDecimals = currencyDecimals;
            CurrencyName = currencyName;
            CurrencySymbol = currencySymbol;
            ExplorerUrl = explorerUrl;
            InfoUrl = infoUrl;
            Name = name;
            NetworkId = networkId;
            Rpc = rpc;
            ShortName = shortName;
            Testnet = testnet;
        }
    }
}

