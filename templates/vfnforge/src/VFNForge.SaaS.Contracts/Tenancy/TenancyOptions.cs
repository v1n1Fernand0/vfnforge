using System.Collections.ObjectModel;

namespace VFNForge.SaaS.Contracts.Tenancy;

public sealed class TenancyOptions
{
    public const string SectionName = "Tenancy";

    public string HeaderName { get; set; } = "X-Tenant-ID";

    public string ClaimName { get; set; } = "tenant_id";

    public string DefaultTenantId { get; set; } = "tenant-default";

    public bool RequireKnownTenant { get; set; } = true;

    public Collection<TenantDefinition> Tenants { get; init; } = new();
}

public sealed class TenantDefinition
{
    public string Id { get; set; } = string.Empty;

    public string? Name { get; set; }
}
