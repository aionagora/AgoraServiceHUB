namespace AgoraHub360.ERP.Api.Authorization.Handlers;

using AgoraHub360.ERP.Api.Authorization.Requirements;
using AgoraHub360.ERP.Domain.Entities.Core;
using AgoraHub360.ERP.Domain.Interfaces;
using AgoraHub360.ERP.Shared.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

public sealed class BranchAccessHandler : AuthorizationHandler<BranchAccessRequirement>
{
    private readonly IRepository<UsuarioEmpresa> _usuarioEmpresaRepository;
    private readonly IRepository<Empresa> _empresaRepository;
    private readonly IRepository<UsuarioSucursalAcceso> _usuarioSucursalAccesoRepository;
    private readonly IRepository<Sucursal> _sucursalRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public BranchAccessHandler(
        IRepository<UsuarioEmpresa> usuarioEmpresaRepository,
        IRepository<Empresa> empresaRepository,
        IRepository<UsuarioSucursalAcceso> usuarioSucursalAccesoRepository,
        IRepository<Sucursal> sucursalRepository,
        IHttpContextAccessor httpContextAccessor)
    {
        _usuarioEmpresaRepository = usuarioEmpresaRepository;
        _empresaRepository = empresaRepository;
        _usuarioSucursalAccesoRepository = usuarioSucursalAccesoRepository;
        _sucursalRepository = sucursalRepository;
        _httpContextAccessor = httpContextAccessor;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, BranchAccessRequirement requirement)
    {
        if (context.User.Identity?.IsAuthenticated != true)
            return;

        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out var userId) || userId <= 0)
            return;

        var tenantIdClaim = context.User.FindFirst(ClaimTypesCustom.TenantId)?.Value
                            ?? context.User.FindFirst(ClaimTypesCustom.EmpresaId)?.Value;
        if (!int.TryParse(tenantIdClaim, out var tenantId) || tenantId <= 0)
            return;

        var membership = await _usuarioEmpresaRepository.FindAsync(
            ue => ue.UsuarioId == userId && ue.EmpresaId == tenantId);
        if (membership.Count == 0)
            return;

        var empresa = await _empresaRepository.GetByIdAsync(tenantId);
        if (empresa is null || !empresa.Activo)
            return;

        var sucursalId = await TryResolveSucursalIdAsync();
        if (!sucursalId.HasValue || sucursalId.Value <= 0)
            return;

        var sucursal = await _sucursalRepository.GetByIdAsync(sucursalId.Value);
        if (sucursal is null || sucursal.EmpresaId != tenantId)
            return;

        var accesos = await _usuarioSucursalAccesoRepository.FindAsync(
            x => x.UsuarioId == userId &&
                 x.EmpresaId == tenantId &&
                 x.SucursalId == sucursalId.Value,
            CancellationToken.None);

        var acceso = accesos.FirstOrDefault();
        if (acceso is null)
            return;

        var autorizado = requirement.CanOperate
            ? acceso.PuedeOperar
            : (acceso.PuedeConsultar || acceso.PuedeOperar);

        if (autorizado)
            context.Succeed(requirement);
    }

    private async Task<int?> TryResolveSucursalIdAsync()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext is null)
            return null;

        static int? ParseRouteValue(IReadOnlyDictionary<string, object?> values, string key)
        {
            if (!values.TryGetValue(key, out var raw) || raw is null)
                return null;

            var value = raw.ToString();
            return int.TryParse(value, out var parsed) ? parsed : null;
        }

        var routeValues = httpContext.Request.RouteValues;

        var candidate = ParseRouteValue(routeValues, "sucursalId")
                        ?? ParseRouteValue(routeValues, "SucursalId");
        if (candidate.HasValue)
            return candidate;

        var idCandidate = ParseRouteValue(routeValues, "id");
        if (!idCandidate.HasValue)
            return null;

        var isSucursalEndpoint = httpContext.Request.Path.Value?
            .Contains("/sucursales", StringComparison.OrdinalIgnoreCase) == true;

        if (!isSucursalEndpoint)
            return null;

        var sucursal = await _sucursalRepository.GetByIdAsync(idCandidate.Value);
        return sucursal?.Id;
    }
}
