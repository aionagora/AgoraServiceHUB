using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Domain.Interfaces;
using AgoraHub360.ERP.Shared.DTOs.FacturacionElectronica;

namespace AgoraHub360.ERP.Application.Services;

/// <summary>
/// Servicio de consulta de auditoría de Facturación Electrónica (read-only, tenant-aware).
/// </summary>
public class AuditoriaFEService : IAuditoriaFEService
{
    private readonly IAuditoriaFERepository _auditoriaRepo;
    private readonly ICurrentUserService _currentUser;

    public AuditoriaFEService(
        IAuditoriaFERepository auditoriaRepo,
        ICurrentUserService currentUser)
    {
        _auditoriaRepo = auditoriaRepo;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<AuditoriaFEDto>> ListarAsync(
        AuditoriaFEFiltroDto filtro,
        CancellationToken cancellationToken = default)
    {
        if (!_currentUser.EmpresaId.HasValue)
            throw new InvalidOperationException("No se pudo determinar la empresa activa.");

        var empresaId = _currentUser.EmpresaId.Value;
        var desde = filtro.Desde ?? DateTime.UtcNow.AddDays(-30);
        var hasta = filtro.Hasta ?? DateTime.UtcNow.AddDays(1);

        var registros = await _auditoriaRepo.ListarPorEmpresaAsync(empresaId, desde, hasta, cancellationToken);

        var enumerable = registros.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(filtro.EstadoSiat))
        {
            var estadoFilter = filtro.EstadoSiat.Trim();
            enumerable = enumerable.Where(r =>
                r.EstadoSiat.ToString().Equals(estadoFilter, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(filtro.ProveedorCodigo))
        {
            var provFilter = filtro.ProveedorCodigo.Trim();
            enumerable = enumerable.Where(r =>
                r.ProveedorCodigo.Equals(provFilter, StringComparison.OrdinalIgnoreCase));
        }

        if (filtro.FacturaVentaId.HasValue)
            enumerable = enumerable.Where(r => r.FacturaVentaId == filtro.FacturaVentaId.Value);

        return enumerable.Select(MapToDto).ToList().AsReadOnly();
    }

    public async Task<IReadOnlyList<AuditoriaFEDto>> ListarPorFacturaAsync(
        long facturaVentaId,
        CancellationToken cancellationToken = default)
    {
        if (!_currentUser.EmpresaId.HasValue)
            throw new InvalidOperationException("No se pudo determinar la empresa activa.");

        var registros = await _auditoriaRepo.ListarPorFacturaAsync(facturaVentaId, cancellationToken);

        // Filtro adicional de seguridad: solo registros de la empresa actual
        return registros
            .Where(r => r.EmpresaId == _currentUser.EmpresaId.Value)
            .Select(MapToDto)
            .ToList()
            .AsReadOnly();
    }

    private static AuditoriaFEDto MapToDto(Domain.Entities.FE.AuditoriaFacturacion entity)
    {
        return new AuditoriaFEDto
        {
            Id = entity.Id,
            EmpresaId = entity.EmpresaId,
            FacturaVentaId = entity.FacturaVentaId,
            ProveedorCodigo = entity.ProveedorCodigo,
            ProveedorNombre = entity.ProveedorNombre,
            AmbienteCodigo = entity.AmbienteCodigo,
            AmbienteNombre = entity.AmbienteNombre,
            BillUuid = entity.BillUuid,
            Cuf = entity.Cuf,
            EstadoSiat = entity.EstadoSiat.ToString(),
            MensajeError = entity.MensajeError,
            UsuarioId = entity.UsuarioId,
            FechaHora = entity.FechaHora,
            TiempoRespuestaMs = entity.TiempoRespuestaMs,
            CodigoRespuestaProveedor = entity.CodigoRespuestaProveedor,
            DescripcionRespuestaProveedor = entity.DescripcionRespuestaProveedor,
            Exitoso = entity.Exitoso
        };
    }
}
