using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ProductGrpc.Protos;
using ShoppingCartGrpc.Protos;
using System.Threading.Tasks;
using static ProductGrpc.Protos.ProductProtoService;
using static ShoppingCartGrpc.Protos.ShoppingCartProtoService;

namespace ShoppingCartWorkerService
{
    public class ShoppingCartGrpcManager
    {
        private readonly ShoppingCartProtoServiceClient _client;
        private readonly ProductProtoServiceClient _productGrpcClient;
        private readonly IConfiguration _config;
        private readonly ILogger<ShoppingCartGrpcManager> _logger;

        public ShoppingCartGrpcManager(IConfiguration config, ILogger<ShoppingCartGrpcManager> logger)
        {
            _config = config;
            _logger = logger;

            var shoppingCartGrpcServerUrl = _config.GetValue<string>("WorkerService:ShoppingCartServerUrl");
            var channel = GrpcChannel.ForAddress(shoppingCartGrpcServerUrl);
            _client = new ShoppingCartProtoServiceClient(channel);

            var productGrpcServerUrl = _config.GetValue<string>("WorkerService:ProductServerUrl");
            var productChannel = GrpcChannel.ForAddress(productGrpcServerUrl);
            _productGrpcClient = new ProductProtoServiceClient(productChannel);
        }

        public async Task<ShoppingCartModel> GetOrCreateShoppingCartAsync(string token)
        {
            ShoppingCartModel shoppingCartModel;
            try
            {
                _logger.LogInformation("GetOrCreateShoppingCartAsync started.");

                shoppingCartModel = await _client.GetShoppingCartAsync(new GetShoppingCartRequest()
                {
                    Username = _config.GetValue<string>("WorkerService:UserName")
                }, GetHeaders(token));

                _logger.LogInformation($"GetOrCreateShoppingCartAsync response: {shoppingCartModel}");
            }
            catch (RpcException ex)
            {
                if (ex.StatusCode == StatusCode.NotFound)
                {
                    shoppingCartModel = await CreateShoppingCartAsync(token);
                }
                else
                {
                    throw;
                }
            }

            return shoppingCartModel;
        }

        public async Task AddItemIntoShoppingCartAsync(string token)
        {
            _logger.LogInformation("AddItemIntoShoppingCartAsync started.");

            var stream = _client.AddItemIntoShoppingCart(GetHeaders(token));

            var productStream = _productGrpcClient.GetAllProducts(new GetAllProductsRequest());
            await foreach (var product in productStream.ResponseStream.ReadAllAsync())
            {
                var request = new AddItemIntoShoppingCartRequest()
                {
                    Username = _config.GetValue<string>("WorkerService:UserName"),
                    DiscountCode = "CODE_100",
                    NewCartItem = new ShoppingCartItemModel()
                    {
                        ProductId = product.ProductId,
                        ProductName = product.Name,
                        Price = product.Price,
                        Color = "Black",
                        Quantity = 1
                    }
                };

                await stream.RequestStream.WriteAsync(request);

                _logger.LogInformation($"Item added into shopping cart: {request}");
            }

            await stream.RequestStream.CompleteAsync();
            _logger.LogInformation($"AddItemIntoShoppingCartAsync response: {stream.ResponseAsync.Result}");
        }

        private async Task<ShoppingCartModel> CreateShoppingCartAsync(string token)
        {
            _logger.LogInformation("CreateShoppingCartAsync started.");
            var shoppingCartModel = await _client.CreateShoppingCartAsync(new ShoppingCartModel()
            {
                Username = _config.GetValue<string>("WorkerService:UserName")
            }, GetHeaders(token));

            _logger.LogInformation($"CreateShoppingCartAsync response: {shoppingCartModel}");

            return shoppingCartModel;
        }

        private Metadata GetHeaders(string token)
        {
            var headers = new Metadata();
            headers.Add("Authorization", $"Bearer {token}");

            return headers;
        }
    }
}
