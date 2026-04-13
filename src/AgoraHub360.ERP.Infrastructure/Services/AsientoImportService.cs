namespace AgoraHub360.ERP.Infrastructure.Services;

using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Domain.Entities.ACC;
using AgoraHub360.ERP.Domain.Entities.CST;
using AgoraHub360.ERP.Domain.Interfaces;
using AgoraHub360.ERP.Shared.DTOs.Contabilidad.Importacion;
using ClosedXML.Excel;
using Microsoft.Extensions.Logging;

/// <summary>
/// Implementación de <see cref="IAsientoImportService"/> que lee archivos Excel con ClosedXML,
/// valida la estructura y contenido, y persiste asientos contables agrupados por Fecha + Glosa.
/// </summary>
public class AsientoImportService : IAsientoImportService
{
    private const string NombreHoja = "Asientos";
    private const int MaxAsientosPorArchivo = 500;
    private const int FilaInicio = 2; // fila 1 = encabezados

    // Columnas esperadas (1-based)
    private const int ColFecha = 1;            // A
    private const int ColTipoComprobante = 2;  // B
    private const int ColGlosa = 3;            // C
    private const int ColNumeroLinea = 4;      // D
    private const int ColCodigoCuenta = 5;     // E
    private const int ColDebe = 6;             // F
    private const int ColHaber = 7;            // G
    private const int ColGlosaLinea = 8;       // H
    private const int ColCentroCosto = 9;      // I

    private readonly IRepository<AsientoContable> _asientoRepo;
    private readonly IRepository<AsientoContableLinea> _lineaRepo;
    private readonly IRepository<CuentaContable> _cuentaRepo;
    private readonly IRepository<CentroCosto> _centroCostoRepo;
    private readonly IRepository<TipoComprobante> _tipoCompRepo;
    private readonly IRepository<PeriodoContable> _periodoRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<AsientoImportService> _logger;

    public AsientoImportService(
        IRepository<AsientoContable> asientoRepo,
        IRepository<AsientoContableLinea> lineaRepo,
        IRepository<CuentaContable> cuentaRepo,
        IRepository<CentroCosto> centroCostoRepo,
        IRepository<TipoComprobante> tipoCompRepo,
        IRepository<PeriodoContable> periodoRepo,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        ILogger<AsientoImportService> logger)
    {
        _asientoRepo     = asientoRepo;
        _lineaRepo       = lineaRepo;
        _cuentaRepo      = cuentaRepo;
        _centroCostoRepo = centroCostoRepo;
        _tipoCompRepo    = tipoCompRepo;
        _periodoRepo     = periodoRepo;
        _unitOfWork      = unitOfWork;
        _currentUser     = currentUser;
        _logger          = logger;
    }

    // ─────────────────────────────────────────────────────────────
    // Contrato público
    // ─────────────────────────────────────────────────────────────

    public async Task<ImportValidacionDto> ValidarArchivoAsync(
        Stream excelStream, int empresaId, CancellationToken ct = default)
    {
        var (filas, erroresLectura) = LeerHojaAsientos(excelStream);

        if (erroresLectura.Count > 0)
            return new ImportValidacionDto { Errores = erroresLectura };

        var errores = new List<ImportErrorDto>();
        var cuentasCache = await CargarCuentasAsync(empresaId, ct);
        var centrosCache = await CargarCentrosCostoAsync(empresaId, ct);
        var tiposCache   = await CargarTiposComprobanteAsync(empresaId, ct);

        foreach (var fila in filas)
            ValidarFila(fila, cuentasCache, centrosCache, tiposCache, errores);

        var grupos = AgruparAsientos(filas);

        if (grupos.Count > MaxAsientosPorArchivo)
            errores.Add(new ImportErrorDto
            {
                Mensaje = $"El archivo contiene {grupos.Count} asientos. Máximo permitido: {MaxAsientosPorArchivo}."
            });

        ValidarCuadratura(grupos, errores);

        return new ImportValidacionDto
        {
            EsValido                = errores.Count == 0,
            TotalFilas              = filas.Count,
            TotalAsientosDetectados = grupos.Count,
            Errores                 = errores
        };
    }

    public async Task<ImportResultDto> ImportarAsync(
        Stream excelStream, int empresaId,
        bool contabilizarInmediatamente = false, CancellationToken ct = default)
    {
        // 1. Validar primero
        excelStream.Position = 0;
        var validacion = await ValidarArchivoAsync(excelStream, empresaId, ct);

        if (!validacion.EsValido)
        {
            return new ImportResultDto
            {
                TotalErrores = validacion.Errores.Count,
                Errores      = validacion.Errores
            };
        }

        // 2. Releer filas (stream ya fue consumido por la validación)
        excelStream.Position = 0;
        var (filas, _) = LeerHojaAsientos(excelStream);
        var grupos = AgruparAsientos(filas);

        // 3. Cachés de catálogos
        var cuentasCache = await CargarCuentasAsync(empresaId, ct);
        var centrosCache = await CargarCentrosCostoAsync(empresaId, ct);
        var tiposCache   = await CargarTiposComprobanteAsync(empresaId, ct);

        var errores = new List<ImportErrorDto>();
        int totalAsientos = 0;
        int totalLineas   = 0;

        foreach (var grupo in grupos)
        {
            try
            {
                var (asientosCreados, lineasCreadas) = await CrearAsientoDesdeGrupoAsync(
                    grupo, empresaId, cuentasCache, centrosCache, tiposCache,
                    contabilizarInmediatamente, ct);

                totalAsientos += asientosCreados;
                totalLineas   += lineasCreadas;
            }
            catch (Exception ex)
            {
                var primeraFila = grupo.Filas.FirstOrDefault()?.NumeroFila;
                errores.Add(new ImportErrorDto
                {
                    Fila    = primeraFila,
                    Mensaje = $"Error al crear asiento (Fecha={grupo.Fecha:dd/MM/yyyy}, Glosa='{grupo.Glosa}'): {ex.Message}"
                });
                _logger.LogError(ex,
                    "Error importando asiento Fecha={Fecha} Glosa={Glosa} EmpresaId={EmpresaId}",
                    grupo.Fecha, grupo.Glosa, empresaId);
            }
        }

        _logger.LogInformation(
            "Importación finalizada: {Asientos} asientos, {Lineas} líneas, {Errores} errores — EmpresaId={EmpresaId}",
            totalAsientos, totalLineas, errores.Count, empresaId);

        return new ImportResultDto
        {
            TotalAsientosImportados = totalAsientos,
            TotalLineasImportadas   = totalLineas,
            TotalErrores            = errores.Count,
            Errores                 = errores
        };
    }

    // ─────────────────────────────────────────────────────────────
    // Lectura de Excel
    // ─────────────────────────────────────────────────────────────

    private static (List<FilaExcel> Filas, List<ImportErrorDto> Errores) LeerHojaAsientos(Stream excelStream)
    {
        var filas   = new List<FilaExcel>();
        var errores = new List<ImportErrorDto>();

        using var workbook = new XLWorkbook(excelStream);

        if (!workbook.Worksheets.TryGetWorksheet(NombreHoja, out var ws))
        {
            errores.Add(new ImportErrorDto
            {
                Mensaje = $"No se encontró la hoja '{NombreHoja}' en el archivo."
            });
            return (filas, errores);
        }

        var ultimaFila = ws.LastRowUsed()?.RowNumber() ?? 1;
        if (ultimaFila < FilaInicio)
        {
            errores.Add(new ImportErrorDto { Mensaje = "La hoja no contiene datos." });
            return (filas, errores);
        }

        for (int row = FilaInicio; row <= ultimaFila; row++)
        {
            var wsRow = ws.Row(row);

            // Ignorar filas completamente vacías
            if (wsRow.IsEmpty())
                continue;

            var fila = new FilaExcel
            {
                NumeroFila         = row,
                FechaTexto         = wsRow.Cell(ColFecha).GetFormattedString().Trim(),
                TipoComprobante    = wsRow.Cell(ColTipoComprobante).GetFormattedString().Trim(),
                Glosa              = wsRow.Cell(ColGlosa).GetFormattedString().Trim(),
                NumeroLineaTexto   = wsRow.Cell(ColNumeroLinea).GetFormattedString().Trim(),
                CodigoCuenta       = wsRow.Cell(ColCodigoCuenta).GetFormattedString().Trim(),
                DebeTexto          = wsRow.Cell(ColDebe).GetFormattedString().Trim(),
                HaberTexto         = wsRow.Cell(ColHaber).GetFormattedString().Trim(),
                GlosaLinea         = wsRow.Cell(ColGlosaLinea).GetFormattedString().Trim(),
                CodigoCentroCosto  = wsRow.Cell(ColCentroCosto).GetFormattedString().Trim()
            };

            // Parsear fecha
            if (wsRow.Cell(ColFecha).DataType == XLDataType.DateTime)
                fila.Fecha = wsRow.Cell(ColFecha).GetDateTime();
            else if (DateTime.TryParse(fila.FechaTexto, out var fechaParsed))
                fila.Fecha = fechaParsed;

            // Parsear numéricos
            if (decimal.TryParse(fila.DebeTexto, System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out var debe))
                fila.Debe = debe;

            if (decimal.TryParse(fila.HaberTexto, System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out var haber))
                fila.Haber = haber;

            if (int.TryParse(fila.NumeroLineaTexto, out var numLinea))
                fila.NumeroLinea = numLinea;

            filas.Add(fila);
        }

        if (filas.Count == 0)
            errores.Add(new ImportErrorDto { Mensaje = "La hoja no contiene filas de datos válidas." });

        return (filas, errores);
    }

    // ─────────────────────────────────────────────────────────────
    // Validación por fila
    // ─────────────────────────────────────────────────────────────

    private static void ValidarFila(
        FilaExcel fila,
        Dictionary<string, CuentaContable> cuentas,
        Dictionary<string, CentroCosto> centros,
        Dictionary<string, TipoComprobante> tipos,
        List<ImportErrorDto> errores)
    {
        int row = fila.NumeroFila;

        // 1. Fecha válida
        if (!fila.Fecha.HasValue)
            errores.Add(new ImportErrorDto { Fila = row, Mensaje = $"Fecha inválida o vacía: '{fila.FechaTexto}'." });

        // 2. Tipo comprobante válido
        if (string.IsNullOrWhiteSpace(fila.TipoComprobante))
            errores.Add(new ImportErrorDto { Fila = row, Mensaje = "TipoComprobante vacío." });
        else if (!tipos.ContainsKey(fila.TipoComprobante.ToUpperInvariant()))
            errores.Add(new ImportErrorDto { Fila = row, Mensaje = $"TipoComprobante '{fila.TipoComprobante}' no encontrado en catálogos." });

        // 3. Glosa obligatoria
        if (string.IsNullOrWhiteSpace(fila.Glosa))
            errores.Add(new ImportErrorDto { Fila = row, Mensaje = "Glosa general vacía." });

        // 4. Código cuenta existe y permite movimientos
        if (string.IsNullOrWhiteSpace(fila.CodigoCuenta))
        {
            errores.Add(new ImportErrorDto { Fila = row, Mensaje = "CodigoCuenta vacío." });
        }
        else if (!cuentas.TryGetValue(fila.CodigoCuenta, out var cuenta))
        {
            errores.Add(new ImportErrorDto { Fila = row, Mensaje = $"Cuenta '{fila.CodigoCuenta}' no encontrada." });
        }
        else if (!cuenta.PermiteMovimientos)
        {
            errores.Add(new ImportErrorDto { Fila = row, Mensaje = $"Cuenta '{fila.CodigoCuenta}' es agrupadora y no permite movimientos." });
        }

        // 5. Debe y Haber no ambos > 0
        if (fila.Debe > 0 && fila.Haber > 0)
            errores.Add(new ImportErrorDto { Fila = row, Mensaje = "Debe y Haber no pueden ser ambos mayores a cero en la misma línea." });

        // 6. Al menos uno de Debe/Haber > 0
        if (fila.Debe == 0 && fila.Haber == 0)
            errores.Add(new ImportErrorDto { Fila = row, Mensaje = "La línea debe tener un monto en Debe o en Haber." });

        // 7. Montos no negativos
        if (fila.Debe < 0)
            errores.Add(new ImportErrorDto { Fila = row, Mensaje = "El monto Debe no puede ser negativo." });
        if (fila.Haber < 0)
            errores.Add(new ImportErrorDto { Fila = row, Mensaje = "El monto Haber no puede ser negativo." });

        // 8. Centro de costo (opcional, pero si viene debe existir)
        if (!string.IsNullOrWhiteSpace(fila.CodigoCentroCosto) &&
            !centros.ContainsKey(fila.CodigoCentroCosto))
        {
            errores.Add(new ImportErrorDto { Fila = row, Mensaje = $"Centro de costo '{fila.CodigoCentroCosto}' no encontrado." });
        }
    }

    // ─────────────────────────────────────────────────────────────
    // Agrupación y cuadratura
    // ─────────────────────────────────────────────────────────────

    private static List<GrupoAsiento> AgruparAsientos(List<FilaExcel> filas)
    {
        return filas
            .Where(f => f.Fecha.HasValue)
            .GroupBy(f => (Fecha: f.Fecha!.Value.Date, f.TipoComprobante, f.Glosa))
            .Select(g => new GrupoAsiento
            {
                Fecha           = g.Key.Fecha,
                TipoComprobante = g.Key.TipoComprobante,
                Glosa           = g.Key.Glosa,
                Filas           = g.ToList()
            })
            .ToList();
    }

    private static void ValidarCuadratura(List<GrupoAsiento> grupos, List<ImportErrorDto> errores)
    {
        foreach (var g in grupos)
        {
            var totalDebe  = g.Filas.Sum(f => f.Debe);
            var totalHaber = g.Filas.Sum(f => f.Haber);
            var diferencia = Math.Abs(totalDebe - totalHaber);

            if (diferencia > 0.01m)
            {
                var primeraFila = g.Filas.FirstOrDefault()?.NumeroFila;
                errores.Add(new ImportErrorDto
                {
                    Fila    = primeraFila,
                    Mensaje = $"Asiento (Fecha={g.Fecha:dd/MM/yyyy}, Glosa='{g.Glosa}') no cuadra. " +
                              $"Debe={totalDebe:N2}, Haber={totalHaber:N2}, Diferencia={diferencia:N2}."
                });
            }

            if (g.Filas.Count < 2)
            {
                var primeraFila = g.Filas.FirstOrDefault()?.NumeroFila;
                errores.Add(new ImportErrorDto
                {
                    Fila    = primeraFila,
                    Mensaje = $"Asiento (Fecha={g.Fecha:dd/MM/yyyy}, Glosa='{g.Glosa}') requiere al menos 2 líneas."
                });
            }
        }
    }

    // ─────────────────────────────────────────────────────────────
    // Persistencia
    // ─────────────────────────────────────────────────────────────

    private async Task<(int Asientos, int Lineas)> CrearAsientoDesdeGrupoAsync(
        GrupoAsiento grupo, int empresaId,
        Dictionary<string, CuentaContable> cuentasCache,
        Dictionary<string, CentroCosto> centrosCache,
        Dictionary<string, TipoComprobante> tiposCache,
        bool contabilizar, CancellationToken ct)
    {
        var tipoComp = tiposCache[grupo.TipoComprobante.ToUpperInvariant()];
        var gestion  = grupo.Fecha.Year;

        var numero = await GenerarNumeroComprobanteAsync(empresaId, tipoComp, gestion, ct);

        var totalDebe  = grupo.Filas.Sum(f => f.Debe);
        var totalHaber = grupo.Filas.Sum(f => f.Haber);

        var asiento = new AsientoContable
        {
            EmpresaId           = empresaId,
            TipoComprobanteId   = tipoComp.TipoComprobanteId,
            Numero              = numero,
            Fecha               = grupo.Fecha,
            Gestion             = gestion,
            TipoRegistro        = "Manual",
            Estado              = "Borrador",
            Glosa               = grupo.Glosa,
            RegistradoPorId     = _currentUser.UserIdInt,
            RegistradoPorNombre = _currentUser.UserName,
            OrigenTipo          = "ImportacionExcel",
            Activo              = true
        };

        asiento.EstablecerTotales(totalDebe, totalHaber);

        if (contabilizar)
            asiento.Contabilizar(_currentUser.UserName);

        await _asientoRepo.AddAsync(asiento, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        int lineNum = 1;
        foreach (var fila in grupo.Filas.OrderBy(f => f.NumeroLinea ?? f.NumeroFila))
        {
            var cuenta = cuentasCache[fila.CodigoCuenta];
            int? centroCostoId = null;

            if (!string.IsNullOrWhiteSpace(fila.CodigoCentroCosto) &&
                centrosCache.TryGetValue(fila.CodigoCentroCosto, out var cc))
            {
                centroCostoId = cc.Id;
            }

            await _lineaRepo.AddAsync(new AsientoContableLinea
            {
                AsientoContableId = asiento.AsientoContableId,
                NumeroLinea       = fila.NumeroLinea ?? lineNum,
                CuentaContableId  = cuenta.CuentaContableId,
                Debe              = fila.Debe,
                Haber             = fila.Haber,
                Glosa             = string.IsNullOrWhiteSpace(fila.GlosaLinea) ? grupo.Glosa : fila.GlosaLinea,
                CentroCostoId     = centroCostoId,
                Activo            = true
            }, ct);
            lineNum++;
        }

        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogDebug(
            "Asiento {Numero} creado: {Lineas} líneas, Debe={Debe:N2} Haber={Haber:N2}",
            numero, grupo.Filas.Count, totalDebe, totalHaber);

        return (1, grupo.Filas.Count);
    }

    // ─────────────────────────────────────────────────────────────
    // Catálogos (carga en caché por empresa)
    // ─────────────────────────────────────────────────────────────

    private async Task<Dictionary<string, CuentaContable>> CargarCuentasAsync(
        int empresaId, CancellationToken ct)
    {
        var cuentas = await _cuentaRepo.FindAsync(
            c => c.EmpresaId == empresaId && c.Activo, ct);

        return cuentas.ToDictionary(c => c.Codigo, c => c);
    }

    private async Task<Dictionary<string, CentroCosto>> CargarCentrosCostoAsync(
        int empresaId, CancellationToken ct)
    {
        var centros = await _centroCostoRepo.FindAsync(
            c => c.EmpresaId == empresaId && c.Activo, ct);

        return centros.ToDictionary(c => c.Codigo, c => c);
    }

    private async Task<Dictionary<string, TipoComprobante>> CargarTiposComprobanteAsync(
        int empresaId, CancellationToken ct)
    {
        var tipos = await _tipoCompRepo.FindAsync(
            t => t.EmpresaId == empresaId && t.Activo, ct);

        return tipos.ToDictionary(t => t.Codigo.ToUpperInvariant(), t => t);
    }

    private async Task<string> GenerarNumeroComprobanteAsync(
        int empresaId, TipoComprobante tipo, int gestion, CancellationToken ct)
    {
        var existentes = await _asientoRepo.FindAsync(
            a => a.EmpresaId == empresaId
                 && a.TipoComprobanteId == tipo.TipoComprobanteId
                 && a.Gestion == gestion, ct);

        var siguiente = existentes.Count + 1;
        return $"{tipo.Prefijo}-{siguiente:D4}";
    }

    // ─────────────────────────────────────────────────────────────
    // Plantilla de importación
    // ─────────────────────────────────────────────────────────────

    public byte[] GenerarPlantillaImportacion()
    {
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add(NombreHoja);

        // Encabezados
        string[] headers =
        [
            "Fecha", "TipoComprobante", "Glosa general", "NumeroLinea",
            "CodigoCuenta", "Debe", "Haber", "Glosa linea", "CodigoCentroCosto"
        ];

        for (int i = 0; i < headers.Length; i++)
        {
            var cell = ws.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#4472C4");
            cell.Style.Font.FontColor = XLColor.White;
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        }

        // Fila de ejemplo
        ws.Cell(2, 1).Value = DateTime.Today;
        ws.Cell(2, 1).Style.DateFormat.Format = "dd/MM/yyyy";
        ws.Cell(2, 2).Value = "ING";
        ws.Cell(2, 3).Value = "Venta de mercadería";
        ws.Cell(2, 4).Value = 1;
        ws.Cell(2, 5).Value = "1.1.1.01";
        ws.Cell(2, 6).Value = 1000.00m;
        ws.Cell(2, 7).Value = 0m;
        ws.Cell(2, 8).Value = "Cobro cliente X";
        ws.Cell(2, 9).Value = "";

        ws.Cell(3, 1).Value = DateTime.Today;
        ws.Cell(3, 1).Style.DateFormat.Format = "dd/MM/yyyy";
        ws.Cell(3, 2).Value = "ING";
        ws.Cell(3, 3).Value = "Venta de mercadería";
        ws.Cell(3, 4).Value = 2;
        ws.Cell(3, 5).Value = "4.1.1.01";
        ws.Cell(3, 6).Value = 0m;
        ws.Cell(3, 7).Value = 1000.00m;
        ws.Cell(3, 8).Value = "Ingreso por venta";
        ws.Cell(3, 9).Value = "CC-VEN-01";

        // Formato de columnas numéricas
        ws.Column(6).Style.NumberFormat.Format = "#,##0.00";
        ws.Column(7).Style.NumberFormat.Format = "#,##0.00";

        ws.Columns().AdjustToContents();

        using var ms = new MemoryStream();
        workbook.SaveAs(ms);
        return ms.ToArray();
    }

    // ─────────────────────────────────────────────────────────────
    // DTOs internos de lectura
    // ─────────────────────────────────────────────────────────────

    private sealed class FilaExcel
    {
        public int NumeroFila { get; init; }

        // Textos crudos del Excel
        public string FechaTexto { get; init; } = string.Empty;
        public string TipoComprobante { get; init; } = string.Empty;
        public string Glosa { get; init; } = string.Empty;
        public string NumeroLineaTexto { get; init; } = string.Empty;
        public string CodigoCuenta { get; init; } = string.Empty;
        public string DebeTexto { get; init; } = string.Empty;
        public string HaberTexto { get; init; } = string.Empty;
        public string GlosaLinea { get; init; } = string.Empty;
        public string CodigoCentroCosto { get; init; } = string.Empty;

        // Valores parseados
        public DateTime? Fecha { get; set; }
        public int? NumeroLinea { get; set; }
        public decimal Debe { get; set; }
        public decimal Haber { get; set; }
    }

    private sealed class GrupoAsiento
    {
        public DateTime Fecha { get; init; }
        public string TipoComprobante { get; init; } = string.Empty;
        public string Glosa { get; init; } = string.Empty;
        public List<FilaExcel> Filas { get; init; } = new();
    }
}
