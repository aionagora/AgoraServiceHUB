namespace AgoraHub360.ERP.Application.Services;

using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Domain.Entities.ACC;
using AgoraHub360.ERP.Domain.Entities.Core;
using AgoraHub360.ERP.Domain.Enums;
using AgoraHub360.ERP.Domain.Interfaces;
using AgoraHub360.ERP.Shared.DTOs.Contabilidad;

public class AsientoContableService : IAsientoContableService
{
    private readonly IRepository<AsientoContable> _asientoRepo;
    private readonly IRepository<AsientoContableLinea> _lineaRepo;
    private readonly IRepository<CuentaContable> _cuentaRepo;
    private readonly IRepository<PeriodoContable> _periodoRepo;
    private readonly IRepository<NumeracionDocumento> _numRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public AsientoContableService(
        IRepository<AsientoContable> asientoRepo,
        IRepository<AsientoContableLinea> lineaRepo,
        IRepository<CuentaContable> cuentaRepo,
        IRepository<PeriodoContable> periodoRepo,
        IRepository<NumeracionDocumento> numRepo,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser)
    {
        _asientoRepo = asientoRepo;
        _lineaRepo = lineaRepo;
        _cuentaRepo = cuentaRepo;
        _periodoRepo = periodoRepo;
        _numRepo = numRepo;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<Result<IReadOnlyList<AsientoContableDto>>> GetAllAsync(
        DateTime? fechaDesde, DateTime? fechaHasta, string? estado,
        string? origenTipo, string? search, CancellationToken ct)
    {
        var empresaId = _currentUser.EmpresaId;
        if (!empresaId.HasValue)
            return Result<IReadOnlyList<AsientoContableDto>>.Failure("No active company.");

        var asientos = await _asientoRepo.FindAsync(
            a => a.EmpresaId == empresaId.Value && a.Activo
                && (!fechaDesde.HasValue || a.Fecha >= fechaDesde.Value)
                && (!fechaHasta.HasValue || a.Fecha <= fechaHasta.Value.AddDays(1).AddSeconds(-1))
                && (string.IsNullOrEmpty(estado) || a.Estado == estado)
                && (string.IsNullOrEmpty(origenTipo) || a.OrigenTipo == origenTipo)
                && (string.IsNullOrEmpty(search) || a.Numero.Contains(search) || a.Glosa.Contains(search)),
            ct);

        var dtos = new List<AsientoContableDto>();
        foreach (var a in asientos.OrderByDescending(x => x.Fecha).ThenByDescending(x => x.AsientoContableId))
            dtos.Add(await BuildDto(a, ct));

        return Result<IReadOnlyList<AsientoContableDto>>.Success(dtos.AsReadOnly());
    }

    public async Task<Result<AsientoContableDto>> GetByIdAsync(long id, CancellationToken ct)
    {
        var empresaId = _currentUser.EmpresaId;
        if (!empresaId.HasValue)
            return Result<AsientoContableDto>.Failure("No active company.");

        var asiento = await _asientoRepo.GetByIdAsync(id, ct);
        if (asiento is null || asiento.EmpresaId != empresaId.Value || !asiento.Activo)
            return Result<AsientoContableDto>.Failure("Journal entry not found.");

        return Result<AsientoContableDto>.Success(await BuildDto(asiento, ct));
    }

    public async Task<Result<AsientoContableDto>> CreateAsync(CreateAsientoContableDto dto, CancellationToken ct)
    {
        var empresaId = _currentUser.EmpresaId;
        if (!empresaId.HasValue)
            return Result<AsientoContableDto>.Failure("No active company.");

        // Validate accounting period is open
        var periodError = await ValidarPeriodoAbiertoAsync(empresaId.Value, dto.Fecha, ct);
        if (periodError != null)
            return Result<AsientoContableDto>.Failure(periodError);

        // Validate lines
        if (dto.Lineas.Count < 2)
            return Result<AsientoContableDto>.Failure("A journal entry requires at least 2 lines.");

        foreach (var l in dto.Lineas)
        {
            if (l.Debe < 0 || l.Haber < 0)
                return Result<AsientoContableDto>.Failure("Debit and credit amounts cannot be negative.");
            if (l.Debe == 0 && l.Haber == 0)
                return Result<AsientoContableDto>.Failure("Each line must have either a debit or credit amount.");
            if (l.Debe > 0 && l.Haber > 0)
                return Result<AsientoContableDto>.Failure("A line cannot have both debit and credit amounts.");
        }

        // Validate accounts exist and allow transactions
        foreach (var l in dto.Lineas)
        {
            var cuenta = await _cuentaRepo.GetByIdAsync(l.CuentaContableId, ct);
            if (cuenta is null || cuenta.EmpresaId != empresaId.Value || !cuenta.Activo)
                return Result<AsientoContableDto>.Failure($"Account {l.CuentaContableId} not found.");
            if (!cuenta.PermiteMovimientos)
                return Result<AsientoContableDto>.Failure($"Account '{cuenta.Codigo} — {cuenta.Nombre}' does not allow transactions (grouping account).");
        }

        // Validate balanced
        var totalDebe = dto.Lineas.Sum(l => l.Debe);
        var totalHaber = dto.Lineas.Sum(l => l.Haber);
        if (totalDebe != totalHaber)
            return Result<AsientoContableDto>.Failure(
                $"Entry is not balanced. Debit: {totalDebe:N2}, Credit: {totalHaber:N2}. Difference: {Math.Abs(totalDebe - totalHaber):N2}.");

        var numero = await GenerarNumeroAsync(empresaId.Value, dto.Fecha, ct);

        var asiento = new AsientoContable
        {
            EmpresaId = empresaId.Value,
            Numero = numero,
            Fecha = dto.Fecha,
            Tipo = "Manual",
            Glosa = dto.Glosa,
            Estado = "Borrador",
            OrigenTipo = dto.OrigenTipo,
            OrigenId = dto.OrigenId,
            OrigenReferencia = dto.OrigenReferencia,
            TotalDebe = totalDebe,
            TotalHaber = totalHaber,
            Activo = true
        };

        await _asientoRepo.AddAsync(asiento, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        int lineNum = 1;
        foreach (var l in dto.Lineas)
        {
            var linea = new AsientoContableLinea
            {
                AsientoContableId = asiento.AsientoContableId,
                NumeroLinea = lineNum++,
                CuentaContableId = l.CuentaContableId,
                Debe = l.Debe,
                Haber = l.Haber,
                Glosa = l.Glosa,
                Referencia = l.Referencia,
                Activo = true
            };
            await _lineaRepo.AddAsync(linea, ct);
        }

        await _unitOfWork.SaveChangesAsync(ct);
        return Result<AsientoContableDto>.Success(await BuildDto(asiento, ct));
    }

    public async Task<Result<AsientoContableDto>> ContabilizarAsync(long id, CancellationToken ct)
    {
        var empresaId = _currentUser.EmpresaId;
        if (!empresaId.HasValue)
            return Result<AsientoContableDto>.Failure("No active company.");

        var asiento = await _asientoRepo.GetByIdAsync(id, ct);
        if (asiento is null || asiento.EmpresaId != empresaId.Value || !asiento.Activo)
            return Result<AsientoContableDto>.Failure("Journal entry not found.");

        if (asiento.Estado != "Borrador")
            return Result<AsientoContableDto>.Failure($"Only draft entries can be posted. Current status: {asiento.Estado}.");

        // Validate accounting period is open
        var periodError = await ValidarPeriodoAbiertoAsync(empresaId.Value, asiento.Fecha, ct);
        if (periodError != null)
            return Result<AsientoContableDto>.Failure(periodError);

        var lineas = await _lineaRepo.FindAsync(l => l.AsientoContableId == id, ct);
        if (lineas.Count < 2)
            return Result<AsientoContableDto>.Failure("A journal entry requires at least 2 lines.");

        // Re-validate balanced
        var totalDebe = lineas.Sum(l => l.Debe);
        var totalHaber = lineas.Sum(l => l.Haber);
        if (totalDebe != totalHaber)
            return Result<AsientoContableDto>.Failure(
                $"Entry is not balanced. Debit: {totalDebe:N2}, Credit: {totalHaber:N2}.");

        // Update account balances
        foreach (var linea in lineas)
        {
            var cuenta = await _cuentaRepo.GetByIdAsync(linea.CuentaContableId, ct);
            if (cuenta is null) continue;

            // Deudora: Debe increases, Haber decreases
            // Acreedora: Haber increases, Debe decreases
            if (cuenta.Naturaleza == NaturalezaCuenta.Deudora)
                cuenta.SaldoActual += linea.Debe - linea.Haber;
            else
                cuenta.SaldoActual += linea.Haber - linea.Debe;

            await _cuentaRepo.UpdateAsync(cuenta, ct);
        }

        asiento.Estado = "Contabilizado";
        asiento.TotalDebe = totalDebe;
        asiento.TotalHaber = totalHaber;
        await _asientoRepo.UpdateAsync(asiento, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<AsientoContableDto>.Success(await BuildDto(asiento, ct));
    }

    public async Task<Result<AsientoContableDto>> AnularAsync(long id, CancellationToken ct)
    {
        var empresaId = _currentUser.EmpresaId;
        if (!empresaId.HasValue)
            return Result<AsientoContableDto>.Failure("No active company.");

        var asiento = await _asientoRepo.GetByIdAsync(id, ct);
        if (asiento is null || asiento.EmpresaId != empresaId.Value || !asiento.Activo)
            return Result<AsientoContableDto>.Failure("Journal entry not found.");

        if (asiento.Estado != "Contabilizado")
            return Result<AsientoContableDto>.Failure($"Only posted entries can be voided. Current status: {asiento.Estado}.");

        // Reverse account balances
        var lineas = await _lineaRepo.FindAsync(l => l.AsientoContableId == id, ct);
        foreach (var linea in lineas)
        {
            var cuenta = await _cuentaRepo.GetByIdAsync(linea.CuentaContableId, ct);
            if (cuenta is null) continue;

            if (cuenta.Naturaleza == NaturalezaCuenta.Deudora)
                cuenta.SaldoActual -= linea.Debe - linea.Haber;
            else
                cuenta.SaldoActual -= linea.Haber - linea.Debe;

            await _cuentaRepo.UpdateAsync(cuenta, ct);
        }

        asiento.Estado = "Anulado";
        await _asientoRepo.UpdateAsync(asiento, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<AsientoContableDto>.Success(await BuildDto(asiento, ct));
    }

    public async Task<Result<bool>> DeleteAsync(long id, CancellationToken ct)
    {
        var empresaId = _currentUser.EmpresaId;
        if (!empresaId.HasValue)
            return Result<bool>.Failure("No active company.");

        var asiento = await _asientoRepo.GetByIdAsync(id, ct);
        if (asiento is null || asiento.EmpresaId != empresaId.Value)
            return Result<bool>.Failure("Journal entry not found.");

        if (asiento.Estado != "Borrador")
            return Result<bool>.Failure("Only draft entries can be deleted.");

        asiento.Activo = false;
        await _asientoRepo.UpdateAsync(asiento, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<bool>.Success(true);
    }

    // ?? Helpers ??

    /// <summary>
    /// Validates the accounting period for the given date is open.
    /// Returns null if OK, or an error message if the period is closed/missing.
    /// </summary>
    private async Task<string?> ValidarPeriodoAbiertoAsync(int empresaId, DateTime fecha, CancellationToken ct)
    {
        var periodos = await _periodoRepo.FindAsync(
            p => p.EmpresaId == empresaId && p.Anio == fecha.Year && p.Mes == fecha.Month && p.Activo, ct);

        var periodo = periodos.FirstOrDefault();
        if (periodo is null)
            return null; // No period defined = allowed (periods are optional until created)

        if (periodo.Estado == "Cerrado")
            return $"Accounting period '{periodo.Nombre}' is closed (closed on {periodo.FechaCierre:dd/MM/yyyy} by {periodo.CerradoPor}). Reopen it first.";

        return null;
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

    private async Task<AsientoContableDto> BuildDto(AsientoContable asiento, CancellationToken ct)
    {
        var lineas = await _lineaRepo.FindAsync(l => l.AsientoContableId == asiento.AsientoContableId, ct);
        var cuentaIds = lineas.Select(l => l.CuentaContableId).Distinct().ToList();
        var cuentas = cuentaIds.Count > 0
            ? await _cuentaRepo.FindAsync(c => cuentaIds.Contains(c.CuentaContableId), ct)
            : new List<CuentaContable>();
        var cuentaMap = cuentas.ToDictionary(c => c.CuentaContableId);

        return new AsientoContableDto
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
            TotalDebe = asiento.TotalDebe,
            TotalHaber = asiento.TotalHaber,
            Cuadrado = asiento.TotalDebe == asiento.TotalHaber,
            Lineas = lineas.OrderBy(l => l.NumeroLinea).Select(l =>
            {
                cuentaMap.TryGetValue(l.CuentaContableId, out var cuenta);
                return new AsientoContableLineaDto
                {
                    AsientoContableLineaId = l.AsientoContableLineaId,
                    NumeroLinea = l.NumeroLinea,
                    CuentaContableId = l.CuentaContableId,
                    CuentaCodigo = cuenta?.Codigo ?? "—",
                    CuentaNombre = cuenta?.Nombre ?? "—",
                    Debe = l.Debe,
                    Haber = l.Haber,
                    Glosa = l.Glosa,
                    Referencia = l.Referencia
                };
            }).ToList()
        };
    }
}
