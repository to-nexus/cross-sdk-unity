using System.Linq;
using System.Threading.Tasks;
using Nethereum.RPC.Eth.DTOs;
using Cross.Sign.Interfaces;
using Cross.Sign.Nethereum.Model;
using Cross.Sign.Models;
using EthSignTypedDataV4 = Cross.Sign.Nethereum.Model.EthSignTypedDataV4;
using Transaction = Cross.Sign.Nethereum.Model.Transaction;
using WalletAddEthereumChain = Cross.Sign.Nethereum.Model.WalletAddEthereumChain;
using WalletSwitchEthereumChain = Cross.Sign.Nethereum.Model.WalletSwitchEthereumChain;
using Cross.Core.Common.Logging;
using Newtonsoft.Json;

namespace Cross.Sign.Nethereum
{
    public class CrossSignServiceCore : CrossSignService
    {
        private readonly ISignClient _signClient;
        private readonly ILogger _logger;

        public CrossSignServiceCore(ISignClient signClient)
        {
            _signClient = signClient;
            _logger = CrossLogger.WithContext("CrossSignServiceCore");
        }

        public override bool IsWalletConnected
        {
            get => _signClient.AddressProvider.DefaultSession != null;
        }

        private string GetDefaultAddress()
        {
            var addressProvider = _signClient.AddressProvider;
            var defaultChainId = addressProvider.DefaultChainId;
            return addressProvider.DefaultSession.CurrentAccount(defaultChainId).Address;
        }

        protected override bool IsMethodSupportedCore(string method)
        {
            var addressProvider = _signClient.AddressProvider;
            var defaultNamespace = addressProvider.DefaultNamespace;
            return addressProvider.DefaultSession.Namespaces[defaultNamespace].Methods.Contains(method);
        }

        protected override async Task<object> SendTransactionAsyncCore(TransactionInput transaction, CustomData customData = null)
        {
            var fromAddress = GetDefaultAddress();
            var txData = new Transaction
            {
                From = fromAddress,
                To = transaction.To,
                Value = transaction.Value?.HexValue,
                Gas = transaction.Gas?.HexValue,
                GasPrice = transaction.GasPrice?.HexValue,
                Data = transaction.Data,
            };
            var sendTransactionRequest = new EthSendTransaction(txData);

            _logger.Log($"sent sendTransactionRequest");
            var txHash = await _signClient.Request<EthSendTransaction, string>(sendTransactionRequest, customData);
            _logger.Log($"txHash: {txHash}");
            
            return txHash;
        }

        protected override async Task<object> PersonalSignAsyncCore(string message, string address, CustomData customData = null)
        {
            
            address = (address == null || address == "0x") ? GetDefaultAddress() : address;
            var signDataRequest = new PersonalSign(message);

            return await _signClient.RequestWithAddress<PersonalSign, string>(signDataRequest, address, customData);
        }

        protected override async Task<object> EthSignTypedDataV4AsyncCore(string data, string address = null)
        {
            address ??= GetDefaultAddress();
            var signDataRequest = new EthSignTypedDataV4(address, data);
            return await _signClient.Request<EthSignTypedDataV4, string>(signDataRequest);
        }

        protected override async Task<object> WalletSwitchEthereumChainAsyncCore(SwitchEthereumChain arg)
        {
            var switchChainRequest = new WalletSwitchEthereumChain(arg.chainId);
            return await _signClient.Request<WalletSwitchEthereumChain, string>(switchChainRequest);
        }

        protected override async Task<object> WalletAddEthereumChainAsyncCore(EthereumChain chain)
        {
            var addEthereumChainRequest = new WalletAddEthereumChain(chain);
            return await _signClient.Request<WalletAddEthereumChain, string>(addEthereumChainRequest);
        }
    }
}