using System.Security.Claims;
using Microsoft.Extensions.Options;
using VFNForge.SaaS.Contracts.Tenancy;

namespace VFNForge.SaaS.Api.Middleware;

public sealed class TenantResolutionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ITenantAccessor _tenantAccessor;
    private readonly TenancyOptions _options;
    private readonly ILogger<TenantResolutionMiddleware> _logger;

    public TenantResolutionMiddleware(
        RequestDelegate next,
        ITenantAccessor tenantAccessor,
        IOptions<TenancyOptions> options,
        ILogger<TenantResolutionMiddleware> logger)
    {
        _next = next;
        _tenantAccessor = tenantAccessor;
        _options = options.Value;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var tenantId = ResolveFromHeader(context) ?? ResolveFromClaim(context.User) ?? _options.DefaultTenantId;

        if (string.IsNullOrWhiteSpace(tenantId))
        {
            _logger.LogWarning("Tenant nao encontrado.");
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsJsonAsync(new { error = "Tenant nao informado." });
            return;
        }

        if (_options.RequireKnownTenant && !_options.Tenants.Any(t => string.Equals(t.Id, tenantId, StringComparison.OrdinalIgnoreCase)))
        {
            _logger.LogWarning("Tenant {TenantId} nao cadastrado.", tenantId);
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsJsonAsync(new { error = "Tenant nao autorizado." });
            return;
        }

        var tenantDefinition = _options.Tenants.FirstOrDefault(t => string.Equals(t.Id, tenantId, StringComparison.OrdinalIgnoreCase));
        _tenantAccessor.CurrentTenant = new TenantInfo(tenantId, tenantDefinition?.Name ?? tenantId);

        await _next(context);
    }

    private string? ResolveFromHeader(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue(_options.HeaderName, out var header) && !string.IsNullOrWhiteSpace(header))
        {
            return header.ToString();
        }

        return null;
    }

    private string? ResolveFromClaim(ClaimsPrincipal user)
    {
        return user?.FindFirst(_options.ClaimName)?.Value;
    }
}

public sealed class TenantRequirementFilter : IEndpointFilter
{
    private readonly ITenantProvider _tenantProvider;

    public TenantRequirementFilter(ITenantProvider tenantProvider)
    {
        _tenantProvider = tenantProvider;
    }

    public ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        if (!_tenantProvider.HasTenant)
        {
            return ValueTask.FromResult<object?>(Results.Problem("Tenant nao resolvido para esta requisicao.", statusCode: StatusCodes.Status400BadRequest));
        }

        return next(context);
    }
}
