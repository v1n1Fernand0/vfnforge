using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VFNForge.SaaS.Infrastructure.Data;

namespace VFNForge.SaaS.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    private const string DefaultConnectionName = "SqlServer";

    public static IServiceCollection AddInfrastructureData(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(DefaultConnectionName);
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException($"ConnectionStrings:{DefaultConnectionName} nao configurada no appsettings.");
        }

        services.AddDbContext<VFNForgeDbContext>(options =>
        {
            options.UseSqlServer(connectionString, sql =>
            {
                sql.EnableRetryOnFailure();
            });
        });

        return services;
    }
}
