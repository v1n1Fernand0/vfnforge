using VFNForge.SaaS.Contracts.Tenants;

namespace VFNForge.SaaS.Application.Tenants.Queries;

public interface ITenantQueryService
{
    Task<IReadOnlyCollection<TenantSummary>> ListAsync(CancellationToken cancellationToken = default);
    Task<TenantSummary?> GetAsync(string tenantId, CancellationToken cancellationToken = default);
}
