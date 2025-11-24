using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Nethereum.ABI.FunctionEncoding;
using Nethereum.Contracts;
using Nethereum.Contracts.Standards.ERC1271.ContractDefinition;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Signer;
using Nethereum.Signer.EIP712;
using Nethereum.Util;
using Nethereum.Web3;
using Cross.Sign.Nethereum.Unity;
using Cross.Sign.Unity;
using UnityEngine;
using HexBigInteger = Nethereum.Hex.HexTypes.HexBigInteger;
using Cross.Sign.Nethereum.Model;
using Cross.Core.Models;
using Cross.Sign.Models;
using Transaction = Nethereum.RPC.Eth.DTOs.Transaction;
using Newtonsoft.Json;

namespace Cross.Sdk.Unity
{
    public class NethereumEvmService : EvmService
    {
        private readonly Eip712TypedDataSigner _eip712TypedDataSigner = new();

        private readonly EthereumMessageSigner _ethereumMessageSigner = new();
        private CrossSignUnityInterceptor _interceptor;

        public IWeb3 Web3 { get; private set; }

        private readonly HashSet<string> _chainsSupportedByBlockchainApi = new()
        {
            "eip155:1",
            "eip155:10",
            "eip155:56",
            "eip155:97",
            "eip155:100",
            "eip155:137",
            "eip155:300",
            "eip155:324",
            "eip155:1101",
            "eip155:1301",
            "eip155:1329",
            "eip155:2810",
            "eip155:2818",
            "eip155:5000",
            "eip155:5003",
            "eip155:8217",
            "eip155:8453",
            "eip155:17000",
            "eip155:42161",
            "eip155:42220",
            "eip155:43113",
            "eip155:43114",
            "eip155:59144",
            "eip155:80002",
            "eip155:80084",
            "eip155:84532",
            "eip155:421614",
            "eip155:534352",
            "eip155:534351",
            "eip155:7777777",
            "eip155:11155111",
            "eip155:11155420",
            "eip155:999999999",
            "eip155:1313161554",
            "eip155:1313161555"
        };
        
        protected override Task InitializeAsyncCore(SignClientUnity signClient)
        {
            _interceptor = new CrossSignUnityInterceptor(signClient);

            SetInitialWeb3Instance();

            CrossSdk.ChainChanged += ChainChangedHandler;
            return Task.CompletedTask;
        }
        

        // -- Nethereum Web3 Instance ---------------------------------

        private void ChainChangedHandler(object sender, NetworkController.ChainChangedEventArgs e)
        {
            if (e.NewChain != null)
                UpdateWeb3Instance(e.NewChain.ChainId);
        }

        private void SetInitialWeb3Instance()
        {
            if (Web3 != null)
                return;

            var networkController = CrossSdk.NetworkController;
            var activeChain = networkController.ActiveChain;
            var chainId = string.Empty;
            if (activeChain != null)
            {
                chainId = activeChain.ChainId;
            }
            else if (networkController.Chains.Values != null && networkController.Chains.Values.Count != 0)
            {
                chainId = networkController.Chains.Values.First().ChainId;
            }

            if (!string.IsNullOrWhiteSpace(chainId))
                UpdateWeb3Instance(chainId);
        }

        private void UpdateWeb3Instance(string chainId)
        {
            Web3 = new Web3(CreateRpcUrl(chainId))
            {
                Client =
                {
                    OverridingRequestInterceptor = _interceptor
                }
            };
        }

        private string CreateRpcUrl(string chainId)
        {
            if (_chainsSupportedByBlockchainApi.Contains(chainId))
                return $"https://rpc.walletconnect.com/v1?chainId={chainId}&projectId={CrossSdk.Config.projectId}";

            var chain = CrossSdk.Config.supportedChains.FirstOrDefault(x => x.ChainId == chainId);
            if (chain == null || string.IsNullOrWhiteSpace(chain.RpcUrl))
                throw new InvalidOperationException($"Chain with id {chainId} is not supported or doesn't have an RPC URL. Make sure it's added to the supported chains in the CrossSdk config.");

            Debug.Log($"Creating RPC URL for chain {chainId}: {chain.RpcUrl}");

            return chain.RpcUrl;
        }

        // -- Get Transaction by Hash ---------------------------------
        public override async Task<Transaction> GetTransactionByHash(string hash)
        {
            return await Web3.Eth.Transactions.GetTransactionByHash.SendRequestAsync(hash);
        }

        // -- Poll Transaction
        public override async Task<Transaction> PollTransaction(string hash)
        {
            Debug.Log($"pollingTx with hash: {hash}");

            var timeouts = new Queue<int>(new[] { 1000, 100 }); // milliseconds

            while (true)
            {
                try
                {
                    var tx = await GetTransactionByHash(hash);

                    if (tx != null)
                    {
                        Debug.Log($"tx found: {JsonConvert.SerializeObject(tx, Formatting.Indented)}");
                        return tx;
                    }
                }
                catch (System.Exception ex)
                {
                    throw new Exception($"Transaction not found. hash: {hash} errors: {ex}");
                }

                int delay = timeouts.Count > 0 ? timeouts.Dequeue() : 4000;
                await Task.Delay(delay);
            }
        }

        // -- Get Balance ----------------------------------------------

        protected override async Task<BigInteger> GetBalanceAsyncCore(string address)
        {
            var hexBigInt = await Web3.Eth.GetBalance.SendRequestAsync(address);
            return hexBigInt.Value;
        }


        // -- Sign Message ---------------------------------------------

        protected override async Task<string> SignMessageAsyncCore(string message, string address, CustomData customData = null)
        {
            var encodedMessage = message.ToHexUTF8();
            return await Web3.Client.SendRequestAsync<string>("personal_sign", null, encodedMessage, address, customData);
        }

        protected override async Task<string> SignMessageAsyncCore(byte[] rawMessage, string address, CustomData customData = null)
        {
            var encodedMessage = rawMessage.ToHex(true);
            return await Web3.Client.SendRequestAsync<string>("personal_sign", null, encodedMessage, address, customData);
        }


        // -- Verify Message -------------------------------------------

        protected override async Task<bool> VerifyMessageSignatureAsyncCore(string address, string message, string signature)
        {

            // -- ERC-6492
            var erc6492Service = Web3.Eth.SignatureValidationPredeployContractERC6492;
            if (erc6492Service.IsERC6492Signature(signature))
            {
                return await erc6492Service.IsValidSignatureMessageAsync(address, message, signature.HexToByteArray());
            }

            // -- EOA
            var recoveredAddress = _ethereumMessageSigner.EncodeUTF8AndEcRecover(message, signature);
            if (recoveredAddress.IsTheSameAddress(address))
            {
                return true;
            }

            // -- ERC-1271
            var ethGetCode = await Web3.Eth.GetCode.SendRequestAsync(address);
            if (ethGetCode is { Length: > 2 })
            {
                var hashedMessage = _ethereumMessageSigner.HashPrefixedMessage(Encoding.UTF8.GetBytes(message));

                var isValidSignatureFunctionMessage = new IsValidSignatureFunction()
                {
                    Hash = hashedMessage,
                    Signature = signature.HexToByteArray()
                };

                var handler = Web3.Eth.GetContractQueryHandler<IsValidSignatureFunction>();

                try
                {
                    var result = await handler.QueryAsync<byte[]>(address, isValidSignatureFunctionMessage);

                    // The magic value 0x1626ba7e
                    var magicValue = new byte[]
                    {
                        0x16,
                        0x26,
                        0xBA,
                        0x7E
                    };

                    return result != null && result.SequenceEqual(magicValue);
                }
                catch (SmartContractRevertException)
                {
                    return false;
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                    return false;
                }
            }

            return false;
        }


        // -- Sign Typed Data ------------------------------------------

        protected override Task<string> SignTypedDataAsyncCore(string dataJson)
        {
            return Web3.Client.SendRequestAsync<string>("eth_signTypedData_v4", null, dataJson);
        }


        // -- Verify Typed Data ----------------------------------------

        protected override Task<bool> VerifyTypedDataSignatureAsyncCore(string address, string dataJson, string signature)
        {
            var recoveredAddress = _eip712TypedDataSigner.RecoverFromSignatureV4(dataJson, signature);
            return Task.FromResult(recoveredAddress.IsTheSameAddress(address));
        }


        // -- Read Contract -------------------------------------------

        protected override async Task<TReturn> ReadContractAsyncCore<TReturn>(string contractAddress, string contractAbi, string methodName, object[] arguments = null)
        {
            var contract = Web3.Eth.GetContract(contractAbi, contractAddress);
            var function = contract.GetFunction(methodName);

            return await function.CallAsync<TReturn>(arguments);
        }


        // -- Write Contract ------------------------------------------

        private async Task<(HexBigInteger maxFeePerGas, HexBigInteger maxPriorityFeePerGas)> CalculateEIP1559Fees()
        {
            // 현재 블록의 base fee per gas 조회
            var feeHistory = await Web3.Client.SendRequestAsync<FeeHistoryResult>("eth_feeHistory", null, 1, "latest", new[] { 25, 75 });
            
            // maxPriorityFeePerGas 계산 (25th percentile)
            var maxPriorityFeePerGas = feeHistory.Reward[0][0];
            if (maxPriorityFeePerGas.Value == 0)
            {
                maxPriorityFeePerGas = new HexBigInteger(1000000000); // 1 Gwei 기본값
            }
            
            // base fee per gas 계산
            var baseFeePerGas = feeHistory.BaseFeePerGas[0];
            
            // maxFeePerGas 계산 (base fee + max priority fee)
            var maxFeePerGas = new HexBigInteger(baseFeePerGas.Value + maxPriorityFeePerGas.Value);
            
            return (maxFeePerGas, maxPriorityFeePerGas);
        }

        private async Task<TransactionInput> CreateTransactionInput(Nethereum.Contracts.Function function, string contractAddress, string contractAbi, string methodName, BigInteger value, BigInteger gas, int type, object[] arguments)
        {
            string addressFrom = default; // will be overrided using GetDefaultAddress within intercetper(CrossSignServiceCore). just send 0x here.
            var data = function.GetData(arguments);
            var gasLimit = gas == default 
                ? await EstimateGasAsyncCore(contractAddress, contractAbi, methodName, value, arguments)
                : gas;
            
            if (type == 0) // Legacy 트랜잭션
            {
                var gasPrice = await GetGasPriceAsyncCore();
                
                return new TransactionInput(
                    data,
                    contractAddress,
                    addressFrom,
                    new HexBigInteger(gasLimit),
                    new HexBigInteger(gasPrice),
                    new HexBigInteger(value)
                );
            }
            else if (type == 2) // EIP-1559 트랜잭션
            {
                var (maxFeePerGas, maxPriorityFeePerGas) = await CalculateEIP1559Fees();
                
                Debug.Log($"maxFeePerGas: {maxFeePerGas}, maxPriorityFeePerGas: {maxPriorityFeePerGas}");

                return new TransactionInput(
                    new HexBigInteger(type),
                    data,
                    contractAddress,
                    addressFrom,
                    new HexBigInteger(gasLimit),
                    new HexBigInteger(value),
                    maxFeePerGas,
                    maxPriorityFeePerGas
                );
            }
            else
            {
                throw new ArgumentException("Unsupported transaction type. Use 0 for Legacy or 2 for EIP-1559 transactions.");
            }
        }

        protected override async Task<string> WriteContractAsyncCore(string contractAddress, string contractAbi, string methodName, CustomData customData, BigInteger value = default, BigInteger gas = default, int type = default, params object[] arguments)
        {
            var contract = Web3.Eth.GetContract(contractAbi, contractAddress);
            var function = contract.GetFunction(methodName);
            
            var transactionInput = await CreateTransactionInput(function, contractAddress, contractAbi, methodName, value, gas, type, arguments);
            return await Web3.Client.SendRequestAsync<string>("eth_sendTransaction", null, transactionInput, customData);
        }

        // -- Send Transaction ----------------------------------------

        protected override async Task<string> SendTransactionAsyncCore(string addressTo, BigInteger value, string data = null, int type = 0, CustomData customData = null)
        {
            var gasLimit = await EstimateGasAsyncCore(addressTo, value, data);
            string addressFrom = default; // will be overrided using GetDefaultAddress within intercetper(CrossSignServiceCore). just send 0x here.
            TransactionInput transactionInput;
            
            if (type == 0) // Legacy 트랜잭션
            {
                var gasPrice = await GetGasPriceAsyncCore();
                
                transactionInput = new TransactionInput(
                    data,
                    addressTo,
                    addressFrom,
                    new HexBigInteger(gasLimit),
                    new HexBigInteger(gasPrice),
                    new HexBigInteger(value)
                );
            }
            else if (type == 2) // EIP-1559 트랜잭션
            {
                var (maxFeePerGas, maxPriorityFeePerGas) = await CalculateEIP1559Fees();
                
                transactionInput = new TransactionInput(
                    new HexBigInteger(type),
                    data,
                    addressTo,
                    addressFrom,
                    new HexBigInteger(gasLimit),
                    new HexBigInteger(value),
                    maxFeePerGas,
                    maxPriorityFeePerGas
                );
            }
            else
            {
                throw new ArgumentException("Unsupported transaction type. Use 0 for Legacy or 2 for EIP-1559 transactions.");
            }
            
            return await Web3.Client.SendRequestAsync<string>("eth_sendTransaction", null, transactionInput, customData);
        }
        
        
        // -- Send Raw Transaction ------------------------------------

        protected override Task<string> SendRawTransactionAsyncCore(string signedTransaction)
        {
            return Web3.Eth.Transactions.SendRawTransaction.SendRequestAsync(signedTransaction);
        }


        // -- Estimate Gas ---------------------------------------------
        
        protected override async Task<BigInteger> EstimateGasAsyncCore(string addressTo, BigInteger value, string data = null)
        {
            var transactionInput = new TransactionInput(data, addressTo, new HexBigInteger(value));
            return await Web3.Eth.Transactions.EstimateGas.SendRequestAsync(transactionInput);
        }

        protected override async Task<BigInteger> EstimateGasAsyncCore(string contractAddress, string contractAbi, string methodName, BigInteger value = default, params object[] arguments)
        {
            var contract = Web3.Eth.GetContract(contractAbi, contractAddress);
            var function = contract.GetFunction(methodName);
            
            // 가스 추정을 위해 현재 연결된 계정의 주소를 가져옴
            var account = await CrossSdk.GetAccountAsync();
            var fromAddress = account.Address ?? "0x0000000000000000000000000000000000000000";
            
            var transactionInput = new TransactionInput(
                function.GetData(arguments), 
                contractAddress, 
                fromAddress,
                default,  // gas - 가스 추정이므로 default
                new HexBigInteger(value)
            );
            return await Web3.Eth.Transactions.EstimateGas.SendRequestAsync(transactionInput);
        }
        
        
        // -- Get Gas Price -------------------------------------------

        protected override async Task<BigInteger> GetGasPriceAsyncCore()
        {
            var hexBigInt = await Web3.Eth.GasPrice.SendRequestAsync();
            return hexBigInt.Value;
        }
    }

    // FeeHistory 응답을 위한 클래스
    public class FeeHistoryResult
    {
        [JsonProperty("baseFeePerGas")]
        public HexBigInteger[] BaseFeePerGas { get; set; }
        
        [JsonProperty("gasUsedRatio")]
        public decimal[] GasUsedRatio { get; set; }
        
        [JsonProperty("oldestBlock")]
        public HexBigInteger OldestBlock { get; set; }
        
        [JsonProperty("reward")]
        public HexBigInteger[][] Reward { get; set; }
    }
}