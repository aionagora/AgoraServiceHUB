namespace AgoraHub360.ERP.Application.Interfaces;

using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Shared.DTOs.Empresa;

public interface IEmpresaService
{
    Task<Result<IReadOnlyList<EmpresaDto>>> GetAllAsync(CancellationToken ct = default);
    Task<Result<EmpresaDto>> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Result<EmpresaDto>> CreateAsync(CreateEmpresaDto dto, CancellationToken ct = default);
    Task<Result<EmpresaDto>> UpdateAsync(int id, UpdateEmpresaDto dto, CancellationToken ct = default);
    Task<Result<bool>> DeleteAsync(int id, CancellationToken ct = default);
}
