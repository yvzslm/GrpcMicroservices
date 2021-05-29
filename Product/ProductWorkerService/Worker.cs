using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ProductWorkerService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IConfiguration _config;
        private readonly ProductGrpcManager _productGrpcManager;
        private readonly ProductFactory _productFactory;

        public Worker(ILogger<Worker> logger, IConfiguration config, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _config = config;

            var serviceProviderFromScope = serviceProvider.CreateScope().ServiceProvider;
            _productGrpcManager = serviceProviderFromScope.GetRequiredService<ProductGrpcManager>();
            _productFactory = serviceProviderFromScope.GetRequiredService<ProductFactory>();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var interval = _config.GetValue<int>("WorkerService:TaskInterval");
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

                var addProductRequest = await _productFactory.Generate();
                await _productGrpcManager.AddProductAsync(addProductRequest);
                await Task.Delay(interval, stoppingToken);
            }
        }
    }
}
