using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Shared.DTOs.Empresa;

namespace AgoraHub360.ERP.Application.Interfaces;

public interface IEmpresaDemoService
{
    Task<Result<CrearEmpresaDemoCompletaResponseDto>> CrearCompletaAsync(CrearEmpresaDemoCompletaRequestDto request, CancellationToken ct = default);
}
