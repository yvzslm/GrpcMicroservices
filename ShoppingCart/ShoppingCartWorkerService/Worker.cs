using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ShoppingCartWorkerService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IConfiguration _config;
        private readonly ShoppingCartGrpcManager _shoppingCartGrpcManager;
        private readonly IdentityService _identityService;

        public Worker(ILogger<Worker> logger, IConfiguration config, IServiceProvider serviceProvider, IdentityService identityService)
        {
            _logger = logger;
            _config = config;

            var serviceProviderFromScope = serviceProvider.CreateScope().ServiceProvider;
            _shoppingCartGrpcManager = serviceProviderFromScope.GetRequiredService<ShoppingCartGrpcManager>();

            _identityService = identityService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var interval = _config.GetValue<int>("WorkerService:TaskInterval");
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

                var token = await _identityService.GetTokenFromIdentityServerAsync();
                await _shoppingCartGrpcManager.GetOrCreateShoppingCartAsync(token);
                await _shoppingCartGrpcManager.AddItemIntoShoppingCartAsync(token);

                await Task.Delay(interval, stoppingToken);
            }
        }
    }
}
