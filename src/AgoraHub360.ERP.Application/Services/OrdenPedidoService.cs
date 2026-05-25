namespace AgoraHub360.ERP.Application.Services;

using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Domain.Entities.CMP;
using AgoraHub360.ERP.Domain.Entities.Core;
using AgoraHub360.ERP.Domain.Entities.MDM;
using AgoraHub360.ERP.Domain.Enums;
using AgoraHub360.ERP.Domain.Interfaces;
using AgoraHub360.ERP.Shared.DTOs.Compras;
using AgoraHub360.ERP.Shared.DTOs.Workflow;

public class OrdenPedidoService : IOrdenPedidoService
{
    private readonly IRepository<OrdenPedido> _opRepo;
    private readonly IRepository<OrdenPedidoLinea> _lineaRepo;
    private readonly IRepository<Empresa> _empresaRepo;
    private readonly IRepository<Almacen> _almacenRepo;
    private readonly IRepository<Sucursal> _sucursalRepo;
    private readonly IRepository<Cliente> _clienteRepo;
    private readonly IRepository<ClienteSucursal> _clienteSucursalRepo;
    private readonly IRepository<CompanyProduct> _cpRepo;
    private readonly IRepository<NumeracionDocumento> _numRepo;
    private readonly IRepository<Usuario> _usuarioRepo;
    private readonly IRepository<UsuarioEmpresa> _usuarioEmpresaRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IWorkflowService _workflow;

    public OrdenPedidoService(
        IRepository<OrdenPedido> opRepo,
        IRepository<OrdenPedidoLinea> lineaRepo,
        IRepository<Empresa> empresaRepo,
        IRepository<Almacen> almacenRepo,
        IRepository<Sucursal> sucursalRepo,
        IRepository<Cliente> clienteRepo,
        IRepository<ClienteSucursal> clienteSucursalRepo,
        IRepository<CompanyProduct> cpRepo,
        IRepository<NumeracionDocumento> numRepo,
        IRepository<Usuario> usuarioRepo,
        IRepository<UsuarioEmpresa> usuarioEmpresaRepo,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        IWorkflowService workflow)
    {
        _opRepo = opRepo;
        _lineaRepo = lineaRepo;
        _empresaRepo = empresaRepo;
        _almacenRepo = almacenRepo;
        _sucursalRepo = sucursalRepo;
        _clienteRepo = clienteRepo;
        _clienteSucursalRepo = clienteSucursalRepo;
        _cpRepo = cpRepo;
        _numRepo = numRepo;
        _usuarioRepo = usuarioRepo;
        _usuarioEmpresaRepo = usuarioEmpresaRepo;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _workflow = workflow;
    }

    public async Task<Result<IReadOnlyList<OrdenPedidoDto>>> GetAllAsync(
        string? estado,
        DateTime? fechaDesde,
        DateTime? fechaHasta,
        int? sucursalId,
        int? clienteId,
        int? clienteSucursalId,
        CancellationToken ct)
    {
        var empresaId = _currentUser.EmpresaId;
        if (!empresaId.HasValue)
            return Result<IReadOnlyList<OrdenPedidoDto>>.Failure("No active company.");

        var ops = await _opRepo.FindAsync(
            o => o.EmpresaId == empresaId.Value && o.Activo
                && (!fechaDesde.HasValue || o.FechaEmision >= fechaDesde.Value)
                && (!fechaHasta.HasValue || o.FechaEmision <= fechaHasta.Value.AddDays(1).AddSeconds(-1))
                && (!sucursalId.HasValue || o.SucursalId == sucursalId.Value)
                && (!clienteId.HasValue || o.ClienteId == clienteId.Value)
                && (!clienteSucursalId.HasValue || o.ClienteSucursalId == clienteSucursalId.Value),
            ct);

        if (!string.IsNullOrEmpty(estado) && Enum.TryParse<EstadoDocumento>(estado, true, out var estadoEnum))
            ops = ops.Where(o => o.Estado == estadoEnum).ToList();

        var almacenes = await _almacenRepo.FindAsync(a => a.EmpresaId == empresaId.Value, ct);
        var almMap = almacenes.ToDictionary(a => a.Id, a => a.Nombre);

        var empresa = await _empresaRepo.GetByIdAsync(empresaId.Value, ct);
        var empresaNombre = empresa?.Nombre ?? "—";

        var sucursalIds = ops.Where(o => o.SucursalId.HasValue).Select(o => o.SucursalId!.Value).Distinct().ToList();
        var sucursalMap = sucursalIds.Count > 0
            ? (await _sucursalRepo.FindAsync(s => sucursalIds.Contains(s.Id), ct)).ToDictionary(s => s.Id, s => s.Nombre)
            : new Dictionary<int, string>();

        var clienteIds = ops.Where(o => o.ClienteId.HasValue).Select(o => o.ClienteId!.Value).Distinct().ToList();
        var clienteMap = clienteIds.Count > 0
            ? (await _clienteRepo.FindAsync(c => clienteIds.Contains(c.Id), ct)).ToDictionary(c => c.Id)
            : new Dictionary<int, Cliente>();

        var clienteSucursalIds = ops.Where(o => o.ClienteSucursalId.HasValue).Select(o => o.ClienteSucursalId!.Value).Distinct().ToList();
        var clienteSucursalMap = clienteSucursalIds.Count > 0
            ? (await _clienteSucursalRepo.FindAsync(cs => clienteSucursalIds.Contains(cs.Id), ct)).ToDictionary(cs => cs.Id)
            : new Dictionary<int, ClienteSucursal>();

        var lineas = await _lineaRepo.FindAsync(l => ops.Select(o => o.OrdenPedidoId).Contains(l.OrdenPedidoId), ct);
        var cpIds = lineas.Select(l => l.CompanyProductId).Distinct().ToList();
        var cps = cpIds.Count > 0
            ? await _cpRepo.FindAsync(p => cpIds.Contains(p.CompanyProductId), ct)
            : new List<CompanyProduct>();
        var cpMap = cps.ToDictionary(p => p.CompanyProductId, p => p.Sku);

        var userMap = await GetUsuarioMapAsync(empresaId.Value, ct);

        var dtos = ops
            .OrderByDescending(o => o.FechaEmision)
            .ThenByDescending(o => o.OrdenPedidoId)
            .Select(o => MapToDto(o,
                lineas.Where(l => l.OrdenPedidoId == o.OrdenPedidoId).ToList(),
                almMap, cpMap, userMap, empresaNombre, sucursalMap, clienteMap, clienteSucursalMap))
            .ToList()
            .AsReadOnly();

        return Result<IReadOnlyList<OrdenPedidoDto>>.Success(dtos);
    }

    public async Task<Result<OrdenPedidoDto>> GetByIdAsync(long id, CancellationToken ct)
    {
        var empresaId = _currentUser.EmpresaId;
        if (!empresaId.HasValue)
            return Result<OrdenPedidoDto>.Failure("No active company.");

        var op = await _opRepo.GetByIdAsync(id, ct);
        if (op is null || op.EmpresaId != empresaId.Value || !op.Activo)
            return Result<OrdenPedidoDto>.Failure($"Purchase request {id} not found.");

        return Result<OrdenPedidoDto>.Success(await BuildFullDto(op, ct));
    }

    public async Task<Result<OrdenPedidoDto>> CreateAsync(CreateOrdenPedidoDto dto, CancellationToken ct)
    {
        var empresaId = _currentUser.EmpresaId;
        if (!empresaId.HasValue)
            return Result<OrdenPedidoDto>.Failure("No active company.");

        if (dto.Lineas is null || dto.Lineas.Count == 0)
            return Result<OrdenPedidoDto>.Failure("A purchase request must have at least one line.");

        // Resolver el SolicitanteId desde el usuario autenticado
        var solicitanteId = _currentUser.UserIdInt;
        if (!solicitanteId.HasValue)
            return Result<OrdenPedidoDto>.Failure("No se pudo identificar al usuario solicitante.");

        // Verificar que el solicitante pertenece a la empresa
        var asignacion = await _usuarioEmpresaRepo.FindAsync(
            ue => ue.UsuarioId == solicitanteId.Value && ue.EmpresaId == empresaId.Value, ct);
        if (!asignacion.Any())
            return Result<OrdenPedidoDto>.Failure("El usuario no está asignado a la empresa activa.");

        var almacen = await _almacenRepo.GetByIdAsync(dto.AlmacenDestinoId, ct);
        if (almacen is null || almacen.EmpresaId != empresaId.Value)
            return Result<OrdenPedidoDto>.Failure("Warehouse not found.");

        if (dto.SucursalId.HasValue)
        {
            var sucursal = await _sucursalRepo.GetByIdAsync(dto.SucursalId.Value, ct);
            if (sucursal is null || sucursal.EmpresaId != empresaId.Value || !sucursal.Activo)
                return Result<OrdenPedidoDto>.Failure("La sucursal de la empresa no es válida para la empresa activa.");

            if (!sucursal.PermiteVentas)
                return Result<OrdenPedidoDto>.Failure("La sucursal seleccionada no tiene capacidad comercial para registrar pedidos.");
        }

        int? clienteSucursalId = null;
        if (dto.ClienteId.HasValue)
        {
            var cliente = await _clienteRepo.GetByIdAsync(dto.ClienteId.Value, ct);
            if (cliente is null || cliente.EmpresaId != empresaId.Value || !cliente.Activo)
                return Result<OrdenPedidoDto>.Failure("El cliente no es válido para la empresa activa.");

            var clienteSucursalResult = await ResolveClienteSucursalIdAsync(
                empresaId.Value,
                dto.ClienteId.Value,
                dto.ClienteSucursalId,
                ct);

            if (!clienteSucursalResult.IsSuccess)
                return Result<OrdenPedidoDto>.Failure(clienteSucursalResult.Error!);

            clienteSucursalId = clienteSucursalResult.Value;
        }
        else if (dto.ClienteSucursalId.HasValue)
        {
            return Result<OrdenPedidoDto>.Failure("Para enviar ClienteSucursalId también debe enviar ClienteId.");
        }

        foreach (var lineaDto in dto.Lineas)
        {
            var cp = await _cpRepo.GetByIdAsync(lineaDto.CompanyProductId, ct);
            if (cp is null || cp.EmpresaId != empresaId.Value)
                return Result<OrdenPedidoDto>.Failure($"Product {lineaDto.CompanyProductId} not found.");
        }

        var numero = await GenerarNumeroAsync(empresaId.Value, ct);
        var urgencia = Enum.TryParse<NivelUrgencia>(dto.Urgencia, true, out var u) ? u : NivelUrgencia.Normal;

        var op = new OrdenPedido
        {
            EmpresaId = empresaId.Value,
            Numero = numero,
            FechaEmision = dto.FechaEmision,
            FechaRequerida = dto.FechaRequerida,
            SolicitanteId = solicitanteId.Value,
            CentroCosto = dto.CentroCosto,
            Urgencia = urgencia,
            AlmacenDestinoId = dto.AlmacenDestinoId,
            SucursalId = dto.SucursalId,
            ClienteId = dto.ClienteId,
            ClienteSucursalId = clienteSucursalId,
            Estado = EstadoDocumento.Borrador,
            Observaciones = dto.Observaciones,
            Activo = true
        };

        await _opRepo.AddAsync(op, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        int lineNum = 1;
        foreach (var lineaDto in dto.Lineas)
        {
            var linea = new OrdenPedidoLinea
            {
                OrdenPedidoId = op.OrdenPedidoId,
                NumeroLinea = lineNum++,
                CompanyProductId = lineaDto.CompanyProductId,
                Descripcion = lineaDto.Descripcion,
                UnidadMedida = lineaDto.UnidadMedida,
                CantidadSolicitada = lineaDto.CantidadSolicitada,
                Notas = lineaDto.Notas,
                Activo = true
            };
            await _lineaRepo.AddAsync(linea, ct);
        }

        await _unitOfWork.SaveChangesAsync(ct);

        // Generar tareas automáticas desde plantilla configurada para "OrdenPedido".
        // Si no existe plantilla el resultado es Failure pero no interrumpe la creación.
        await _workflow.GenerarHitosInicialesAsync(new GenerarHitosDto
        {
            EntityType = "OrdenPedido",
            EntityId   = (int)op.OrdenPedidoId,
            EmpresaId  = empresaId.Value,
            SubTipo    = null
        }, ct);

        return Result<OrdenPedidoDto>.Success(await BuildFullDto(op, ct));
    }

    public async Task<Result<OrdenPedidoDto>> UpdateAsync(long id, UpdateOrdenPedidoDto dto, CancellationToken ct)
    {
        var empresaId = _currentUser.EmpresaId;
        if (!empresaId.HasValue)
            return Result<OrdenPedidoDto>.Failure("No active company.");

        var op = await _opRepo.GetByIdAsync(id, ct);
        if (op is null || op.EmpresaId != empresaId.Value || !op.Activo)
            return Result<OrdenPedidoDto>.Failure("Purchase request not found.");
        if (op.Estado != EstadoDocumento.Borrador)
            return Result<OrdenPedidoDto>.Failure("Only draft purchase requests can be edited.");

        var almacen = await _almacenRepo.GetByIdAsync(dto.AlmacenDestinoId, ct);
        if (almacen is null || almacen.EmpresaId != empresaId.Value)
            return Result<OrdenPedidoDto>.Failure("Warehouse not found.");

        if (dto.SucursalId.HasValue)
        {
            var sucursal = await _sucursalRepo.GetByIdAsync(dto.SucursalId.Value, ct);
            if (sucursal is null || sucursal.EmpresaId != empresaId.Value || !sucursal.Activo)
                return Result<OrdenPedidoDto>.Failure("La sucursal de la empresa no es válida para la empresa activa.");

            if (!sucursal.PermiteVentas)
                return Result<OrdenPedidoDto>.Failure("La sucursal seleccionada no tiene capacidad comercial para registrar pedidos.");
        }

        int? clienteSucursalId = null;
        if (dto.ClienteId.HasValue)
        {
            var cliente = await _clienteRepo.GetByIdAsync(dto.ClienteId.Value, ct);
            if (cliente is null || cliente.EmpresaId != empresaId.Value || !cliente.Activo)
                return Result<OrdenPedidoDto>.Failure("El cliente no es válido para la empresa activa.");

            var clienteSucursalResult = await ResolveClienteSucursalIdAsync(
                empresaId.Value,
                dto.ClienteId.Value,
                dto.ClienteSucursalId,
                ct);

            if (!clienteSucursalResult.IsSuccess)
                return Result<OrdenPedidoDto>.Failure(clienteSucursalResult.Error!);

            clienteSucursalId = clienteSucursalResult.Value;
        }
        else if (dto.ClienteSucursalId.HasValue)
        {
            return Result<OrdenPedidoDto>.Failure("Para enviar ClienteSucursalId también debe enviar ClienteId.");
        }

        var urgencia = Enum.TryParse<NivelUrgencia>(dto.Urgencia, true, out var u) ? u : NivelUrgencia.Normal;

        op.FechaEmision = dto.FechaEmision;
        op.FechaRequerida = dto.FechaRequerida;
        // Solicitante no se modifica: queda el usuario que creó la OP
        op.CentroCosto = dto.CentroCosto;
        op.Urgencia = urgencia;
        op.AlmacenDestinoId = dto.AlmacenDestinoId;
        op.SucursalId = dto.SucursalId;
        op.ClienteId = dto.ClienteId;
        op.ClienteSucursalId = clienteSucursalId;
        op.Observaciones = dto.Observaciones;

        await _opRepo.UpdateAsync(op, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        return Result<OrdenPedidoDto>.Success(await BuildFullDto(op, ct));
    }

    public async Task<Result<OrdenPedidoDto>> EnviarARevisionAsync(long id, CancellationToken ct)
    {
        var (op, err) = await GetValidOp(id, ct);
        if (op is null) return Result<OrdenPedidoDto>.Failure(err!);

        if (op.Estado != EstadoDocumento.Borrador)
            return Result<OrdenPedidoDto>.Failure("Only draft purchase requests can be sent for review.");

        var lineas = await _lineaRepo.FindAsync(l => l.OrdenPedidoId == id, ct);
        if (lineas.Count == 0)
            return Result<OrdenPedidoDto>.Failure("Cannot send for review: no lines found.");

        op.Estado = EstadoDocumento.EnRevision;
        await _opRepo.UpdateAsync(op, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        return Result<OrdenPedidoDto>.Success(await BuildFullDto(op, ct));
    }

    public async Task<Result<OrdenPedidoDto>> RevisarStockAsync(long id, RevisarStockOrdenPedidoDto dto, CancellationToken ct)
    {
        var (op, err) = await GetValidOp(id, ct);
        if (op is null) return Result<OrdenPedidoDto>.Failure(err!);

        if (op.Estado != EstadoDocumento.EnRevision)
            return Result<OrdenPedidoDto>.Failure("Purchase request must be in EnRevision state for stock check.");

        // Update per-line stock data if provided
        if (dto.Lineas is { Count: > 0 })
        {
            var lineas = await _lineaRepo.FindAsync(l => l.OrdenPedidoId == id, ct);
            var lineaMap = lineas.ToDictionary(l => l.OrdenPedidoLineaId);

            foreach (var rev in dto.Lineas)
            {
                if (!lineaMap.TryGetValue(rev.OrdenPedidoLineaId, out var linea)) continue;
                linea.CantidadStockDisponible = rev.CantidadStockDisponible;
                linea.CantidadEnTransito = rev.CantidadEnTransito;
                linea.CalcularCantidadAComprar();
                await _lineaRepo.UpdateAsync(linea, ct);
            }
        }

        op.StockCubre = dto.StockCubre;
        op.ObservacionesRevisionStock = dto.ObservacionesRevisionStock;
        // Gate: stock covers demand ? close as AbastecidoConStock; else ? PendienteAprobacion
        op.Estado = dto.StockCubre ? EstadoDocumento.AbastecidoConStock : EstadoDocumento.PendienteAprobacion;

        await _opRepo.UpdateAsync(op, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        return Result<OrdenPedidoDto>.Success(await BuildFullDto(op, ct));
    }

    public async Task<Result<OrdenPedidoDto>> AprobarRechazarAsync(long id, AprobarRechazarOrdenPedidoDto dto, CancellationToken ct)
    {
        var (op, err) = await GetValidOp(id, ct);
        if (op is null) return Result<OrdenPedidoDto>.Failure(err!);

        if (op.Estado != EstadoDocumento.PendienteAprobacion)
            return Result<OrdenPedidoDto>.Failure("Only purchase requests in PendienteAprobacion state can be approved/rejected.");

        if (dto.Aprobado)
        {
            op.Estado = EstadoDocumento.Aprobado;
        }
        else
        {
            if (string.IsNullOrWhiteSpace(dto.MotivoRechazo))
                return Result<OrdenPedidoDto>.Failure("A rejection reason is required.");
            op.Estado = EstadoDocumento.Rechazado;
            op.MotivoRechazo = dto.MotivoRechazo;
        }

        await _opRepo.UpdateAsync(op, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        return Result<OrdenPedidoDto>.Success(await BuildFullDto(op, ct));
    }

    public async Task<Result<OrdenPedidoDto>> AnularAsync(long id, string? motivo, CancellationToken ct)
    {
        var (op, err) = await GetValidOp(id, ct);
        if (op is null) return Result<OrdenPedidoDto>.Failure(err!);

        if (op.Estado is EstadoDocumento.Anulado or EstadoDocumento.AbastecidoConStock or EstadoDocumento.Rechazado)
            return Result<OrdenPedidoDto>.Failure($"Cannot cancel a purchase request in state {op.Estado}.");

        op.Estado = EstadoDocumento.Anulado;
        op.MotivoRechazo = motivo;
        await _opRepo.UpdateAsync(op, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        return Result<OrdenPedidoDto>.Success(await BuildFullDto(op, ct));
    }

    public async Task<Result<bool>> DeleteAsync(long id, CancellationToken ct)
    {
        var empresaId = _currentUser.EmpresaId;
        if (!empresaId.HasValue)
            return Result<bool>.Failure("No active company.");

        var op = await _opRepo.GetByIdAsync(id, ct);
        if (op is null || op.EmpresaId != empresaId.Value)
            return Result<bool>.Failure("Purchase request not found.");
        if (op.Estado != EstadoDocumento.Borrador)
            return Result<bool>.Failure("Only draft purchase requests can be deleted.");

        op.Activo = false;
        await _opRepo.UpdateAsync(op, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        return Result<bool>.Success(true);
    }

    // ?? Private helpers ????????????????????????????????????????????????????????

    private async Task<(OrdenPedido? op, string? error)> GetValidOp(long id, CancellationToken ct)
    {
        var empresaId = _currentUser.EmpresaId;
        if (!empresaId.HasValue)
            return (null, "No active company.");

        var op = await _opRepo.GetByIdAsync(id, ct);
        if (op is null || op.EmpresaId != empresaId.Value || !op.Activo)
            return (null, $"Purchase request {id} not found.");

        return (op, null);
    }

    private async Task<Result<int?>> ResolveClienteSucursalIdAsync(
        int empresaId,
        int clienteId,
        int? clienteSucursalId,
        CancellationToken ct)
    {
        if (clienteSucursalId.HasValue)
        {
            var sucursalCliente = await _clienteSucursalRepo.GetByIdAsync(clienteSucursalId.Value, ct);
            if (sucursalCliente is null || !sucursalCliente.Activo)
                return Result<int?>.Failure("La sucursal del cliente no existe o está inactiva.");

            if (sucursalCliente.EmpresaId != empresaId)
                return Result<int?>.Failure("La sucursal del cliente no pertenece a la empresa activa.");

            if (sucursalCliente.ClienteId != clienteId)
                return Result<int?>.Failure("La sucursal del cliente no pertenece al cliente seleccionado.");

            return Result<int?>.Success(sucursalCliente.Id);
        }

        var sucursalesCliente = await _clienteSucursalRepo.FindAsync(
            cs => cs.EmpresaId == empresaId && cs.ClienteId == clienteId && cs.Activo,
            ct);

        var principal = sucursalesCliente.FirstOrDefault(cs => cs.EsPrincipal);
        if (principal is not null)
            return Result<int?>.Success(principal.Id);

        return Result<int?>.Failure("El cliente no tiene sucursal principal activa. Envíe ClienteSucursalId o configure una principal.");
    }

    private async Task<string> GenerarNumeroAsync(int empresaId, CancellationToken ct)
    {
        var numeraciones = await _numRepo.FindAsync(
            n => n.EmpresaId == empresaId && n.TipoDocumento == "OP", ct);
        var num = numeraciones.FirstOrDefault();
        if (num is not null)
        {
            var numero = num.GenerarSiguiente();
            await _numRepo.UpdateAsync(num, ct);
            return numero;
        }
        var existentes = await _opRepo.FindAsync(o => o.EmpresaId == empresaId, ct);
        return $"OP-{(existentes.Count + 1):D6}";
    }

    private async Task<OrdenPedidoDto> BuildFullDto(OrdenPedido op, CancellationToken ct)
    {
        var lineas = await _lineaRepo.FindAsync(l => l.OrdenPedidoId == op.OrdenPedidoId, ct);
        var almacenes = await _almacenRepo.FindAsync(a => a.Id == op.AlmacenDestinoId, ct);
        var cpIds = lineas.Select(l => l.CompanyProductId).Distinct().ToList();
        var cps = cpIds.Count > 0
            ? await _cpRepo.FindAsync(p => cpIds.Contains(p.CompanyProductId), ct)
            : new List<CompanyProduct>();

        var almMap = almacenes.ToDictionary(a => a.Id, a => a.Nombre);
        var cpMap = cps.ToDictionary(p => p.CompanyProductId, p => p.Sku);
        var userMap = await GetUsuarioMapAsync(op.EmpresaId, ct);

        var empresa = await _empresaRepo.GetByIdAsync(op.EmpresaId, ct);
        var empresaNombre = empresa?.Nombre ?? "—";

        var sucursalMap = new Dictionary<int, string>();
        if (op.SucursalId.HasValue)
        {
            var sucursales = await _sucursalRepo.FindAsync(s => s.Id == op.SucursalId.Value, ct);
            sucursalMap = sucursales.ToDictionary(s => s.Id, s => s.Nombre);
        }

        var clienteMap = new Dictionary<int, Cliente>();
        if (op.ClienteId.HasValue)
        {
            var clientes = await _clienteRepo.FindAsync(c => c.Id == op.ClienteId.Value, ct);
            clienteMap = clientes.ToDictionary(c => c.Id);
        }

        var clienteSucursalMap = new Dictionary<int, ClienteSucursal>();
        if (op.ClienteSucursalId.HasValue)
        {
            var clienteSucursales = await _clienteSucursalRepo.FindAsync(cs => cs.Id == op.ClienteSucursalId.Value, ct);
            clienteSucursalMap = clienteSucursales.ToDictionary(cs => cs.Id);
        }

        return MapToDto(
            op,
            lineas.OrderBy(l => l.NumeroLinea).ToList(),
            almMap,
            cpMap,
            userMap,
            empresaNombre,
            sucursalMap,
            clienteMap,
            clienteSucursalMap);
    }

    private async Task<Dictionary<int, string>> GetUsuarioMapAsync(int empresaId, CancellationToken ct)
    {
        var asignaciones = await _usuarioEmpresaRepo.FindAsync(
            ue => ue.EmpresaId == empresaId, ct);
        var userIds = asignaciones.Select(ue => ue.UsuarioId).Distinct().ToList();
        if (userIds.Count == 0) return new Dictionary<int, string>();

        var usuarios = await _usuarioRepo.FindAsync(u => userIds.Contains(u.Id), ct);
        return usuarios.ToDictionary(u => u.Id, u => u.NombreCompleto);
    }

    private static OrdenPedidoDto MapToDto(
        OrdenPedido op,
        IList<OrdenPedidoLinea> lineas,
        Dictionary<int, string> almMap,
        Dictionary<long, string> cpMap,
        Dictionary<int, string> userMap,
        string empresaNombre,
        Dictionary<int, string> sucursalMap,
        Dictionary<int, Cliente> clienteMap,
        Dictionary<int, ClienteSucursal> clienteSucursalMap)
    {
        var cliente = op.ClienteId.HasValue
            ? clienteMap.GetValueOrDefault(op.ClienteId.Value)
            : null;

        var clienteSucursal = op.ClienteSucursalId.HasValue
            ? clienteSucursalMap.GetValueOrDefault(op.ClienteSucursalId.Value)
            : null;

        return new OrdenPedidoDto(
            op.OrdenPedidoId,
            op.EmpresaId,
            empresaNombre,
            op.SucursalId,
            op.SucursalId.HasValue ? sucursalMap.GetValueOrDefault(op.SucursalId.Value, "—") : null,
            op.ClienteId,
            cliente?.RazonSocial,
            op.ClienteSucursalId,
            clienteSucursal?.Nombre,
            op.Numero,
            op.FechaEmision,
            op.FechaRequerida,
            op.SolicitanteId,
            userMap.GetValueOrDefault(op.SolicitanteId, "—"),
            op.CentroCosto,
            op.Urgencia.ToString(),
            op.AlmacenDestinoId,
            almMap.GetValueOrDefault(op.AlmacenDestinoId, "—"),
            op.Estado.ToString(),
            clienteSucursal?.Direccion,
            clienteSucursal?.Referencia,
            cliente?.NombreContacto,
            op.Observaciones,
            op.MotivoRechazo,
            op.StockCubre,
            op.ObservacionesRevisionStock,
            op.Activo,
            lineas.Select(l => new OrdenPedidoLineaDto(
                l.OrdenPedidoLineaId,
                l.NumeroLinea,
                l.CompanyProductId,
                cpMap.GetValueOrDefault(l.CompanyProductId, "—"),
                l.Descripcion,
                l.UnidadMedida,
                l.CantidadSolicitada,
                l.CantidadStockDisponible,
                l.CantidadEnTransito,
                l.CantidadAComprar,
                l.Notas
            )).ToList());
    }
}
