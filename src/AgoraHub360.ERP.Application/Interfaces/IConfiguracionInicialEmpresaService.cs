using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Shared.DTOs.Empresa;

namespace AgoraHub360.ERP.Application.Interfaces;

public interface IConfiguracionInicialEmpresaService
{
    Task<Result<ConfiguracionInicialEmpresaResultadoDto>> GenerarConfiguracionBasicaAsync(long empresaId, CancellationToken ct = default);
}
