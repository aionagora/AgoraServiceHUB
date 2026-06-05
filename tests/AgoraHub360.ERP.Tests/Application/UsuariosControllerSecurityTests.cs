namespace AgoraHub360.ERP.Tests.Application;

using AgoraHub360.ERP.Api.Controllers.V1;
using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Shared.Constants;
using AgoraHub360.ERP.Shared.DTOs;
using AgoraHub360.ERP.Shared.DTOs.Usuario;
using Microsoft.AspNetCore.Mvc;

public class UsuariosControllerSecurityTests
{
    [Fact]
    public async Task GetAll_AdminEmpresa_SoloVeUsuariosDeSuTenant()
    {
        var sut = BuildController(
            currentUser: new FakeCurrentUserService
            {
                UserIdInt = 10,
                PlatformRole = Roles.None,
                TenantRole = Roles.AdminEmpresa,
                TenantStatus = AgoraHub360.ERP.Shared.Constants.TenantStatus.Selected,
                TenantId = 1,
                EmpresaId = 1
            },
            usuarioService: new FakeUsuarioService
            {
                Usuarios = new[]
                {
                    BuildUsuario(1, "u1", (1, Roles.AdminEmpresa)),
                    BuildUsuario(2, "u2", (2, Roles.Operador)),
                    BuildUsuario(3, "u3", (1, Roles.Viewer))
                }
            });

        var result = await sut.GetAll(CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<IReadOnlyList<UsuarioDto>>>(ok.Value);
        Assert.True(response.Success);
        Assert.Equal(2, response.Data!.Count);
        Assert.All(response.Data!, u => Assert.Contains(u.EmpresasAsignadas, e => e.EmpresaId == 1));
    }

    [Fact]
    public async Task Create_AdminEmpresa_NoPuedeCrearUsuarioEnEmpresaAjena()
    {
        var sut = BuildController(
            currentUser: new FakeCurrentUserService
            {
                UserIdInt = 10,
                PlatformRole = Roles.None,
                TenantRole = Roles.AdminEmpresa,
                TenantStatus = AgoraHub360.ERP.Shared.Constants.TenantStatus.Selected,
                TenantId = 1,
                EmpresaId = 1
            },
            usuarioService: new FakeUsuarioService());

        var dto = new CreateUsuarioDto
        {
            NombreUsuario = "nuevo",
            Email = "nuevo@local",
            Password = "123456",
            NombreCompleto = "Nuevo Usuario",
            EmpresaId = 2,
            Rol = Roles.Operador
        };

        var result = await sut.Create(dto, CancellationToken.None);

        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task AsignarRol_AdminEmpresa_NoPuedeAsignarPlatformRole()
    {
        var sut = BuildController(
            currentUser: new FakeCurrentUserService
            {
                UserIdInt = 10,
                PlatformRole = Roles.None,
                TenantRole = Roles.AdminEmpresa,
                TenantStatus = AgoraHub360.ERP.Shared.Constants.TenantStatus.Selected,
                TenantId = 1,
                EmpresaId = 1
            },
            usuarioService: new FakeUsuarioService());

        var result = await sut.AsignarRol(20, new AsignarRolDto { EmpresaId = 1, Rol = Roles.SystemAdmin }, CancellationToken.None);

        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task GetAll_PlatformAdmin_ConservaAccesoGlobal()
    {
        var sut = BuildController(
            currentUser: new FakeCurrentUserService
            {
                UserIdInt = 1,
                PlatformRole = Roles.SystemAdmin,
                TenantRole = Roles.NoAccess,
                TenantStatus = AgoraHub360.ERP.Shared.Constants.TenantStatus.NotSelected
            },
            usuarioService: new FakeUsuarioService
            {
                Usuarios = new[]
                {
                    BuildUsuario(1, "u1", (1, Roles.AdminEmpresa)),
                    BuildUsuario(2, "u2", (2, Roles.Operador))
                }
            });

        var result = await sut.GetAll(CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<IReadOnlyList<UsuarioDto>>>(ok.Value);
        Assert.True(response.Success);
        Assert.Equal(2, response.Data!.Count);
    }

    [Fact]
    public async Task GetAll_Operador_NoPuedeListarUsuarios()
    {
        var sut = BuildController(
            currentUser: new FakeCurrentUserService
            {
                UserIdInt = 30,
                PlatformRole = Roles.None,
                TenantRole = Roles.Operador,
                TenantStatus = AgoraHub360.ERP.Shared.Constants.TenantStatus.Selected,
                TenantId = 1,
                EmpresaId = 1
            },
            usuarioService: new FakeUsuarioService
            {
                Usuarios = new[] { BuildUsuario(1, "u1", (1, Roles.AdminEmpresa)) }
            });

        var result = await sut.GetAll(CancellationToken.None);

        Assert.IsType<ForbidResult>(result);
    }

    private static UsuariosController BuildController(FakeCurrentUserService currentUser, FakeUsuarioService usuarioService)
        => new(usuarioService, currentUser);

    private static UsuarioDto BuildUsuario(int id, string username, params (int empresaId, string rol)[] empresas)
        => new()
        {
            Id = id,
            NombreUsuario = username,
            Email = $"{username}@local",
            NombreCompleto = username,
            Activo = true,
            EmpresasAsignadas = empresas.Select(e => new UsuarioEmpresaRolDto
            {
                EmpresaId = e.empresaId,
                EmpresaNombre = $"Empresa {e.empresaId}",
                Rol = e.rol
            }).ToList()
        };

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

    private sealed class FakeUsuarioService : IUsuarioService
    {
        public IReadOnlyList<UsuarioDto> Usuarios { get; set; } = Array.Empty<UsuarioDto>();

        public Task<Result<IReadOnlyList<UsuarioDto>>> GetAllAsync(CancellationToken ct = default)
            => Task.FromResult(Result<IReadOnlyList<UsuarioDto>>.Success(Usuarios));

        public Task<Result<UsuarioDto>> GetByIdAsync(int id, CancellationToken ct = default)
        {
            var user = Usuarios.FirstOrDefault(u => u.Id == id);
            return user is null
                ? Task.FromResult(Result<UsuarioDto>.Failure($"Usuario con Id {id} no encontrado."))
                : Task.FromResult(Result<UsuarioDto>.Success(user));
        }

        public Task<Result<UsuarioDto>> CreateAsync(CreateUsuarioDto dto, CancellationToken ct = default)
            => Task.FromResult(Result<UsuarioDto>.Success(BuildUsuario(999, dto.NombreUsuario, (dto.EmpresaId ?? 0, dto.Rol ?? Roles.Viewer))));

        public Task<Result<UsuarioDto>> UpdateAsync(int id, UpdateUsuarioDto dto, CancellationToken ct = default)
            => Task.FromResult(Result<UsuarioDto>.Success(BuildUsuario(id, dto.NombreUsuario, (1, Roles.AdminEmpresa))));

        public Task<Result<bool>> DeleteAsync(int id, CancellationToken ct = default)
            => Task.FromResult(Result<bool>.Success(true));

        public Task<Result<bool>> AsignarRolAsync(int usuarioId, AsignarRolDto dto, CancellationToken ct = default)
            => Task.FromResult(Result<bool>.Success(true));

        public Task<Result<bool>> RemoverDeEmpresaAsync(int usuarioId, int empresaId, CancellationToken ct = default)
            => Task.FromResult(Result<bool>.Success(true));

        public Task<Result<IReadOnlyList<UsuarioEmpresaRolDto>>> GetEmpresasAsignadasAsync(int usuarioId, CancellationToken ct = default)
            => Task.FromResult(Result<IReadOnlyList<UsuarioEmpresaRolDto>>.Success(Array.Empty<UsuarioEmpresaRolDto>()));

        public Task<Result<bool>> ResetPasswordAsync(int usuarioId, ResetPasswordDto dto, CancellationToken ct = default)
            => Task.FromResult(Result<bool>.Success(true));
    }
}
