namespace AgoraHub360.ERP.Api.Authorization.Handlers;

using AgoraHub360.ERP.Api.Authorization.Requirements;
using AgoraHub360.ERP.Domain.Entities.Core;
using AgoraHub360.ERP.Domain.Interfaces;
using AgoraHub360.ERP.Shared.Constants;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

public sealed class TenantMembershipHandler : AuthorizationHandler<TenantMembershipRequirement>
{
    private readonly IRepository<UsuarioEmpresa> _usuarioEmpresaRepository;
    private readonly IRepository<Empresa> _empresaRepository;

    public TenantMembershipHandler(
        IRepository<UsuarioEmpresa> usuarioEmpresaRepository,
        IRepository<Empresa> empresaRepository)
    {
        _usuarioEmpresaRepository = usuarioEmpresaRepository;
        _empresaRepository = empresaRepository;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, TenantMembershipRequirement requirement)
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

        var memberships = await _usuarioEmpresaRepository.FindAsync(
            ue => ue.UsuarioId == userId && ue.EmpresaId == tenantId);

        if (memberships.Count == 0)
            return;

        // Nota Fase 3A: UsuarioEmpresa aún no expone columna Activo en el modelo.
        // Se valida pertenencia efectiva por existencia de asignación + Empresa activa.
        var empresa = await _empresaRepository.GetByIdAsync(tenantId);
        if (empresa is null || !empresa.Activo)
            return;

        context.Succeed(requirement);
    }
}
