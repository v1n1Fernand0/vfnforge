using System.Threading;
using VFNForge.SaaS.Contracts.Tenancy;

namespace VFNForge.SaaS.Infrastructure.Tenancy;

public sealed class TenantContextAccessor : ITenantAccessor
{
    private static readonly AsyncLocal<TenantInfo?> Current = new();

    public TenantInfo? CurrentTenant
    {
        get => Current.Value;
        set => Current.Value = value;
    }
}

public sealed class TenantProvider : ITenantProvider
{
    private readonly ITenantAccessor _accessor;

    public TenantProvider(ITenantAccessor accessor)
    {
        _accessor = accessor;
    }

    public TenantInfo? Current => _accessor.CurrentTenant;
}
