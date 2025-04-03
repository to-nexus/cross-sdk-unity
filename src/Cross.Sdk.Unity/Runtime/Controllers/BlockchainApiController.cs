using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cross.Sdk.Unity.Http;
using Cross.Sdk.Unity.Model.BlockchainApi;
using Cross.Sign.Interfaces;

namespace Cross.Sdk.Unity
{
    public class BlockchainApiController
    {
        private const string BasePath = "https://stg-wallet-server.crosstoken.io";
        private const int TimoutSeconds = 5;

        private readonly IDictionary<string, string> _getBalanceHeaders = new Dictionary<string, string>
        {
            { "x-sdk-version", CrossSdk.Version }
        };

        private readonly UnityHttpClient _httpClient = new(new Uri(BasePath), TimeSpan.FromSeconds(TimoutSeconds));
        private string _clientIdQueryParam;

        private ISignClient _signClient;

        public Task InitializeAsync(ISignClient signClient)
        {
            _signClient = signClient;
            return Task.CompletedTask;
        }

        public async Task<GetIdentityResponse> GetIdentityAsync(string address)
        {
            if (string.IsNullOrWhiteSpace(address))
                throw new ArgumentNullException(nameof(address));

            var projectId = CrossSdk.Config.projectId;

            if (string.IsNullOrWhiteSpace(projectId))
                throw new InvalidOperationException("Project ID is not set");

            var path = $"identity/{address}?projectId={projectId}";

            if (string.IsNullOrWhiteSpace(_clientIdQueryParam))
            {
                if (_signClient != null)
                {
                    var rawClientId = await _signClient.CoreClient.Crypto.GetClientId();
                    _clientIdQueryParam = $"&clientId={Uri.EscapeDataString(rawClientId)}";
                }
            }

            if (!string.IsNullOrWhiteSpace(_clientIdQueryParam))
                path += _clientIdQueryParam;

            return await _httpClient.GetAsync<GetIdentityResponse>(path);
        }

        public async Task<GetBalanceResponse> GetBalanceAsync(string address)
        {
            if (string.IsNullOrWhiteSpace(address))
                throw new ArgumentNullException(nameof(address));

            var projectId = CrossSdk.Config.projectId;
            return await _httpClient.GetAsync<GetBalanceResponse>($"api/v1/public/token/balance?account={address}", headers: _getBalanceHeaders);
        }
    }
}