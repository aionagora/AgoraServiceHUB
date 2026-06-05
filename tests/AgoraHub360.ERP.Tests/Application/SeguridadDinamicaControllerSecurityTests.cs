namespace AgoraHub360.ERP.Tests.Application;

using AgoraHub360.ERP.Api.Controllers.V1;
using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Shared.Constants;
using AgoraHub360.ERP.Shared.DTOs;
using AgoraHub360.ERP.Shared.DTOs.Seguridad;
using Microsoft.AspNetCore.Mvc;

public class SeguridadDinamicaControllerSecurityTests
{
    [Fact]
    public async Task ContextoSesion_RequiereTenantSeleccionado()
    {
        var sut = BuildController(
            service: new FakeSeguridadDinamicaService(),
            currentUser: new FakeCurrentUserService
            {
                TenantStatus = AgoraHub360.ERP.Shared.Constants.TenantStatus.NotSelected,
                TenantRole = Roles.AdminEmpresa,
                TenantId = null,
                EmpresaId = null
            });

        var result = await sut.GetContextoSesion(CancellationToken.None);

        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task Operador_NoPuedeAdministrarPerfilesPermisos()
    {
        var sut = BuildController(
            service: new FakeSeguridadDinamicaService(),
            currentUser: new FakeCurrentUserService
            {
                TenantStatus = AgoraHub360.ERP.Shared.Constants.TenantStatus.Selected,
                TenantRole = Roles.Operador,
                TenantId = 1,
                EmpresaId = 1
            });

        var result = await sut.CreatePerfil(new UpsertPerfilAccesoDto
        {
            Codigo = "OP",
            Nombre = "Operador",
            Activo = true
        }, CancellationToken.None);

        Assert.IsType<ForbidResult>(result);
    }

    private static SeguridadDinamicaController BuildController(FakeSeguridadDinamicaService service, FakeCurrentUserService currentUser)
        => new(service, currentUser);

    private sealed class FakeCurrentUserService : ICurrentUserService
    {
        public string? UserId { get; set; }
        public int? UserIdInt { get; set; }
        public string? UserName { get; set; }
        public int? EmpresaId { get; set; }
        public int? TenantId { get; set; }
        public string PlatformRole { get; set; } = Roles.None;
        public string TenantRole { get; set; } = Roles.NoAccess;
        public string TenantStatus { get; set; } = AgoraHub360.ERP.Shared.Constants.TenantStatus.NotSelected;
        public bool IsInRole(string role) => false;
    }

    private sealed class FakeSeguridadDinamicaService : ISeguridadDinamicaService
    {
        public Task<Result<SesionContextoDto>> GetSesionContextAsync(int? usuarioId = null, int? empresaId = null, CancellationToken ct = default)
            => Task.FromResult(Result<SesionContextoDto>.Success(new SesionContextoDto()));

        public Task<Result<IReadOnlyList<ModuloSistemaDto>>> GetMenuUsuarioAsync(int? usuarioId = null, int? empresaId = null, CancellationToken ct = default)
            => Task.FromResult(Result<IReadOnlyList<ModuloSistemaDto>>.Success(Array.Empty<ModuloSistemaDto>()));

        public Task<Result<bool>> HasPermissionAsync(string formularioCodigo, string? accionCodigo = null, int? usuarioId = null, int? empresaId = null, CancellationToken ct = default)
            => Task.FromResult(Result<bool>.Success(true));

        public Task<Result<PerfilAccesoDto>> CreatePerfilAsync(UpsertPerfilAccesoDto dto, CancellationToken ct = default)
            => Task.FromResult(Result<PerfilAccesoDto>.Success(new PerfilAccesoDto()));

        public Task<Result<bool>> SetPermisoPerfilAsync(UpsertPerfilPermisoDto dto, CancellationToken ct = default)
            => Task.FromResult(Result<bool>.Success(true));

        public Task<Result<bool>> AsignarPerfilUsuarioAsync(AsignarUsuarioPerfilDto dto, CancellationToken ct = default)
            => Task.FromResult(Result<bool>.Success(true));
    }
}
