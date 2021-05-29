using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ProductGrpc.Data;

namespace ProductGrpc.Extensions
{
    public static class HostExtension
    {
        public static IHost SeedData(this IHost host)
        {
            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var context = services.GetRequiredService<ProductContext>();

                ProductContextSeeder.Seed(context);
            }

            return host;
        }
    }
}
