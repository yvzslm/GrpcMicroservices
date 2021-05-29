using IdentityModel.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Threading.Tasks;

namespace ShoppingCartWorkerService
{
    public class IdentityService
    {
        private readonly HttpClient _client;
        private readonly IConfiguration _config;
        private readonly ILogger<IdentityService> _logger;

        public IdentityService(HttpClient client, IConfiguration config, ILogger<IdentityService> logger)
        {
            _client = client;
            _config = config;
            _logger = logger;
        }

        public async Task<string> GetTokenFromIdentityServerAsync()
        {
            var discoveryDoc = await _client.GetDiscoveryDocumentAsync(_config.GetValue<string>("WorkerService:IdentityServerUrl"));
            if (discoveryDoc.IsError)
            {
                _logger.LogError(discoveryDoc.Error);
                return string.Empty;
            }

            var tokenResponse = await _client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
            {
                Address = discoveryDoc.TokenEndpoint,
                ClientId = "ShoppingCartClient",
                ClientSecret = "secret",
                Scope = "ShoppingCartAPI"
            });

            if (tokenResponse.IsError)
            {
                _logger.LogError(tokenResponse.Error);
                return string.Empty;
            }

            return tokenResponse.AccessToken;
        }
    }
}
