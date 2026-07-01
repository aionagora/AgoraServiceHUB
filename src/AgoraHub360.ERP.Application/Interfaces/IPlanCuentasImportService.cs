using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Shared.DTOs.Contabilidad.Importacion;

namespace AgoraHub360.ERP.Application.Interfaces;

/// <summary>Servicio para importación masiva de plan de cuentas desde archivo CSV.</summary>
public interface IPlanCuentasImportService
{
    /// <summary>Valida el archivo y retorna una vista previa con las filas válidas y errores.</summary>
    Task<Result<PlanCuentaImportPreviewDto>> PreviewAsync(Stream fileStream, string fileName, CancellationToken ct = default);

    /// <summary>Ejecuta la importación de las cuentas validadas. No guarda nada si hay errores.</summary>
    Task<Result<PlanCuentaImportResultDto>> ImportAsync(Stream fileStream, string fileName, CancellationToken ct = default);

    /// <summary>Genera un archivo Excel con el formato esperado para importar plan de cuentas.</summary>
    byte[] GenerateTemplate();
}
