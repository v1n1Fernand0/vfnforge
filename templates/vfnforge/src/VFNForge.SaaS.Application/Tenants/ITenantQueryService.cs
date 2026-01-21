using VFNForge.SaaS.Contracts.Tenants;

namespace VFNForge.SaaS.Application.Tenants;

public interface ITenantQueryService
{
    Task<IReadOnlyCollection<TenantSummary>> ListAsync(CancellationToken cancellationToken = default);
}
