namespace AgoraHub360.ERP.Application.Services;

using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Domain.Entities.ACC;
using AgoraHub360.ERP.Domain.Entities.Core;
using AgoraHub360.ERP.Domain.Entities.CST;
using AgoraHub360.ERP.Domain.Enums;
using AgoraHub360.ERP.Domain.Interfaces;
using AgoraHub360.ERP.Shared.DTOs.Contabilidad;

public class AsientoContableService : IAsientoContableService
{
    private readonly IRepository<AsientoContable> _asientoRepo;
    private readonly IRepository<AsientoContableLinea> _lineaRepo;
    private readonly IRepository<CuentaContable> _cuentaRepo;
    private readonly IRepository<PeriodoContable> _periodoRepo;
    private readonly IRepository<TipoComprobante> _tipoCompRepo;
    private readonly IRepository<TipoCambio> _tipoCambioRepo;
    private readonly IRepository<TipoPago> _tipoPagoRepo;
    private readonly IRepository<NumeracionDocumento> _numRepo;
    private readonly IRepository<CentroCosto> _centroCostoRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IServiceProvider _serviceProvider;

    public AsientoContableService(
        IRepository<AsientoContable> asientoRepo,
        IRepository<AsientoContableLinea> lineaRepo,
        IRepository<CuentaContable> cuentaRepo,
        IRepository<PeriodoContable> periodoRepo,
        IRepository<TipoComprobante> tipoCompRepo,
        IRepository<TipoCambio> tipoCambioRepo,
        IRepository<TipoPago> tipoPagoRepo,
        IRepository<NumeracionDocumento> numRepo,
        IRepository<CentroCosto> centroCostoRepo,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        IServiceProvider serviceProvider)
    {
        _asientoRepo     = asientoRepo;
        _lineaRepo       = lineaRepo;
        _cuentaRepo      = cuentaRepo;
        _periodoRepo     = periodoRepo;
        _tipoCompRepo    = tipoCompRepo;
        _tipoCambioRepo  = tipoCambioRepo;
        _tipoPagoRepo    = tipoPagoRepo;
        _numRepo         = numRepo;
        _centroCostoRepo = centroCostoRepo;
        _unitOfWork      = unitOfWork;
        _currentUser     = currentUser;
        _serviceProvider = serviceProvider;
    }

    // ??????????????????????????????????????????????????????????????????
    // GET ALL
    // ??????????????????????????????????????????????????????????????????
    public async Task<Result<IReadOnlyList<AsientoContableDto>>> GetAllAsync(
        DateTime? fechaDesde, DateTime? fechaHasta, string? estado,
        int? tipoComprobanteId, string? search, CancellationToken ct)
    {
        var empresaId = _currentUser.EmpresaId;
        if (!empresaId.HasValue)
            return Result<IReadOnlyList<AsientoContableDto>>.Failure("No active company.");

        var asientos = await _asientoRepo.FindAsync(
            a => a.EmpresaId == empresaId.Value && a.Activo
                && (!fechaDesde.HasValue || a.Fecha >= fechaDesde.Value)
                && (!fechaHasta.HasValue || a.Fecha <= fechaHasta.Value.AddDays(1).AddSeconds(-1))
                && (string.IsNullOrEmpty(estado) || a.Estado == estado)
                && (!tipoComprobanteId.HasValue || a.TipoComprobanteId == tipoComprobanteId.Value)
                && (string.IsNullOrEmpty(search) || a.Numero.Contains(search) || a.Glosa.Contains(search)),
            ct);

        var dtos = new List<AsientoContableDto>();
        foreach (var a in asientos.OrderByDescending(x => x.Fecha).ThenByDescending(x => x.AsientoContableId))
            dtos.Add(await BuildDto(a, ct));

        return Result<IReadOnlyList<AsientoContableDto>>.Success(dtos.AsReadOnly());
    }

    // ??????????????????????????????????????????????????????????????????
    // GET BY ID
    // ??????????????????????????????????????????????????????????????????
    public async Task<Result<AsientoContableDto>> GetByIdAsync(long id, CancellationToken ct)
    {
        var empresaId = _currentUser.EmpresaId;
        if (!empresaId.HasValue)
            return Result<AsientoContableDto>.Failure("No active company.");

        var asiento = await _asientoRepo.GetByIdAsync(id, ct);
        if (asiento is null || asiento.EmpresaId != empresaId.Value || !asiento.Activo)
            return Result<AsientoContableDto>.Failure("Comprobante no encontrado.");

        return Result<AsientoContableDto>.Success(await BuildDto(asiento, ct));
    }

    // ??????????????????????????????????????????????????????????????????
    // CREATE
    // ??????????????????????????????????????????????????????????????????
    public async Task<Result<AsientoContableDto>> CreateAsync(CreateAsientoContableDto dto, CancellationToken ct)
    {
        var empresaId = _currentUser.EmpresaId;
        if (!empresaId.HasValue)
            return Result<AsientoContableDto>.Failure("No active company.");

        // Validate tipo comprobante
        var tipoComp = await _tipoCompRepo.GetByIdAsync(dto.TipoComprobanteId, ct);
        if (tipoComp is null || tipoComp.EmpresaId != empresaId.Value || !tipoComp.Activo)
            return Result<AsientoContableDto>.Failure("Tipo de comprobante no válido.");

        // Validate period
        var periodError = await ValidarPeriodoAbiertoAsync(empresaId.Value, dto.Fecha, ct);
        if (periodError != null)
            return Result<AsientoContableDto>.Failure(periodError);

        // Validate cierre contable
        await ValidarCierreContableAsync(empresaId.Value, dto.Fecha.Year, ct);

        // Validate lines
        var lineError = ValidarLineas(dto.Lineas);
        if (lineError != null)
            return Result<AsientoContableDto>.Failure(lineError);

        // Validate accounts
        foreach (var l in dto.Lineas)
        {
            var cuenta = await _cuentaRepo.GetByIdAsync(l.CuentaContableId, ct);
            if (cuenta is null || cuenta.EmpresaId != empresaId.Value || !cuenta.Activo)
                return Result<AsientoContableDto>.Failure($"Cuenta {l.CuentaContableId} no encontrada.");
            if (!cuenta.PermiteMovimientos)
                return Result<AsientoContableDto>.Failure($"La cuenta '{cuenta.Codigo} – {cuenta.Nombre}' no permite movimientos (cuenta agrupadora).");

            // Validate centro de costo (nullable — no rompe datos existentes)
            if (l.CentroCostoId.HasValue)
            {
                var cc = await _centroCostoRepo.GetByIdAsync(l.CentroCostoId.Value, ct);
                if (cc is null || cc.EmpresaId != empresaId.Value || !cc.Activo)
                    return Result<AsientoContableDto>.Failure($"Centro de costo {l.CentroCostoId} no encontrado o no pertenece a la empresa.");
            }
        }

        // Validate balanced
        var totalDebe = dto.Lineas.Sum(l => l.Debe);
        var totalHaber = dto.Lineas.Sum(l => l.Haber);
        if (totalDebe != totalHaber)
            return Result<AsientoContableDto>.Failure(
                $"El comprobante no cuadra. Debe: {totalDebe:N2}, Haber: {totalHaber:N2}. Diferencia: {Math.Abs(totalDebe - totalHaber):N2}.");

        // Resolve tipo cambio
        decimal? valorTc = null;
        if (dto.TipoCambioId.HasValue)
        {
            var tc = await _tipoCambioRepo.GetByIdAsync(dto.TipoCambioId.Value, ct);
            if (tc is not null) valorTc = tc.TasaVenta;
        }

        var gestion = dto.Fecha.Year;
        var numero = await GenerarNumeroComprobanteAsync(empresaId.Value, tipoComp, gestion, ct);

        var asiento = new AsientoContable
        {
            EmpresaId = empresaId.Value,
            TipoComprobanteId = dto.TipoComprobanteId,
            Numero = numero,
            Fecha = dto.Fecha,
            Gestion = gestion,
            TipoRegistro = "Manual",
            Estado = "Borrador",
            Concepto = dto.Concepto,
            Glosa = dto.Glosa,
            TipoCambioId = dto.TipoCambioId,
            ValorTipoCambio = valorTc,
            TipoPagoId = dto.TipoPagoId,
            NumeroDocumentoPago = dto.NumeroDocumentoPago,
            RegistradoPorId = _currentUser.UserIdInt,
            RegistradoPorNombre = _currentUser.UserName,
            OrigenTipo = dto.OrigenTipo,
            OrigenId = dto.OrigenId,
            OrigenReferencia = dto.OrigenReferencia,
            Activo = true
        };

        asiento.EstablecerTotales(totalDebe, totalHaber);
        if (!asiento.EstaCuadrado())
            return Result<AsientoContableDto>.Failure("El comprobante no cuadra de acuerdo a las reglas de dominio.");

        await _asientoRepo.AddAsync(asiento, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        await SaveLineasAsync(asiento.AsientoContableId, dto.Lineas, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<AsientoContableDto>.Success(await BuildDto(asiento, ct));
    }

    // ??????????????????????????????????????????????????????????????????
    // UPDATE (solo Borrador)
    // ??????????????????????????????????????????????????????????????????
    public async Task<Result<AsientoContableDto>> UpdateAsync(long id, UpdateAsientoContableDto dto, CancellationToken ct)
    {
        var empresaId = _currentUser.EmpresaId;
        if (!empresaId.HasValue)
            return Result<AsientoContableDto>.Failure("No active company.");

        var asiento = await _asientoRepo.GetByIdAsync(id, ct);
        if (asiento is null || asiento.EmpresaId != empresaId.Value || !asiento.Activo)
            return Result<AsientoContableDto>.Failure("Comprobante no encontrado.");

        if (asiento.Estado != "Borrador")
            return Result<AsientoContableDto>.Failure("Solo se pueden editar comprobantes en Borrador.");

        var tipoComp = await _tipoCompRepo.GetByIdAsync(dto.TipoComprobanteId, ct);
        if (tipoComp is null || tipoComp.EmpresaId != empresaId.Value || !tipoComp.Activo)
            return Result<AsientoContableDto>.Failure("Tipo de comprobante no válido.");

        var periodError = await ValidarPeriodoAbiertoAsync(empresaId.Value, dto.Fecha, ct);
        if (periodError != null)
            return Result<AsientoContableDto>.Failure(periodError);

        await ValidarCierreContableAsync(empresaId.Value, asiento.Fecha.Year, ct);
        if (asiento.Fecha.Year != dto.Fecha.Year)
        {
            await ValidarCierreContableAsync(empresaId.Value, dto.Fecha.Year, ct);
        }

        var lineError = ValidarLineas(dto.Lineas);
        if (lineError != null)
            return Result<AsientoContableDto>.Failure(lineError);

        foreach (var l in dto.Lineas)
        {
            var cuenta = await _cuentaRepo.GetByIdAsync(l.CuentaContableId, ct);
            if (cuenta is null || cuenta.EmpresaId != empresaId.Value || !cuenta.Activo)
                return Result<AsientoContableDto>.Failure($"Cuenta {l.CuentaContableId} no encontrada.");
            if (!cuenta.PermiteMovimientos)
                return Result<AsientoContableDto>.Failure($"La cuenta '{cuenta.Codigo}' no permite movimientos.");

            if (l.CentroCostoId.HasValue)
            {
                var cc = await _centroCostoRepo.GetByIdAsync(l.CentroCostoId.Value, ct);
                if (cc is null || cc.EmpresaId != empresaId.Value || !cc.Activo)
                    return Result<AsientoContableDto>.Failure($"Centro de costo {l.CentroCostoId} no encontrado o no pertenece a la empresa.");
            }
        }

        var totalDebe = dto.Lineas.Sum(l => l.Debe);
        var totalHaber = dto.Lineas.Sum(l => l.Haber);
        if (totalDebe != totalHaber)
            return Result<AsientoContableDto>.Failure(
                $"El comprobante no cuadra. Debe: {totalDebe:N2}, Haber: {totalHaber:N2}. Diferencia: {Math.Abs(totalDebe - totalHaber):N2}.");

        decimal? valorTc = null;
        if (dto.TipoCambioId.HasValue)
        {
            var tc = await _tipoCambioRepo.GetByIdAsync(dto.TipoCambioId.Value, ct);
            if (tc is not null) valorTc = tc.TasaVenta;
        }

        // Re-generate number if tipo changed
        if (asiento.TipoComprobanteId != dto.TipoComprobanteId)
        {
            asiento.TipoComprobanteId = dto.TipoComprobanteId;
            asiento.Numero = await GenerarNumeroComprobanteAsync(empresaId.Value, tipoComp, dto.Fecha.Year, ct);
        }

        asiento.Fecha = dto.Fecha;
        asiento.Gestion = dto.Fecha.Year;
        asiento.Concepto = dto.Concepto;
        asiento.Glosa = dto.Glosa;
        asiento.TipoCambioId = dto.TipoCambioId;
        asiento.ValorTipoCambio = valorTc;
        asiento.TipoPagoId = dto.TipoPagoId;
        asiento.NumeroDocumentoPago = dto.NumeroDocumentoPago;
        asiento.EstablecerTotales(totalDebe, totalHaber);
        if (!asiento.EstaCuadrado())
            return Result<AsientoContableDto>.Failure("El comprobante no cuadra.");

        await _asientoRepo.UpdateAsync(asiento, ct);

        // Delete old lines
        var oldLineas = await _lineaRepo.FindAsync(l => l.AsientoContableId == id, ct);
        foreach (var old in oldLineas)
        {
            old.Activo = false;
            await _lineaRepo.UpdateAsync(old, ct);
        }

        await SaveLineasAsync(id, dto.Lineas, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<AsientoContableDto>.Success(await BuildDto(asiento, ct));
    }

    // ??????????????????????????????????????????????????????????????????
    // COPIAR
    // ??????????????????????????????????????????????????????????????????
    public async Task<Result<AsientoContableDto>> CopiarAsync(long id, CancellationToken ct)
    {
        var empresaId = _currentUser.EmpresaId;
        if (!empresaId.HasValue)
            return Result<AsientoContableDto>.Failure("No active company.");

        var original = await _asientoRepo.GetByIdAsync(id, ct);
        if (original is null || original.EmpresaId != empresaId.Value || !original.Activo)
            return Result<AsientoContableDto>.Failure("Comprobante no encontrado.");

        var tipoComp = original.TipoComprobanteId.HasValue
            ? await _tipoCompRepo.GetByIdAsync(original.TipoComprobanteId.Value, ct) : null;
        if (tipoComp is null)
            return Result<AsientoContableDto>.Failure("Tipo de comprobante no encontrado.");

        var hoy = DateTime.Today;
        var gestion = hoy.Year;

        await ValidarCierreContableAsync(empresaId.Value, gestion, ct);

        var numero = await GenerarNumeroComprobanteAsync(empresaId.Value, tipoComp, gestion, ct);

        var copia = new AsientoContable
        {
            EmpresaId = empresaId.Value,
            TipoComprobanteId = original.TipoComprobanteId,
            Numero = numero,
            Fecha = hoy,
            Gestion = gestion,
            TipoRegistro = "Manual",
            Estado = "Borrador",
            Concepto = original.Concepto,
            Glosa = original.Glosa,
            TipoCambioId = original.TipoCambioId,
            ValorTipoCambio = original.ValorTipoCambio,
            TipoPagoId = original.TipoPagoId,
            NumeroDocumentoPago = null,
            RegistradoPorId = _currentUser.UserIdInt,
            RegistradoPorNombre = _currentUser.UserName,
            Activo = true
        };

        copia.EstablecerTotales(original.TotalDebe, original.TotalHaber);

        await _asientoRepo.AddAsync(copia, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        var originalLineas = await _lineaRepo.FindAsync(l => l.AsientoContableId == id && l.Activo, ct);
        int lineNum = 1;
        foreach (var ol in originalLineas.OrderBy(l => l.NumeroLinea))
        {
            await _lineaRepo.AddAsync(new AsientoContableLinea
            {
                AsientoContableId = copia.AsientoContableId,
                NumeroLinea       = lineNum++,
                CuentaContableId  = ol.CuentaContableId,
                Debe              = ol.Debe,
                Haber             = ol.Haber,
                Glosa             = ol.Glosa,
                Referencia        = ol.Referencia,
                CentroCostoId     = ol.CentroCostoId,
                Activo            = true
            }, ct);
        }

        await _unitOfWork.SaveChangesAsync(ct);
        return Result<AsientoContableDto>.Success(await BuildDto(copia, ct));
    }

    // ??????????????????????????????????????????????????????????????????
    // CONTABILIZAR
    // ??????????????????????????????????????????????????????????????????
    public async Task<Result<AsientoContableDto>> ContabilizarAsync(long id, CancellationToken ct)
    {
        var empresaId = _currentUser.EmpresaId;
        if (!empresaId.HasValue)
            return Result<AsientoContableDto>.Failure("No active company.");

        var asiento = await _asientoRepo.GetByIdAsync(id, ct);
        if (asiento is null || asiento.EmpresaId != empresaId.Value || !asiento.Activo)
            return Result<AsientoContableDto>.Failure("Comprobante no encontrado.");

        if (asiento.Estado != "Borrador")
            return Result<AsientoContableDto>.Failure($"Solo comprobantes en Borrador pueden contabilizarse. Estado actual: {asiento.Estado}.");

        var periodError = await ValidarPeriodoAbiertoAsync(empresaId.Value, asiento.Fecha, ct);
        if (periodError != null)
            return Result<AsientoContableDto>.Failure(periodError);

        await ValidarCierreContableAsync(empresaId.Value, asiento.Fecha.Year, ct);

        var lineas = await _lineaRepo.FindAsync(l => l.AsientoContableId == id && l.Activo, ct);
        if (lineas.Count < 2)
            return Result<AsientoContableDto>.Failure("Un comprobante requiere al menos 2 líneas.");

        var totalDebe = lineas.Sum(l => l.Debe);
        var totalHaber = lineas.Sum(l => l.Haber);
        if (totalDebe != totalHaber)
            return Result<AsientoContableDto>.Failure(
                $"El comprobante no cuadra. Debe: {totalDebe:N2}, Haber: {totalHaber:N2}.");

        foreach (var linea in lineas)
        {
            var cuenta = await _cuentaRepo.GetByIdAsync(linea.CuentaContableId, ct);
            if (cuenta is null) continue;

            if (cuenta.Naturaleza == NaturalezaCuenta.Deudora)
                cuenta.SaldoActual += linea.Debe - linea.Haber;
            else
                cuenta.SaldoActual += linea.Haber - linea.Debe;

            await _cuentaRepo.UpdateAsync(cuenta, ct);
        }

        asiento.Estado = "Contabilizado";
        asiento.EstablecerTotales(totalDebe, totalHaber);
        if (!asiento.EstaCuadrado())
            return Result<AsientoContableDto>.Failure("El comprobante no cuadra de acuerdo a las reglas de dominio al intentar contabilizar.");

        await _asientoRepo.UpdateAsync(asiento, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<AsientoContableDto>.Success(await BuildDto(asiento, ct));
    }

    // ??????????????????????????????????????????????????????????????????
    // ANULAR
    // ??????????????????????????????????????????????????????????????????
    public async Task<Result<AsientoContableDto>> AnularAsync(long id, CancellationToken ct)
    {
        var empresaId = _currentUser.EmpresaId;
        if (!empresaId.HasValue)
            return Result<AsientoContableDto>.Failure("No active company.");

        var asiento = await _asientoRepo.GetByIdAsync(id, ct);
        if (asiento is null || asiento.EmpresaId != empresaId.Value || !asiento.Activo)
            return Result<AsientoContableDto>.Failure("Comprobante no encontrado.");

        if (asiento.Estado != "Contabilizado")
            return Result<AsientoContableDto>.Failure($"Solo comprobantes Contabilizados pueden anularse. Estado actual: {asiento.Estado}.");

        await ValidarCierreContableAsync(empresaId.Value, asiento.Fecha.Year, ct);

        var lineas = await _lineaRepo.FindAsync(l => l.AsientoContableId == id && l.Activo, ct);
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

    // ??????????????????????????????????????????????????????????????????
    // DELETE (solo Borrador)
    // ??????????????????????????????????????????????????????????????????
    public async Task<Result<bool>> DeleteAsync(long id, CancellationToken ct)
    {
        var empresaId = _currentUser.EmpresaId;
        if (!empresaId.HasValue)
            return Result<bool>.Failure("No active company.");

        var asiento = await _asientoRepo.GetByIdAsync(id, ct);
        if (asiento is null || asiento.EmpresaId != empresaId.Value)
            return Result<bool>.Failure("Comprobante no encontrado.");

        if (asiento.Estado != "Borrador")
            return Result<bool>.Failure("Solo comprobantes en Borrador pueden eliminarse.");

        await ValidarCierreContableAsync(empresaId.Value, asiento.Fecha.Year, ct);

        asiento.Activo = false;
        await _asientoRepo.UpdateAsync(asiento, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<bool>.Success(true);
    }

    // ??????????????????????????????????????????????????????????????????
    // CATÁLOGOS
    // ??????????????????????????????????????????????????????????????????
    public async Task<Result<IReadOnlyList<TipoComprobanteDto>>> GetTiposComprobanteAsync(CancellationToken ct)
    {
        var empresaId = _currentUser.EmpresaId;
        if (!empresaId.HasValue)
            return Result<IReadOnlyList<TipoComprobanteDto>>.Failure("No active company.");

        var items = await _tipoCompRepo.FindAsync(t => t.EmpresaId == empresaId.Value && t.Activo, ct);
        var dtos = items.OrderBy(t => t.Orden).Select(t => new TipoComprobanteDto
        {
            TipoComprobanteId = t.TipoComprobanteId, Codigo = t.Codigo, Nombre = t.Nombre,
            Prefijo = t.Prefijo, Orden = t.Orden
        }).ToList().AsReadOnly();
        return Result<IReadOnlyList<TipoComprobanteDto>>.Success(dtos);
    }

    public async Task<Result<IReadOnlyList<TipoCambioDto>>> GetTiposCambioAsync(CancellationToken ct)
    {
        var empresaId = _currentUser.EmpresaId;
        if (!empresaId.HasValue)
            return Result<IReadOnlyList<TipoCambioDto>>.Failure("No active company.");

        var items = await _tipoCambioRepo.FindAsync(t => t.EmpresaId == empresaId.Value && t.Activo, ct);
        var dtos = items.OrderByDescending(t => t.FechaVigencia).Select(t => new TipoCambioDto
        {
            TipoCambioId = t.TipoCambioId, Moneda = t.Moneda, Nombre = t.Nombre, Simbolo = t.Simbolo,
            TasaCompra = t.TasaCompra, TasaVenta = t.TasaVenta, FechaVigencia = t.FechaVigencia
        }).ToList().AsReadOnly();
        return Result<IReadOnlyList<TipoCambioDto>>.Success(dtos);
    }

    public async Task<Result<IReadOnlyList<TipoPagoDto>>> GetTiposPagoAsync(CancellationToken ct)
    {
        var empresaId = _currentUser.EmpresaId;
        if (!empresaId.HasValue)
            return Result<IReadOnlyList<TipoPagoDto>>.Failure("No active company.");

        var items = await _tipoPagoRepo.FindAsync(t => t.EmpresaId == empresaId.Value && t.Activo, ct);
        var dtos = items.OrderBy(t => t.Orden).Select(t => new TipoPagoDto
        {
            TipoPagoId = t.TipoPagoId, Codigo = t.Codigo, Nombre = t.Nombre,
            RequiereReferencia = t.RequiereReferencia, Orden = t.Orden
        }).ToList().AsReadOnly();
        return Result<IReadOnlyList<TipoPagoDto>>.Success(dtos);
    }

    public async Task<Result<int>> SeedCatalogosAsync(CancellationToken ct)
    {
        var empresaId = _currentUser.EmpresaId;
        if (!empresaId.HasValue)
            return Result<int>.Failure("No active company.");

        int count = 0;

        // Tipos de comprobante
        var tiposComp = await _tipoCompRepo.FindAsync(t => t.EmpresaId == empresaId.Value && t.Activo, ct);
        if (tiposComp.Count == 0)
        {
            var tipos = new[]
            {
                ("ING", "Comprobante de Ingreso", "CI", 1),
                ("EGR", "Comprobante de Egreso", "CE", 2),
                ("TRA", "Comprobante de Traspaso", "CT", 3),
            };
            foreach (var (cod, nom, pre, ord) in tipos)
            {
                await _tipoCompRepo.AddAsync(new TipoComprobante
                {
                    EmpresaId = empresaId.Value, Codigo = cod, Nombre = nom,
                    Prefijo = pre, Orden = ord, Activo = true
                }, ct);
                count++;
            }
        }

        // Tipos de cambio
        var tiposTc = await _tipoCambioRepo.FindAsync(t => t.EmpresaId == empresaId.Value && t.Activo, ct);
        if (tiposTc.Count == 0)
        {
            await _tipoCambioRepo.AddAsync(new TipoCambio
            {
                EmpresaId = empresaId.Value, Moneda = "USD", Nombre = "Dólar Americano",
                Simbolo = "$", TasaCompra = 6.96m, TasaVenta = 6.96m,
                FechaVigencia = DateTime.Today, Activo = true
            }, ct);
            await _tipoCambioRepo.AddAsync(new TipoCambio
            {
                EmpresaId = empresaId.Value, Moneda = "UFV", Nombre = "Unidad de Fomento a la Vivienda",
                Simbolo = "UFV", TasaCompra = 2.66m, TasaVenta = 2.66m,
                FechaVigencia = DateTime.Today, Activo = true
            }, ct);
            count += 2;
        }

        // Tipos de pago
        var tiposPago = await _tipoPagoRepo.FindAsync(t => t.EmpresaId == empresaId.Value && t.Activo, ct);
        if (tiposPago.Count == 0)
        {
            var pagos = new[]
            {
                ("S/D", "Sin Describir", false, 1),
                ("EFE", "Efectivo", false, 2),
                ("CHQ", "Cheque", true, 3),
                ("TRF", "Transferencia Bancaria", true, 4),
                ("QR", "Pago QR", true, 5),
                ("TJD", "Tarjeta de Débito", true, 6),
                ("TJC", "Tarjeta de Crédito", true, 7),
            };
            foreach (var (cod, nom, req, ord) in pagos)
            {
                await _tipoPagoRepo.AddAsync(new TipoPago
                {
                    EmpresaId = empresaId.Value, Codigo = cod, Nombre = nom,
                    RequiereReferencia = req, Orden = ord, Activo = true
                }, ct);
                count++;
            }
        }

        await _unitOfWork.SaveChangesAsync(ct);
        return Result<int>.Success(count);
    }

    // ??????????????????????????????????????????????????????????????????
    // HELPERS
    // ??????????????????????????????????????????????????????????????????

    private static string? ValidarLineas(List<CreateAsientoLineaDto> lineas)
    {
        if (lineas.Count < 2)
            return "Un comprobante requiere al menos 2 líneas.";
        foreach (var l in lineas)
        {
            if (l.Debe < 0 || l.Haber < 0)
                return "Los montos no pueden ser negativos.";
            if (l.Debe == 0 && l.Haber == 0)
                return "Cada línea debe tener un monto en Debe o en Haber.";
            if (l.Debe > 0 && l.Haber > 0)
                return "Una línea no puede tener monto en Debe y Haber simultáneamente.";
        }
        return null;
    }

    private async Task<string?> ValidarPeriodoAbiertoAsync(int empresaId, DateTime fecha, CancellationToken ct)
    {
        var periodos = await _periodoRepo.FindAsync(
            p => p.EmpresaId == empresaId && p.Anio == fecha.Year && p.Mes == fecha.Month && p.Activo, ct);
        var periodo = periodos.FirstOrDefault();
        if (periodo is null) return null;
        if (periodo.Estado == "Cerrado")
            return $"El per\u00edodo '{periodo.Nombre}' est\u00e1 cerrado (cerrado el {periodo.FechaCierre:dd/MM/yyyy} por {periodo.CerradoPorNombre}). Debe reabrirlo primero.";
        return null;
    }

    private async Task ValidarCierreContableAsync(int empresaId, int gestion, CancellationToken ct)
    {
        var cierreContableService = Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<ICierreContableService>(_serviceProvider);
        var existeCierre = await cierreContableService.ExisteCierreAsync(empresaId, gestion, ct);
        if (existeCierre)
        {
            throw new InvalidOperationException($"La gestión {gestion} ya cuenta con un cierre contable definitivo.");
        }
    }

    private async Task<string> GenerarNumeroComprobanteAsync(int empresaId, TipoComprobante tipo, int gestion, CancellationToken ct)
    {
        // Number per type + gestion: CI-001, CE-001, CT-001
        var existentes = await _asientoRepo.FindAsync(
            a => a.EmpresaId == empresaId && a.TipoComprobanteId == tipo.TipoComprobanteId
                && a.Gestion == gestion, ct);
        var siguiente = existentes.Count + 1;
        return $"{tipo.Prefijo}-{siguiente:D4}";
    }

    private async Task SaveLineasAsync(long asientoId, List<CreateAsientoLineaDto> lineas, CancellationToken ct)
    {
        int lineNum = 1;
        foreach (var l in lineas)
        {
            await _lineaRepo.AddAsync(new AsientoContableLinea
            {
                AsientoContableId = asientoId,
                NumeroLinea       = lineNum++,
                CuentaContableId  = l.CuentaContableId,
                Debe              = l.Debe,
                Haber             = l.Haber,
                Glosa             = l.Glosa,
                Referencia        = l.Referencia,
                CentroCostoId     = l.CentroCostoId,
                Activo            = true
            }, ct);
        }
    }

    private async Task<AsientoContableDto> BuildDto(AsientoContable asiento, CancellationToken ct)
    {
        var lineas = await _lineaRepo.FindAsync(l => l.AsientoContableId == asiento.AsientoContableId && l.Activo, ct);

        var cuentaIds = lineas.Select(l => l.CuentaContableId).Distinct().ToList();
        var cuentas = cuentaIds.Count > 0
            ? await _cuentaRepo.FindAsync(c => cuentaIds.Contains(c.CuentaContableId), ct)
            : new List<CuentaContable>();
        var cuentaMap = cuentas.ToDictionary(c => c.CuentaContableId);

        // Cargar centros de costo referenciados en las líneas
        var ccIds = lineas
            .Where(l => l.CentroCostoId.HasValue)
            .Select(l => l.CentroCostoId!.Value)
            .Distinct()
            .ToList();
        var centrosCosto = ccIds.Count > 0
            ? await _centroCostoRepo.FindAsync(c => ccIds.Contains(c.Id), ct)
            : new List<CentroCosto>();
        var ccMap = centrosCosto.ToDictionary(c => c.Id);

        TipoComprobante? tipoComp = asiento.TipoComprobanteId.HasValue
            ? await _tipoCompRepo.GetByIdAsync(asiento.TipoComprobanteId.Value, ct) : null;
        TipoCambio? tipoCambio = asiento.TipoCambioId.HasValue
            ? await _tipoCambioRepo.GetByIdAsync(asiento.TipoCambioId.Value, ct) : null;
        TipoPago? tipoPago = asiento.TipoPagoId.HasValue
            ? await _tipoPagoRepo.GetByIdAsync(asiento.TipoPagoId.Value, ct) : null;

        return new AsientoContableDto
        {
            AsientoContableId = asiento.AsientoContableId,
            EmpresaId = asiento.EmpresaId,
            TipoComprobanteId = asiento.TipoComprobanteId,
            TipoComprobanteCodigo = tipoComp?.Codigo ?? "—",
            TipoComprobanteNombre = tipoComp?.Nombre ?? "—",
            Numero = asiento.Numero,
            Fecha = asiento.Fecha,
            Gestion = asiento.Gestion,
            TipoRegistro = asiento.TipoRegistro,
            Estado = asiento.Estado,
            Concepto = asiento.Concepto,
            Glosa = asiento.Glosa,
            TipoCambioId = asiento.TipoCambioId,
            TipoCambioMoneda = tipoCambio?.Moneda,
            ValorTipoCambio = asiento.ValorTipoCambio,
            TipoPagoId = asiento.TipoPagoId,
            TipoPagoCodigo = tipoPago?.Codigo,
            TipoPagoNombre = tipoPago?.Nombre,
            NumeroDocumentoPago = asiento.NumeroDocumentoPago,
            RegistradoPorId = asiento.RegistradoPorId,
            RegistradoPorNombre = asiento.RegistradoPorNombre,
            OrigenTipo = asiento.OrigenTipo,
            OrigenId = asiento.OrigenId,
            OrigenReferencia = asiento.OrigenReferencia,
            TotalDebe = asiento.TotalDebe,
            TotalHaber = asiento.TotalHaber,
            Cuadrado = asiento.TotalDebe == asiento.TotalHaber,
            Lineas = lineas.OrderBy(l => l.NumeroLinea).Select(l =>
            {
                cuentaMap.TryGetValue(l.CuentaContableId, out var cuenta);
                ccMap.TryGetValue(l.CentroCostoId ?? 0, out var cc);
                return new AsientoContableLineaDto
                {
                    AsientoContableLineaId = l.AsientoContableLineaId,
                    NumeroLinea            = l.NumeroLinea,
                    CuentaContableId       = l.CuentaContableId,
                    CuentaCodigo           = cuenta?.Codigo ?? "–",
                    CuentaNombre           = cuenta?.Nombre ?? "–",
                    Debe                   = l.Debe,
                    Haber                  = l.Haber,
                    Glosa                  = l.Glosa,
                    Referencia             = l.Referencia,
                    CentroCostoId          = l.CentroCostoId,
                    CentroCostoCodigo      = cc?.Codigo,
                    CentroCostoNombre      = cc?.Nombre
                };
            }).ToList()
        };
    }
}
