namespace AgoraHub360.ERP.Application.Services;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Domain.Common;
using AgoraHub360.ERP.Domain.Entities.ACC;
using AgoraHub360.ERP.Domain.Entities.CST;
using AgoraHub360.ERP.Domain.Exceptions;
using AgoraHub360.ERP.Domain.Interfaces;
using AgoraHub360.ERP.Shared.DTOs.Presupuestos;

public class PresupuestoService : IPresupuestoService
{
    private readonly IRepository<PresupuestoContable> _presupuestoRepo;
    private readonly IRepository<CuentaContable> _cuentaRepo;
    private readonly IRepository<CentroCosto> _centroCostoRepo;
    private readonly IRepository<AsientoContable> _asientoRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    // Opcional: private readonly IPresupuestoImportService _importService;

    public PresupuestoService(
        IRepository<PresupuestoContable> presupuestoRepo,
        IRepository<CuentaContable> cuentaRepo,
        IRepository<CentroCosto> centroCostoRepo,
        IRepository<AsientoContable> asientoRepo,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _presupuestoRepo = presupuestoRepo;
        _cuentaRepo = cuentaRepo;
        _centroCostoRepo = centroCostoRepo;
        _asientoRepo = asientoRepo;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    private int ObtenerEmpresaId()
    {
        return _currentUserService.EmpresaId ?? throw new DomainException("No se pudo obtener la empresa actual.");
    }

    public async Task<Result<PresupuestoContableDto>> CreateAsync(CreatePresupuestoDto dto, CancellationToken ct = default)
    {
        var empresaId = ObtenerEmpresaId();

        // Validar lógica de negocio, no otro presupuesto Aprobado en la misma gestión
        // (Aunque si se crea en Borrador, podría permitirse)
        var existenteAprobado = (await _presupuestoRepo.FindAsync(p => p.EmpresaId == empresaId && p.Gestion == dto.Gestion && p.Estado == "Aprobado", ct)).FirstOrDefault();
        // Si queremos permitir crear en Borrador pero no aprobarlo después si ya hay uno, está bien. 
        // Lo dejamos para el Aprobar() esa validación fuerte, pero aquí también podemos advertir.

        var presupuesto = new PresupuestoContable(empresaId, dto.Gestion, dto.Nombre, dto.Observaciones);

        foreach (var lineaDto in dto.Lineas)
        {
            if (lineaDto.Mes < 1 || lineaDto.Mes > 12)
                return Result<PresupuestoContableDto>.Failure($"Mes inválido {lineaDto.Mes}. Debe ser entre 1 y 12.");
            if (lineaDto.MontoPresupuestado < 0)
                return Result<PresupuestoContableDto>.Failure("El monto presupuestado no puede ser negativo.");

            var linea = new PresupuestoContableLinea(lineaDto.CuentaContableId, lineaDto.CentroCostoId, lineaDto.Mes, lineaDto.MontoPresupuestado);
            presupuesto.AgregarLinea(linea);
        }

        await _presupuestoRepo.AddAsync(presupuesto, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return await GetByIdAsync(presupuesto.PresupuestoContableId, ct);
    }

    public async Task<Result<PresupuestoContableDto>> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var empresaId = ObtenerEmpresaId();
        
        // Puesto que IRepository.GetByIdAsync(...) por defecto no hace includes,
        // haríamos una búsqueda simple en el repo que permita includes, o consultamos por id con FindAsync
        var resultados = await _presupuestoRepo.FindAsync(p => p.PresupuestoContableId == id && p.EmpresaId == empresaId, ct);
        var presupuesto = resultados.FirstOrDefault();

        if (presupuesto == null)
            return Result<PresupuestoContableDto>.Failure("Presupuesto no encontrado.");

        // TODO: En un escenario real, _presupuestoRepo debería cargar la colección de Líneas e include(Cuenta, CentroCosto)
        
        var dto = new PresupuestoContableDto
        {
            PresupuestoContableId = presupuesto.PresupuestoContableId,
            Gestion = presupuesto.Gestion,
            Nombre = presupuesto.Nombre,
            Estado = presupuesto.Estado,
            Observaciones = presupuesto.Observaciones,
            Lineas = presupuesto.Lineas.Select(l => new PresupuestoContableLineaDto
            {
                PresupuestoContableLineaId = l.PresupuestoContableLineaId,
                CuentaContableId = l.CuentaContableId,
                CuentaCodigo = l.CuentaContable?.Codigo,
                CuentaNombre = l.CuentaContable?.Nombre,
                CentroCostoId = l.CentroCostoId,
                CentroCostoNombre = l.CentroCosto?.Nombre,
                Mes = l.Mes,
                MontoPresupuestado = l.MontoPresupuestado
            }).ToList()
        };

        return Result<PresupuestoContableDto>.Success(dto);
    }

    public async Task<Result<bool>> AprobarAsync(int id, CancellationToken ct = default)
    {
        var empresaId = ObtenerEmpresaId();
        var presupuesto = (await _presupuestoRepo.FindAsync(p => p.PresupuestoContableId == id && p.EmpresaId == empresaId, ct)).FirstOrDefault();

        if (presupuesto == null)
            return Result<bool>.Failure("Presupuesto no encontrado.");

        var existenteAprobado = (await _presupuestoRepo.FindAsync(p => p.EmpresaId == empresaId && p.Gestion == presupuesto.Gestion && p.Estado == "Aprobado" && p.PresupuestoContableId != id, ct)).Any();

        if (existenteAprobado)
            return Result<bool>.Failure("Ya existe un presupuesto aprobado para esta gestión.");

        var resAprobar = presupuesto.Aprobar();
        if (!resAprobar.IsSuccess)
            return Result<bool>.Failure(resAprobar.Error!);

        await _presupuestoRepo.UpdateAsync(presupuesto, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<bool>.Success(true);
    }

    public async Task<Result<IEnumerable<PresupuestoVsRealDto>>> GetComparativoAsync(int gestion, int? mes = null, int? cuentaId = null, int? centroCostoId = null, CancellationToken ct = default)
    {
        var empresaId = ObtenerEmpresaId();

        var presupuesto = (await _presupuestoRepo.FindAsync(p => p.EmpresaId == empresaId && p.Gestion == gestion && p.Estado != "Borrador", ct)).FirstOrDefault();
        
        if (presupuesto == null)
            return Result<IEnumerable<PresupuestoVsRealDto>>.Failure("No se encontró un presupuesto aprobado/cerrado para la gestión seleccionada.");

        // TODO: Traer datos reales contables de un servicio financiero consolidado
        // Temporalmente asumiremos montos en 0 para evitar queries complejas 
        
        var lineas = presupuesto.Lineas.AsQueryable();

        if (mes.HasValue) lineas = lineas.Where(l => l.Mes == mes.Value);
        if (cuentaId.HasValue) lineas = lineas.Where(l => l.CuentaContableId == cuentaId.Value);
        if (centroCostoId.HasValue) lineas = lineas.Where(l => l.CentroCostoId == centroCostoId.Value);

        var result = new List<PresupuestoVsRealDto>();

        foreach (var l in lineas)
        {
            decimal montoReal = 0m; // TODO: Integrar con saldos
            decimal variacion = montoReal - l.MontoPresupuestado;
            decimal pct = l.MontoPresupuestado == 0 ? 0 : variacion / l.MontoPresupuestado;

            // TODO: Determinar si es cuenta de ingreso o de gasto para la variación favorable
            bool esFavorable = variacion <= 0;

            result.Add(new PresupuestoVsRealDto
            {
                CuentaCodigo = l.CuentaContable?.Codigo ?? string.Empty,
                CuentaNombre = l.CuentaContable?.Nombre ?? string.Empty,
                Mes = l.Mes,
                MontoPresupuestado = l.MontoPresupuestado,
                MontoReal = montoReal,
                Variacion = variacion,
                VariacionPct = pct,
                EsFavorable = esFavorable,
                CentroCostoId = l.CentroCostoId,
                CentroCostoNombre = l.CentroCosto?.Nombre
            });
        }

        return Result<IEnumerable<PresupuestoVsRealDto>>.Success(result.OrderBy(r => r.CuentaCodigo).ThenBy(r => r.Mes));
    }

    public Task<Result<CargaMasivaResultDto>> ImportarDesdeExcelAsync(int gestion, Stream excelStream, CancellationToken ct = default)
    {
        // TODO: Implementar en siguiente paso de Infrastructure inyectando IPresupuestoImportService
        throw new NotImplementedException("Importación Excel pendiente en Infrastructure.");
    }
}
