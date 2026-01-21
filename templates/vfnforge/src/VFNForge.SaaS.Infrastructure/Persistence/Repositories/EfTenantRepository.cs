using Microsoft.EntityFrameworkCore;
using VFNForge.SaaS.Domain.Tenants;
using VFNForge.SaaS.Infrastructure.Persistence.Data;

namespace VFNForge.SaaS.Infrastructure.Persistence.Repositories;

public sealed class EfTenantRepository : ITenantRepository
{
    private readonly VFNForgeDbContext _context;

    public EfTenantRepository(VFNForgeDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Tenant>> ListAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Tenants.AsNoTracking().ToListAsync(cancellationToken);
    }

    public Task<Tenant?> FindByIdAsync(string id, CancellationToken cancellationToken = default)
        => _context.Tenants.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

    public async Task AddAsync(Tenant tenant, CancellationToken cancellationToken = default)
    {
        await _context.Tenants.AddAsync(tenant, cancellationToken);
    }

    public Task UpdateAsync(Tenant tenant, CancellationToken cancellationToken = default)
    {
        _context.Tenants.Update(tenant);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}
