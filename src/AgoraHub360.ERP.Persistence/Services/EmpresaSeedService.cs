namespace AgoraHub360.ERP.Persistence.Services;

using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Domain.Entities.MDM;
using AgoraHub360.ERP.Domain.Interfaces;
using AgoraHub360.ERP.Persistence.Context;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Seeds default MDM master data when a new company is created:
/// - Default catalog
/// - Standard units of measure
/// - Product lifecycle statuses
/// Uses IgnoreQueryFilters to work regardless of the current tenant context.
/// </summary>
public class EmpresaSeedService : IEmpresaSeedService
{
    private readonly AgoraDbContext _db;
    private readonly IUnitOfWork _uow;

    public EmpresaSeedService(AgoraDbContext db, IUnitOfWork uow)
    {
        _db = db;
        _uow = uow;
    }

    public async Task<Result<bool>> SeedDefaultDataAsync(int empresaId, CancellationToken ct = default)
    {
        // ?? Default Catalog ??????????????????????????????????????????????
        var hasCatalogs = await _db.Set<Catalog>()
            .IgnoreQueryFilters()
            .AnyAsync(c => c.EmpresaId == empresaId, ct);

        if (!hasCatalogs)
        {
            _db.Set<Catalog>().Add(new Catalog
            {
                EmpresaId = empresaId,
                Scope = 1,
                Name = "Catálogo General",
                IsDefault = true,
                Activo = true
            });
        }

        // ?? Default Units of Measure ?????????????????????????????????????
        var hasUoms = await _db.Set<Uom>()
            .IgnoreQueryFilters()
            .AnyAsync(u => u.EmpresaId == empresaId, ct);

        if (!hasUoms)
        {
            var uoms = new[]
            {
                new Uom { EmpresaId = empresaId, Code = "UND", Name = "Unidad",    Activo = true },
                new Uom { EmpresaId = empresaId, Code = "CJ",  Name = "Caja",      Activo = true },
                new Uom { EmpresaId = empresaId, Code = "KG",  Name = "Kilogramo", Activo = true },
                new Uom { EmpresaId = empresaId, Code = "LT",  Name = "Litro",     Activo = true },
                new Uom { EmpresaId = empresaId, Code = "MT",  Name = "Metro",     Activo = true },
                new Uom { EmpresaId = empresaId, Code = "PZ",  Name = "Pieza",     Activo = true },
                new Uom { EmpresaId = empresaId, Code = "PAR", Name = "Par",       Activo = true },
                new Uom { EmpresaId = empresaId, Code = "JGO", Name = "Juego",     Activo = true },
            };
            _db.Set<Uom>().AddRange(uoms);
        }

        // ?? Default Product Statuses ?????????????????????????????????????
        var hasStatuses = await _db.Set<ProductStatus>()
            .IgnoreQueryFilters()
            .AnyAsync(s => s.EmpresaId == empresaId, ct);

        if (!hasStatuses)
        {
            var statuses = new[]
            {
                new ProductStatus { EmpresaId = empresaId, Code = "ACTIVE",       Name = "Activo",        IsDefault = true,  Activo = true },
                new ProductStatus { EmpresaId = empresaId, Code = "INACTIVE",     Name = "Inactivo",      IsDefault = false, Activo = true },
                new ProductStatus { EmpresaId = empresaId, Code = "DRAFT",        Name = "Borrador",      IsDefault = false, Activo = true },
                new ProductStatus { EmpresaId = empresaId, Code = "DISCONTINUED", Name = "Descontinuado", IsDefault = false, Activo = true },
            };
            _db.Set<ProductStatus>().AddRange(statuses);
        }

        await _uow.SaveChangesAsync(ct);
        return Result<bool>.Success(true);
    }
}
