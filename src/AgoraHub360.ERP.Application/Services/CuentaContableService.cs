namespace AgoraHub360.ERP.Application.Services;

using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Domain.Entities.ACC;
using AgoraHub360.ERP.Domain.Enums;
using AgoraHub360.ERP.Domain.Interfaces;
using AgoraHub360.ERP.Shared.DTOs.Contabilidad;

public class CuentaContableService : ICuentaContableService
{
    private readonly IRepository<CuentaContable> _repo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public CuentaContableService(
        IRepository<CuentaContable> repo,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser)
    {
        _repo = repo;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<Result<IReadOnlyList<CuentaContableDto>>> GetAllAsync(
        byte? tipo, bool? permiteMovimientos, string? search, CancellationToken ct)
    {
        var empresaId = _currentUser.EmpresaId;
        if (!empresaId.HasValue)
            return Result<IReadOnlyList<CuentaContableDto>>.Failure("No active company.");

        var cuentas = await _repo.FindAsync(
            c => c.EmpresaId == empresaId.Value && c.Activo
                && (!tipo.HasValue || (byte)c.Tipo == tipo.Value)
                && (!permiteMovimientos.HasValue || c.PermiteMovimientos == permiteMovimientos.Value)
                && (string.IsNullOrEmpty(search) || c.Codigo.Contains(search) || c.Nombre.Contains(search)),
            ct);

        var all = cuentas.OrderBy(c => c.Codigo).ToList();
        var padreMap = all.ToDictionary(c => c.CuentaContableId);

        var dtos = all.Select(c => MapToDto(c, padreMap)).ToList().AsReadOnly();
        return Result<IReadOnlyList<CuentaContableDto>>.Success(dtos);
    }

    public async Task<Result<IReadOnlyList<CuentaContableDto>>> GetTreeAsync(CancellationToken ct)
    {
        var empresaId = _currentUser.EmpresaId;
        if (!empresaId.HasValue)
            return Result<IReadOnlyList<CuentaContableDto>>.Failure("No active company.");

        var cuentas = await _repo.FindAsync(
            c => c.EmpresaId == empresaId.Value && c.Activo, ct);

        var all = cuentas.OrderBy(c => c.Codigo).ToList();
        var padreMap = all.ToDictionary(c => c.CuentaContableId);

        // Build tree
        var dtoMap = all.ToDictionary(c => c.CuentaContableId, c => MapToDto(c, padreMap));
        var roots = new List<CuentaContableDto>();

        foreach (var dto in dtoMap.Values)
        {
            var entity = all.First(c => c.CuentaContableId == dto.CuentaContableId);
            if (entity.CuentaPadreId.HasValue && dtoMap.TryGetValue(entity.CuentaPadreId.Value, out var padre))
                padre.SubCuentas.Add(dto);
            else
                roots.Add(dto);
        }

        return Result<IReadOnlyList<CuentaContableDto>>.Success(roots.AsReadOnly());
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

        return Result<int>.Success(cuentas.Count);
    }

    // ?? Helpers ??

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
