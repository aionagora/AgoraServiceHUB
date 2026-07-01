namespace AgoraHub360.ERP.Application.Services;

using System.Text;
using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Domain.Entities.ACC;
using AgoraHub360.ERP.Domain.Enums;
using AgoraHub360.ERP.Domain.Interfaces;
using AgoraHub360.ERP.Shared.DTOs.Contabilidad;
using AgoraHub360.ERP.Shared.DTOs.Contabilidad.Importacion;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

public class CuentaContableService : ICuentaContableService
{
    private readonly IRepository<CuentaContable> _repo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IMemoryCache _cache;

    private const string CacheKeyPrefix = "plan-cuentas";
    private static readonly TimeSpan CacheTTL = TimeSpan.FromMinutes(30);
    private static readonly TimeSpan CacheSlidingExpiration = TimeSpan.FromMinutes(10);

    public CuentaContableService(
        IRepository<CuentaContable> repo,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        IMemoryCache cache)
    {
        _repo = repo;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _cache = cache;
    }

    public async Task<Result<IReadOnlyList<CuentaContableDto>>> GetAllAsync(
        byte? tipo, bool? permiteMovimientos, string? search, CancellationToken ct)
    {
        var empresaId = _currentUser.EmpresaId;
        if (!empresaId.HasValue)
            return Result<IReadOnlyList<CuentaContableDto>>.Failure("No active company.");

        var flatList = await GetFlatCachedAsync(empresaId.Value, ct);

        // Apply optional filters in-memory over the cached flat list
        var filtered = flatList.AsEnumerable();
        if (tipo.HasValue)
            filtered = filtered.Where(c => c.Tipo == ((TipoCuenta)tipo.Value).ToString());
        if (permiteMovimientos.HasValue)
            filtered = filtered.Where(c => c.PermiteMovimientos == permiteMovimientos.Value);
        if (!string.IsNullOrEmpty(search))
            filtered = filtered.Where(c =>
                c.Codigo.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                c.Nombre.Contains(search, StringComparison.OrdinalIgnoreCase));

        return Result<IReadOnlyList<CuentaContableDto>>.Success(filtered.ToList().AsReadOnly());
    }

    public async Task<Result<IReadOnlyList<CuentaContableDto>>> GetTreeAsync(CancellationToken ct)
    {
        var empresaId = _currentUser.EmpresaId;
        if (!empresaId.HasValue)
            return Result<IReadOnlyList<CuentaContableDto>>.Failure("No active company.");

        var treeKey = $"{CacheKeyPrefix}-tree-{empresaId.Value}";

        if (!_cache.TryGetValue(treeKey, out IReadOnlyList<CuentaContableDto>? cachedTree) || cachedTree is null)
        {
            var flatList = await GetFlatCachedAsync(empresaId.Value, ct);

            var dtoMap = flatList.ToDictionary(c => c.CuentaContableId);
            var roots = new List<CuentaContableDto>();

            foreach (var dto in dtoMap.Values)
            {
                if (dto.CuentaPadreId.HasValue && dtoMap.TryGetValue(dto.CuentaPadreId.Value, out var padre))
                    padre.SubCuentas.Add(dto);
                else
                    roots.Add(dto);
            }

            cachedTree = roots.AsReadOnly();
            _cache.Set(treeKey, cachedTree, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = CacheTTL,
                SlidingExpiration = CacheSlidingExpiration,
                Priority = CacheItemPriority.High
            });
        }

        return Result<IReadOnlyList<CuentaContableDto>>.Success(cachedTree);
    }

    public async Task<Result<CuentaContableDto>> GetByIdAsync(int id, CancellationToken ct)
    {
        var empresaId = _currentUser.EmpresaId;
        if (!empresaId.HasValue)
            return Result<CuentaContableDto>.Failure("No active company.");

        var cuenta = await _repo.GetByIdAsync(id, ct);
        if (cuenta is null || cuenta.EmpresaId != empresaId.Value || !cuenta.Activo)
            return Result<CuentaContableDto>.Failure("Account not found.");

        var all = await _repo.FindAsync(c => c.EmpresaId == empresaId.Value && c.Activo, ct);
        var padreMap = all.ToDictionary(c => c.CuentaContableId);

        return Result<CuentaContableDto>.Success(MapToDto(cuenta, padreMap));
    }

    public async Task<Result<CuentaContableDto>> CreateAsync(CreateCuentaContableDto dto, CancellationToken ct)
    {
        var empresaId = _currentUser.EmpresaId;
        if (!empresaId.HasValue)
            return Result<CuentaContableDto>.Failure("No active company.");

        // Validate unique code
        var existentes = await _repo.FindAsync(
            c => c.EmpresaId == empresaId.Value && c.Codigo == dto.Codigo && c.Activo, ct);
        if (existentes.Count > 0)
            return Result<CuentaContableDto>.Failure($"Account code '{dto.Codigo}' already exists.");

        // Validate parent
        if (dto.CuentaPadreId.HasValue)
        {
            var padre = await _repo.GetByIdAsync(dto.CuentaPadreId.Value, ct);
            if (padre is null || padre.EmpresaId != empresaId.Value || !padre.Activo)
                return Result<CuentaContableDto>.Failure("Parent account not found.");
            if (padre.PermiteMovimientos)
                return Result<CuentaContableDto>.Failure("Cannot create sub-accounts under an account that allows transactions. Change the parent to a grouping account first.");
        }

        var cuenta = new CuentaContable
        {
            EmpresaId = empresaId.Value,
            Codigo = dto.Codigo,
            Nombre = dto.Nombre,
            Tipo = (TipoCuenta)dto.Tipo,
            Naturaleza = (NaturalezaCuenta)dto.Naturaleza,
            Nivel = dto.Nivel,
            CuentaPadreId = dto.CuentaPadreId,
            PermiteMovimientos = dto.PermiteMovimientos,
            Descripcion = dto.Descripcion,
            SaldoActual = 0,
            Activo = true
        };

        await _repo.AddAsync(cuenta, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        InvalidateCache(empresaId.Value);

        return Result<CuentaContableDto>.Success(MapToDto(cuenta, new Dictionary<int, CuentaContable>()));
    }

    public async Task<Result<CuentaContableDto>> UpdateAsync(int id, UpdateCuentaContableDto dto, CancellationToken ct)
    {
        var empresaId = _currentUser.EmpresaId;
        if (!empresaId.HasValue)
            return Result<CuentaContableDto>.Failure("No active company.");

        var cuenta = await _repo.GetByIdAsync(id, ct);
        if (cuenta is null || cuenta.EmpresaId != empresaId.Value || !cuenta.Activo)
            return Result<CuentaContableDto>.Failure("Account not found.");

        // If disabling movements, check no sub-accounts exist
        if (dto.PermiteMovimientos && !cuenta.PermiteMovimientos)
        {
            var hijos = await _repo.FindAsync(c => c.CuentaPadreId == id && c.Activo, ct);
            if (hijos.Count > 0)
                return Result<CuentaContableDto>.Failure("Cannot enable transactions on a grouping account with sub-accounts.");
        }

        cuenta.Nombre = dto.Nombre;
        cuenta.Tipo = (TipoCuenta)dto.Tipo;
        cuenta.Naturaleza = (NaturalezaCuenta)dto.Naturaleza;
        cuenta.PermiteMovimientos = dto.PermiteMovimientos;
        cuenta.Descripcion = dto.Descripcion;

        await _repo.UpdateAsync(cuenta, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        InvalidateCache(empresaId.Value);

        var all = await _repo.FindAsync(c => c.EmpresaId == empresaId.Value && c.Activo, ct);
        return Result<CuentaContableDto>.Success(MapToDto(cuenta, all.ToDictionary(c => c.CuentaContableId)));
    }

    public async Task<Result<bool>> DeleteAsync(int id, CancellationToken ct)
    {
        var empresaId = _currentUser.EmpresaId;
        if (!empresaId.HasValue)
            return Result<bool>.Failure("No active company.");

        var cuenta = await _repo.GetByIdAsync(id, ct);
        if (cuenta is null || cuenta.EmpresaId != empresaId.Value)
            return Result<bool>.Failure("Account not found.");

        if (cuenta.SaldoActual != 0)
            return Result<bool>.Failure("Cannot delete an account with a balance. Adjust the balance first.");

        // Check no sub-accounts
        var hijos = await _repo.FindAsync(c => c.CuentaPadreId == id && c.Activo, ct);
        if (hijos.Count > 0)
            return Result<bool>.Failure("Cannot delete an account with sub-accounts. Delete sub-accounts first.");

        cuenta.Activo = false;
        await _repo.UpdateAsync(cuenta, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        InvalidateCache(empresaId.Value);

        return Result<bool>.Success(true);
    }

    public async Task<Result<int>> SeedPlanCuentasAsync(CancellationToken ct)
    {
        var empresaId = _currentUser.EmpresaId;
        if (!empresaId.HasValue)
            return Result<int>.Failure("No active company.");

        // Check if already seeded
        var existentes = await _repo.FindAsync(c => c.EmpresaId == empresaId.Value, ct);
        if (existentes.Count > 0)
            return Result<int>.Failure("Chart of accounts already exists for this company. Delete existing accounts first or add accounts manually.");

        var cuentas = BuildPlanCuentasBolivia(empresaId.Value);

        // First pass: add all level-1 accounts
        var nivel1 = cuentas.Where(c => c.Nivel == 1).ToList();
        foreach (var c in nivel1)
            await _repo.AddAsync(c, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        // Build code ? id map for parent resolution
        var codeMap = new Dictionary<string, int>();
        foreach (var c in nivel1)
            codeMap[c.Codigo] = c.CuentaContableId;

        // Add remaining levels in order
        for (int nivel = 2; nivel <= 4; nivel++)
        {
            var nivelCuentas = cuentas.Where(c => c.Nivel == nivel).ToList();
            foreach (var c in nivelCuentas)
            {
                // Resolve parent from code
                var parentCode = GetParentCode(c.Codigo);
                if (parentCode != null && codeMap.TryGetValue(parentCode, out var parentId))
                    c.CuentaPadreId = parentId;

                await _repo.AddAsync(c, ct);
            }
            await _unitOfWork.SaveChangesAsync(ct);

            // Update code map with new ids
            var saved = await _repo.FindAsync(cc => cc.EmpresaId == empresaId.Value && cc.Nivel == nivel, ct);
            foreach (var s in saved)
                codeMap[s.Codigo] = s.CuentaContableId;
        }

        InvalidateCache(empresaId.Value);
        return Result<int>.Success(cuentas.Count);
    }

    // ── Importación CSV ─────────────────────────────────────────────────────

    public async Task<Result<PlanCuentaImportPreviewDto>> PreviewImportAsync(
        string csvContent, CancellationToken ct)
    {
        var empresaId = _currentUser.EmpresaId;
        if (!empresaId.HasValue)
            return Result<PlanCuentaImportPreviewDto>.Failure("No active company.");

        if (string.IsNullOrWhiteSpace(csvContent))
            return Result<PlanCuentaImportPreviewDto>.Failure("CSV content is empty.");

        var preview = new PlanCuentaImportPreviewDto();
        var allRows = new List<PlanCuentaImportRowDto>();

        var lines = csvContent.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length < 2)
        {
            preview.CanImport = false;
            preview.TotalRows = lines.Length;
            return Result<PlanCuentaImportPreviewDto>.Success(preview);
        }

        // Skip header row
        var dataLines = lines.Skip(1).Where(l => !string.IsNullOrWhiteSpace(l.Trim('\r', ' '))).ToArray();
        preview.TotalRows = dataLines.Length;

        // Pre-load existing codes for this company
        var existentes = await _repo.FindAsync(c => c.EmpresaId == empresaId.Value && c.Activo, ct);
        var codigosExistentes = new HashSet<string>(existentes.Select(c => c.Codigo), StringComparer.OrdinalIgnoreCase);
        var codigosEnArchivo = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var rowsByCodigo = new Dictionary<string, PlanCuentaImportRowDto>(StringComparer.OrdinalIgnoreCase);

        int filaNum = 0;
        foreach (var rawLine in dataLines)
        {
            filaNum++;
            var line = rawLine.Trim('\r', ' ');
            if (string.IsNullOrWhiteSpace(line)) continue;

            var row = new PlanCuentaImportRowDto { RowNumber = filaNum };
            var errors = new List<string>();

            var cols = ParseCsvLine(line);

            // ── Codigo (required) ──
            if (cols.Count < 1 || string.IsNullOrWhiteSpace(cols[0]))
                errors.Add("El código es obligatorio.");
            else
            {
                row.Codigo = cols[0].Trim();
                if (!codigosEnArchivo.Add(row.Codigo))
                    errors.Add($"Código duplicado en el archivo: '{row.Codigo}'.");
                else if (codigosExistentes.Contains(row.Codigo))
                    errors.Add($"El código '{row.Codigo}' ya existe en el plan de cuentas.");
            }

            // ── Nombre (required) ──
            if (cols.Count < 2 || string.IsNullOrWhiteSpace(cols[1]))
                errors.Add("El nombre es obligatorio.");
            else
                row.Nombre = cols[1].Trim();

            // ── TipoCuenta (required) ──
            if (cols.Count < 3 || string.IsNullOrWhiteSpace(cols[2]))
                errors.Add("El tipo de cuenta es obligatorio.");
            else
            {
                row.TipoCuenta = cols[2].Trim();
                if (MapTipoCuenta(row.TipoCuenta) is null)
                    errors.Add($"Tipo de cuenta inválido '{row.TipoCuenta}'. Valores: Activo, Pasivo, Patrimonio, Ingreso, Gasto, Costo.");
            }

            // ── CodigoPadre (optional) ──
            if (cols.Count > 4 && !string.IsNullOrWhiteSpace(cols[4]))
            {
                row.CodigoPadre = cols[4].Trim();
                bool parentInFile = rowsByCodigo.ContainsKey(row.CodigoPadre) || codigosEnArchivo.Contains(row.CodigoPadre);
                bool parentInDb = codigosExistentes.Contains(row.CodigoPadre);
                if (!parentInFile && !parentInDb)
                    errors.Add($"El código de cuenta padre '{row.CodigoPadre}' no existe en el archivo ni en la BD.");
            }

            // ── Nivel ──
            if (cols.Count > 5 && int.TryParse(cols[5].Trim(), out var parsedNivel) && parsedNivel > 0)
                row.Nivel = parsedNivel;
            else
                row.Nivel = row.Codigo.Count(c => c == '.') + 1;

            // ── EsMovimiento ──
            if (cols.Count > 6)
                row.EsMovimiento = cols[6].Trim().ToLowerInvariant() switch
                {
                    "true" or "1" or "yes" or "sí" or "si" => true,
                    _ => false
                };

            // ── Activo ──
            if (cols.Count > 7)
                row.Activo = cols[7].Trim().ToLowerInvariant() switch
                {
                    "false" or "0" or "no" => false,
                    _ => true
                };

            // Validate parent not movimiento
            if (row.CodigoPadre is not null && rowsByCodigo.TryGetValue(row.CodigoPadre, out var parentRow) && parentRow.EsMovimiento)
                errors.Add($"La cuenta padre '{row.CodigoPadre}' es de detalle (EsMovimiento=true). No puede tener subcuentas.");

            // Validate level > 1 requires parent
            if (row.Nivel > 1 && string.IsNullOrEmpty(row.CodigoPadre))
                errors.Add($"La cuenta tiene nivel {row.Nivel} (>1) y requiere un código de cuenta padre.");

            row.IsValid = errors.Count == 0;
            row.Errors = errors;
            allRows.Add(row);
            if (row.IsValid)
                rowsByCodigo[row.Codigo] = row;
        }

        preview.Rows = allRows;
        preview.ValidRows = allRows.Count(r => r.IsValid);
        preview.InvalidRows = allRows.Count(r => !r.IsValid);
        preview.CanImport = preview.ValidRows > 0 && preview.InvalidRows == 0;

        return Result<PlanCuentaImportPreviewDto>.Success(preview);
    }

    public async Task<Result<PlanCuentaImportResultDto>> ConfirmImportAsync(
        string csvContent, CancellationToken ct)
    {
        var empresaId = _currentUser.EmpresaId;
        if (!empresaId.HasValue)
            return Result<PlanCuentaImportResultDto>.Failure("No active company.");

        if (string.IsNullOrWhiteSpace(csvContent))
            return Result<PlanCuentaImportResultDto>.Failure("CSV content is empty.");

        var previewResult = await PreviewImportAsync(csvContent, ct);
        if (!previewResult.IsSuccess)
            return Result<PlanCuentaImportResultDto>.Failure(previewResult.Error!);

        var preview = previewResult.Value!;
        if (!preview.CanImport || preview.Rows.Count == 0)
            return Result<PlanCuentaImportResultDto>.Failure("La validación previa falló. Corrija los errores antes de importar.");

        var validRows = preview.Rows.Where(r => r.IsValid).ToList();
        var result = new PlanCuentaImportResultDto();
        var errorRows = new List<ImportErrorDto>();

        // Pre-load existing codes for parent ID resolution
        var existentes = await _repo.FindAsync(c => c.EmpresaId == empresaId.Value && c.Activo, ct);
        var codigoToIdMap = existentes.ToDictionary(c => c.Codigo, c => (int?)c.CuentaContableId, StringComparer.OrdinalIgnoreCase);

        // Insert in order: parents before children
        var ordered = validRows.OrderBy(r => r.Nivel).ThenBy(r => r.Codigo.Length).ThenBy(r => r.Codigo).ToList();
        var inserted = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        int creadas = 0;

        foreach (var row in ordered)
        {
            try
            {
                int? padreId = null;
                if (!string.IsNullOrEmpty(row.CodigoPadre))
                {
                    if (inserted.TryGetValue(row.CodigoPadre, out var newId))
                        padreId = newId;
                    else if (codigoToIdMap.TryGetValue(row.CodigoPadre, out var existingId))
                        padreId = existingId;
                }

                var cuenta = new CuentaContable
                {
                    EmpresaId = empresaId.Value,
                    Codigo = row.Codigo,
                    Nombre = row.Nombre,
                    Tipo = MapTipoCuenta(row.TipoCuenta) ?? TipoCuenta.Activo,
                    Naturaleza = NaturalezaCuenta.Deudora,
                    Nivel = row.Nivel,
                    CuentaPadreId = padreId,
                    PermiteMovimientos = row.EsMovimiento,
                    Descripcion = null,
                    SaldoActual = 0,
                    Activo = row.Activo
                };

                await _repo.AddAsync(cuenta, ct);
                await _unitOfWork.SaveChangesAsync(ct);
                inserted[row.Codigo] = cuenta.CuentaContableId;
                creadas++;
            }
            catch (Exception ex)
            {
                errorRows.Add(new ImportErrorDto { Fila = row.RowNumber, Mensaje = $"Error al importar '{row.Codigo}': {ex.Message}" });
            }
        }

        if (creadas > 0) InvalidateCache(empresaId.Value);

        result.Created = creadas;
        result.Skipped = validRows.Count - creadas;
        result.Errors = errorRows.Count;
        result.ErrorRows = errorRows;
        result.Message = errorRows.Count == 0
            ? $"Se importaron {creadas} cuentas correctamente."
            : $"Se importaron {creadas} cuentas con {errorRows.Count} errores.";

        return Result<PlanCuentaImportResultDto>.Success(result);
    }

    // ── CSV parsing helpers ──

    /// <summary>Parses a CSV line respecting simple quoted fields.</summary>
    private static List<string> ParseCsvLine(string line)
    {
        var result = new List<string>();
        var current = new System.Text.StringBuilder();
        bool inQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            var c = line[i];
            if (c == '"')
            {
                inQuotes = !inQuotes;
            }
            else if (c == ';' && !inQuotes)
            {
                result.Add(current.ToString());
                current.Clear();
            }
            else
            {
                current.Append(c);
            }
        }

        result.Add(current.ToString());
        return result;
    }

    private static TipoCuenta? MapTipoCuenta(string tipo)
    {
        return tipo.Trim().ToLowerInvariant() switch
        {
            "activo" or "1" => TipoCuenta.Activo,
            "pasivo" or "2" => TipoCuenta.Pasivo,
            "patrimonio" or "3" => TipoCuenta.Patrimonio,
            "ingreso" or "4" => TipoCuenta.Ingreso,
            "gasto" or "5" => TipoCuenta.Gasto,
            "costo" or "6" => TipoCuenta.Costo,
            _ => null
        };
    }

    private static string MapNaturaleza(string naturaleza)
    {
        return naturaleza.Trim().ToLowerInvariant() switch
        {
            "deudora" or "1" => "Deudora",
            "acreedora" or "2" => "Acreedora",
            _ => null!
        };
    }

    // ── Cache helpers ──

    private async Task<IReadOnlyList<CuentaContableDto>> GetFlatCachedAsync(int empresaId, CancellationToken ct)
    {
        var flatKey = $"{CacheKeyPrefix}-{empresaId}";

        if (!_cache.TryGetValue(flatKey, out IReadOnlyList<CuentaContableDto>? cached) || cached is null)
        {
            var cuentas = await _repo.FindAsync(
                c => c.EmpresaId == empresaId && c.Activo, ct);

            var all = cuentas.OrderBy(c => c.Codigo).ToList();
            var padreMap = all.ToDictionary(c => c.CuentaContableId);

            cached = all.Select(c => MapToDto(c, padreMap)).ToList().AsReadOnly();

            _cache.Set(flatKey, cached, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = CacheTTL,
                SlidingExpiration = CacheSlidingExpiration,
                Priority = CacheItemPriority.High
            });
        }

        return cached;
    }

    private void InvalidateCache(int empresaId)
    {
        _cache.Remove($"{CacheKeyPrefix}-{empresaId}");
        _cache.Remove($"{CacheKeyPrefix}-tree-{empresaId}");
    }

    private static CuentaContableDto MapToDto(CuentaContable c, Dictionary<int, CuentaContable> padreMap)
    {
        CuentaContable? padre = null;
        if (c.CuentaPadreId.HasValue)
            padreMap.TryGetValue(c.CuentaPadreId.Value, out padre);

        return new CuentaContableDto
        {
            CuentaContableId = c.CuentaContableId,
            EmpresaId = c.EmpresaId,
            Codigo = c.Codigo,
            Nombre = c.Nombre,
            Tipo = c.Tipo.ToString(),
            Naturaleza = c.Naturaleza.ToString(),
            Nivel = c.Nivel,
            CuentaPadreId = c.CuentaPadreId,
            CuentaPadreCodigo = padre?.Codigo,
            CuentaPadreNombre = padre?.Nombre,
            PermiteMovimientos = c.PermiteMovimientos,
            Descripcion = c.Descripcion,
            SaldoActual = c.SaldoActual,
            Activo = c.Activo
        };
    }

    private static string? GetParentCode(string codigo)
    {
        var lastDot = codigo.LastIndexOf('.');
        return lastDot > 0 ? codigo[..lastDot] : null;
    }

    /// <summary>
    /// Plan de cuentas estándar Bolivia / NIIF adaptado.
    /// </summary>
    private static List<CuentaContable> BuildPlanCuentasBolivia(int empresaId)
    {
        var cuentas = new List<CuentaContable>();

        void Add(string codigo, string nombre, TipoCuenta tipo, NaturalezaCuenta nat, int nivel, bool movimientos = false)
        {
            cuentas.Add(new CuentaContable
            {
                EmpresaId = empresaId,
                Codigo = codigo,
                Nombre = nombre,
                Tipo = tipo,
                Naturaleza = nat,
                Nivel = nivel,
                PermiteMovimientos = movimientos,
                SaldoActual = 0,
                Activo = true
            });
        }

        // ????????????????????????????????????????????
        // 1. ACTIVO
        // ????????????????????????????????????????????
        Add("1", "ACTIVO", TipoCuenta.Activo, NaturalezaCuenta.Deudora, 1);

        // 1.1 Activo Corriente
        Add("1.1", "ACTIVO CORRIENTE", TipoCuenta.Activo, NaturalezaCuenta.Deudora, 2);
        Add("1.1.1", "Caja y Bancos", TipoCuenta.Activo, NaturalezaCuenta.Deudora, 3);
        Add("1.1.1.01", "Caja General", TipoCuenta.Activo, NaturalezaCuenta.Deudora, 4, true);
        Add("1.1.1.02", "Caja Chica", TipoCuenta.Activo, NaturalezaCuenta.Deudora, 4, true);
        Add("1.1.1.03", "Bancos Moneda Nacional", TipoCuenta.Activo, NaturalezaCuenta.Deudora, 4, true);
        Add("1.1.1.04", "Bancos Moneda Extranjera", TipoCuenta.Activo, NaturalezaCuenta.Deudora, 4, true);

        Add("1.1.2", "Cuentas por Cobrar", TipoCuenta.Activo, NaturalezaCuenta.Deudora, 3);
        Add("1.1.2.01", "Clientes por Cobrar", TipoCuenta.Activo, NaturalezaCuenta.Deudora, 4, true);
        Add("1.1.2.02", "Anticipos a Proveedores", TipoCuenta.Activo, NaturalezaCuenta.Deudora, 4, true);
        Add("1.1.2.03", "Documentos por Cobrar", TipoCuenta.Activo, NaturalezaCuenta.Deudora, 4, true);
        Add("1.1.2.04", "Deudores Diversos", TipoCuenta.Activo, NaturalezaCuenta.Deudora, 4, true);
        Add("1.1.2.05", "Provisión Cuentas Incobrables", TipoCuenta.Activo, NaturalezaCuenta.Acreedora, 4, true);

        Add("1.1.3", "Inventarios", TipoCuenta.Activo, NaturalezaCuenta.Deudora, 3);
        Add("1.1.3.01", "Inventario de Mercaderías", TipoCuenta.Activo, NaturalezaCuenta.Deudora, 4, true);
        Add("1.1.3.02", "Inventario en Tránsito", TipoCuenta.Activo, NaturalezaCuenta.Deudora, 4, true);
        Add("1.1.3.03", "Materias Primas", TipoCuenta.Activo, NaturalezaCuenta.Deudora, 4, true);
        Add("1.1.3.04", "Productos en Proceso", TipoCuenta.Activo, NaturalezaCuenta.Deudora, 4, true);
        Add("1.1.3.05", "Productos Terminados", TipoCuenta.Activo, NaturalezaCuenta.Deudora, 4, true);

        Add("1.1.4", "Impuestos por Recuperar", TipoCuenta.Activo, NaturalezaCuenta.Deudora, 3);
        Add("1.1.4.01", "Crédito Fiscal IVA", TipoCuenta.Activo, NaturalezaCuenta.Deudora, 4, true);
        Add("1.1.4.02", "Anticipo IT", TipoCuenta.Activo, NaturalezaCuenta.Deudora, 4, true);
        Add("1.1.4.03", "Anticipo IUE", TipoCuenta.Activo, NaturalezaCuenta.Deudora, 4, true);

        // 1.2 Activo No Corriente
        Add("1.2", "ACTIVO NO CORRIENTE", TipoCuenta.Activo, NaturalezaCuenta.Deudora, 2);
        Add("1.2.1", "Activos Fijos", TipoCuenta.Activo, NaturalezaCuenta.Deudora, 3);
        Add("1.2.1.01", "Terrenos", TipoCuenta.Activo, NaturalezaCuenta.Deudora, 4, true);
        Add("1.2.1.02", "Edificios", TipoCuenta.Activo, NaturalezaCuenta.Deudora, 4, true);
        Add("1.2.1.03", "Maquinaria y Equipo", TipoCuenta.Activo, NaturalezaCuenta.Deudora, 4, true);
        Add("1.2.1.04", "Vehículos", TipoCuenta.Activo, NaturalezaCuenta.Deudora, 4, true);
        Add("1.2.1.05", "Muebles y Enseres", TipoCuenta.Activo, NaturalezaCuenta.Deudora, 4, true);
        Add("1.2.1.06", "Equipos de Computación", TipoCuenta.Activo, NaturalezaCuenta.Deudora, 4, true);
        Add("1.2.2", "Depreciación Acumulada", TipoCuenta.Activo, NaturalezaCuenta.Acreedora, 3);
        Add("1.2.2.01", "Dep. Acum. Edificios", TipoCuenta.Activo, NaturalezaCuenta.Acreedora, 4, true);
        Add("1.2.2.02", "Dep. Acum. Maquinaria", TipoCuenta.Activo, NaturalezaCuenta.Acreedora, 4, true);
        Add("1.2.2.03", "Dep. Acum. Vehículos", TipoCuenta.Activo, NaturalezaCuenta.Acreedora, 4, true);
        Add("1.2.2.04", "Dep. Acum. Muebles", TipoCuenta.Activo, NaturalezaCuenta.Acreedora, 4, true);
        Add("1.2.2.05", "Dep. Acum. Eq. Computación", TipoCuenta.Activo, NaturalezaCuenta.Acreedora, 4, true);

        // ????????????????????????????????????????????
        // 2. PASIVO
        // ????????????????????????????????????????????
        Add("2", "PASIVO", TipoCuenta.Pasivo, NaturalezaCuenta.Acreedora, 1);

        Add("2.1", "PASIVO CORRIENTE", TipoCuenta.Pasivo, NaturalezaCuenta.Acreedora, 2);
        Add("2.1.1", "Cuentas por Pagar", TipoCuenta.Pasivo, NaturalezaCuenta.Acreedora, 3);
        Add("2.1.1.01", "Proveedores por Pagar", TipoCuenta.Pasivo, NaturalezaCuenta.Acreedora, 4, true);
        Add("2.1.1.02", "Anticipos de Clientes", TipoCuenta.Pasivo, NaturalezaCuenta.Acreedora, 4, true);
        Add("2.1.1.03", "Documentos por Pagar", TipoCuenta.Pasivo, NaturalezaCuenta.Acreedora, 4, true);
        Add("2.1.1.04", "Acreedores Diversos", TipoCuenta.Pasivo, NaturalezaCuenta.Acreedora, 4, true);

        Add("2.1.2", "Obligaciones Laborales", TipoCuenta.Pasivo, NaturalezaCuenta.Acreedora, 3);
        Add("2.1.2.01", "Sueldos por Pagar", TipoCuenta.Pasivo, NaturalezaCuenta.Acreedora, 4, true);
        Add("2.1.2.02", "Aportes Patronales por Pagar", TipoCuenta.Pasivo, NaturalezaCuenta.Acreedora, 4, true);
        Add("2.1.2.03", "Aguinaldos por Pagar", TipoCuenta.Pasivo, NaturalezaCuenta.Acreedora, 4, true);
        Add("2.1.2.04", "Indemnización por Pagar", TipoCuenta.Pasivo, NaturalezaCuenta.Acreedora, 4, true);
        Add("2.1.2.05", "Retenciones AFC por Pagar", TipoCuenta.Pasivo, NaturalezaCuenta.Acreedora, 4, true);
        Add("2.1.2.06", "Retenciones RC-IVA por Pagar", TipoCuenta.Pasivo, NaturalezaCuenta.Acreedora, 4, true);

        Add("2.1.3", "Impuestos por Pagar", TipoCuenta.Pasivo, NaturalezaCuenta.Acreedora, 3);
        Add("2.1.3.01", "Débito Fiscal IVA", TipoCuenta.Pasivo, NaturalezaCuenta.Acreedora, 4, true);
        Add("2.1.3.02", "IT por Pagar", TipoCuenta.Pasivo, NaturalezaCuenta.Acreedora, 4, true);
        Add("2.1.3.03", "IUE por Pagar", TipoCuenta.Pasivo, NaturalezaCuenta.Acreedora, 4, true);
        Add("2.1.3.04", "Retenciones IUE por Pagar", TipoCuenta.Pasivo, NaturalezaCuenta.Acreedora, 4, true);

        Add("2.1.4", "Gastos de Importación por Pagar", TipoCuenta.Pasivo, NaturalezaCuenta.Acreedora, 3);
        Add("2.1.4.01", "Fletes por Pagar", TipoCuenta.Pasivo, NaturalezaCuenta.Acreedora, 4, true);
        Add("2.1.4.02", "Seguros por Pagar", TipoCuenta.Pasivo, NaturalezaCuenta.Acreedora, 4, true);
        Add("2.1.4.03", "Aranceles por Pagar", TipoCuenta.Pasivo, NaturalezaCuenta.Acreedora, 4, true);
        Add("2.1.4.04", "Agencia Despachante por Pagar", TipoCuenta.Pasivo, NaturalezaCuenta.Acreedora, 4, true);

        // 2.2 Pasivo No Corriente
        Add("2.2", "PASIVO NO CORRIENTE", TipoCuenta.Pasivo, NaturalezaCuenta.Acreedora, 2);
        Add("2.2.1", "Deudas a Largo Plazo", TipoCuenta.Pasivo, NaturalezaCuenta.Acreedora, 3);
        Add("2.2.1.01", "Préstamos Bancarios LP", TipoCuenta.Pasivo, NaturalezaCuenta.Acreedora, 4, true);
        Add("2.2.1.02", "Hipotecas por Pagar", TipoCuenta.Pasivo, NaturalezaCuenta.Acreedora, 4, true);

        Add("2.2.2", "Previsiones", TipoCuenta.Pasivo, NaturalezaCuenta.Acreedora, 3);
        Add("2.2.2.01", "Previsión para Indemnización", TipoCuenta.Pasivo, NaturalezaCuenta.Acreedora, 4, true);
        Add("2.2.2.02", "Previsión para Aguinaldos", TipoCuenta.Pasivo, NaturalezaCuenta.Acreedora, 4, true);

        // ????????????????????????????????????????????
        // 3. PATRIMONIO
        // ????????????????????????????????????????????
        Add("3", "PATRIMONIO", TipoCuenta.Patrimonio, NaturalezaCuenta.Acreedora, 1);

        Add("3.1", "CAPITAL", TipoCuenta.Patrimonio, NaturalezaCuenta.Acreedora, 2);
        Add("3.1.1", "Capital Social", TipoCuenta.Patrimonio, NaturalezaCuenta.Acreedora, 3);
        Add("3.1.1.01", "Capital Pagado", TipoCuenta.Patrimonio, NaturalezaCuenta.Acreedora, 4, true);
        Add("3.1.1.02", "Aportes por Capitalizar", TipoCuenta.Patrimonio, NaturalezaCuenta.Acreedora, 4, true);

        Add("3.2", "RESERVAS", TipoCuenta.Patrimonio, NaturalezaCuenta.Acreedora, 2);
        Add("3.2.1", "Reservas de Capital", TipoCuenta.Patrimonio, NaturalezaCuenta.Acreedora, 3);
        Add("3.2.1.01", "Reserva Legal", TipoCuenta.Patrimonio, NaturalezaCuenta.Acreedora, 4, true);
        Add("3.2.1.02", "Reservas Estatutarias", TipoCuenta.Patrimonio, NaturalezaCuenta.Acreedora, 4, true);
        Add("3.2.1.03", "Ajuste por Inflación", TipoCuenta.Patrimonio, NaturalezaCuenta.Acreedora, 4, true);

        Add("3.3", "RESULTADOS", TipoCuenta.Patrimonio, NaturalezaCuenta.Acreedora, 2);
        Add("3.3.1", "Resultados Acumulados", TipoCuenta.Patrimonio, NaturalezaCuenta.Acreedora, 3);
        Add("3.3.1.01", "Resultados Acumulados", TipoCuenta.Patrimonio, NaturalezaCuenta.Acreedora, 4, true);
        Add("3.3.1.02", "Resultado del Ejercicio", TipoCuenta.Patrimonio, NaturalezaCuenta.Acreedora, 4, true);

        // ????????????????????????????????????????????
        // 4. INGRESOS
        // ????????????????????????????????????????????
        Add("4", "INGRESOS", TipoCuenta.Ingreso, NaturalezaCuenta.Acreedora, 1);

        Add("4.1", "INGRESOS OPERACIONALES", TipoCuenta.Ingreso, NaturalezaCuenta.Acreedora, 2);
        Add("4.1.1", "Ventas", TipoCuenta.Ingreso, NaturalezaCuenta.Acreedora, 3);
        Add("4.1.1.01", "Ventas de Mercaderías", TipoCuenta.Ingreso, NaturalezaCuenta.Acreedora, 4, true);
        Add("4.1.1.02", "Ventas de Servicios", TipoCuenta.Ingreso, NaturalezaCuenta.Acreedora, 4, true);
        Add("4.1.1.03", "Devoluciones sobre Ventas", TipoCuenta.Ingreso, NaturalezaCuenta.Deudora, 4, true);
        Add("4.1.1.04", "Descuentos sobre Ventas", TipoCuenta.Ingreso, NaturalezaCuenta.Deudora, 4, true);

        Add("4.2", "OTROS INGRESOS", TipoCuenta.Ingreso, NaturalezaCuenta.Acreedora, 2);
        Add("4.2.1", "Ingresos No Operacionales", TipoCuenta.Ingreso, NaturalezaCuenta.Acreedora, 3);
        Add("4.2.1.01", "Ingresos Financieros", TipoCuenta.Ingreso, NaturalezaCuenta.Acreedora, 4, true);
        Add("4.2.1.02", "Diferencia de Cambio (Ganancia)", TipoCuenta.Ingreso, NaturalezaCuenta.Acreedora, 4, true);
        Add("4.2.1.03", "Otros Ingresos", TipoCuenta.Ingreso, NaturalezaCuenta.Acreedora, 4, true);

        // ????????????????????????????????????????????
        // 5. GASTOS
        // ????????????????????????????????????????????
        Add("5", "GASTOS", TipoCuenta.Gasto, NaturalezaCuenta.Deudora, 1);

        Add("5.1", "GASTOS DE ADMINISTRACIÓN", TipoCuenta.Gasto, NaturalezaCuenta.Deudora, 2);
        Add("5.1.1", "Gastos de Personal", TipoCuenta.Gasto, NaturalezaCuenta.Deudora, 3);
        Add("5.1.1.01", "Sueldos y Salarios", TipoCuenta.Gasto, NaturalezaCuenta.Deudora, 4, true);
        Add("5.1.1.02", "Horas Extra", TipoCuenta.Gasto, NaturalezaCuenta.Deudora, 4, true);
        Add("5.1.1.03", "Bonos y Comisiones", TipoCuenta.Gasto, NaturalezaCuenta.Deudora, 4, true);
        Add("5.1.1.04", "Aportes Patronales", TipoCuenta.Gasto, NaturalezaCuenta.Deudora, 4, true);
        Add("5.1.1.05", "Aguinaldos", TipoCuenta.Gasto, NaturalezaCuenta.Deudora, 4, true);
        Add("5.1.1.06", "Indemnización", TipoCuenta.Gasto, NaturalezaCuenta.Deudora, 4, true);

        Add("5.1.2", "Gastos Generales", TipoCuenta.Gasto, NaturalezaCuenta.Deudora, 3);
        Add("5.1.2.01", "Alquileres", TipoCuenta.Gasto, NaturalezaCuenta.Deudora, 4, true);
        Add("5.1.2.02", "Servicios Básicos", TipoCuenta.Gasto, NaturalezaCuenta.Deudora, 4, true);
        Add("5.1.2.03", "Comunicaciones", TipoCuenta.Gasto, NaturalezaCuenta.Deudora, 4, true);
        Add("5.1.2.04", "Material de Escritorio", TipoCuenta.Gasto, NaturalezaCuenta.Deudora, 4, true);
        Add("5.1.2.05", "Seguros", TipoCuenta.Gasto, NaturalezaCuenta.Deudora, 4, true);
        Add("5.1.2.06", "Mantenimiento y Reparaciones", TipoCuenta.Gasto, NaturalezaCuenta.Deudora, 4, true);
        Add("5.1.2.07", "Depreciación", TipoCuenta.Gasto, NaturalezaCuenta.Deudora, 4, true);
        Add("5.1.2.08", "Gastos de Viaje", TipoCuenta.Gasto, NaturalezaCuenta.Deudora, 4, true);
        Add("5.1.2.09", "Honorarios Profesionales", TipoCuenta.Gasto, NaturalezaCuenta.Deudora, 4, true);
        Add("5.1.2.10", "Gastos Varios", TipoCuenta.Gasto, NaturalezaCuenta.Deudora, 4, true);

        Add("5.2", "GASTOS DE COMERCIALIZACIÓN", TipoCuenta.Gasto, NaturalezaCuenta.Deudora, 2);
        Add("5.2.1", "Gastos de Venta", TipoCuenta.Gasto, NaturalezaCuenta.Deudora, 3);
        Add("5.2.1.01", "Publicidad y Propaganda", TipoCuenta.Gasto, NaturalezaCuenta.Deudora, 4, true);
        Add("5.2.1.02", "Comisiones sobre Ventas", TipoCuenta.Gasto, NaturalezaCuenta.Deudora, 4, true);
        Add("5.2.1.03", "Fletes de Venta", TipoCuenta.Gasto, NaturalezaCuenta.Deudora, 4, true);
        Add("5.2.1.04", "Empaques y Embalajes", TipoCuenta.Gasto, NaturalezaCuenta.Deudora, 4, true);

        Add("5.3", "GASTOS FINANCIEROS", TipoCuenta.Gasto, NaturalezaCuenta.Deudora, 2);
        Add("5.3.1", "Costos Financieros", TipoCuenta.Gasto, NaturalezaCuenta.Deudora, 3);
        Add("5.3.1.01", "Intereses Bancarios", TipoCuenta.Gasto, NaturalezaCuenta.Deudora, 4, true);
        Add("5.3.1.02", "Comisiones Bancarias", TipoCuenta.Gasto, NaturalezaCuenta.Deudora, 4, true);
        Add("5.3.1.03", "Diferencia de Cambio (Pérdida)", TipoCuenta.Gasto, NaturalezaCuenta.Deudora, 4, true);
        Add("5.3.1.04", "ITF", TipoCuenta.Gasto, NaturalezaCuenta.Deudora, 4, true);

        Add("5.4", "IMPUESTOS", TipoCuenta.Gasto, NaturalezaCuenta.Deudora, 2);
        Add("5.4.1", "Impuestos a las Utilidades", TipoCuenta.Gasto, NaturalezaCuenta.Deudora, 3);
        Add("5.4.1.01", "IUE", TipoCuenta.Gasto, NaturalezaCuenta.Deudora, 4, true);
        Add("5.4.1.02", "IT", TipoCuenta.Gasto, NaturalezaCuenta.Deudora, 4, true);

        // ????????????????????????????????????????????
        // 6. COSTOS
        // ????????????????????????????????????????????
        Add("6", "COSTOS", TipoCuenta.Costo, NaturalezaCuenta.Deudora, 1);

        Add("6.1", "COSTO DE VENTAS", TipoCuenta.Costo, NaturalezaCuenta.Deudora, 2);
        Add("6.1.1", "Costo de Mercaderías Vendidas", TipoCuenta.Costo, NaturalezaCuenta.Deudora, 3);
        Add("6.1.1.01", "Costo de Ventas", TipoCuenta.Costo, NaturalezaCuenta.Deudora, 4, true);

        Add("6.2", "COSTOS DE IMPORTACIÓN", TipoCuenta.Costo, NaturalezaCuenta.Deudora, 2);
        Add("6.2.1", "Costos de Importación", TipoCuenta.Costo, NaturalezaCuenta.Deudora, 3);
        Add("6.2.1.01", "Fletes de Importación", TipoCuenta.Costo, NaturalezaCuenta.Deudora, 4, true);
        Add("6.2.1.02", "Seguros de Importación", TipoCuenta.Costo, NaturalezaCuenta.Deudora, 4, true);
        Add("6.2.1.03", "Aranceles e Impuestos Aduana", TipoCuenta.Costo, NaturalezaCuenta.Deudora, 4, true);
        Add("6.2.1.04", "Gastos de Agencia Despachante", TipoCuenta.Costo, NaturalezaCuenta.Deudora, 4, true);
        Add("6.2.1.05", "Almacenaje y Manipuleo", TipoCuenta.Costo, NaturalezaCuenta.Deudora, 4, true);
        Add("6.2.1.06", "Otros Costos de Importación", TipoCuenta.Costo, NaturalezaCuenta.Deudora, 4, true);

        return cuentas;
    }
}
