namespace AgoraHub360.ERP.Application.Interfaces;

using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Shared.DTOs.Usuario;

public interface IUsuarioService
{
    Task<Result<IReadOnlyList<UsuarioDto>>> GetAllAsync(CancellationToken ct = default);
    Task<Result<UsuarioDto>> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Result<UsuarioDto>> CreateAsync(CreateUsuarioDto dto, CancellationToken ct = default);
    Task<Result<UsuarioDto>> UpdateAsync(int id, UpdateUsuarioDto dto, CancellationToken ct = default);
    Task<Result<bool>> DeleteAsync(int id, CancellationToken ct = default);
    Task<Result<bool>> AsignarRolAsync(int usuarioId, AsignarRolDto dto, CancellationToken ct = default);
    Task<Result<bool>> RemoverDeEmpresaAsync(int usuarioId, int empresaId, CancellationToken ct = default);
    Task<Result<IReadOnlyList<UsuarioEmpresaRolDto>>> GetEmpresasAsignadasAsync(int usuarioId, CancellationToken ct = default);
    Task<Result<bool>> ResetPasswordAsync(int usuarioId, ResetPasswordDto dto, CancellationToken ct = default);
}
