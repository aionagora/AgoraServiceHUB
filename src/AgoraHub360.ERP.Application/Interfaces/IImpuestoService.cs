using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Shared.DTOs.Tributario;

namespace AgoraHub360.ERP.Application.Interfaces;

public interface IImpuestoService
{
    Task<Result<RegistroImpuestoDto>> CalcularIVAAsync(int periodoId, CancellationToken ct = default);
    Task<Result<RegistroImpuestoDto>> CalcularITAsync(int periodoId, CancellationToken ct = default);
    Task<Result<RegistroImpuestoDto>> CalcularIUEAsync(int gestion, CancellationToken ct = default);
    Task<Result<FormularioSINDto>> GetDatosFormulario200Async(int periodoId, CancellationToken ct = default);
    Task<Result<FormularioSINDto>> GetDatosFormulario400Async(int periodoId, CancellationToken ct = default);
    Task<Result<FormularioSINDto>> GetDatosFormulario500Async(int gestion, CancellationToken ct = default);
    Task<Result<RegistroImpuestoDto>> MarcarDeclaradoAsync(long registroId, string nroCertificado, CancellationToken ct = default);
}
