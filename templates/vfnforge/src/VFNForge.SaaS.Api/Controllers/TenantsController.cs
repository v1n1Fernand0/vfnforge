using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VFNForge.SaaS.Application.Tenants.Commands;
using VFNForge.SaaS.Application.Tenants.Queries;
using VFNForge.SaaS.Contracts.Tenants;
using VFNForge.SaaS.Domain.Common;

namespace VFNForge.SaaS.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class TenantsController : ControllerBase
{
    private readonly ITenantQueryService _queryService;
    private readonly ITenantCommandService _commandService;

    public TenantsController(ITenantQueryService queryService, ITenantCommandService commandService)
    {
        _queryService = queryService;
        _commandService = commandService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<TenantSummary>>> Get(CancellationToken cancellationToken)
    {
        var tenants = await _queryService.ListAsync(cancellationToken);
        return Ok(tenants);
    }

    [HttpGet("{tenantId}")]
    public async Task<ActionResult<TenantSummary>> GetById(string tenantId, CancellationToken cancellationToken)
    {
        var tenant = await _queryService.GetAsync(tenantId, cancellationToken);
        if (tenant is null)
        {
            return NotFound();
        }

        return Ok(tenant);
    }

    [HttpPost]
    public async Task<ActionResult<TenantSummary>> Create([FromBody] TenantCreateRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var tenant = await _commandService.CreateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { tenantId = tenant.Id }, tenant);
        }
        catch (DomainException ex)
        {
            return MapError(ex);
        }
    }

    [HttpPut("{tenantId}")]
    public async Task<ActionResult<TenantSummary>> Rename(string tenantId, [FromBody] TenantRenameRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var tenant = await _commandService.RenameAsync(tenantId, request, cancellationToken);
            return Ok(tenant);
        }
        catch (DomainException ex)
        {
            return MapError(ex);
        }
    }

    [HttpPost("{tenantId}/activate")]
    public async Task<ActionResult<TenantSummary>> Activate(string tenantId, CancellationToken cancellationToken)
    {
        try
        {
            var tenant = await _commandService.ActivateAsync(tenantId, cancellationToken);
            return Ok(tenant);
        }
        catch (DomainException ex)
        {
            return MapError(ex);
        }
    }

    [HttpPost("{tenantId}/deactivate")]
    public async Task<ActionResult<TenantSummary>> Deactivate(string tenantId, CancellationToken cancellationToken)
    {
        try
        {
            var tenant = await _commandService.DeactivateAsync(tenantId, cancellationToken);
            return Ok(tenant);
        }
        catch (DomainException ex)
        {
            return MapError(ex);
        }
    }

    private ActionResult MapError(DomainException exception)
    {
        return exception.Error.Code switch
        {
            "Tenant.AlreadyExists" => Conflict(new { exception.Error.Code, exception.Error.Message }),
            "Tenant.NotFound" => NotFound(new { exception.Error.Code, exception.Error.Message }),
            _ => BadRequest(new { exception.Error.Code, exception.Error.Message })
        };
    }
}
