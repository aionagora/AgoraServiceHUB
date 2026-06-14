using AgoraHub360.ERP.Shared.DTOs.Notificacion;

namespace AgoraHub360.ERP.Application.Interfaces;

public interface INotificacionService
{
    Task EnviarAsync(CreateNotificacionDto dto, CancellationToken ct = default);
}
