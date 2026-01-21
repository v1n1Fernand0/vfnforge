using Microsoft.EntityFrameworkCore;
using VFNForge.SaaS.Domain.Tenants;

namespace VFNForge.SaaS.Infrastructure.Persistence.Data;

public sealed class VFNForgeDbContext : DbContext
{
    public VFNForgeDbContext(DbContextOptions<VFNForgeDbContext> options) : base(options)
    {
    }

    public DbSet<Tenant> Tenants => Set<Tenant>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(VFNForgeDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
