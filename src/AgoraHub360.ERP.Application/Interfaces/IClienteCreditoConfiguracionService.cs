using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Shared.DTOs.CxC;

namespace AgoraHub360.ERP.Application.Interfaces;

/// <summary>
/// Servicio de configuración de crédito para clientes.
/// Gestiona la habilitación y límites de crédito por cliente y empresa.
/// </summary>
public interface IClienteCreditoConfiguracionService
{
    /// <summary>
    /// Obtiene la configuración de crédito de un cliente específico.
    /// </summary>
    Task<Result<ClienteCreditoConfiguracionDto>> GetByClienteAsync(int clienteId, CancellationToken ct = default);

    /// <summary>
    /// Guarda o actualiza la configuración de crédito de un cliente.
    /// </summary>
    Task<Result<ClienteCreditoConfiguracionDto>> GuardarAsync(
        int clienteId,
        GuardarClienteCreditoConfiguracionRequestDto request,
        CancellationToken ct = default);
}
