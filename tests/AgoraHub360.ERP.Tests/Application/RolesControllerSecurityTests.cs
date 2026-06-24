namespace AgoraHub360.ERP.Tests.Application;

using AgoraHub360.ERP.Api.Controllers.V1;
using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Shared.Constants;
using AgoraHub360.ERP.Shared.DTOs;
using AgoraHub360.ERP.Shared.DTOs.Rol;
using Microsoft.AspNetCore.Mvc;

public class RolesControllerSecurityTests
{
    [Fact]
    public async Task Operador_NoPuedeAdministrarRoles()
    {
        var sut = BuildController(new FakeRolService(), new FakeCurrentUserService
        {
            PlatformRole = Roles.None,
            TenantRole = Roles.Operador,
            TenantStatus = TenantStatus.Selected,
            TenantId = 1,
            EmpresaId = 1
        });

        var result = await sut.Create(new CreateRolDto { Nombre = "Nuevo" }, CancellationToken.None);

        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task AdminEmpresa_NoPuedeCrearRolGlobal()
    {
        var sut = BuildController(new FakeRolService(), new FakeCurrentUserService
        {
            PlatformRole = Roles.None,
            TenantRole = Roles.AdminEmpresa,
            TenantStatus = TenantStatus.Selected,
            TenantId = 1,
            EmpresaId = 1
        });

        var result = await sut.Create(new CreateRolDto { Nombre = "GlobalRole" }, CancellationToken.None);

        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task PlatformAdmin_PuedeAdministrarRolesGlobales()
    {
        var service = new FakeRolService
        {
            CreateResult = Result<RolDto>.Success(new RolDto { Id = 1, Nombre = "GlobalRole", Activo = true })
        };

        var sut = BuildController(service, new FakeCurrentUserService
        {
            PlatformRole = Roles.SystemAdmin,
            TenantRole = Roles.NoAccess,
            TenantStatus = TenantStatus.NotSelected
        });

        var result = await sut.Create(new CreateRolDto { Nombre = "GlobalRole" }, CancellationToken.None);

        Assert.IsType<CreatedAtActionResult>(result);
    }

    private static RolesController BuildController(IRolService rolService, ICurrentUserService currentUser)
        => new(rolService, currentUser);

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

    private sealed class FakeRolService : IRolService
    {
        public Result<IReadOnlyList<RolDto>> GetAllResult { get; set; }
            = Result<IReadOnlyList<RolDto>>.Success(Array.Empty<RolDto>());
        public Result<RolDto> GetByIdResult { get; set; } = Result<RolDto>.Failure("No encontrado");
        public Result<RolDto> CreateResult { get; set; } = Result<RolDto>.Failure("No permitido");
        public Result<RolDto> UpdateResult { get; set; } = Result<RolDto>.Failure("No permitido");
        public Result<bool> DeleteResult { get; set; } = Result<bool>.Failure("No permitido");

        public Task<Result<IReadOnlyList<RolDto>>> GetAllAsync(CancellationToken ct = default) => Task.FromResult(GetAllResult);
        public Task<Result<RolDto>> GetByIdAsync(int id, CancellationToken ct = default) => Task.FromResult(GetByIdResult);
        public Task<Result<RolDto>> CreateAsync(CreateRolDto dto, CancellationToken ct = default) => Task.FromResult(CreateResult);
        public Task<Result<RolDto>> UpdateAsync(int id, UpdateRolDto dto, CancellationToken ct = default) => Task.FromResult(UpdateResult);
        public Task<Result<bool>> DeleteAsync(int id, CancellationToken ct = default) => Task.FromResult(DeleteResult);
    }
}
