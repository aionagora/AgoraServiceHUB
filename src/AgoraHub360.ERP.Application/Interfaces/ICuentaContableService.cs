namespace AgoraHub360.ERP.Application.Interfaces;

using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Shared.DTOs.Contabilidad;
using AgoraHub360.ERP.Shared.DTOs.Contabilidad.Importacion;

public interface ICuentaContableService
{
    /// <summary>Obtiene todas las cuentas (flat list) con filtros opcionales.</summary>
    Task<Result<IReadOnlyList<CuentaContableDto>>> GetAllAsync(
        byte? tipo = null,
        bool? permiteMovimientos = null,
        string? search = null,
        CancellationToken ct = default);

    /// <summary>Obtiene el plan de cuentas en estructura de árbol.</summary>
    Task<Result<IReadOnlyList<CuentaContableDto>>> GetTreeAsync(CancellationToken ct = default);

    /// <summary>Obtiene una cuenta por Id.</summary>
    Task<Result<CuentaContableDto>> GetByIdAsync(int id, CancellationToken ct = default);

    /// <summary>Crea una nueva cuenta contable.</summary>
    Task<Result<CuentaContableDto>> CreateAsync(CreateCuentaContableDto dto, CancellationToken ct = default);

    /// <summary>Actualiza una cuenta existente.</summary>
    Task<Result<CuentaContableDto>> UpdateAsync(int id, UpdateCuentaContableDto dto, CancellationToken ct = default);

    /// <summary>Elimina (soft-delete) una cuenta sin movimientos.</summary>
    Task<Result<bool>> DeleteAsync(int id, CancellationToken ct = default);

    /// <summary>Genera el plan de cuentas estándar Bolivia/NIIF para la empresa activa.</summary>
    Task<Result<int>> SeedPlanCuentasAsync(CancellationToken ct = default);

    // ── Importación CSV ─────────────────────────────────────────────────────

    /// <summary>Valida un CSV y devuelve vista previa con errores.</summary>
    Task<Result<PlanCuentaImportPreviewDto>> PreviewImportAsync(string csvContent, CancellationToken ct = default);

    /// <summary>Ejecuta la importación confirmada de un CSV válido.</summary>
    Task<Result<PlanCuentaImportResultDto>> ConfirmImportAsync(string csvContent, CancellationToken ct = default);
}
