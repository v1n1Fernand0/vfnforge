using VFNForge.SaaS.Api.Middleware;

namespace VFNForge.SaaS.Api.Extensions;

public static class EndpointRouteBuilderExtensions
{
    public static RouteGroupBuilder MapSecuredApi(this IEndpointRouteBuilder app, string routePrefix = "/api")
    {
        return app.MapGroup(routePrefix)
            .RequireAuthorization()
            .AddEndpointFilter<TenantRequirementFilter>();
    }
}
