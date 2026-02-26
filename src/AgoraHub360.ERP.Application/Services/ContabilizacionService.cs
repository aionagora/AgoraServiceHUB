namespace AgoraHub360.ERP.Application.Services;

using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Domain.Entities.ACC;
using AgoraHub360.ERP.Domain.Entities.Core;
using AgoraHub360.ERP.Domain.Enums;
using AgoraHub360.ERP.Domain.Interfaces;
using AgoraHub360.ERP.Shared.DTOs.Contabilidad;

/// <summary>
/// Motor de contabilización automática.
/// Lee la plantilla configurada para un tipo de documento,
/// resuelve los montos, genera el asiento y lo contabiliza en un solo paso.
/// </summary>
public class ContabilizacionService : IContabilizacionService
{
    private readonly IRepository<PlantillaContable> _plantillaRepo;
    private readonly IRepository<PlantillaContableLinea> _plantillaLineaRepo;
    private readonly IRepository<AsientoContable> _asientoRepo;
    private readonly IRepository<AsientoContableLinea> _asientoLineaRepo;
    private readonly IRepository<CuentaContable> _cuentaRepo;
    private readonly IRepository<PeriodoContable> _periodoRepo;
    private readonly IRepository<NumeracionDocumento> _numRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public ContabilizacionService(
        IRepository<PlantillaContable> plantillaRepo,
        IRepository<PlantillaContableLinea> plantillaLineaRepo,
        IRepository<AsientoContable> asientoRepo,
        IRepository<AsientoContableLinea> asientoLineaRepo,
        IRepository<CuentaContable> cuentaRepo,
        IRepository<PeriodoContable> periodoRepo,
        IRepository<NumeracionDocumento> numRepo,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser)
    {
        _plantillaRepo = plantillaRepo;
        _plantillaLineaRepo = plantillaLineaRepo;
        _asientoRepo = asientoRepo;
        _asientoLineaRepo = asientoLineaRepo;
        _cuentaRepo = cuentaRepo;
        _periodoRepo = periodoRepo;
        _numRepo = numRepo;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<Result<AsientoContableDto>> ContabilizarDocumentoAsync(
        string tipoDocumento,
        Dictionary<string, decimal> montos,
        long origenId,
        string origenReferencia,
        DateTime fecha,
        string? glosaExtra,
        CancellationToken ct)
    {
        var empresaId = _currentUser.EmpresaId;
        if (!empresaId.HasValue)
            return Result<AsientoContableDto>.Failure("No active company.");

        // ?? 1. Find template ??
        var plantillas = await _plantillaRepo.FindAsync(
            p => p.EmpresaId == empresaId.Value && p.TipoDocumento == tipoDocumento && p.Activo, ct);

        var plantilla = plantillas.FirstOrDefault();
        if (plantilla is null)
            return Result<AsientoContableDto>.Success(null!); // No template = skip silently (contabilización opcional)

        var plantillaLineas = await _plantillaLineaRepo.FindAsync(
            l => l.PlantillaContableId == plantilla.PlantillaContableId && l.Activo, ct);

        if (plantillaLineas.Count < 2)
            return Result<AsientoContableDto>.Failure($"Template '{plantilla.Codigo}' has fewer than 2 lines.");

        // ?? 2. Validate accounting period ??
        var periodos = await _periodoRepo.FindAsync(
            p => p.EmpresaId == empresaId.Value && p.Anio == fecha.Year && p.Mes == fecha.Month && p.Activo, ct);

        var periodo = periodos.FirstOrDefault();
        if (periodo is not null && periodo.Estado == "Cerrado")
            return Result<AsientoContableDto>.Failure(
                $"Cannot post to closed period '{periodo.Nombre}'. Reopen it first.");

        // ?? 3. Resolve amounts from template lines ??
        var asientoLineas = new List<(int cuentaId, decimal debe, decimal haber, string? glosa)>();

        foreach (var tl in plantillaLineas.OrderBy(l => l.NumeroLinea))
        {
            var monto = ResolverMonto(tl.CampoMonto, montos) * tl.Factor;
            if (monto <= 0) continue; // Skip zero-amount lines

            var debe = tl.TipoMovimiento == "Debe" ? monto : 0m;
            var haber = tl.TipoMovimiento == "Haber" ? monto : 0m;

            asientoLineas.Add((tl.CuentaContableId, debe, haber, tl.Glosa));
        }

        if (asientoLineas.Count < 2)
            return Result<AsientoContableDto>.Failure(
                $"Template '{plantilla.Codigo}' produced fewer than 2 lines with the given amounts.");

        // ?? 4. Validate balanced ??
        var totalDebe = asientoLineas.Sum(l => l.debe);
        var totalHaber = asientoLineas.Sum(l => l.haber);

        if (totalDebe != totalHaber)
        {
            // Auto-balance: adjust the difference on the largest haber line
            var diff = totalDebe - totalHaber;
            if (Math.Abs(diff) < 0.01m)
            {
                // Rounding difference — adjust last haber line
                var lastHaber = asientoLineas.FindLastIndex(l => l.haber > 0);
                if (lastHaber >= 0)
                {
                    var l = asientoLineas[lastHaber];
                    asientoLineas[lastHaber] = (l.cuentaId, l.debe, l.haber + diff, l.glosa);
                    totalHaber += diff;
                }
            }

            if (totalDebe != totalHaber)
                return Result<AsientoContableDto>.Failure(
                    $"Auto-generated entry is not balanced. Debit: {totalDebe:N2}, Credit: {totalHaber:N2}.");
        }

        // ?? 5. Build glosa ??
        var glosa = plantilla.GlosaPlantilla
            .Replace("{Numero}", origenReferencia)
            .Replace("{OrigenReferencia}", origenReferencia)
            .Replace("{Fecha}", fecha.ToString("dd/MM/yyyy"));
        if (!string.IsNullOrEmpty(glosaExtra))
            glosa = glosa.Replace("{Proveedor}", glosaExtra);

        // ?? 6. Generate entry number ??
        var numero = await GenerarNumeroAsync(empresaId.Value, fecha, ct);

        // ?? 7. Create journal entry ??
        var asiento = new AsientoContable
        {
            EmpresaId = empresaId.Value,
            Numero = numero,
            Fecha = fecha,
            Tipo = "Automático",
            Glosa = glosa,
            Estado = "Contabilizado", // Auto-posted
            OrigenTipo = tipoDocumento,
            OrigenId = origenId,
            OrigenReferencia = origenReferencia,
            TotalDebe = totalDebe,
            TotalHaber = totalHaber,
            Activo = true
        };

        await _asientoRepo.AddAsync(asiento, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        // ?? 8. Create lines ??
        int lineNum = 1;
        foreach (var (cuentaId, debe, haber, lineGlosa) in asientoLineas)
        {
            await _asientoLineaRepo.AddAsync(new AsientoContableLinea
            {
                AsientoContableId = asiento.AsientoContableId,
                NumeroLinea = lineNum++,
                CuentaContableId = cuentaId,
                Debe = debe,
                Haber = haber,
                Glosa = lineGlosa,
                Referencia = origenReferencia,
                Activo = true
            }, ct);
        }

        // ?? 9. Update account balances ??
        foreach (var (cuentaId, debe, haber, _) in asientoLineas)
        {
            var cuenta = await _cuentaRepo.GetByIdAsync(cuentaId, ct);
            if (cuenta is null) continue;

            if (cuenta.Naturaleza == NaturalezaCuenta.Deudora)
                cuenta.SaldoActual += debe - haber;
            else
                cuenta.SaldoActual += haber - debe;

            await _cuentaRepo.UpdateAsync(cuenta, ct);
        }

        await _unitOfWork.SaveChangesAsync(ct);

        // ?? 10. Return DTO ??
        var cuentaIds = asientoLineas.Select(l => l.cuentaId).Distinct().ToList();
        var cuentas = await _cuentaRepo.FindAsync(c => cuentaIds.Contains(c.CuentaContableId), ct);
        var cuentaMap = cuentas.ToDictionary(c => c.CuentaContableId);

        return Result<AsientoContableDto>.Success(new AsientoContableDto
        {
            AsientoContableId = asiento.AsientoContableId,
            EmpresaId = asiento.EmpresaId,
            Numero = asiento.Numero,
            Fecha = asiento.Fecha,
            Tipo = asiento.Tipo,
            Glosa = asiento.Glosa,
            Estado = asiento.Estado,
            OrigenTipo = asiento.OrigenTipo,
            OrigenId = asiento.OrigenId,
            OrigenReferencia = asiento.OrigenReferencia,
            TotalDebe = totalDebe,
            TotalHaber = totalHaber,
            Cuadrado = true,
            Lineas = asientoLineas.Select((l, i) =>
            {
                cuentaMap.TryGetValue(l.cuentaId, out var cuenta);
                return new AsientoContableLineaDto
                {
                    NumeroLinea = i + 1,
                    CuentaContableId = l.cuentaId,
                    CuentaCodigo = cuenta?.Codigo ?? "—",
                    CuentaNombre = cuenta?.Nombre ?? "—",
                    Debe = l.debe,
                    Haber = l.haber,
                    Glosa = l.glosa,
                    Referencia = origenReferencia
                };
            }).ToList()
        });
    }

    // ?? Helpers ??

    private static decimal ResolverMonto(string campoMonto, Dictionary<string, decimal> montos)
    {
        return montos.TryGetValue(campoMonto, out var valor) ? valor : 0m;
    }

    private async Task<string> GenerarNumeroAsync(int empresaId, DateTime fecha, CancellationToken ct)
    {
        var numeraciones = await _numRepo.FindAsync(
            n => n.EmpresaId == empresaId && n.TipoDocumento == "AST", ct);
        var num = numeraciones.FirstOrDefault();
        if (num is not null)
        {
            var numero = num.GenerarSiguiente();
            await _numRepo.UpdateAsync(num, ct);
            return numero;
        }
        var existentes = await _asientoRepo.FindAsync(a => a.EmpresaId == empresaId, ct);
        return $"AST-{fecha.Year}-{(existentes.Count + 1):D5}";
    }
}
