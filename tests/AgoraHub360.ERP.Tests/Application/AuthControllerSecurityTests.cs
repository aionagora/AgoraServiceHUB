namespace AgoraHub360.ERP.Tests.Application;

using System.Security.Claims;
using AgoraHub360.ERP.Api.Controllers.V1;
using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Shared.Constants;
using AgoraHub360.ERP.Shared.DTOs;
using AgoraHub360.ERP.Shared.DTOs.Auth;
using AgoraHub360.ERP.Shared.DTOs.Empresa;
using AgoraHub360.ERP.Shared.DTOs.Usuario;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

public class AuthControllerSecurityTests
{
    [Fact]
    public async Task MisEmpresas_AdminEmpresa_NoDebeObtenerTodas()
    {
        var auth = new FakeAuthService();
        var usuario = new FakeUsuarioService
        {
            EmpresasAsignadas = new[]
            {
                new UsuarioEmpresaRolDto { EmpresaId = 1, EmpresaNombre = "Empresa A", Rol = Roles.AdminEmpresa }
            }
        };
        var empresa = new FakeEmpresaService
        {
            Empresas = new[]
            {
                new EmpresaDto { Id = 1, Nombre = "Empresa A", Activo = true },
                new EmpresaDto { Id = 2, Nombre = "Empresa B", Activo = true }
            }
        };

        var sut = BuildController(auth, usuario, empresa, userId: 10, platformRole: Roles.None, tenantRole: Roles.AdminEmpresa);

        var result = await sut.MisEmpresas(CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<IReadOnlyList<EmpresaDto>>>(ok.Value);
        Assert.True(response.Success);
        Assert.Single(response.Data!);
        Assert.Equal(1, response.Data![0].Id);
    }

    [Fact]
    public async Task MisEmpresas_SystemAdmin_DebePoderVerTodasActivas()
    {
        var auth = new FakeAuthService();
        var usuario = new FakeUsuarioService
        {
            EmpresasAsignadas = Array.Empty<UsuarioEmpresaRolDto>()
        };
        var empresa = new FakeEmpresaService
        {
            Empresas = new[]
            {
                new EmpresaDto { Id = 1, Nombre = "Empresa A", Activo = true },
                new EmpresaDto { Id = 2, Nombre = "Empresa B", Activo = false },
                new EmpresaDto { Id = 3, Nombre = "Empresa C", Activo = true }
            }
        };

        var sut = BuildController(auth, usuario, empresa, userId: 99, platformRole: Roles.SystemAdmin, tenantRole: Roles.NoAccess);

        var result = await sut.MisEmpresas(CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<IReadOnlyList<EmpresaDto>>>(ok.Value);
        Assert.True(response.Success);
        Assert.Equal(2, response.Data!.Count);
        Assert.All(response.Data!, e => Assert.True(e.Activo));
    }

    [Fact]
    public async Task SeleccionarEmpresa_UsuarioEmpresaA_NoPuedeSeleccionarEmpresaB()
    {
        var auth = new FakeAuthService
        {
            CambiarEmpresaResult = Result<CambiarEmpresaResponseDto>.Failure("La empresa seleccionada no pertenece al usuario.")
        };
        var usuario = new FakeUsuarioService();
        var empresa = new FakeEmpresaService();
        var sut = BuildController(auth, usuario, empresa, userId: 7, platformRole: Roles.None, tenantRole: Roles.Operador);

        var result = await sut.SeleccionarEmpresa(new SeleccionarEmpresaRequestDto { EmpresaId = 2 }, CancellationToken.None);

        var bad = Assert.IsType<BadRequestObjectResult>(result);
        var response = Assert.IsType<ApiResponse<CambiarEmpresaResponseDto>>(bad.Value);
        Assert.False(response.Success);
        Assert.Contains("no pertenece", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task SeleccionarEmpresa_UsuarioConAsignacion_PuedeSeleccionarla()
    {
        var auth = new FakeAuthService
        {
            CambiarEmpresaResult = Result<CambiarEmpresaResponseDto>.Success(new CambiarEmpresaResponseDto
            {
                Token = "jwt",
                PlatformRole = Roles.None,
                TenantId = 1,
                TenantRole = Roles.AdminEmpresa,
                TenantStatus = AgoraHub360.ERP.Shared.Constants.TenantStatus.Selected,
                EmpresaActiva = new EmpresaSesionDto { Id = 1, Nombre = "Empresa A", Rol = Roles.AdminEmpresa, EsActiva = true }
            })
        };
        var usuario = new FakeUsuarioService();
        var empresa = new FakeEmpresaService();
        var sut = BuildController(auth, usuario, empresa, userId: 7, platformRole: Roles.None, tenantRole: Roles.AdminEmpresa);

        var result = await sut.SeleccionarEmpresa(new SeleccionarEmpresaRequestDto { EmpresaId = 1 }, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<CambiarEmpresaResponseDto>>(ok.Value);
        Assert.True(response.Success);
        Assert.Equal(1, response.Data!.TenantId);
        Assert.Equal(AgoraHub360.ERP.Shared.Constants.TenantStatus.Selected, response.Data.TenantStatus);
    }

    private static AuthController BuildController(
        IAuthService authService,
        IUsuarioService usuarioService,
        IEmpresaService empresaService,
        int userId,
        string platformRole,
        string tenantRole)
    {
        var controller = new AuthController(authService, usuarioService, empresaService);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Name, "tester"),
            new(ClaimTypes.Email, "tester@local"),
            new(ClaimTypes.Role, tenantRole),
            new(ClaimTypesCustom.PlatformRole, platformRole),
            new(ClaimTypesCustom.TenantRole, tenantRole)
        };

        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = principal
            }
        };

        return controller;
    }

    private sealed class FakeAuthService : IAuthService
    {
        public Result<CambiarEmpresaResponseDto> CambiarEmpresaResult { get; set; }
            = Result<CambiarEmpresaResponseDto>.Failure("No configurado");

        public Task<AuthResponseDto?> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default)
            => Task.FromResult<AuthResponseDto?>(null);

        public Task<AuthResponseDto?> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
            => Task.FromResult<AuthResponseDto?>(null);

        public Task<Result<CambiarEmpresaResponseDto>> CambiarEmpresaActivaAsync(int usuarioId, int empresaId, CancellationToken cancellationToken = default)
            => Task.FromResult(CambiarEmpresaResult);
    }

    private sealed class FakeUsuarioService : IUsuarioService
    {
        public IReadOnlyList<UsuarioEmpresaRolDto> EmpresasAsignadas { get; set; } = Array.Empty<UsuarioEmpresaRolDto>();

        public Task<Result<IReadOnlyList<UsuarioDto>>> GetAllAsync(CancellationToken ct = default) => throw new NotImplementedException();
        public Task<Result<UsuarioDto>> GetByIdAsync(int id, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<Result<UsuarioDto>> CreateAsync(CreateUsuarioDto dto, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<Result<UsuarioDto>> UpdateAsync(int id, UpdateUsuarioDto dto, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<Result<bool>> DeleteAsync(int id, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<Result<bool>> AsignarRolAsync(int usuarioId, AsignarRolDto dto, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<Result<bool>> RemoverDeEmpresaAsync(int usuarioId, int empresaId, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<Result<bool>> ResetPasswordAsync(int usuarioId, ResetPasswordDto dto, CancellationToken ct = default) => throw new NotImplementedException();

        public Task<Result<IReadOnlyList<UsuarioEmpresaRolDto>>> GetEmpresasAsignadasAsync(int usuarioId, CancellationToken ct = default)
            => Task.FromResult(Result<IReadOnlyList<UsuarioEmpresaRolDto>>.Success(EmpresasAsignadas));
    }

    private sealed class FakeEmpresaService : IEmpresaService
    {
        public IReadOnlyList<EmpresaDto> Empresas { get; set; } = Array.Empty<EmpresaDto>();

        public Task<Result<IReadOnlyList<EmpresaDto>>> GetAllAsync(CancellationToken ct = default)
            => Task.FromResult(Result<IReadOnlyList<EmpresaDto>>.Success(Empresas));

        public Task<Result<EmpresaDto>> GetByIdAsync(int id, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<Result<EmpresaDto>> CreateAsync(CreateEmpresaDto dto, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<Result<EmpresaDto>> UpdateAsync(int id, UpdateEmpresaDto dto, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<Result<bool>> DeleteAsync(int id, CancellationToken ct = default) => throw new NotImplementedException();
    }
}
