namespace AgoraHub360.ERP.Application.Services;

using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Domain.Entities.CMP;
using AgoraHub360.ERP.Domain.Entities.Core;
using AgoraHub360.ERP.Domain.Enums;
using AgoraHub360.ERP.Domain.Interfaces;
using AgoraHub360.ERP.Shared.DTOs.Compras;

public class ExpedienteImportacionService : IExpedienteImportacionService
{
    private readonly IRepository<ExpedienteImportacion> _expRepo;
    private readonly IRepository<HitoExpediente> _hitoRepo;
    private readonly IRepository<OrdenCompra> _ocRepo;
    private readonly IRepository<NumeracionDocumento> _numRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public ExpedienteImportacionService(
        IRepository<ExpedienteImportacion> expRepo,
        IRepository<HitoExpediente> hitoRepo,
        IRepository<OrdenCompra> ocRepo,
        IRepository<NumeracionDocumento> numRepo,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser)
    {
        _expRepo = expRepo;
        _hitoRepo = hitoRepo;
        _ocRepo = ocRepo;
        _numRepo = numRepo;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<Result<IReadOnlyList<ExpedienteImportacionDto>>> GetAllAsync(
        string? estado, DateTime? fechaDesde, DateTime? fechaHasta, CancellationToken ct)
    {
        var empresaId = _currentUser.EmpresaId;
        if (!empresaId.HasValue)
            return Result<IReadOnlyList<ExpedienteImportacionDto>>.Failure("No active company.");

        var expedientes = await _expRepo.FindAsync(
            e => e.EmpresaId == empresaId.Value && e.Activo
                && (!fechaDesde.HasValue || (e.ETD ?? e.FechaCreacion) >= fechaDesde.Value)
                && (!fechaHasta.HasValue || (e.ETD ?? e.FechaCreacion) <= fechaHasta.Value.AddDays(1).AddSeconds(-1)),
            ct);

        if (!string.IsNullOrEmpty(estado) && Enum.TryParse<EstadoDocumento>(estado, true, out var estadoEnum))
            expedientes = expedientes.Where(e => e.Estado == estadoEnum).ToList();

        var dtos = new List<ExpedienteImportacionDto>();
        foreach (var exp in expedientes.OrderByDescending(e => e.ExpedienteImportacionId))
            dtos.Add(await BuildDto(exp, ct));

        return Result<IReadOnlyList<ExpedienteImportacionDto>>.Success(dtos.AsReadOnly());
    }

    public async Task<Result<ExpedienteImportacionDto>> GetByIdAsync(long id, CancellationToken ct)
    {
        var empresaId = _currentUser.EmpresaId;
        if (!empresaId.HasValue)
            return Result<ExpedienteImportacionDto>.Failure("No active company.");

        var exp = await _expRepo.GetByIdAsync(id, ct);
        if (exp is null || exp.EmpresaId != empresaId.Value || !exp.Activo)
            return Result<ExpedienteImportacionDto>.Failure("Import expedition not found.");

        return Result<ExpedienteImportacionDto>.Success(await BuildDto(exp, ct));
    }

    public async Task<Result<ExpedienteImportacionDto>> CreateAsync(CreateExpedienteImportacionDto dto, CancellationToken ct)
    {
        var empresaId = _currentUser.EmpresaId;
        if (!empresaId.HasValue)
            return Result<ExpedienteImportacionDto>.Failure("No active company.");

        if (dto.OrdenesCompraIds is null || dto.OrdenesCompraIds.Count == 0)
            return Result<ExpedienteImportacionDto>.Failure("At least one purchase order is required.");

        // Validate all OCs
        foreach (var ocId in dto.OrdenesCompraIds)
        {
            var oc = await _ocRepo.GetByIdAsync(ocId, ct);
            if (oc is null || oc.EmpresaId != empresaId.Value || !oc.Activo)
                return Result<ExpedienteImportacionDto>.Failure($"Purchase order {ocId} not found.");

            var validStates = new[]
            {
                EstadoDocumento.ConfirmadaProveedor,
                EstadoDocumento.PagoProgramado,
                EstadoDocumento.Aprobado
            };
            if (!validStates.Contains(oc.Estado))
                return Result<ExpedienteImportacionDto>.Failure(
                    $"Purchase order {oc.Numero} must be in ConfirmadaProveedor, PagoProgramado, or Aprobado state. Current: {oc.Estado}.");

            if (oc.ExpedienteImportacionId.HasValue)
                return Result<ExpedienteImportacionDto>.Failure(
                    $"Purchase order {oc.Numero} is already assigned to expedition {oc.ExpedienteImportacionId}.");
        }

        var numero = await GenerarNumeroAsync(empresaId.Value, ct);

        var expediente = new ExpedienteImportacion
        {
            EmpresaId = empresaId.Value,
            Numero = numero,
            Estado = EstadoDocumento.Borrador,
            Incoterm = dto.Incoterm,
            ModalidadTransporte = dto.ModalidadTransporte,
            PaisOrigen = dto.PaisOrigen,
            PuertoOrigen = dto.PuertoOrigen,
            PuertoDestino = dto.PuertoDestino,
            Forwarder = dto.Forwarder,
            Aseguradora = dto.Aseguradora,
            NumeroPólizaSeguro = dto.NumeroPólizaSeguro,
            Observaciones = dto.Observaciones,
            Activo = true
        };

        await _expRepo.AddAsync(expediente, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        // Link OCs
        foreach (var ocId in dto.OrdenesCompraIds)
        {
            var oc = await _ocRepo.GetByIdAsync(ocId, ct);
            oc!.ExpedienteImportacionId = expediente.ExpedienteImportacionId;
            oc.Estado = EstadoDocumento.EnTransito;
            await _ocRepo.UpdateAsync(oc, ct);
        }

        await _unitOfWork.SaveChangesAsync(ct);
        return Result<ExpedienteImportacionDto>.Success(await BuildDto(expediente, ct));
    }

    public async Task<Result<ExpedienteImportacionDto>> UpdateEmbarqueAsync(long id, UpdateExpedienteEmbarqueDto dto, CancellationToken ct)
    {
        var (exp, err) = await GetValidExp(id, ct);
        if (exp is null) return Result<ExpedienteImportacionDto>.Failure(err!);

        exp.Incoterm = dto.Incoterm;
        exp.ModalidadTransporte = dto.ModalidadTransporte;
        exp.PaisOrigen = dto.PaisOrigen;
        exp.PuertoOrigen = dto.PuertoOrigen;
        exp.PuertoDestino = dto.PuertoDestino;
        exp.Forwarder = dto.Forwarder;
        exp.Aseguradora = dto.Aseguradora;
        exp.NumeroPólizaSeguro = dto.NumeroPólizaSeguro;
        exp.NumeroBLAWB = dto.NumeroBLAWB;
        exp.ETD = dto.ETD;
        exp.ETA = dto.ETA;
        exp.Observaciones = dto.Observaciones;

        await _expRepo.UpdateAsync(exp, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        return Result<ExpedienteImportacionDto>.Success(await BuildDto(exp, ct));
    }

    public async Task<Result<ExpedienteImportacionDto>> ConfirmarSalidaAsync(long id, DateTime fechaSalida, CancellationToken ct)
    {
        var (exp, err) = await GetValidExp(id, ct);
        if (exp is null) return Result<ExpedienteImportacionDto>.Failure(err!);

        if (exp.Estado != EstadoDocumento.Borrador)
            return Result<ExpedienteImportacionDto>.Failure("Only draft expeditions can confirm departure.");

        exp.ATD = fechaSalida;
        exp.Estado = EstadoDocumento.EnTransito;
        await _expRepo.UpdateAsync(exp, ct);

        await AddHitoInterno(exp.ExpedienteImportacionId, "ATD", fechaSalida, "Salida confirmada / ATD registrado", ct);
        await _unitOfWork.SaveChangesAsync(ct);
        return Result<ExpedienteImportacionDto>.Success(await BuildDto(exp, ct));
    }

    public async Task<Result<ExpedienteImportacionDto>> RegistrarArriboAsync(long id, RegistrarArriboDto dto, CancellationToken ct)
    {
        var (exp, err) = await GetValidExp(id, ct);
        if (exp is null) return Result<ExpedienteImportacionDto>.Failure(err!);

        if (exp.Estado != EstadoDocumento.EnTransito)
            return Result<ExpedienteImportacionDto>.Failure("Expedition must be EnTransito to register arrival.");

        exp.ATA = dto.FechaArribo;
        exp.Estado = EstadoDocumento.Arribado;
        if (!string.IsNullOrEmpty(dto.Observaciones))
            exp.Observaciones = dto.Observaciones;

        await _expRepo.UpdateAsync(exp, ct);
        await AddHitoInterno(exp.ExpedienteImportacionId, "ATA", dto.FechaArribo, $"Arribo registrado. {dto.Observaciones}", ct);
        await _unitOfWork.SaveChangesAsync(ct);
        return Result<ExpedienteImportacionDto>.Success(await BuildDto(exp, ct));
    }

    public async Task<Result<ExpedienteImportacionDto>> IniciarDespachoAduaneroAsync(long id, RegistrarDespachoAduaneroDto dto, CancellationToken ct)
    {
        var (exp, err) = await GetValidExp(id, ct);
        if (exp is null) return Result<ExpedienteImportacionDto>.Failure(err!);

        if (exp.Estado != EstadoDocumento.Arribado)
            return Result<ExpedienteImportacionDto>.Failure("Expedition must be Arribado to start customs clearance.");

        exp.NumeroDUIDIM = dto.NumeroDUIDIM;
        exp.Despachante = dto.Despachante;
        exp.FechaPresentacionAduana = dto.FechaPresentacion;
        exp.TotalTributos = dto.TotalTributos;
        exp.Estado = EstadoDocumento.EnAduana;
        if (!string.IsNullOrEmpty(dto.Observaciones))
            exp.Observaciones = dto.Observaciones;

        await _expRepo.UpdateAsync(exp, ct);
        await AddHitoInterno(exp.ExpedienteImportacionId, "Aduana", dto.FechaPresentacion,
            $"Despacho iniciado. DUI/DIM: {dto.NumeroDUIDIM}. Despachante: {dto.Despachante}.", ct);
        await _unitOfWork.SaveChangesAsync(ct);
        return Result<ExpedienteImportacionDto>.Success(await BuildDto(exp, ct));
    }

    public async Task<Result<ExpedienteImportacionDto>> RegistrarObservacionAduanaAsync(long id, RegistrarObservacionAduanaDto dto, CancellationToken ct)
    {
        var (exp, err) = await GetValidExp(id, ct);
        if (exp is null) return Result<ExpedienteImportacionDto>.Failure(err!);

        if (exp.Estado != EstadoDocumento.EnAduana)
            return Result<ExpedienteImportacionDto>.Failure("Expedition must be EnAduana to register a customs observation.");

        exp.TuvoObservacionAduana = true;
        exp.DetalleObservacionAduana = dto.DetalleObservacion;
        exp.Estado = EstadoDocumento.ObservacionAduana;

        await _expRepo.UpdateAsync(exp, ct);
        await AddHitoInterno(exp.ExpedienteImportacionId, "ObservacionAduana", DateTime.UtcNow,
            $"Observación/aforo: {dto.DetalleObservacion}", ct);
        await _unitOfWork.SaveChangesAsync(ct);
        return Result<ExpedienteImportacionDto>.Success(await BuildDto(exp, ct));
    }

    public async Task<Result<ExpedienteImportacionDto>> SubsanarObservacionAsync(long id, string? observaciones, CancellationToken ct)
    {
        var (exp, err) = await GetValidExp(id, ct);
        if (exp is null) return Result<ExpedienteImportacionDto>.Failure(err!);

        if (exp.Estado != EstadoDocumento.ObservacionAduana)
            return Result<ExpedienteImportacionDto>.Failure("Expedition must be in ObservacionAduana state.");

        exp.Estado = EstadoDocumento.EnAduana;
        await _expRepo.UpdateAsync(exp, ct);
        await AddHitoInterno(exp.ExpedienteImportacionId, "SubsanacionAduana", DateTime.UtcNow,
            $"Observación subsanada. {observaciones}", ct);
        await _unitOfWork.SaveChangesAsync(ct);
        return Result<ExpedienteImportacionDto>.Success(await BuildDto(exp, ct));
    }

    public async Task<Result<ExpedienteImportacionDto>> RegistrarLevanteAsync(long id, RegistrarLevanteDto dto, CancellationToken ct)
    {
        var (exp, err) = await GetValidExp(id, ct);
        if (exp is null) return Result<ExpedienteImportacionDto>.Failure(err!);

        if (exp.Estado != EstadoDocumento.EnAduana)
            return Result<ExpedienteImportacionDto>.Failure("Expedition must be EnAduana to register customs clearance.");

        exp.FechaLevante = dto.FechaLevante;
        exp.Estado = EstadoDocumento.Liberado;
        if (!string.IsNullOrEmpty(dto.Observaciones))
            exp.Observaciones = dto.Observaciones;

        await _expRepo.UpdateAsync(exp, ct);
        await AddHitoInterno(exp.ExpedienteImportacionId, "Levante", dto.FechaLevante,
            "Levante/liberación aduanera obtenida.", ct);
        await _unitOfWork.SaveChangesAsync(ct);
        return Result<ExpedienteImportacionDto>.Success(await BuildDto(exp, ct));
    }

    public async Task<Result<ExpedienteImportacionDto>> AddHitoAsync(long id, AddHitoExpedienteDto dto, CancellationToken ct)
    {
        var (exp, err) = await GetValidExp(id, ct);
        if (exp is null) return Result<ExpedienteImportacionDto>.Failure(err!);

        await AddHitoInterno(exp.ExpedienteImportacionId, dto.TipoHito, dto.FechaHito, dto.Descripcion, ct,
            dto.ReferenciaDocumento);
        await _unitOfWork.SaveChangesAsync(ct);
        return Result<ExpedienteImportacionDto>.Success(await BuildDto(exp, ct));
    }

    public async Task<Result<ExpedienteImportacionDto>> CerrarAsync(long id, CancellationToken ct)
    {
        var (exp, err) = await GetValidExp(id, ct);
        if (exp is null) return Result<ExpedienteImportacionDto>.Failure(err!);

        if (exp.Estado != EstadoDocumento.Liberado)
            return Result<ExpedienteImportacionDto>.Failure("Only Liberado expeditions can be closed.");

        var ocs = await _ocRepo.FindAsync(o => o.ExpedienteImportacionId == id, ct);
        var abiertas = ocs.Where(o => o.Estado != EstadoDocumento.Cerrado && o.Estado != EstadoDocumento.Anulado).ToList();
        if (abiertas.Count > 0)
            return Result<ExpedienteImportacionDto>.Failure(
                $"Cannot close: {abiertas.Count} purchase order(s) are not yet closed.");

        exp.Estado = EstadoDocumento.Cerrado;
        await _expRepo.UpdateAsync(exp, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        return Result<ExpedienteImportacionDto>.Success(await BuildDto(exp, ct));
    }

    public async Task<Result<bool>> DeleteAsync(long id, CancellationToken ct)
    {
        var empresaId = _currentUser.EmpresaId;
        if (!empresaId.HasValue)
            return Result<bool>.Failure("No active company.");

        var exp = await _expRepo.GetByIdAsync(id, ct);
        if (exp is null || exp.EmpresaId != empresaId.Value)
            return Result<bool>.Failure("Import expedition not found.");
        if (exp.Estado != EstadoDocumento.Borrador)
            return Result<bool>.Failure("Only draft expeditions can be deleted.");

        exp.Activo = false;
        await _expRepo.UpdateAsync(exp, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        return Result<bool>.Success(true);
    }

    // ?? Private helpers ????????????????????????????????????????????????????????

    private async Task<(ExpedienteImportacion? exp, string? error)> GetValidExp(long id, CancellationToken ct)
    {
        var empresaId = _currentUser.EmpresaId;
        if (!empresaId.HasValue)
            return (null, "No active company.");

        var exp = await _expRepo.GetByIdAsync(id, ct);
        if (exp is null || exp.EmpresaId != empresaId.Value || !exp.Activo)
            return (null, $"Import expedition {id} not found.");

        return (exp, null);
    }

    private async Task AddHitoInterno(long expedienteId, string tipo, DateTime fecha, string? descripcion,
        CancellationToken ct, string? referencia = null)
    {
        var hito = new HitoExpediente
        {
            ExpedienteImportacionId = expedienteId,
            TipoHito = tipo,
            FechaHito = fecha,
            Descripcion = descripcion,
            ReferenciaDocumento = referencia,
            Activo = true
        };
        await _hitoRepo.AddAsync(hito, ct);
    }

    private async Task<string> GenerarNumeroAsync(int empresaId, CancellationToken ct)
    {
        var numeraciones = await _numRepo.FindAsync(
            n => n.EmpresaId == empresaId && n.TipoDocumento == "EXP", ct);
        var num = numeraciones.FirstOrDefault();
        if (num is not null)
        {
            var numero = num.GenerarSiguiente();
            await _numRepo.UpdateAsync(num, ct);
            return numero;
        }
        var existentes = await _expRepo.FindAsync(e => e.EmpresaId == empresaId, ct);
        return $"EXP-{(existentes.Count + 1):D6}";
    }

    private async Task<ExpedienteImportacionDto> BuildDto(ExpedienteImportacion exp, CancellationToken ct)
    {
        var ocs = await _ocRepo.FindAsync(o => o.ExpedienteImportacionId == exp.ExpedienteImportacionId, ct);
        var hitos = await _hitoRepo.FindAsync(h => h.ExpedienteImportacionId == exp.ExpedienteImportacionId, ct);

        return new ExpedienteImportacionDto(
            exp.ExpedienteImportacionId,
            exp.EmpresaId,
            exp.Numero,
            exp.Estado.ToString(),
            exp.Incoterm,
            exp.ModalidadTransporte,
            exp.PaisOrigen,
            exp.PuertoOrigen,
            exp.PuertoDestino,
            exp.Forwarder,
            exp.Aseguradora,
            exp.NumeroPólizaSeguro,
            exp.NumeroBLAWB,
            exp.ETD,
            exp.ATD,
            exp.ETA,
            exp.ATA,
            exp.NumeroDUIDIM,
            exp.Despachante,
            exp.FechaPresentacionAduana,
            exp.TotalTributos,
            exp.TuvoObservacionAduana,
            exp.DetalleObservacionAduana,
            exp.FechaLevante,
            exp.Observaciones,
            exp.Activo,
            ocs.Select(o => o.OrdenCompraId).ToList(),
            ocs.Select(o => o.Numero).ToList(),
            hitos.OrderBy(h => h.FechaHito).Select(h => new HitoExpedienteDto(
                h.HitoExpedienteId,
                h.TipoHito,
                h.FechaHito,
                h.Descripcion,
                h.ReferenciaDocumento)).ToList());
    }
}
