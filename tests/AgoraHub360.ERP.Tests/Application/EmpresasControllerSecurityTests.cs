namespace AgoraHub360.ERP.Tests.Application;

using AgoraHub360.ERP.Api.Controllers.V1;
using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Shared.Constants;
using AgoraHub360.ERP.Shared.DTOs;
using AgoraHub360.ERP.Shared.DTOs.Empresa;
using AgoraHub360.ERP.Shared.DTOs.Usuario;
using Microsoft.AspNetCore.Mvc;

public class EmpresasControllerSecurityTests
{
    [Fact]
    public async Task GetAll_AdminEmpresa_NoPuedeGlobal()
    {
        var sut = BuildController(
            currentUser: new FakeCurrentUserService
            {
                UserIdInt = 10,
                PlatformRole = Roles.None,
                TenantRole = Roles.AdminEmpresa,
                TenantStatus = TenantStatus.Selected,
                TenantId = 1,
                EmpresaId = 1
            },
            empresaService: new FakeEmpresaService
            {
                Empresas = new[]
                {
                    new EmpresaDto { Id = 1, Nombre = "A", Activo = true },
                    new EmpresaDto { Id = 2, Nombre = "B", Activo = true }
                }
            },
            usuarioService: new FakeUsuarioService());

        var result = await sut.GetAll(CancellationToken.None);

        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task CrearEmpresaDemoCompleta_SystemAdmin_SiPuede()
    {
        var sut = BuildController(
            currentUser: new FakeCurrentUserService
            {
                UserIdInt = 1,
                PlatformRole = Roles.SystemAdmin,
                TenantRole = Roles.NoAccess,
                TenantStatus = TenantStatus.NotSelected
            },
            empresaService: new FakeEmpresaService(),
            usuarioService: new FakeUsuarioService());

        var result = await sut.CrearEmpresaDemoCompleta(new CrearEmpresaDemoCompletaRequestDto
        {
            CodigoDemo = "DEMO01"
        }, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<CrearEmpresaDemoCompletaResponseDto>>(ok.Value);
        Assert.True(response.Success);
    }

    [Fact]
    public async Task CrearEmpresaDemoCompleta_SuperAdmin_SinTenantSeleccionado_SiPuede()
    {
        var sut = BuildController(
            currentUser: new FakeCurrentUserService
            {
                UserIdInt = 1,
                PlatformRole = Roles.SuperAdmin,
                TenantRole = Roles.NoAccess,
                TenantStatus = TenantStatus.NotSelected,
                TenantId = null,
                EmpresaId = null
            },
            empresaService: new FakeEmpresaService(),
            usuarioService: new FakeUsuarioService());

        var result = await sut.CrearEmpresaDemoCompleta(new CrearEmpresaDemoCompletaRequestDto
        {
            CodigoDemo = "DEMO01"
        }, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<CrearEmpresaDemoCompletaResponseDto>>(ok.Value);
        Assert.True(response.Success);
    }

    [Fact]
    public async Task CreateEmpresa_SuperAdmin_SinTenantSeleccionado_SiPuede()
    {
        var sut = BuildController(
            currentUser: new FakeCurrentUserService
            {
                UserIdInt = 1,
                PlatformRole = Roles.SuperAdmin,
                TenantRole = Roles.NoAccess,
                TenantStatus = TenantStatus.NotSelected,
                TenantId = null,
                EmpresaId = null
            },
            empresaService: new FakeEmpresaService(),
            usuarioService: new FakeUsuarioService());

        var result = await sut.Create(new CreateEmpresaDto
        {
            Nombre = "Empresa Global",
            NIT = "123"
        }, CancellationToken.None);

        var created = Assert.IsType<CreatedAtActionResult>(result);
        var response = Assert.IsType<ApiResponse<EmpresaDto>>(created.Value);
        Assert.True(response.Success);
        Assert.Equal("Empresa Global", response.Data!.Nombre);
    }

    [Fact]
    public async Task CrearEmpresaDemoCompleta_AdminEmpresa_NoPuede()
    {
        var sut = BuildController(
            currentUser: new FakeCurrentUserService
            {
                UserIdInt = 20,
                PlatformRole = Roles.None,
                TenantRole = Roles.AdminEmpresa,
                TenantStatus = TenantStatus.Selected,
                TenantId = 1,
                EmpresaId = 1
            },
            empresaService: new FakeEmpresaService(),
            usuarioService: new FakeUsuarioService());

        var result = await sut.CrearEmpresaDemoCompleta(new CrearEmpresaDemoCompletaRequestDto
        {
            CodigoDemo = "DEMO01"
        }, CancellationToken.None);

        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task GetAll_PlatformAdmin_SiPuedeGlobal()
    {
        var sut = BuildController(
            currentUser: new FakeCurrentUserService
            {
                UserIdInt = 1,
                PlatformRole = Roles.SystemAdmin,
                TenantRole = Roles.NoAccess,
                TenantStatus = TenantStatus.NotSelected
            },
            empresaService: new FakeEmpresaService
            {
                Empresas = new[]
                {
                    new EmpresaDto { Id = 1, Nombre = "A", Activo = true },
                    new EmpresaDto { Id = 2, Nombre = "B", Activo = true }
                }
            },
            usuarioService: new FakeUsuarioService());

        var result = await sut.GetAll(CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<IReadOnlyList<EmpresaDto>>>(ok.Value);
        Assert.True(response.Success);
        Assert.Equal(2, response.Data!.Count);
    }

    [Fact]
    public async Task GetMisEmpresas_AdminEmpresa_SoloAsignadasActivas()
    {
        var sut = BuildController(
            currentUser: new FakeCurrentUserService
            {
                UserIdInt = 20,
                PlatformRole = Roles.None,
                TenantRole = Roles.AdminEmpresa,
                TenantStatus = TenantStatus.Selected,
                TenantId = 1,
                EmpresaId = 1
            },
            empresaService: new FakeEmpresaService
            {
                Empresas = new[]
                {
                    new EmpresaDto { Id = 1, Nombre = "A", Activo = true },
                    new EmpresaDto { Id = 2, Nombre = "B", Activo = false },
                    new EmpresaDto { Id = 3, Nombre = "C", Activo = true }
                }
            },
            usuarioService: new FakeUsuarioService
            {
                Asignaciones = new[]
                {
                    new UsuarioEmpresaRolDto { EmpresaId = 1, EmpresaNombre = "A", Rol = Roles.AdminEmpresa },
                    new UsuarioEmpresaRolDto { EmpresaId = 2, EmpresaNombre = "B", Rol = Roles.AdminEmpresa }
                }
            });

        var result = await sut.GetMisEmpresas(CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<IReadOnlyList<EmpresaDto>>>(ok.Value);
        Assert.True(response.Success);
        Assert.Single(response.Data!);
        Assert.Equal(1, response.Data![0].Id);
    }

    [Fact]
    public async Task GetById_AdminEmpresa_NoPuedeVerEmpresaAjena()
    {
        var sut = BuildController(
            currentUser: new FakeCurrentUserService
            {
                UserIdInt = 20,
                PlatformRole = Roles.None,
                TenantRole = Roles.AdminEmpresa,
                TenantStatus = TenantStatus.Selected,
                TenantId = 1,
                EmpresaId = 1
            },
            empresaService: new FakeEmpresaService
            {
                Empresas = new[]
                {
                    new EmpresaDto { Id = 1, Nombre = "A", Activo = true },
                    new EmpresaDto { Id = 2, Nombre = "B", Activo = true }
                }
            },
            usuarioService: new FakeUsuarioService
            {
                Asignaciones = new[]
                {
                    new UsuarioEmpresaRolDto { EmpresaId = 1, EmpresaNombre = "A", Rol = Roles.AdminEmpresa }
                }
            });

        var result = await sut.GetById(2, CancellationToken.None);

        Assert.IsType<ForbidResult>(result);
    }

    private static EmpresasController BuildController(
        FakeCurrentUserService currentUser,
        FakeEmpresaService empresaService,
        FakeUsuarioService usuarioService)
    {
        return new EmpresasController(
            empresaService,
            new FakeConfiguracionInicialEmpresaService(),
            currentUser,
            usuarioService,
            new FakeEmpresaSeedService(),
            new FakeEmpresaDemoService());
    }

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

    private sealed class FakeEmpresaService : IEmpresaService
    {
        public IReadOnlyList<EmpresaDto> Empresas { get; set; } = Array.Empty<EmpresaDto>();
        private int _nextId = 100;

        public Task<Result<IReadOnlyList<EmpresaDto>>> GetAllAsync(CancellationToken ct = default)
            => Task.FromResult(Result<IReadOnlyList<EmpresaDto>>.Success(Empresas));

        public Task<Result<EmpresaDto>> GetByIdAsync(int id, CancellationToken ct = default)
        {
            var found = Empresas.FirstOrDefault(e => e.Id == id);
            return found is null
                ? Task.FromResult(Result<EmpresaDto>.Failure("Empresa con Id no encontrada."))
                : Task.FromResult(Result<EmpresaDto>.Success(found));
        }

        public Task<Result<EmpresaDto>> CreateAsync(CreateEmpresaDto dto, CancellationToken ct = default)
        {
            var created = new EmpresaDto
            {
                Id = _nextId++,
                Nombre = dto.Nombre,
                NIT = dto.NIT,
                Direccion = dto.Direccion,
                Telefono = dto.Telefono,
                Email = dto.Email,
                MonedaBaseId = dto.MonedaBaseId,
                Activo = true
            };

            Empresas = Empresas.Concat([created]).ToArray();
            return Task.FromResult(Result<EmpresaDto>.Success(created));
        }
        public Task<Result<EmpresaDto>> UpdateAsync(int id, UpdateEmpresaDto dto, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<Result<bool>> DeleteAsync(int id, CancellationToken ct = default) => throw new NotImplementedException();
    }

    private sealed class FakeUsuarioService : IUsuarioService
    {
        public IReadOnlyList<UsuarioEmpresaRolDto> Asignaciones { get; set; } = Array.Empty<UsuarioEmpresaRolDto>();

        public Task<Result<IReadOnlyList<UsuarioEmpresaRolDto>>> GetEmpresasAsignadasAsync(int usuarioId, CancellationToken ct = default)
            => Task.FromResult(Result<IReadOnlyList<UsuarioEmpresaRolDto>>.Success(Asignaciones));

        public Task<Result<IReadOnlyList<UsuarioDto>>> GetAllAsync(CancellationToken ct = default) => throw new NotImplementedException();
        public Task<Result<UsuarioDto>> GetByIdAsync(int id, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<Result<UsuarioDto>> CreateAsync(CreateUsuarioDto dto, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<Result<UsuarioDto>> UpdateAsync(int id, UpdateUsuarioDto dto, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<Result<bool>> DeleteAsync(int id, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<Result<bool>> AsignarRolAsync(int usuarioId, AsignarRolDto dto, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<Result<bool>> RemoverDeEmpresaAsync(int usuarioId, int empresaId, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<Result<bool>> ResetPasswordAsync(int usuarioId, ResetPasswordDto dto, CancellationToken ct = default) => throw new NotImplementedException();
    }

    private sealed class FakeEmpresaSeedService : IEmpresaSeedService
    {
        public Task<Result<bool>> SeedDefaultDataAsync(int empresaId, CancellationToken ct = default)
            => Task.FromResult(Result<bool>.Success(true));
    }

    private sealed class FakeConfiguracionInicialEmpresaService : IConfiguracionInicialEmpresaService
    {
        public Task<Result<ConfiguracionInicialEmpresaResultadoDto>> GenerarConfiguracionBasicaAsync(long empresaId, CancellationToken ct = default)
            => Task.FromResult(Result<ConfiguracionInicialEmpresaResultadoDto>.Success(new ConfiguracionInicialEmpresaResultadoDto()));
    }

    private sealed class FakeEmpresaDemoService : IEmpresaDemoService
    {
        public Task<Result<CrearEmpresaDemoCompletaResponseDto>> CrearCompletaAsync(
            CrearEmpresaDemoCompletaRequestDto request,
            CancellationToken ct = default)
            => Task.FromResult(Result<CrearEmpresaDemoCompletaResponseDto>.Success(new CrearEmpresaDemoCompletaResponseDto
            {
                CodigoDemo = request.CodigoDemo,
                EmpresaId = 1,
                NombreEmpresa = "Demo",
                Nit = "990001001",
                Mensaje = "ok"
            }));
    }
}
