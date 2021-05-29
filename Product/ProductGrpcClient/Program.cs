using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Net.Client;
using ProductGrpc.Protos;
using System;
using System.Threading.Tasks;
using static ProductGrpc.Protos.ProductProtoService;

namespace ProductGrpcClient
{
    class Program
    {
        private static ProductProtoServiceClient _client;

        static async Task Main(string[] args)
        {
            var channel = GrpcChannel.ForAddress("https://localhost:5001");
            _client = new ProductProtoServiceClient(channel);

            //await GetProductAsync();
            await GetAllProductAsync();
            //await AddProductAsync();
            //await UpdateProductAsync();
            //await DeleteProductAsync();
            await InsertBulkProduct();
            await GetAllProductAsync();
            Console.ReadLine();
        }

        static async Task GetProductAsync()
        {
            Console.WriteLine("GetProductAsync started.");

            var response = await _client.GetProductAsync(new GetProductRequest()
            {
                ProductId = 3
            });

            Console.WriteLine($"GetProductAsync response: {response}");
        }

        static async Task GetAllProductAsync()
        {
            Console.WriteLine("GetAllProductAsync started.");
            
            using (var stream = _client.GetAllProducts(new GetAllProductsRequest()))
            {
                while (await stream.ResponseStream.MoveNext())
                {
                    var currentProduct = stream.ResponseStream.Current;
                    Console.WriteLine(currentProduct);
                }
            }
        }

        static async Task AddProductAsync()
        {
            Console.WriteLine("AddProductAsync started.");

            var response = await _client.AddProductAsync(new AddProductRequest()
            {
                Product = new ProductModel()
                {
                    Name = "Red",
                    Description = "New Red Phone Mi 10T",
                    Price = 699,
                    Status = ProductStatus.None,
                    CreatedTime = Timestamp.FromDateTime(DateTime.UtcNow)
                }
            });

            Console.WriteLine($"AddProductAsync response: {response}");
        }

        static async Task UpdateProductAsync()
        {
            Console.WriteLine("UpdateProductAsync started.");

            var response = await _client.UpdateProductAsync(new UpdateProductRequest()
            {
                Product = new ProductModel()
                {
                    ProductId = 2,
                    Name = "P40",
                    Description = "New Huawei Phone P40",
                    Price = 999,
                    Status = ProductStatus.Low,
                    CreatedTime = Timestamp.FromDateTime(DateTime.UtcNow)
                }
            });

            Console.WriteLine($"UpdateProductAsync response: {response}");
        }

        static async Task DeleteProductAsync()
        {
            Console.WriteLine("DeleteProductAsync started.");

            var response = await _client.DeleteProductAsync(new DeleteProductRequest()
            {
               ProductId = 2
            });

            Console.WriteLine($"DeleteProductAsync response: {response}");
        }

        static async Task InsertBulkProduct()
        {
            Console.WriteLine("InsertBulkProduct started.");

            using (var stream = _client.InsertBulkProduct())
            {
                for (int i = 1; i < 4; i++)
                {
                    await stream.RequestStream.WriteAsync(new ProductModel()
                    {
                        Name = $"Product {i}",
                        Description = "Bulk inserted product",
                        Price = 99 * i,
                        Status = ProductStatus.Instock,
                        CreatedTime = Timestamp.FromDateTime(DateTime.UtcNow)
                    });
                }

                await stream.RequestStream.CompleteAsync();

                Console.WriteLine($"InsertBulkProduct response: {stream.ResponseAsync.Result}");
            }
        }
    }
}
