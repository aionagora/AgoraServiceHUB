namespace AgoraHub360.ERP.Tests.Application;

using AgoraHub360.ERP.Api.Controllers.V1;
using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Shared.Constants;
using AgoraHub360.ERP.Shared.DTOs;
using AgoraHub360.ERP.Shared.DTOs.Numeracion;
using Microsoft.AspNetCore.Mvc;

public class NumeracionesControllerSecurityTests
{
    [Fact]
    public async Task Operador_NoPuedeModificarNumeracion()
    {
        var sut = new NumeracionesController(new FakeNumeracionDocumentoService(), new FakeCurrentUserService
        {
            TenantId = 1,
            EmpresaId = 1,
            TenantRole = Roles.Operador,
            TenantStatus = TenantStatus.Selected
        });

        var result = await sut.Update(1, new ActualizarNumeracionDocumentoRequestDto
        {
            TipoDocumento = "FAC",
            Descripcion = "Factura",
            Prefijo = "FAC",
            LongitudNumero = 6,
            SiguienteNumero = 1
        }, CancellationToken.None);

        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task AdminEmpresa_NoPuedeModificarNumeracionDeEmpresaAjena()
    {
        var service = new FakeNumeracionDocumentoService
        {
            UpdateResult = Result<NumeracionDocumentoDto>.Failure("No tiene permisos para modificar esta numeración.")
        };

        var sut = new NumeracionesController(service, new FakeCurrentUserService
        {
            TenantId = 1,
            EmpresaId = 1,
            TenantRole = Roles.AdminEmpresa,
            TenantStatus = TenantStatus.Selected
        });

        var result = await sut.Update(999, new ActualizarNumeracionDocumentoRequestDto
        {
            TipoDocumento = "FAC",
            Descripcion = "Factura",
            Prefijo = "FAC",
            LongitudNumero = 6,
            SiguienteNumero = 1,
            SucursalId = 2
        }, CancellationToken.None);

        var bad = Assert.IsType<BadRequestObjectResult>(result);
        var response = Assert.IsType<ApiResponse<NumeracionDocumentoDto>>(bad.Value);
        Assert.False(response.Success);
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

    private sealed class FakeNumeracionDocumentoService : INumeracionDocumentoService
    {
        public Result<IReadOnlyList<NumeracionDocumentoDto>> GetAllResult { get; set; }
            = Result<IReadOnlyList<NumeracionDocumentoDto>>.Success(Array.Empty<NumeracionDocumentoDto>());
        public Result<NumeracionDocumentoDto> GetByIdResult { get; set; } = Result<NumeracionDocumentoDto>.Failure("No encontrada");
        public Result<NumeracionDocumentoDto> CreateResult { get; set; } = Result<NumeracionDocumentoDto>.Failure("No permitido");
        public Result<NumeracionDocumentoDto> UpdateResult { get; set; } = Result<NumeracionDocumentoDto>.Failure("No permitido");
        public Result<bool> ActivarResult { get; set; } = Result<bool>.Failure("No permitido");
        public Result<bool> DesactivarResult { get; set; } = Result<bool>.Failure("No permitido");

        public Task<Result<IReadOnlyList<NumeracionDocumentoDto>>> GetAllAsync(CancellationToken ct = default) => Task.FromResult(GetAllResult);
        public Task<Result<NumeracionDocumentoDto>> GetByIdAsync(int id, CancellationToken ct = default) => Task.FromResult(GetByIdResult);
        public Task<Result<NumeracionDocumentoDto>> CreateAsync(CrearNumeracionDocumentoRequestDto dto, CancellationToken ct = default) => Task.FromResult(CreateResult);
        public Task<Result<NumeracionDocumentoDto>> UpdateAsync(int id, ActualizarNumeracionDocumentoRequestDto dto, CancellationToken ct = default) => Task.FromResult(UpdateResult);
        public Task<Result<bool>> ActivarAsync(int id, CancellationToken ct = default) => Task.FromResult(ActivarResult);
        public Task<Result<bool>> DesactivarAsync(int id, CancellationToken ct = default) => Task.FromResult(DesactivarResult);
        public Task<Result<string>> GenerarSiguienteNumeroAsync(string tipoDocumento, long sucursalId, CancellationToken ct = default)
            => Task.FromResult(Result<string>.Failure("No implementado"));
    }
}
