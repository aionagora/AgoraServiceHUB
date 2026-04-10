namespace AgoraHub360.ERP.Application.Services;

using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Domain.Entities.ACC;
using AgoraHub360.ERP.Domain.Interfaces;
using AgoraHub360.ERP.Shared.DTOs.Contabilidad;

public class PlantillaContableService : IPlantillaContableService
{
    private readonly IRepository<PlantillaContable> _plantillaRepo;
    private readonly IRepository<PlantillaContableLinea> _lineaRepo;
    private readonly IRepository<CuentaContable> _cuentaRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public PlantillaContableService(
        IRepository<PlantillaContable> plantillaRepo,
        IRepository<PlantillaContableLinea> lineaRepo,
        IRepository<CuentaContable> cuentaRepo,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser)
    {
        _plantillaRepo = plantillaRepo;
        _lineaRepo = lineaRepo;
        _cuentaRepo = cuentaRepo;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<Result<IReadOnlyList<PlantillaContableDto>>> GetAllAsync(string? tipoDocumento, CancellationToken ct)
    {
        var empresaId = _currentUser.EmpresaId;
        if (!empresaId.HasValue)
            return Result<IReadOnlyList<PlantillaContableDto>>.Failure("No active company.");

        var plantillas = await _plantillaRepo.FindAsync(
            p => p.EmpresaId == empresaId.Value && p.Activo
                && (string.IsNullOrEmpty(tipoDocumento) || p.TipoDocumento == tipoDocumento), ct);

        var dtos = new List<PlantillaContableDto>();
        foreach (var p in plantillas.OrderBy(x => x.Codigo))
            dtos.Add(await BuildDto(p, ct));

        return Result<IReadOnlyList<PlantillaContableDto>>.Success(dtos.AsReadOnly());
    }

    public async Task<Result<PlantillaContableDto>> GetByIdAsync(int id, CancellationToken ct)
    {
        var empresaId = _currentUser.EmpresaId;
        if (!empresaId.HasValue)
            return Result<PlantillaContableDto>.Failure("No active company.");

        var plantilla = await _plantillaRepo.GetByIdAsync(id, ct);
        if (plantilla is null || plantilla.EmpresaId != empresaId.Value || !plantilla.Activo)
            return Result<PlantillaContableDto>.Failure("Template not found.");

        return Result<PlantillaContableDto>.Success(await BuildDto(plantilla, ct));
    }

    public async Task<Result<PlantillaContableDto>> GetByTipoDocumentoAsync(string tipoDocumento, CancellationToken ct)
    {
        var empresaId = _currentUser.EmpresaId;
        if (!empresaId.HasValue)
            return Result<PlantillaContableDto>.Failure("No active company.");

        var plantillas = await _plantillaRepo.FindAsync(
            p => p.EmpresaId == empresaId.Value && p.TipoDocumento == tipoDocumento && p.Activo, ct);

        var plantilla = plantillas.FirstOrDefault();
        if (plantilla is null)
            return Result<PlantillaContableDto>.Failure($"No active template found for document type '{tipoDocumento}'.");

        return Result<PlantillaContableDto>.Success(await BuildDto(plantilla, ct));
    }

    public async Task<Result<PlantillaContableDto>> CreateAsync(CreatePlantillaContableDto dto, CancellationToken ct)
    {
        var empresaId = _currentUser.EmpresaId;
        if (!empresaId.HasValue)
            return Result<PlantillaContableDto>.Failure("No active company.");

        if (dto.Lineas.Count < 2)
            return Result<PlantillaContableDto>.Failure("A template requires at least 2 lines (debit and credit).");

        // Validate unique code
        var existentes = await _plantillaRepo.FindAsync(
            p => p.EmpresaId == empresaId.Value && p.Codigo == dto.Codigo && p.Activo, ct);
        if (existentes.Count > 0)
            return Result<PlantillaContableDto>.Failure($"Template code '{dto.Codigo}' already exists.");

        // Validate lines have both Debe and Haber
        var hasDebe = dto.Lineas.Any(l => l.TipoMovimiento == "Debe");
        var hasHaber = dto.Lineas.Any(l => l.TipoMovimiento == "Haber");
        if (!hasDebe || !hasHaber)
            return Result<PlantillaContableDto>.Failure("Template must have at least one debit and one credit line.");

        // Validate accounts
        foreach (var l in dto.Lineas)
        {
            var cuenta = await _cuentaRepo.GetByIdAsync(l.CuentaContableId, ct);
            if (cuenta is null || cuenta.EmpresaId != empresaId.Value || !cuenta.Activo)
                return Result<PlantillaContableDto>.Failure($"Account {l.CuentaContableId} not found.");
            if (!cuenta.PermiteMovimientos)
                return Result<PlantillaContableDto>.Failure($"Account '{cuenta.Codigo}' does not allow transactions.");
        }

        var plantilla = new PlantillaContable
        {
            EmpresaId = empresaId.Value,
            Codigo = dto.Codigo,
            Nombre = dto.Nombre,
            TipoDocumento = dto.TipoDocumento,
            Descripcion = dto.Descripcion,
            GlosaPlantilla = dto.GlosaPlantilla,
            Activo = true
        };

        await _plantillaRepo.AddAsync(plantilla, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        int lineNum = 1;
        foreach (var l in dto.Lineas)
        {
            await _lineaRepo.AddAsync(new PlantillaContableLinea
            {
                PlantillaContableId = plantilla.PlantillaContableId,
                NumeroLinea = lineNum++,
                CuentaContableId = l.CuentaContableId,
                TipoMovimiento = l.TipoMovimiento,
                CampoMonto = l.CampoMonto,
                Factor = l.Factor,
                Glosa = l.Glosa,
                Activo = true
            }, ct);
        }

        await _unitOfWork.SaveChangesAsync(ct);
        return Result<PlantillaContableDto>.Success(await BuildDto(plantilla, ct));
    }

    public async Task<Result<PlantillaContableDto>> UpdateAsync(int id, UpdatePlantillaContableDto dto, CancellationToken ct)
    {
        var empresaId = _currentUser.EmpresaId;
        if (!empresaId.HasValue)
            return Result<PlantillaContableDto>.Failure("No active company.");

        var plantilla = await _plantillaRepo.GetByIdAsync(id, ct);
        if (plantilla is null || plantilla.EmpresaId != empresaId.Value || !plantilla.Activo)
            return Result<PlantillaContableDto>.Failure("Template not found.");

        if (dto.Lineas.Count < 2)
            return Result<PlantillaContableDto>.Failure("A template requires at least 2 lines.");

        var hasDebe = dto.Lineas.Any(l => l.TipoMovimiento == "Debe");
        var hasHaber = dto.Lineas.Any(l => l.TipoMovimiento == "Haber");
        if (!hasDebe || !hasHaber)
            return Result<PlantillaContableDto>.Failure("Template must have at least one debit and one credit line.");

        foreach (var l in dto.Lineas)
        {
            var cuenta = await _cuentaRepo.GetByIdAsync(l.CuentaContableId, ct);
            if (cuenta is null || cuenta.EmpresaId != empresaId.Value || !cuenta.Activo)
                return Result<PlantillaContableDto>.Failure($"Account {l.CuentaContableId} not found.");
            if (!cuenta.PermiteMovimientos)
                return Result<PlantillaContableDto>.Failure($"Account '{cuenta.Codigo}' does not allow transactions.");
        }

        plantilla.Nombre = dto.Nombre;
        plantilla.Descripcion = dto.Descripcion;
        plantilla.GlosaPlantilla = dto.GlosaPlantilla;
        await _plantillaRepo.UpdateAsync(plantilla, ct);

        // Remove old lines
        var oldLineas = await _lineaRepo.FindAsync(l => l.PlantillaContableId == id, ct);
        foreach (var old in oldLineas)
        {
            old.Activo = false;
            await _lineaRepo.UpdateAsync(old, ct);
        }

        // Add new lines
        int lineNum = 1;
        foreach (var l in dto.Lineas)
        {
            await _lineaRepo.AddAsync(new PlantillaContableLinea
            {
                PlantillaContableId = id,
                NumeroLinea = lineNum++,
                CuentaContableId = l.CuentaContableId,
                TipoMovimiento = l.TipoMovimiento,
                CampoMonto = l.CampoMonto,
                Factor = l.Factor,
                Glosa = l.Glosa,
                Activo = true
            }, ct);
        }

        await _unitOfWork.SaveChangesAsync(ct);
        return Result<PlantillaContableDto>.Success(await BuildDto(plantilla, ct));
    }

    public async Task<Result<bool>> DeleteAsync(int id, CancellationToken ct)
    {
        var empresaId = _currentUser.EmpresaId;
        if (!empresaId.HasValue)
            return Result<bool>.Failure("No active company.");

        var plantilla = await _plantillaRepo.GetByIdAsync(id, ct);
        if (plantilla is null || plantilla.EmpresaId != empresaId.Value)
            return Result<bool>.Failure("Template not found.");

        plantilla.Activo = false;
        await _plantillaRepo.UpdateAsync(plantilla, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<bool>.Success(true);
    }

    public async Task<Result<int>> SeedPlantillasAsync(CancellationToken ct)
    {
        var empresaId = _currentUser.EmpresaId;
        if (!empresaId.HasValue)
            return Result<int>.Failure("No active company.");

        var existentes = await _plantillaRepo.FindAsync(p => p.EmpresaId == empresaId.Value && p.Activo, ct);
        if (existentes.Count > 0)
            return Result<int>.Failure("Templates already exist for this company.");

        // Find required accounts by code
        var cuentas = await _cuentaRepo.FindAsync(c => c.EmpresaId == empresaId.Value && c.Activo && c.PermiteMovimientos, ct);
        var cMap = cuentas.ToDictionary(c => c.Codigo, c => c.CuentaContableId);

        // Validate required accounts exist
        var required = new[] { "1.1.3.01", "2.1.1.01", "1.1.4.01", "2.1.3.01", "6.1.1.01" };
        foreach (var code in required)
        {
            if (!cMap.ContainsKey(code))
                return Result<int>.Failure($"Required account '{code}' not found. Generate the chart of accounts first.");
        }

        int count = 0;

        // ?? Plantilla: Recepción de Compra ??
        count += await SeedPlantilla(empresaId.Value, "REC", "Recepción de Compra", "Recepcion",
            "Recepción {Numero} — OC {OrigenReferencia}",
            new[]
            {
                ("1.1.3.01", "Debe", "Subtotal", 1m, "Inventario de Mercaderías"),
                ("1.1.4.01", "Debe", "Impuesto", 1m, "Crédito Fiscal IVA"),
                ("2.1.1.01", "Haber", "Total", 1m, "Proveedores por Pagar"),
            }, cMap, ct);

        // ?? Plantilla: Liquidación de Importación ??
        count += await SeedPlantilla(empresaId.Value, "IMP", "Liquidación de Importación", "Importacion",
            "Liquidación importación {Numero} — OC {OrigenReferencia}",
            new[]
            {
                ("1.1.3.01", "Debe", "GastoAsignado", 1m, "Inventario — Gastos de importación"),
                ("2.1.1.01", "Haber", "GastoAsignado", 1m, "Gastos de importación por pagar"),
            }, cMap, ct);

        // ?? Plantilla: Venta (futura) ??
        count += await SeedPlantilla(empresaId.Value, "VTA", "Venta / Factura", "Venta",
            "Venta {Numero} — Cliente {OrigenReferencia}",
            new[]
            {
                ("1.1.2.01", "Debe", "Total", 1m, "Clientes por Cobrar"),
                ("2.1.3.01", "Haber", "Impuesto", 1m, "Débito Fiscal IVA"),
                ("4.1.1.01", "Haber", "Subtotal", 1m, "Ventas de Mercaderías"),
            }, cMap, ct);

        // ?? Plantilla: Costo de Venta (futura) ??
        count += await SeedPlantilla(empresaId.Value, "CMV", "Costo de Mercaderías Vendidas", "CostoVenta",
            "Costo de venta {Numero}",
            new[]
            {
                ("6.1.1.01", "Debe", "CostoTotal", 1m, "Costo de Ventas"),
                ("1.1.3.01", "Haber", "CostoTotal", 1m, "Inventario de Mercaderías"),
            }, cMap, ct);

        // ?? Plantilla: Ajuste de Inventario ??
        count += await SeedPlantilla(empresaId.Value, "AJU", "Ajuste de Inventario", "AjusteInventario",
            "Ajuste de inventario {Numero}",
            new[]
            {
                ("1.1.3.01", "Debe", "Total", 1m, "Inventario — Ajuste positivo"),
                ("5.1.2.10", "Haber", "Total", 1m, "Gastos varios — Ajuste"),
            }, cMap, ct);

        return Result<int>.Success(count);
    }

    // ?? Helpers ??

    private async Task<int> SeedPlantilla(int empresaId, string codigo, string nombre,
        string tipoDoc, string glosa,
        (string cuenta, string tipo, string campo, decimal factor, string glosaLinea)[] lineas,
        Dictionary<string, int> cMap, CancellationToken ct)
    {
        // Skip if any account is missing
        foreach (var l in lineas)
            if (!cMap.ContainsKey(l.cuenta)) return 0;

        var plantilla = new PlantillaContable
        {
            EmpresaId = empresaId,
            Codigo = codigo,
            Nombre = nombre,
            TipoDocumento = tipoDoc,
            GlosaPlantilla = glosa,
            Activo = true
        };
        await _plantillaRepo.AddAsync(plantilla, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        int num = 1;
        foreach (var l in lineas)
        {
            await _lineaRepo.AddAsync(new PlantillaContableLinea
            {
                PlantillaContableId = plantilla.PlantillaContableId,
                NumeroLinea = num++,
                CuentaContableId = cMap[l.cuenta],
                TipoMovimiento = l.tipo,
                CampoMonto = l.campo,
                Factor = l.factor,
                Glosa = l.glosaLinea,
                Activo = true
            }, ct);
        }
        await _unitOfWork.SaveChangesAsync(ct);
        return 1;
    }

    private async Task<PlantillaContableDto> BuildDto(PlantillaContable p, CancellationToken ct)
    {
        var lineas = await _lineaRepo.FindAsync(l => l.PlantillaContableId == p.PlantillaContableId && l.Activo, ct);
        var cuentaIds = lineas.Select(l => l.CuentaContableId).Distinct().ToList();
        var cuentas = cuentaIds.Count > 0
            ? await _cuentaRepo.FindAsync(c => cuentaIds.Contains(c.CuentaContableId), ct)
            : new List<CuentaContable>();
        var cuentaMap = cuentas.ToDictionary(c => c.CuentaContableId);

        return new PlantillaContableDto
        {
            PlantillaContableId = p.PlantillaContableId,
            EmpresaId = p.EmpresaId,
            Codigo = p.Codigo,
            Nombre = p.Nombre,
            TipoDocumento = p.TipoDocumento,
            Descripcion = p.Descripcion,
            GlosaPlantilla = p.GlosaPlantilla,
            Activo = p.Activo,
            Lineas = lineas.OrderBy(l => l.NumeroLinea).Select(l =>
            {
                cuentaMap.TryGetValue(l.CuentaContableId, out var cuenta);
                return new PlantillaContableLineaDto
                {
                    PlantillaContableLineaId = l.PlantillaContableLineaId,
                    NumeroLinea = l.NumeroLinea,
                    CuentaContableId = l.CuentaContableId,
                    CuentaCodigo = cuenta?.Codigo ?? "—",
                    CuentaNombre = cuenta?.Nombre ?? "—",
                    TipoMovimiento = l.TipoMovimiento,
                    CampoMonto = l.CampoMonto,
                    Factor = l.Factor,
                    Glosa = l.Glosa
                };
            }).ToList()
        };
    }
}
