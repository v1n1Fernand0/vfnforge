using VFNForge.SaaS.Domain.Abstractions;

namespace VFNForge.SaaS.Domain.Tenants;

public sealed class Tenant
{
    private Tenant(string id, string name, bool isActive)
    {
        Id = id;
        Name = name;
        IsActive = isActive;
    }

    public string Id { get; }
    public string Name { get; private set; }
    public bool IsActive { get; private set; }

    public static Tenant Create(string id, string name, bool isActive = true)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new DomainException(DomainErrors.Tenant.InvalidIdentifier);
        }

        if (string.IsNullOrWhiteSpace(name) || name.Length < 3)
        {
            throw new DomainException(DomainErrors.Tenant.InvalidName);
        }

        return new Tenant(id, name.Trim(), isActive);
    }

    public void Rename(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName) || newName.Length < 3)
        {
            throw new DomainException(DomainErrors.Tenant.InvalidName);
        }

        Name = newName.Trim();
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
}
