using VFNForge.SaaS.Domain.Abstractions;
using VFNForge.SaaS.Domain.Tenants;

namespace VFNForge.SaaS.Domain.Tests;

public class TenantTests
{
    [Fact]
    public void Create_Should_ReturnTenant_WhenDataIsValid()
    {
        var tenant = Tenant.Create("tenant-001", "Tenant One");

        Assert.Equal("tenant-001", tenant.Id);
        Assert.Equal("Tenant One", tenant.Name);
        Assert.True(tenant.IsActive);
    }

    [Fact]
    public void Create_ShouldThrow_WhenIdIsEmpty()
    {
        var ex = Assert.Throws<DomainException>(() => Tenant.Create("", "Tenant"));
        Assert.Equal(DomainErrors.Tenant.InvalidIdentifier, ex.Error);
    }

    [Fact]
    public void Rename_ShouldThrow_WhenNameTooShort()
    {
        var tenant = Tenant.Create("tenant-002", "Tenant Two");
        var ex = Assert.Throws<DomainException>(() => tenant.Rename("ab"));
        Assert.Equal(DomainErrors.Tenant.InvalidName, ex.Error);
    }
}
