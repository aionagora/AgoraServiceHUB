namespace AgoraHub360.ERP.Application.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Domain.Common;
using AgoraHub360.ERP.Domain.Entities.ACC;
using AgoraHub360.ERP.Domain.Entities.ACT;
using AgoraHub360.ERP.Domain.Exceptions;
using AgoraHub360.ERP.Domain.Interfaces;
using AgoraHub360.ERP.Shared.DTOs.ActivosFijos;
using AgoraHub360.ERP.Shared.DTOs.Contabilidad;

public class ActivoFijoService : IActivoFijoService
{
    private readonly IRepository<ActivoFijo> _activoRepo;
    private readonly IRepository<DepreciacionMensual> _depreciacionRepo;
    private readonly IRepository<PeriodoContable> _periodoRepo;
    private readonly IRepository<TipoComprobante> _tipoComprobanteRepo; // Si es necesario para generar asientos
    private readonly IAsientoContableService _asientoService; // Como equivalente de servicio contable
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    public ActivoFijoService(
        IRepository<ActivoFijo> activoRepo,
        IRepository<DepreciacionMensual> depreciacionRepo,
        IRepository<PeriodoContable> periodoRepo,
        IRepository<TipoComprobante> tipoComprobanteRepo,
        IAsientoContableService asientoService,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _activoRepo = activoRepo;
        _depreciacionRepo = depreciacionRepo;
        _periodoRepo = periodoRepo;
        _tipoComprobanteRepo = tipoComprobanteRepo;
        _asientoService = asientoService;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    private int ObtenerEmpresaId()
    {
        return _currentUserService.EmpresaId ?? throw new DomainException("No se pudo obtener la empresa actual.");
    }

    public async Task<Result<ActivoFijoDto>> RegistrarActivoAsync(CreateActivoFijoDto dto, CancellationToken ct = default)
    {
        var empresaId = ObtenerEmpresaId();

        // 1. Validar unicidad del código
        var existentes = await _activoRepo.FindAsync(a => a.EmpresaId == empresaId && a.Codigo == dto.Codigo, ct);
        if (existentes.Any())
            return Result<ActivoFijoDto>.Failure("Ya existe un activo fijo con el código proporcionado en la empresa actual.");

        // 2. Instanciar (Usamos reflection o property inject si el CTOR de ActivoFijo está protegido como en la Tarea 1,
        //   Aunque, a menudo se usa FormatterServices o se adapta la entidad).
        //  Dado que propuso persistir y DTO
        var activo = (ActivoFijo)System.Runtime.Serialization.FormatterServices.GetUninitializedObject(typeof(ActivoFijo));
        
        typeof(TenantEntity).GetProperty("EmpresaId")?.SetValue(activo, empresaId);
        typeof(ActivoFijo).GetProperty("Codigo")?.SetValue(activo, dto.Codigo);
        typeof(ActivoFijo).GetProperty("Descripcion")?.SetValue(activo, dto.Descripcion);
        typeof(ActivoFijo).GetProperty("CategoriaActivo")?.SetValue(activo, dto.CategoriaActivo);
        typeof(ActivoFijo).GetProperty("CuentaContableId")?.SetValue(activo, dto.CuentaContableId);
        typeof(ActivoFijo).GetProperty("CuentaDepreciacionId")?.SetValue(activo, dto.CuentaDepreciacionId);
        typeof(ActivoFijo).GetProperty("CuentaGastoDepreciacionId")?.SetValue(activo, dto.CuentaGastoDepreciacionId);
        typeof(ActivoFijo).GetProperty("FechaAdquisicion")?.SetValue(activo, dto.FechaAdquisicion);
        typeof(ActivoFijo).GetProperty("CostoAdquisicion")?.SetValue(activo, dto.CostoAdquisicion);
        typeof(ActivoFijo).GetProperty("ValorResidual")?.SetValue(activo, dto.ValorResidual);
        typeof(ActivoFijo).GetProperty("TasaAnualDS24051")?.SetValue(activo, dto.TasaAnualDS24051);
        typeof(ActivoFijo).GetProperty("VidaUtilAnios")?.SetValue(activo, dto.VidaUtilAnios);
        typeof(ActivoFijo).GetProperty("Estado")?.SetValue(activo, "Activo");
        typeof(ActivoFijo).GetProperty("Depreciaciones")?.SetValue(activo, new List<DepreciacionMensual>());

        await _activoRepo.AddAsync(activo, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<ActivoFijoDto>.Success(MapToDto(activo));
    }

    public async Task<Result<DepreciacionMensualResultDto>> EjecutarDepreciacionMensualAsync(int periodoId, CancellationToken ct = default)
    {
        var empresaId = ObtenerEmpresaId();
        var periodo = await _periodoRepo.GetByIdAsync(periodoId, ct);

        if (periodo == null || periodo.EmpresaId != empresaId)
            return Result<DepreciacionMensualResultDto>.Failure("Período contable no válido o no pertenece a la empresa.");

        var activos = await _activoRepo.FindAsync(a => a.EmpresaId == empresaId && a.Estado == "Activo" && !a.DepreciacionCompleta, ct);
        
        var resultDto = new DepreciacionMensualResultDto();
        var tipoTraspaso = (await _tipoComprobanteRepo.FindAsync(t => t.Nombre.Contains("Traspaso") || t.Codigo == "TR", ct)).FirstOrDefault();
        int tipoCmprobanteId = tipoTraspaso?.TipoComprobanteId ?? 1; // Fallback

        var fechaAsiento = new DateTime(periodo.Anio, periodo.Mes, DateTime.DaysInMonth(periodo.Anio, periodo.Mes));

        foreach (var activo in activos)
        {
            try
            {
                var deprResult = activo.AplicarDepreciacionMensual(periodoId, fechaAsiento);
                if (!deprResult.IsSuccess)
                {
                    resultDto.Errores.Add($"Activo {activo.Codigo}: {deprResult.Error}");
                    resultDto.TotalErrores++;
                    continue;
                }

                var depreciacion = activo.Depreciaciones.Last();
                if (depreciacion.Monto > 0)
                {
                    // Crear asiento usando IAsientoContableService
                    var createAsientoDto = new CreateAsientoContableDto
                    {
                        TipoComprobanteId = tipoCmprobanteId,
                        Fecha = fechaAsiento,
                        Glosa = $"Depreciación {activo.CategoriaActivo} - {activo.Descripcion} - Período {periodo.Mes}/{periodo.Anio}",
                        OrigenTipo = "Depreciacion",
                        OrigenId = activo.ActivoFijoId,
                        Lineas = new List<CreateAsientoLineaDto>
                        {
                            new CreateAsientoLineaDto
                            {
                                CuentaContableId = activo.CuentaGastoDepreciacionId,
                                Debe = depreciacion.Monto,
                                Haber = 0,
                                Glosa = $"Gasto dep. {activo.Codigo}"
                            },
                            new CreateAsientoLineaDto
                            {
                                CuentaContableId = activo.CuentaDepreciacionId,
                                Debe = 0,
                                Haber = depreciacion.Monto,
                                Glosa = $"Dep. Acumulada {activo.Codigo}"
                            }
                        }
                    };

                    var asientoResult = await _asientoService.CreateAsync(createAsientoDto, ct);
                    if (asientoResult.IsSuccess && asientoResult.Value != null)
                    {
                        depreciacion.VincularAsiento(asientoResult.Value.AsientoContableId);
                        
                        // Contabilizamos el asiento si lo requiere
                        await _asientoService.ContabilizarAsync(asientoResult.Value.AsientoContableId, ct);
                    }
                    else
                    {
                        resultDto.Errores.Add($"Activo {activo.Codigo} - Error creando asiento: {asientoResult.Error}");
                        resultDto.TotalErrores++;
                    }
                }

                resultDto.TotalActivosProcesados++;
                resultDto.TotalMontoDepreciado += depreciacion.Monto;

                await _activoRepo.UpdateAsync(activo, ct);
            }
            catch (Exception ex)
            {
                resultDto.Errores.Add($"Activo {activo.Codigo}: Error inesperado - {ex.Message}");
                resultDto.TotalErrores++;
            }
        }

        // Persistimos todo el unit of work de las depreciaciones
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<DepreciacionMensualResultDto>.Success(resultDto);
    }

    public async Task<Result<IEnumerable<DepreciacionReporteDto>>> GetReporteDepreciacionAsync(int anio, CancellationToken ct = default)
    {
        var empresaId = ObtenerEmpresaId();
        var reportes = new List<DepreciacionReporteDto>();

        var periodosAnio = await _periodoRepo.FindAsync(p => p.EmpresaId == empresaId && p.Anio == anio, ct);
        var periodosIds = periodosAnio.Select(p => p.PeriodoContableId).ToList();

        var activos = await _activoRepo.FindAsync(a => a.EmpresaId == empresaId, ct); // TODO: Includ depreciaciones
        // Ya que usamos repos genéricos que quizá no hagan eager loading, cargamos depreciaciones separadas.
        
        var depreciaciones = await _depreciacionRepo.FindAsync(d => periodosIds.Contains(d.PeriodoContableId), ct);

        foreach (var activo in activos)
        {
            var deps = depreciaciones.Where(d => d.ActivoFijoId == activo.ActivoFijoId).OrderBy(d => d.Fecha);
            foreach(var dep in deps)
            {
                reportes.Add(new DepreciacionReporteDto
                {
                    CodigoActivo = activo.Codigo,
                    Descripcion = activo.Descripcion,
                    CategoriaActivo = activo.CategoriaActivo,
                    PeriodoContableId = dep.PeriodoContableId,
                    Fecha = dep.Fecha,
                    MontoDepreciacion = dep.Monto,
                    DepreciacionAcumulada = activo.DepreciacionAcumulada, // TODO: Valor exacto a esa fecha
                    ValorEnLibros = activo.ValorEnLibros,
                    Estado = activo.Estado
                });
            }
        }

        return Result<IEnumerable<DepreciacionReporteDto>>.Success(reportes.OrderBy(r => r.Fecha).ThenBy(r => r.CodigoActivo));
    }

    public async Task<Result<ActivoFijoDto>> DarDeBajaAsync(long activoId, string motivo, CancellationToken ct = default)
    {
        var empresaId = ObtenerEmpresaId();
        var activo = await _activoRepo.GetByIdAsync(activoId, ct);

        if (activo == null || activo.EmpresaId != empresaId)
            return Result<ActivoFijoDto>.Failure("Activo fijo no encontrado.");

        var resBaja = activo.DarDeBaja(motivo);
        if (!resBaja.IsSuccess)
            return Result<ActivoFijoDto>.Failure(resBaja.Error!);

        // TODO: Generar asiento contable de baja de activo fijo
        
        await _activoRepo.UpdateAsync(activo, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<ActivoFijoDto>.Success(MapToDto(activo));
    }

    private ActivoFijoDto MapToDto(ActivoFijo activo)
    {
        return new ActivoFijoDto
        {
            ActivoFijoId = activo.ActivoFijoId,
            Codigo = activo.Codigo,
            Descripcion = activo.Descripcion,
            CategoriaActivo = activo.CategoriaActivo,
            CuentaContableId = activo.CuentaContableId,
            CuentaDepreciacionId = activo.CuentaDepreciacionId,
            CuentaGastoDepreciacionId = activo.CuentaGastoDepreciacionId,
            FechaAdquisicion = activo.FechaAdquisicion,
            CostoAdquisicion = activo.CostoAdquisicion,
            ValorResidual = activo.ValorResidual,
            TasaAnualDS24051 = activo.TasaAnualDS24051,
            VidaUtilAnios = activo.VidaUtilAnios,
            DepreciacionAcumulada = activo.DepreciacionAcumulada,
            ValorEnLibros = activo.ValorEnLibros,
            DepreciacionCompleta = activo.DepreciacionCompleta,
            Estado = activo.Estado
        };
    }
}
