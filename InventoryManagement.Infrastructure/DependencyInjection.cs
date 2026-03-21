using InventoryManagement.Application.Common.Interfaces;
using InventoryManagement.Infrastructure.Persistence;
using InventoryManagement.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;

namespace InventoryManagement.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddIdentityCore<IdentityUser>()
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>();

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IProductReadRepository, ProductReadRepository>();

        var redisConnectionString = configuration.GetConnectionString("RedisConnection");
        if (!string.IsNullOrEmpty(redisConnectionString))
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnectionString;
            });
        }
        else
        {
            services.AddDistributedMemoryCache();
        }

        return services;
    }
}