using System;
using System.Numerics;
using System.Threading.Tasks;
using Cross.Sign.Unity;
using Cross.Sign.Models;
using Nethereum.RPC.Eth.DTOs;

namespace Cross.Sdk.Unity
{
    public abstract class EvmService
    {
        public Task InitializeAsync(SignClientUnity signClient)
        {
            return InitializeAsyncCore(signClient);
        }

        // -- Get Balance ---------------------------------------------

        public Task<BigInteger> GetBalanceAsync(string address)
        {
            if (string.IsNullOrWhiteSpace(address))
                throw new ArgumentNullException(nameof(address));

            return GetBalanceAsyncCore(address);
        }

        // -- Sign Message ---------------------------------------------

        public Task<string> SignMessageAsync(string message, string address, CustomData customData = null)
        {
            if (string.IsNullOrWhiteSpace(message))
                throw new ArgumentNullException(nameof(message));

            return SignMessageAsyncCore(message, address, customData);
        }

        public Task<string> SignMessageAsync(byte[] rawMessage, string address, CustomData customData = null)
        {
            if (rawMessage == null || rawMessage.Length == 0)
                throw new ArgumentNullException(nameof(rawMessage));

            return SignMessageAsyncCore(rawMessage, address, customData);
        }


        // -- Sign Typed Data ------------------------------------------

        public Task<string> SignTypedDataAsync(string data)
        {
            if (string.IsNullOrWhiteSpace(data))
                throw new ArgumentNullException(nameof(data));

            return SignTypedDataAsyncCore(data);
        }


        // -- Verify Message -------------------------------------------

        public Task<bool> VerifyMessageSignatureAsync(string address, string message, string signature)
        {
            if (string.IsNullOrWhiteSpace(address))
                throw new ArgumentNullException(nameof(address));
            if (string.IsNullOrWhiteSpace(message))
                throw new ArgumentNullException(nameof(message));
            if (string.IsNullOrWhiteSpace(signature))
                throw new ArgumentNullException(nameof(signature));

            return VerifyMessageSignatureAsyncCore(address, message, signature);
        }


        // -- Verify Typed Data ----------------------------------------

        public Task<bool> VerifyTypedDataSignatureAsync(string address, string data, string signature)
        {
            if (string.IsNullOrWhiteSpace(address))
                throw new ArgumentNullException(nameof(address));
            if (string.IsNullOrWhiteSpace(data))
                throw new ArgumentNullException(nameof(data));
            if (string.IsNullOrWhiteSpace(signature))
                throw new ArgumentNullException(nameof(signature));

            return VerifyTypedDataSignatureAsyncCore(address, data, signature);
        }


        // -- Read Contract -------------------------------------------

        public Task<TReturn> ReadContractAsync<TReturn>(string contractAddress, string contractAbi, string methodName, object[] arguments = null)
        {
            return ReadContractAsyncCore<TReturn>(contractAddress, contractAbi, methodName, arguments);
        }


        // -- Write Contract ------------------------------------------

        public Task<string> WriteContractAsync(string contractAddress, string contractAbi, string methodName, CustomData customData, params object[] arguments)
        {
            return WriteContractAsync(contractAddress, contractAbi, methodName, customData, BigInteger.Zero, default, default, arguments);
        }

        public Task<string> WriteContractAsync(string contractAddress, string contractAbi, string methodName, CustomData customData, BigInteger value = default, BigInteger gas = default, int type = default, params object[] arguments)
        {
            // Ensure value is zero for contract interactions (not ETH transfers)
            if (value == default)
                value = BigInteger.Zero;
                
            return WriteContractAsyncCore(contractAddress, contractAbi, methodName, customData, value, gas, type, arguments);
        }


        // -- Send Transaction ----------------------------------------

        public Task<string> SendTransactionAsync(string addressTo, BigInteger value, string data = null, int type = 0, CustomData customData = null)
        {
            if (string.IsNullOrWhiteSpace(addressTo))
                throw new ArgumentNullException(nameof(addressTo));

            return SendTransactionAsyncCore(addressTo, value, data, type, customData);
        }
        
        
        // -- Send Raw Transaction ------------------------------------
        
        public Task<string> SendRawTransactionAsync(string signedTransaction)
        {
            if (string.IsNullOrWhiteSpace(signedTransaction))
                throw new ArgumentNullException(nameof(signedTransaction));
            
            return SendRawTransactionAsyncCore(signedTransaction);
        }
        
        
        // -- Estimate Gas --------------------------------------------
        
        public Task<BigInteger> EstimateGasAsync(string addressTo, BigInteger value, string data = null)
        {
            if (string.IsNullOrWhiteSpace(addressTo))
                throw new ArgumentNullException(nameof(addressTo));
            
            return EstimateGasAsyncCore(addressTo, value, data);
        }

        public Task<BigInteger> EstimateGasAsync(string contractAddress, string contractAbi, string methodName, BigInteger value = default, params object[] arguments)
        {
            if (string.IsNullOrWhiteSpace(contractAddress))
                throw new ArgumentNullException(nameof(contractAddress));
            if (string.IsNullOrWhiteSpace(contractAbi))
                throw new ArgumentNullException(nameof(contractAbi));
            if (string.IsNullOrWhiteSpace(methodName))
                throw new ArgumentNullException(nameof(methodName));
            
            return EstimateGasAsyncCore(contractAddress, contractAbi, methodName, value, arguments);
        }
        
        
        // -- Gas Price ------------------------------------------------
        
        public Task<BigInteger> GetGasPriceAsync()
        {
            return GetGasPriceAsyncCore();
        }

        public abstract Task<Transaction> GetTransactionByHash(string hash);
        public abstract Task<Transaction> PollTransaction(string hash);

        protected abstract Task InitializeAsyncCore(SignClientUnity signClient);
        protected abstract Task<BigInteger> GetBalanceAsyncCore(string address);
        protected abstract Task<string> SignMessageAsyncCore(string message, string address, CustomData customData = null);
        protected abstract Task<string> SignMessageAsyncCore(byte[] rawMessage, string address, CustomData customData = null);
        protected abstract Task<bool> VerifyMessageSignatureAsyncCore(string address, string message, string signature);
        protected abstract Task<string> SignTypedDataAsyncCore(string dataJson);
        protected abstract Task<bool> VerifyTypedDataSignatureAsyncCore(string address, string dataJson, string signature);
        protected abstract Task<TReturn> ReadContractAsyncCore<TReturn>(string contractAddress, string contractAbi, string methodName, object[] arguments = null);
        protected abstract Task<string> WriteContractAsyncCore(string contractAddress, string contractAbi, string methodName, CustomData customData, BigInteger value = default, BigInteger gas = default, int type = default, params object[] arguments);
        protected abstract Task<string> SendTransactionAsyncCore(string addressTo, BigInteger value, string data = null, int type = 0, CustomData customData = null);
        protected abstract Task<string> SendRawTransactionAsyncCore(string signedTransaction);
        protected abstract Task<BigInteger> EstimateGasAsyncCore(string addressTo, BigInteger value, string data = null);
        protected abstract Task<BigInteger> EstimateGasAsyncCore(string contractAddress, string contractAbi, string methodName, BigInteger value = default, params object[] arguments);
        protected abstract Task<BigInteger> GetGasPriceAsyncCore();
    }
}