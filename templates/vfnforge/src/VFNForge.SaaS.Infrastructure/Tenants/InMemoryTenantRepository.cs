using Microsoft.Extensions.Options;
using VFNForge.SaaS.Contracts.Tenancy;
using VFNForge.SaaS.Domain.Tenants;

namespace VFNForge.SaaS.Infrastructure.Tenants;

public sealed class InMemoryTenantRepository : ITenantRepository
{
    private readonly IReadOnlyList<Tenant> _tenants;

    public InMemoryTenantRepository(IOptions<TenancyOptions> options)
    {
        var configured = options.Value;
        _tenants = configured.Tenants
            .Select(t => Tenant.Create(t.Id, t.Name ?? t.Id))
            .ToArray();
    }

    public Task<IReadOnlyList<Tenant>> ListAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(_tenants);
}
