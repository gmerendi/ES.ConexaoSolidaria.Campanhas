using Campanhas.Domain.Interfaces;
using Campanhas.Infrastructure.Data;
using Campanhas.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace Campanhas.Api.Extensions;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddDatabaseServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<CampanhaDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                npgsql => npgsql.MigrationsAssembly("Campanhas.Infrastructure")));

        return services;
    }

    public static IServiceCollection AddRedisServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Redis")
            ?? "localhost:6379";

        var config = ConfigurationOptions.Parse(connectionString);
        config.AbortOnConnectFail = false;

        services.AddSingleton<IConnectionMultiplexer>(
            ConnectionMultiplexer.Connect(config));

        return services;
    }

    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<ICampanhaRepository, CampanhaRepository>();
        return services;
    }
}