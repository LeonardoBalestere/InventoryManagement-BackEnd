using InventoryManagement.Application.Common.Interfaces;
using InventoryManagement.Infrastructure.BackgroundServices;
using InventoryManagement.Infrastructure.Caching;
using InventoryManagement.Infrastructure.Email;
using InventoryManagement.Infrastructure.Persistence;
using InventoryManagement.Infrastructure.Persistence.Outbox;
using InventoryManagement.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

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

        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<ICategoryReadRepository, CategoryReadRepository>();

        var redisConnectionString = configuration.GetConnectionString("RedisConnection");
        if (!string.IsNullOrEmpty(redisConnectionString))
        {
            // Redis wrapped with Polly retries + circuit breaker (ADR-5)
            services.AddSingleton<IDistributedCache>(sp =>
            {
                var redisCache = new RedisCache(Options.Create(new RedisCacheOptions
                {
                    Configuration = redisConnectionString
                }));

                return new ResilientDistributedCache(redisCache, sp.GetRequiredService<ILogger<ResilientDistributedCache>>());
            });
        }
        else
        {
            services.AddDistributedMemoryCache();
        }

        // Async email alerts via transactional outbox (ADR-9)
        services.AddScoped<ILowStockAlertOutbox, LowStockAlertOutbox>();
        services.AddScoped<IOutboxMessageRepository, OutboxMessageRepository>();
        services.AddScoped<OutboxDispatcher>();

        var emailOptions = new EmailOptions
        {
            SendGridApiKey = configuration["Email:SendGridApiKey"] ?? string.Empty,
            From = configuration["Email:From"] ?? string.Empty,
            FromName = configuration["Email:FromName"] ?? "Inventory Management",
            To = configuration["Email:To"] ?? string.Empty
        };
        services.AddSingleton(emailOptions);

        if (!string.IsNullOrEmpty(emailOptions.SendGridApiKey))
        {
            services.AddSingleton<IEmailSender, SendGridEmailSender>();
        }
        else
        {
            services.AddSingleton<IEmailSender, LoggingEmailSender>();
        }

        services.AddHostedService<OutboxProcessor>();

        return services;
    }
}
