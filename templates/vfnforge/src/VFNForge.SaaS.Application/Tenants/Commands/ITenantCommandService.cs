using VFNForge.SaaS.Contracts.Tenants;

namespace VFNForge.SaaS.Application.Tenants.Commands;

public interface ITenantCommandService
{
    Task<TenantSummary> CreateAsync(TenantCreateRequest request, CancellationToken cancellationToken = default);
    Task<TenantSummary> RenameAsync(string tenantId, TenantRenameRequest request, CancellationToken cancellationToken = default);
    Task<TenantSummary> ActivateAsync(string tenantId, CancellationToken cancellationToken = default);
    Task<TenantSummary> DeactivateAsync(string tenantId, CancellationToken cancellationToken = default);
}
