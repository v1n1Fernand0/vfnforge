namespace VFNForge.SaaS.Domain.Tenants;

public interface ITenantRepository
{
    Task<IReadOnlyList<Tenant>> ListAsync(CancellationToken cancellationToken = default);
}
