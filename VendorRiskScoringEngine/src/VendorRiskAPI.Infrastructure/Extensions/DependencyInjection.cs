using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using VendorRiskAPI.Application.Interfaces;
using VendorRiskAPI.Infrastructure.Persistence;
using VendorRiskAPI.Infrastructure.Persistence.Repositories;
using VendorRiskAPI.Infrastructure.Seeders;
using VendorRiskAPI.Infrastructure.Services;

namespace VendorRiskAPI.Infrastructure.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Database
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)
            )
        );

        // Repositories
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Redis
        var redisConnection = configuration.GetValue<string>("Redis:ConnectionString");
        if (!string.IsNullOrEmpty(redisConnection))
        {
            services.AddSingleton<IConnectionMultiplexer>(sp =>
                ConnectionMultiplexer.Connect(redisConnection));
            services.AddScoped<ICacheService, RedisCacheService>();
        }

        // Seeders
        services.AddScoped<DatabaseSeeder>();

        return services;
    }
}
