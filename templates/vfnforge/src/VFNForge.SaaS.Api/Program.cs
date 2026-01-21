using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using VFNForge.SaaS.Api.Extensions;
using VFNForge.SaaS.Api.Middleware;
using VFNForge.SaaS.Application.Tenants;
using VFNForge.SaaS.Contracts.Auth;
using VFNForge.SaaS.Contracts.Tenancy;
using VFNForge.SaaS.Domain.Tenants;
using VFNForge.SaaS.Infrastructure.Tenancy;
using VFNForge.SaaS.Infrastructure.Tenants;
using VFNForge.SaaS.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));
builder.Services.Configure<TenancyOptions>(builder.Configuration.GetSection(TenancyOptions.SectionName));

builder.Services.AddSingleton<ITenantAccessor, TenantContextAccessor>();
builder.Services.AddScoped<ITenantProvider, TenantProvider>();
builder.Services.AddScoped<TenantRequirementFilter>();
builder.Services.AddSingleton<ITenantRepository, InMemoryTenantRepository>();
builder.Services.AddScoped<ITenantQueryService, TenantQueryService>();
builder.Services.AddInfrastructureData(builder.Configuration);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwt = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();
        options.RequireHttpsMetadata = jwt.RequireHttpsMetadata;

        if (!string.IsNullOrWhiteSpace(jwt.Authority))
        {
            options.Authority = jwt.Authority;
            options.Audience = jwt.Audience;
        }
        else
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwt.Issuer,
                ValidateAudience = true,
                ValidAudience = jwt.Audience,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey)),
                ValidateLifetime = jwt.ValidateLifetime
            };
        }
    });

builder.Services.AddAuthorization();
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<TenantResolutionMiddleware>();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

var api = app.MapSecuredApi("/api");
api.MapGet("/weatherforecast", () =>
    {
        var forecast = Enumerable.Range(1, 5).Select(index =>
                new WeatherForecast(
                    DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    Random.Shared.Next(-20, 55),
                    summaries[Random.Shared.Next(summaries.Length)]
                ))
            .ToArray();
        return forecast;
    })
    .WithName("GetWeatherForecast")
    .WithDescription("Exemplo protegido por JWT + Tenant. Atualize appsettings e use bearer token.");

api.MapGet("/tenants", async (ITenantQueryService service, CancellationToken cancellationToken) =>
    Results.Ok(await service.ListAsync(cancellationToken)))
    .WithName("GetTenants")
    .WithSummary("Lista os tenants configurados (exemplo completo de fluxo contrato/aplicacao/infrastructure).");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
