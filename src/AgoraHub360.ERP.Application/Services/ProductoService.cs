namespace AgoraHub360.ERP.Application.Services;

using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Domain.Entities.MDM;
using AgoraHub360.ERP.Domain.Interfaces;
using AgoraHub360.ERP.Shared.DTOs.Producto;

public class ProductoService : IProductoService
{
    private readonly IRepository<Producto> _repository;
    private readonly IRepository<CategoriaProducto> _categoriaRepo;
    private readonly IRepository<UnidadMedida> _unidadRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public ProductoService(
        IRepository<Producto> repository,
        IRepository<CategoriaProducto> categoriaRepo,
        IRepository<UnidadMedida> unidadRepo,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser)
    {
        _repository = repository;
        _categoriaRepo = categoriaRepo;
        _unidadRepo = unidadRepo;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<Result<IReadOnlyList<ProductoDto>>> GetAllAsync(CancellationToken ct = default)
    {
        var empresaId = _currentUser.EmpresaId;
        if (!empresaId.HasValue)
            return Result<IReadOnlyList<ProductoDto>>.Failure("No se pudo determinar la empresa activa.");

        var items = await _repository.FindAsync(p => p.EmpresaId == empresaId.Value, ct);
        var categorias = await _categoriaRepo.FindAsync(c => c.EmpresaId == empresaId.Value, ct);
        var unidades = await _unidadRepo.FindAsync(u => u.EmpresaId == empresaId.Value, ct);
        
        var catMap = categorias.ToDictionary(c => c.Id, c => c.Nombre);
        var uniMap = unidades.ToDictionary(u => u.Id, u => u.Nombre);

        return Result<IReadOnlyList<ProductoDto>>.Success(
            items.Select(p => MapToDto(p, catMap, uniMap)).ToList().AsReadOnly());
    }

    public async Task<Result<ProductoDto>> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var empresaId = _currentUser.EmpresaId;
        if (!empresaId.HasValue)
            return Result<ProductoDto>.Failure("No se pudo determinar la empresa activa.");

        var entity = await _repository.GetByIdAsync(id, ct);
        if (entity is null)
            return Result<ProductoDto>.Failure($"Producto con Id {id} no encontrado.");

        if (entity.EmpresaId != empresaId.Value)
            return Result<ProductoDto>.Failure("No tiene permisos para acceder a este producto.");

        var categorias = await _categoriaRepo.FindAsync(c => c.EmpresaId == empresaId.Value, ct);
        var unidades = await _unidadRepo.FindAsync(u => u.EmpresaId == empresaId.Value, ct);
        
        var catMap = categorias.ToDictionary(c => c.Id, c => c.Nombre);
        var uniMap = unidades.ToDictionary(u => u.Id, u => u.Nombre);

        return Result<ProductoDto>.Success(MapToDto(entity, catMap, uniMap));
    }

    public async Task<Result<ProductoDto>> CreateAsync(CreateProductoDto dto, CancellationToken ct = default)
    {
        var empresaId = _currentUser.EmpresaId;
        if (!empresaId.HasValue)
            return Result<ProductoDto>.Failure("No se pudo determinar la empresa activa.");

        // Validar código único
        var byCodigo = await _repository.FindAsync(
            p => p.EmpresaId == empresaId.Value && p.Codigo == dto.Codigo, ct);
        if (byCodigo.Any())
            return Result<ProductoDto>.Failure($"Ya existe un producto con código '{dto.Codigo}'.");

        // Validar que la categoría existe y pertenece a la empresa
        var categoria = await _categoriaRepo.GetByIdAsync(dto.CategoriaProductoId, ct);
        if (categoria is null || categoria.EmpresaId != empresaId.Value)
            return Result<ProductoDto>.Failure("Categoría no encontrada o no pertenece a su empresa.");

        // Validar que la unidad de medida existe y pertenece a la empresa
        var unidad = await _unidadRepo.GetByIdAsync(dto.UnidadMedidaId, ct);
        if (unidad is null || unidad.EmpresaId != empresaId.Value)
            return Result<ProductoDto>.Failure("Unidad de medida no encontrada o no pertenece a su empresa.");

        var entity = new Producto
        {
            Codigo = dto.Codigo,
            Nombre = dto.Nombre,
            Descripcion = dto.Descripcion,
            CategoriaProductoId = dto.CategoriaProductoId,
            UnidadMedidaId = dto.UnidadMedidaId,
            PrecioCompra = dto.PrecioCompra,
            PrecioVenta = dto.PrecioVenta,
            StockMinimo = dto.StockMinimo,
            Sku = dto.Sku,
            TipoProducto = dto.TipoProducto,
            ControlStock = dto.ControlStock,
            CostoBase = dto.CostoBase,
            EmpresaId = empresaId.Value,
            Activo = true
        };

        await _repository.AddAsync(entity, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        var catMap = new Dictionary<int, string> { { categoria.Id, categoria.Nombre } };
        var uniMap = new Dictionary<int, string> { { unidad.Id, unidad.Nombre } };

        return Result<ProductoDto>.Success(MapToDto(entity, catMap, uniMap));
    }

    public async Task<Result<ProductoDto>> UpdateAsync(int id, UpdateProductoDto dto, CancellationToken ct = default)
    {
        var empresaId = _currentUser.EmpresaId;
        if (!empresaId.HasValue)
            return Result<ProductoDto>.Failure("No se pudo determinar la empresa activa.");

        var entity = await _repository.GetByIdAsync(id, ct);
        if (entity is null)
            return Result<ProductoDto>.Failure($"Producto con Id {id} no encontrado.");

        if (entity.EmpresaId != empresaId.Value)
            return Result<ProductoDto>.Failure("No tiene permisos para modificar este producto.");

        // Validar código único (excluyendo el actual)
        var byCodigo = await _repository.FindAsync(
            p => p.EmpresaId == empresaId.Value && p.Codigo == dto.Codigo && p.Id != id, ct);
        if (byCodigo.Any())
            return Result<ProductoDto>.Failure($"Ya existe otro producto con código '{dto.Codigo}'.");

        // Validar categoría
        var categoria = await _categoriaRepo.GetByIdAsync(dto.CategoriaProductoId, ct);
        if (categoria is null || categoria.EmpresaId != empresaId.Value)
            return Result<ProductoDto>.Failure("Categoría no encontrada o no pertenece a su empresa.");

        // Validar unidad de medida
        var unidad = await _unidadRepo.GetByIdAsync(dto.UnidadMedidaId, ct);
        if (unidad is null || unidad.EmpresaId != empresaId.Value)
            return Result<ProductoDto>.Failure("Unidad de medida no encontrada o no pertenece a su empresa.");

        entity.Codigo = dto.Codigo;
        entity.Nombre = dto.Nombre;
        entity.Descripcion = dto.Descripcion;
        entity.CategoriaProductoId = dto.CategoriaProductoId;
        entity.UnidadMedidaId = dto.UnidadMedidaId;
        entity.PrecioCompra = dto.PrecioCompra;
        entity.PrecioVenta = dto.PrecioVenta;
        entity.StockMinimo = dto.StockMinimo;
        entity.Sku = dto.Sku;
        entity.TipoProducto = dto.TipoProducto;
        entity.ControlStock = dto.ControlStock;
        entity.CostoBase = dto.CostoBase;
        entity.Activo = dto.Activo;

        await _repository.UpdateAsync(entity, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        var catMap = new Dictionary<int, string> { { categoria.Id, categoria.Nombre } };
        var uniMap = new Dictionary<int, string> { { unidad.Id, unidad.Nombre } };

        return Result<ProductoDto>.Success(MapToDto(entity, catMap, uniMap));
    }

    public async Task<Result<bool>> DeleteAsync(int id, CancellationToken ct = default)
    {
        var empresaId = _currentUser.EmpresaId;
        if (!empresaId.HasValue)
            return Result<bool>.Failure("No se pudo determinar la empresa activa.");

        var entity = await _repository.GetByIdAsync(id, ct);
        if (entity is null)
            return Result<bool>.Failure($"Producto con Id {id} no encontrado.");

        if (entity.EmpresaId != empresaId.Value)
            return Result<bool>.Failure("No tiene permisos para eliminar este producto.");

        await _repository.DeleteAsync(entity, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        return Result<bool>.Success(true);
    }

    private static ProductoDto MapToDto(
        Producto p,
        Dictionary<int, string> catMap,
        Dictionary<int, string> uniMap) => new()
    {
        Id = p.Id,
        Codigo = p.Codigo,
        Nombre = p.Nombre,
        Descripcion = p.Descripcion,
        CategoriaProductoId = p.CategoriaProductoId,
        CategoriaNombre = catMap.TryGetValue(p.CategoriaProductoId, out var cn) ? cn : $"Cat #{p.CategoriaProductoId}",
        UnidadMedidaId = p.UnidadMedidaId,
        UnidadMedidaNombre = uniMap.TryGetValue(p.UnidadMedidaId, out var un) ? un : $"UM #{p.UnidadMedidaId}",
        PrecioCompra = p.PrecioCompra,
        PrecioVenta = p.PrecioVenta,
        StockMinimo = p.StockMinimo,
        Sku = p.Sku,
        TipoProducto = p.TipoProducto,
        ControlStock = p.ControlStock,
        CostoBase = p.CostoBase,
        Activo = p.Activo,
        EmpresaId = p.EmpresaId
    };
}
