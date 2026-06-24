namespace AgoraHub360.ERP.Application.Interfaces;

public interface IPeriodoContableNotificacionService
{
    Task VerificarPeriodosProximosAsync(int empresaId, CancellationToken ct = default);
}
