using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ShoppingCartGrpc.Data;

namespace ShoppingCartGrpc.Extensions
{
    public static class HostExtension
    {
        public static IHost SeedData(this IHost host)
        {
            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var context = services.GetRequiredService<ShoppingCartContext>();

                ShoppingCartContextSeeder.Seed(context);
            }

            return host;
        }
    }
}
