using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using ShoppingCartGrpc.Data;
using System;
using static DiscountGrpc.Protos.DiscountProtoService;

namespace ShoppingCartGrpc.Extensions
{
    public static class ServiceCollectionExtension
    {
        public static void AddDbContexts(this IServiceCollection services)
        {
            services.AddDbContext<ShoppingCartContext>(opt =>
            {
                opt.UseInMemoryDatabase("ShoppingCart");
            });
        }

        public static void AddGrpcClients(this IServiceCollection services, IConfiguration config)
        {
            services.AddGrpcClient<DiscountProtoServiceClient>(opt =>
            {
                opt.Address = new Uri(config["GrpcConfigs:DiscountUrl"]);
            });
        }

        public static void AddAuthentication(this IServiceCollection services, IConfiguration config)
        {
            services.AddAuthentication("Bearer").AddJwtBearer("Bearer", opt =>
            {
                opt.Authority = config.GetValue<string>("IdentityServerUrl");
                opt.TokenValidationParameters = new TokenValidationParameters { ValidateAudience = false };
            });
        }
    }
}
