namespace AgoraHub360.ERP.Application.Services;

using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Shared.DTOs.MDM;

public class ProductoService : IProductoService
{
    private readonly IProductService _productService;

    public ProductoService(IProductService productService) => _productService = productService;

    public Task<Result<IReadOnlyList<ProductDto2>>> GetAllAsync(long? catalogId = null, CancellationToken ct = default)
        => _productService.GetAllAsync(catalogId, ct);

    public Task<Result<ProductDto2>> GetByIdAsync(long id, CancellationToken ct = default)
        => _productService.GetByIdAsync(id, ct);

    public Task<Result<ProductDto2>> CreateAsync(CreateProductDto2 dto, CancellationToken ct = default)
        => _productService.CreateAsync(dto, ct);

    public Task<Result<ProductDto2>> UpdateAsync(long id, UpdateProductDto2 dto, CancellationToken ct = default)
        => _productService.UpdateAsync(id, dto, ct);

    public Task<Result<bool>> DeleteAsync(long id, CancellationToken ct = default)
        => _productService.DeleteAsync(id, ct);
}
