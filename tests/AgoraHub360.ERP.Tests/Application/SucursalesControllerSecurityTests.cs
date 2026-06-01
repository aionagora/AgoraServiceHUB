namespace AgoraHub360.ERP.Tests.Application;

using AgoraHub360.ERP.Api.Controllers.V1;
using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Shared.Constants;
using AgoraHub360.ERP.Shared.DTOs;
using AgoraHub360.ERP.Shared.DTOs.Sucursal;
using Microsoft.AspNetCore.Mvc;

public class SucursalesControllerSecurityTests
{
    [Fact]
    public async Task AdminEmpresa_ListaSoloTenantActivo()
    {
        var sut = BuildController(
            new FakeSucursalService
            {
                Listado = new[]
                {
                    new SucursalListadoDto { Id = 1, EmpresaId = 1, Nombre = "Sucursal A", Activo = true },
                    new SucursalListadoDto { Id = 2, EmpresaId = 1, Nombre = "Sucursal B", Activo = true }
                }
            },
            new FakeCurrentUserService
            {
                TenantId = 1,
                EmpresaId = 1,
                TenantStatus = AgoraHub360.ERP.Shared.Constants.TenantStatus.Selected,
                TenantRole = Roles.AdminEmpresa
            });

        var result = await sut.GetAll(CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<IReadOnlyList<SucursalListadoDto>>>(ok.Value);
        Assert.True(response.Success);
        Assert.All(response.Data!, x => Assert.Equal(1, x.EmpresaId));
    }

    [Fact]
    public async Task AdminEmpresa_NoPuedeVerSucursalAjena()
    {
        var sut = BuildController(
            new FakeSucursalService
            {
                GetByIdResult = Result<SucursalDto>.Failure("Sucursal no encontrada.")
            },
            new FakeCurrentUserService
            {
                TenantId = 1,
                EmpresaId = 1,
                TenantStatus = AgoraHub360.ERP.Shared.Constants.TenantStatus.Selected,
                TenantRole = Roles.AdminEmpresa
            });

        var result = await sut.GetById(999, CancellationToken.None);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task AdminEmpresa_PuedeCrearSucursalEnTenantActivo()
    {
        var sut = BuildController(
            new FakeSucursalService
            {
                CreateResult = Result<SucursalDto>.Success(new SucursalDto { Id = 10, EmpresaId = 1, Nombre = "Nueva" })
            },
            new FakeCurrentUserService
            {
                TenantId = 1,
                EmpresaId = 1,
                TenantStatus = AgoraHub360.ERP.Shared.Constants.TenantStatus.Selected,
                TenantRole = Roles.AdminEmpresa
            });

        var result = await sut.Create(new CrearSucursalDto { Nombre = "Nueva", Codigo = "SUC10" }, CancellationToken.None);

        Assert.IsType<CreatedAtActionResult>(result);
    }

    [Fact]
    public async Task Operador_NoPuedeCrearSucursal()
    {
        var sut = BuildController(
            new FakeSucursalService(),
            new FakeCurrentUserService
            {
                TenantId = 1,
                EmpresaId = 1,
                TenantStatus = AgoraHub360.ERP.Shared.Constants.TenantStatus.Selected,
                TenantRole = Roles.Operador
            });

        var result = await sut.Create(new CrearSucursalDto { Nombre = "Nueva", Codigo = "SUC10" }, CancellationToken.None);

        Assert.IsType<ForbidResult>(result);
    }

    private static SucursalesController BuildController(FakeSucursalService service, FakeCurrentUserService currentUser)
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

    private sealed class FakeSucursalService : ISucursalService
    {
        public IReadOnlyList<SucursalListadoDto> Listado { get; set; } = Array.Empty<SucursalListadoDto>();
        public Result<SucursalDto> GetByIdResult { get; set; } = Result<SucursalDto>.Failure("Sucursal no encontrada.");
        public Result<SucursalDto> CreateResult { get; set; } = Result<SucursalDto>.Failure("Error");

        public Task<Result<IReadOnlyList<SucursalListadoDto>>> GetAllAsync(CancellationToken ct = default)
            => Task.FromResult(Result<IReadOnlyList<SucursalListadoDto>>.Success(Listado));

        public Task<Result<SucursalDto>> GetByIdAsync(int id, CancellationToken ct = default)
            => Task.FromResult(GetByIdResult);

        public Task<Result<SucursalDto>> CreateAsync(CrearSucursalDto dto, CancellationToken ct = default)
            => Task.FromResult(CreateResult);

        public Task<Result<SucursalDto>> UpdateAsync(int id, ActualizarSucursalDto dto, CancellationToken ct = default)
            => Task.FromResult(Result<SucursalDto>.Failure("No implementado"));

        public Task<Result<bool>> DeleteAsync(int id, CancellationToken ct = default)
            => Task.FromResult(Result<bool>.Failure("No implementado"));

        public Task<Result<bool>> CambiarEstadoAsync(int id, bool activo, CancellationToken ct = default)
            => Task.FromResult(Result<bool>.Failure("No implementado"));

        public Task<Result<bool>> EstablecerCentralAsync(int id, CancellationToken ct = default)
            => Task.FromResult(Result<bool>.Failure("No implementado"));
    }
}
