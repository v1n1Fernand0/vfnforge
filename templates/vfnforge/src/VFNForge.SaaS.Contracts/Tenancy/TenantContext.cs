namespace VFNForge.SaaS.Contracts.Tenancy;

public sealed record TenantInfo(string Id, string? Name);

public interface ITenantAccessor
{
    TenantInfo? CurrentTenant { get; set; }
}

public interface ITenantProvider
{
    TenantInfo? Current { get; }

    bool HasTenant => Current is not null;
}
