using VFNForge.SaaS.Contracts.Tenants;
using VFNForge.SaaS.Domain.Tenants;

namespace VFNForge.SaaS.Application.Tenants.Queries;

public sealed class TenantQueryService : ITenantQueryService
{
    private readonly ITenantRepository _repository;

    public TenantQueryService(ITenantRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyCollection<TenantSummary>> ListAsync(CancellationToken cancellationToken = default)
    {
        var tenants = await _repository.ListAsync(cancellationToken);
        return tenants.Select(t => new TenantSummary(t.Id, t.Name, t.IsActive)).ToArray();
    }

    public async Task<TenantSummary?> GetAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        var tenant = await _repository.FindByIdAsync(tenantId, cancellationToken);
        if (tenant is null)
        {
            return null;
        }

        return new TenantSummary(tenant.Id, tenant.Name, tenant.IsActive);
    }
}
