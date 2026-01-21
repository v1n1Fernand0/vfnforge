using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using VFNForge.SaaS.Contracts.Tenancy;
using VFNForge.SaaS.Domain.Tenants;
using VFNForge.SaaS.Infrastructure.Data;

namespace VFNForge.SaaS.Infrastructure.Initialization;

public static class DatabaseInitializer
{
    public static async Task InitializeDatabaseAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<VFNForgeDbContext>();
        await context.Database.EnsureCreatedAsync();

        var tenancyOptions = scope.ServiceProvider.GetRequiredService<IOptions<TenancyOptions>>().Value;
        var configuredTenants = new List<(string Id, string? Name)>();
        foreach (var tenant in tenancyOptions.Tenants)
        {
            if (!string.IsNullOrWhiteSpace(tenant.Id))
            {
                configuredTenants.Add((tenant.Id, tenant.Name));
            }
        }

        if (!string.IsNullOrWhiteSpace(tenancyOptions.DefaultTenantId))
        {
            configuredTenants.Add((tenancyOptions.DefaultTenantId, string.Empty));
        }

        var seeds = configuredTenants
            .GroupBy(t => t.Id!, StringComparer.OrdinalIgnoreCase)
            .Select(g => new { Id = g.Key, Name = g.Select(x => x.Name).FirstOrDefault(n => !string.IsNullOrWhiteSpace(n)) ?? g.Key })
            .ToList();

        foreach (var seed in seeds)
        {
            var exists = await context.Tenants.AnyAsync(t => t.Id == seed.Id);
            if (!exists)
            {
                context.Tenants.Add(Tenant.Create(seed.Id, seed.Name));
            }
        }

        if (context.ChangeTracker.HasChanges())
        {
            await context.SaveChangesAsync();
        }
    }
}
