using VFNForge.SaaS.Contracts.Tenants;
using VFNForge.SaaS.Domain.Tenants;

namespace VFNForge.SaaS.Application.Tenants;

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
}
