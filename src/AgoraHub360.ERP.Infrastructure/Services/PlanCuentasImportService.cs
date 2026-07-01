namespace AgoraHub360.ERP.Infrastructure.Services;

using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Domain.Entities.ACC;
using AgoraHub360.ERP.Domain.Enums;
using AgoraHub360.ERP.Domain.Interfaces;
using AgoraHub360.ERP.Shared.DTOs.Contabilidad;
using AgoraHub360.ERP.Shared.DTOs.Contabilidad.Importacion;
using ClosedXML.Excel;
using Microsoft.Extensions.Logging;

/// <summary>
/// Implementación de <see cref="IPlanCuentasImportService"/> que lee archivos Excel con ClosedXML,
/// valida la estructura y contenido, y persiste cuentas contables del plan de cuentas.
/// </summary>
public class PlanCuentasImportService : IPlanCuentasImportService
{
    private const string NombreHoja = "PlanCuentas";
    private const int FilaInicio = 2;

    private const int ColCodigo = 1;
    private const int ColNombre = 2;
    private const int ColTipoCuenta = 3;
    private const int ColNaturaleza = 4;
    private const int ColCodigoPadre = 5;
    private const int ColNivel = 6;
    private const int ColEsMovimiento = 7;
    private const int ColActivo = 8;
    private const int ColDescripcion = 9;
    private const int ColMax = 9;

    private readonly IRepository<CuentaContable> _cuentaRepo;
    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<PlanCuentasImportService> _logger;

    private static readonly HashSet<string> TiposValidos = new(StringComparer.OrdinalIgnoreCase)
    {
        "Activo", "Pasivo", "Patrimonio", "Ingreso", "Gasto", "Costo"
    };

    private static readonly HashSet<string> NaturalezasValidas = new(StringComparer.OrdinalIgnoreCase)
    {
        "Deudora", "Acreedora"
    };

    public PlanCuentasImportService(
        IRepository<CuentaContable> cuentaRepo,
        ICurrentUserService currentUser,
        IUnitOfWork unitOfWork,
        ILogger<PlanCuentasImportService> logger)
    {
        _cuentaRepo = cuentaRepo;
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<PlanCuentaImportPreviewDto>> PreviewAsync(
        Stream fileStream, string fileName, CancellationToken ct = default)
    {
        var empresaId = _currentUser.EmpresaId;
        if (!empresaId.HasValue)
            return Result<PlanCuentaImportPreviewDto>.Failure("No active company.");

        var (filas, erroresLectura) = LeerFilas(fileStream);

        if (erroresLectura.Count > 0)
        {
            return Result<PlanCuentaImportPreviewDto>.Success(new PlanCuentaImportPreviewDto
            {
                FileName = fileName,
                EsValido = false,
                TotalFilas = filas.Count,
                CuentasValidas = new(),
                Errores = erroresLectura
            });
        }

        var existentes = await _cuentaRepo.FindAsync(c => c.EmpresaId == empresaId.Value, ct);
        var codigosExistentes = new HashSet<string>(existentes.Select(c => c.Codigo), StringComparer.OrdinalIgnoreCase);

        var errores = new List<ImportErrorDto>();
        var cuentasValidas = new List<PlanCuentaImportRowDto>();
        var codigosEnArchivo = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        foreach (var fila in filas)
        {
            var erroresFila = new List<string>();
            var filaNum = fila.RowNumber;

            if (string.IsNullOrWhiteSpace(fila.Codigo))
                erroresFila.Add($"Row {filaNum}: Code is required.");

            if (string.IsNullOrWhiteSpace(fila.Nombre))
                erroresFila.Add($"Row {filaNum}: Name is required.");

            if (string.IsNullOrWhiteSpace(fila.TipoCuenta) || !TiposValidos.Contains(fila.TipoCuenta))
                erroresFila.Add($"Row {filaNum}: Invalid TipoCuenta '{fila.TipoCuenta}'.");

            if (string.IsNullOrWhiteSpace(fila.NaturalezaStr) || !NaturalezasValidas.Contains(fila.NaturalezaStr))
                erroresFila.Add($"Row {filaNum}: Invalid Naturaleza '{fila.NaturalezaStr}'.");

            if (fila.Nivel < 1)
                erroresFila.Add($"Row {filaNum}: Nivel must be greater than 0.");

            if (!string.IsNullOrWhiteSpace(fila.Codigo))
            {
                if (codigosEnArchivo.TryGetValue(fila.Codigo, out var exist))
                    erroresFila.Add($"Row {filaNum}: Code '{fila.Codigo}' duplicated (also row {exist}).");
                else
                    codigosEnArchivo[fila.Codigo] = filaNum;

                if (codigosExistentes.Contains(fila.Codigo))
                    erroresFila.Add($"Row {filaNum}: Code '{fila.Codigo}' already exists in the chart of accounts.");
            }

            if (!string.IsNullOrWhiteSpace(fila.CodigoPadre))
            {
                if (!codigosEnArchivo.ContainsKey(fila.CodigoPadre) && !codigosExistentes.Contains(fila.CodigoPadre))
                    erroresFila.Add($"Row {filaNum}: Parent '{fila.CodigoPadre}' not found.");
            }

            if (erroresFila.Count > 0)
            {
                foreach (var err in erroresFila)
                    errores.Add(new ImportErrorDto { Fila = filaNum, Mensaje = err });
            }
            else
            {
                cuentasValidas.Add(MapToRowDto(fila));
            }
        }

        var preview = new PlanCuentaImportPreviewDto
        {
            FileName = fileName,
            EsValido = errores.Count == 0,
            TotalFilas = filas.Count,
            CuentasValidas = cuentasValidas,
            Errores = errores
        };

        return Result<PlanCuentaImportPreviewDto>.Success(preview);
    }

    public async Task<Result<PlanCuentaImportResultDto>> ImportAsync(
        Stream fileStream, string fileName, CancellationToken ct = default)
    {
        var empresaId = _currentUser.EmpresaId;
        if (!empresaId.HasValue)
            return Result<PlanCuentaImportResultDto>.Failure("No active company.");

        var previewResult = await PreviewAsync(fileStream, fileName, ct);
        if (!previewResult.IsSuccess)
            return Result<PlanCuentaImportResultDto>.Failure(previewResult.Error!);

        var preview = previewResult.Value!;

        if (!preview.EsValido || preview.CuentasValidas.Count == 0)
        {
            return Result<PlanCuentaImportResultDto>.Success(new PlanCuentaImportResultDto
            {
                TotalCuentasCreadas = 0,
                TotalErrores = preview.Errores.Count,
                Errores = preview.Errores
            });
        }

        var errores = new List<ImportErrorDto>();
        var cuentasCreadas = 0;
        var cuentasMap = new Dictionary<string, CuentaContable>(StringComparer.OrdinalIgnoreCase);

        try
        {
            await _unitOfWork.BeginTransactionAsync(ct);

            foreach (var fila in preview.CuentasValidas)
            {
                try
                {
                    var cuenta = new CuentaContable
                    {
                        EmpresaId = empresaId.Value,
                        Codigo = fila.Codigo,
                        Nombre = fila.Nombre,
                        Tipo = MapTipo(fila.Tipo),
                        Naturaleza = MapNaturaleza(fila.Naturaleza),
                        Nivel = fila.Nivel,
                        PermiteMovimientos = fila.PermiteMovimientos,
                        Descripcion = fila.Descripcion,
                        Activo = true,
                        SaldoActual = 0
                    };

                    if (!string.IsNullOrWhiteSpace(fila.CuentaPadreCodigo) && cuentasMap.TryGetValue(fila.CuentaPadreCodigo, out var padre))
                        cuenta.CuentaPadreId = padre.CuentaContableId;

                    await _cuentaRepo.AddAsync(cuenta, ct);
                    cuentasMap[fila.Codigo] = cuenta;
                    cuentasCreadas++;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error importing account {Codigo}", fila.Codigo);
                    errores.Add(new ImportErrorDto { Mensaje = $"Account '{fila.Codigo}': {ex.Message}" });
                }
            }

            await _unitOfWork.SaveChangesAsync(ct);
            await _unitOfWork.CommitTransactionAsync(ct);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync(ct);
            _logger.LogError(ex, "Error in mass import of chart of accounts");
            return Result<PlanCuentaImportResultDto>.Failure($"Import error: {ex.Message}");
        }

        return Result<PlanCuentaImportResultDto>.Success(new PlanCuentaImportResultDto
        {
            TotalCuentasCreadas = cuentasCreadas,
            TotalErrores = errores.Count,
            Errores = errores
        });
    }

    public byte[] GenerateTemplate()
    {
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add(NombreHoja);

        ws.Cell(1, ColCodigo).Value = "Codigo";
        ws.Cell(1, ColNombre).Value = "Nombre";
        ws.Cell(1, ColTipoCuenta).Value = "TipoCuenta";
        ws.Cell(1, ColNaturaleza).Value = "Naturaleza";
        ws.Cell(1, ColCodigoPadre).Value = "CodigoPadre";
        ws.Cell(1, ColNivel).Value = "Nivel";
        ws.Cell(1, ColEsMovimiento).Value = "EsMovimiento";
        ws.Cell(1, ColActivo).Value = "Activo";
        ws.Cell(1, ColDescripcion).Value = "Descripcion";

        var headerRange = ws.Range(1, 1, 1, ColMax);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
        headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

        var ejemplos = new[]
        {
            new[] { "1", "ACTIVO", "Activo", "Deudora", "", "1", "false", "true", "Agrupa todos los activos" },
            new[] { "1.1", "ACTIVO CORRIENTE", "Activo", "Deudora", "1", "2", "false", "true", "Activos de corto plazo" },
            new[] { "1.1.01", "CAJA", "Activo", "Deudora", "1.1", "3", "true", "true", "Efectivo en caja" },
            new[] { "1.1.02", "BANCOS", "Activo", "Deudora", "1.1", "3", "true", "true", "Saldo en cuentas bancarias" },
            new[] { "2", "PASIVO", "Pasivo", "Acreedora", "", "1", "false", "true", "Obligaciones con terceros" },
            new[] { "3", "PATRIMONIO", "Patrimonio", "Acreedora", "", "1", "false", "true", "Capital y reservas" },
            new[] { "4", "INGRESO", "Ingreso", "Acreedora", "", "1", "false", "true", "Ingresos operativos" },
            new[] { "5", "GASTO", "Gasto", "Deudora", "", "1", "false", "true", "Gastos operativos" },
        };

        for (int i = 0; i < ejemplos.Length; i++)
        {
            var fila = FilaInicio + i;
            for (int c = 0; c < ejemplos[i].Length; c++)
                ws.Cell(fila, c + 1).Value = ejemplos[i][c];
        }

        ws.Columns().AdjustToContents();

        using var ms = new MemoryStream();
        workbook.SaveAs(ms);
        return ms.ToArray();
    }

    private static (List<FilaExcel> filas, List<ImportErrorDto> errores) LeerFilas(Stream stream)
    {
        var filas = new List<FilaExcel>();
        var errores = new List<ImportErrorDto>();

        try
        {
            using var workbook = new XLWorkbook(stream);
            var ws = workbook.Worksheets.FirstOrDefault();
            if (ws is null)
            {
                errores.Add(new ImportErrorDto { Mensaje = "The Excel file has no worksheets." });
                return (filas, errores);
            }

            var lastRow = ws.LastRowUsed()?.RowNumber() ?? 1;

            for (int row = FilaInicio; row <= lastRow; row++)
            {
                var codigo = GetCellString(ws, row, ColCodigo);
                if (string.IsNullOrWhiteSpace(codigo))
                    continue;

                var nombre = GetCellString(ws, row, ColNombre);
                var tipoCuenta = GetCellString(ws, row, ColTipoCuenta);
                var naturaleza = GetCellString(ws, row, ColNaturaleza);
                var codigoPadre = GetCellString(ws, row, ColCodigoPadre);
                var nivelStr = GetCellString(ws, row, ColNivel);
                var esMovimientoStr = GetCellString(ws, row, ColEsMovimiento);
                var activoStr = GetCellString(ws, row, ColActivo);
                var descripcion = GetCellString(ws, row, ColDescripcion);

                int.TryParse(nivelStr, out var nivel);
                if (nivel == 0 && !string.IsNullOrWhiteSpace(nivelStr))
                    nivel = 1;

                filas.Add(new FilaExcel
                {
                    RowNumber = row,
                    Codigo = codigo.Trim(),
                    Nombre = nombre?.Trim() ?? string.Empty,
                    TipoCuenta = tipoCuenta?.Trim() ?? string.Empty,
                    NaturalezaStr = naturaleza?.Trim() ?? string.Empty,
                    CodigoPadre = string.IsNullOrWhiteSpace(codigoPadre) ? null : codigoPadre.Trim(),
                    Nivel = nivel > 0 ? nivel : CodigoToNivel(codigo),
                    EsMovimiento = ParseBool(esMovimientoStr),
                    Activo = ParseBool(activoStr),
                    Descripcion = string.IsNullOrWhiteSpace(descripcion) ? null : descripcion.Trim()
                });
            }
        }
        catch (Exception ex)
        {
            errores.Add(new ImportErrorDto { Mensaje = $"Error reading file: {ex.Message}" });
        }

        return (filas, errores);
    }

    private static string GetCellString(IXLWorksheet ws, int row, int col)
    {
        var cell = ws.Cell(row, col);
        var val = cell.Value;
        if (val.IsBlank)
            return string.Empty;
        return val.ToString()?.Trim() ?? string.Empty;
    }

    private static bool ParseBool(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return true;
        value = value.Trim().ToLowerInvariant();
        return value switch
        {
            "true" or "1" or "si" or "yes" or "verdadero" => true,
            "false" or "0" or "no" or "not" or "falso" => false,
            _ => true
        };
    }

    private static int CodigoToNivel(string codigo) => codigo.Count(c => c == '.') + 1;

    private static TipoCuenta MapTipo(string tipo) => tipo?.ToLowerInvariant() switch
    {
        "activo" => TipoCuenta.Activo,
        "pasivo" => TipoCuenta.Pasivo,
        "patrimonio" => TipoCuenta.Patrimonio,
        "ingreso" => TipoCuenta.Ingreso,
        "gasto" => TipoCuenta.Gasto,
        "costo" => TipoCuenta.Costo,
        _ => TipoCuenta.Activo
    };

    private static NaturalezaCuenta MapNaturaleza(string naturaleza) => naturaleza?.ToLowerInvariant() switch
    {
        "deudora" => NaturalezaCuenta.Deudora,
        "acreedora" => NaturalezaCuenta.Acreedora,
        _ => NaturalezaCuenta.Deudora
    };

    private static PlanCuentaImportRowDto MapToRowDto(FilaExcel fila) => new()
    {
        Codigo = fila.Codigo,
        Nombre = fila.Nombre,
        Tipo = fila.TipoCuenta,
        Naturaleza = fila.NaturalezaStr,
        Nivel = fila.Nivel,
        CuentaPadreCodigo = fila.CodigoPadre,
        PermiteMovimientos = fila.EsMovimiento,
        Descripcion = fila.Descripcion
    };

    private class FilaExcel
    {
        public int RowNumber { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string TipoCuenta { get; set; } = string.Empty;
        public string NaturalezaStr { get; set; } = string.Empty;
        public string? CodigoPadre { get; set; }
        public int Nivel { get; set; } = 1;
        public bool EsMovimiento { get; set; }
        public bool Activo { get; set; } = true;
        public string? Descripcion { get; set; }
    }
}
