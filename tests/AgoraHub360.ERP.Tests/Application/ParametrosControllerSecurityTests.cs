namespace AgoraHub360.ERP.Tests.Application;

using AgoraHub360.ERP.Api.Controllers.V1;
using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Shared.Constants;
using AgoraHub360.ERP.Shared.DTOs;
using AgoraHub360.ERP.Shared.DTOs.Parametro;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;

public class ParametrosControllerSecurityTests
{
    [Fact]
    public async Task AdminEmpresa_PuedeLeerParametrosDeSuTenant()
    {
        var service = new FakeParametroSistemaService
        {
            GetAllResult = Result<IReadOnlyList<ParametroSistemaDto>>.Success(new[]
            {
                new ParametroSistemaDto { Id = 1, Clave = "X", Valor = "1", EmpresaId = 1 }
            })
        };

        var sut = new ParametrosController(service, NullLogger<ParametrosController>.Instance, new FakeCurrentUserService
        {
            TenantId = 1,
            EmpresaId = 1,
            TenantRole = Roles.AdminEmpresa,
            TenantStatus = TenantStatus.Selected
        });

        var result = await sut.GetAll(CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<IReadOnlyList<ParametroSistemaDto>>>(ok.Value);
        Assert.True(response.Success);
        Assert.Single(response.Data!);
    }

    [Fact]
    public async Task AdminEmpresa_PuedeModificarParametrosDeSuTenant()
    {
        var service = new FakeParametroSistemaService
        {
            UpsertResult = Result<ParametroSistemaDto>.Success(new ParametroSistemaDto { Id = 10, Clave = "CFG", Valor = "ON", EmpresaId = 1 })
        };

        var sut = new ParametrosController(service, NullLogger<ParametrosController>.Instance, new FakeCurrentUserService
        {
            TenantId = 1,
            EmpresaId = 1,
            TenantRole = Roles.AdminEmpresa,
            TenantStatus = TenantStatus.Selected
        });

        var result = await sut.Upsert(new UpsertParametroDto { Clave = "CFG", Valor = "ON" }, CancellationToken.None);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task Operador_NoPuedeModificarParametros()
    {
        var sut = new ParametrosController(new FakeParametroSistemaService(), NullLogger<ParametrosController>.Instance, new FakeCurrentUserService
        {
            TenantId = 1,
            EmpresaId = 1,
            TenantRole = Roles.Operador,
            TenantStatus = TenantStatus.Selected
        });

        var result = await sut.Upsert(new UpsertParametroDto { Clave = "CFG", Valor = "OFF" }, CancellationToken.None);

        Assert.IsType<ForbidResult>(result);
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

    private sealed class FakeParametroSistemaService : IParametroSistemaService
    {
        public Result<IReadOnlyList<ParametroSistemaDto>> GetAllResult { get; set; }
            = Result<IReadOnlyList<ParametroSistemaDto>>.Success(Array.Empty<ParametroSistemaDto>());
        public Result<IReadOnlyList<ParametroSistemaDto>> GetByCategoriaResult { get; set; }
            = Result<IReadOnlyList<ParametroSistemaDto>>.Success(Array.Empty<ParametroSistemaDto>());
        public Result<ParametroSistemaDto> GetByClaveResult { get; set; } = Result<ParametroSistemaDto>.Failure("No encontrado");
        public Result<ParametroSistemaDto> UpsertResult { get; set; } = Result<ParametroSistemaDto>.Failure("No permitido");
        public Result<bool> DeleteResult { get; set; } = Result<bool>.Failure("No permitido");

        public Task<Result<IReadOnlyList<ParametroSistemaDto>>> GetAllAsync(CancellationToken ct = default) => Task.FromResult(GetAllResult);
        public Task<Result<IReadOnlyList<ParametroSistemaDto>>> GetByCategoriaAsync(string categoria, CancellationToken ct = default) => Task.FromResult(GetByCategoriaResult);
        public Task<Result<ParametroSistemaDto>> GetByClaveAsync(string clave, CancellationToken ct = default) => Task.FromResult(GetByClaveResult);
        public Task<Result<ParametroSistemaDto>> UpsertAsync(UpsertParametroDto dto, CancellationToken ct = default) => Task.FromResult(UpsertResult);
        public Task<Result<bool>> DeleteAsync(int id, CancellationToken ct = default) => Task.FromResult(DeleteResult);
    }
}
