using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ProductGrpc.Protos;
using System.Threading.Tasks;
using static ProductGrpc.Protos.ProductProtoService;

namespace ProductWorkerService
{
    public class ProductGrpcManager
    {
        private readonly ProductProtoServiceClient _client;
        private readonly IConfiguration _config;
        private readonly ILogger<ProductFactory> _logger;

        public ProductGrpcManager(ILogger<ProductFactory> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;
            var productGrpcServerUrl = _config.GetValue<string>("WorkerService:ServerUrl");
            var channel = GrpcChannel.ForAddress(productGrpcServerUrl);
            _client = new ProductProtoServiceClient(channel);
        }

        public async Task AddProductAsync(AddProductRequest addProductRequest)
        {
            _logger.LogInformation("AddProductAsync started.");

            var response = await _client.AddProductAsync(addProductRequest);

            _logger.LogInformation($"AddProductAsync response: {response}");
        }
    }
}
