using VFNForge.SaaS.Contracts.Tenants;
using VFNForge.SaaS.Domain.Common;
using VFNForge.SaaS.Domain.Tenants;

namespace VFNForge.SaaS.Application.Tenants.Commands;

public sealed class TenantCommandService : ITenantCommandService
{
    private readonly ITenantRepository _repository;

    public TenantCommandService(ITenantRepository repository)
    {
        _repository = repository;
    }

    public async Task<TenantSummary> CreateAsync(TenantCreateRequest request, CancellationToken cancellationToken = default)
    {
        var existing = await _repository.FindByIdAsync(request.Id, cancellationToken);
        if (existing is not null)
        {
            throw new DomainException(DomainErrors.Tenant.AlreadyExists);
        }

        var tenant = Tenant.Create(request.Id, request.Name);
        await _repository.AddAsync(tenant, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return new TenantSummary(tenant.Id, tenant.Name, tenant.IsActive);
    }

    public async Task<TenantSummary> RenameAsync(string tenantId, TenantRenameRequest request, CancellationToken cancellationToken = default)
    {
        var tenant = await RequireTenantAsync(tenantId, cancellationToken);
        tenant.Rename(request.Name);
        await _repository.UpdateAsync(tenant, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
        return new TenantSummary(tenant.Id, tenant.Name, tenant.IsActive);
    }

    public async Task<TenantSummary> ActivateAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        var tenant = await RequireTenantAsync(tenantId, cancellationToken);
        tenant.Activate();
        await _repository.UpdateAsync(tenant, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
        return new TenantSummary(tenant.Id, tenant.Name, tenant.IsActive);
    }

    public async Task<TenantSummary> DeactivateAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        var tenant = await RequireTenantAsync(tenantId, cancellationToken);
        tenant.Deactivate();
        await _repository.UpdateAsync(tenant, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
        return new TenantSummary(tenant.Id, tenant.Name, tenant.IsActive);
    }

    private async Task<Tenant> RequireTenantAsync(string tenantId, CancellationToken cancellationToken)
    {
        var tenant = await _repository.FindByIdAsync(tenantId, cancellationToken);
        if (tenant is null)
        {
            throw new DomainException(DomainErrors.Tenant.NotFound);
        }

        return tenant;
    }
}
