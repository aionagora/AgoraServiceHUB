namespace AgoraHub360.ERP.Application.Services;

using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Domain.Entities.Core;
using AgoraHub360.ERP.Domain.Entities.MDM;
using AgoraHub360.ERP.Domain.Interfaces;
using AgoraHub360.ERP.Shared.Constants;
using AgoraHub360.ERP.Shared.DTOs.Empresa;
using AgoraHub360.ERP.Shared.DTOs.MDM;
using AgoraHub360.ERP.Shared.DTOs.Numeracion;
using AgoraHub360.ERP.Shared.DTOs.Sucursal;
using AgoraHub360.ERP.Shared.DTOs.Usuario;

public class EmpresaDemoService : IEmpresaDemoService
{
    private static readonly Regex CodigoDemoRegex = new("^DEMO\\d{2,3}$", RegexOptions.Compiled | RegexOptions.CultureInvariant);
    private static readonly string[] TiposNumeracionBase = ["VTA", "PED", "FAC", "OC", "RC", "MOV", "ASI"];

    private readonly IRepository<Empresa> _empresaRepo;
    private readonly IRepository<Usuario> _usuarioRepo;
    private readonly IRepository<UsuarioEmpresa> _usuarioEmpresaRepo;
    private readonly IRepository<Sucursal> _sucursalRepo;
    private readonly IRepository<Almacen> _almacenRepo;
    private readonly IRepository<Cliente> _clienteRepo;
    private readonly IRepository<Proveedor> _proveedorRepo;
    private readonly IRepository<Catalog> _catalogRepo;
    private readonly IRepository<Uom> _uomRepo;
    private readonly IRepository<ProductStatus> _productStatusRepo;
    private readonly IRepository<Product> _productRepo;
    private readonly IRepository<CompanyProduct> _companyProductRepo;
    private readonly IRepository<UsuarioSucursalAcceso> _usuarioSucursalAccesoRepo;
    private readonly IParametroSistemaService _parametroSistemaService;
    private readonly INumeracionDocumentoService _numeracionDocumentoService;
    private readonly IConfiguracionInicialEmpresaService _configuracionInicialEmpresaService;
    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork _uow;

    public EmpresaDemoService(
        IRepository<Empresa> empresaRepo,
        IRepository<Usuario> usuarioRepo,
        IRepository<UsuarioEmpresa> usuarioEmpresaRepo,
        IRepository<Sucursal> sucursalRepo,
        IRepository<Almacen> almacenRepo,
        IRepository<Cliente> clienteRepo,
        IRepository<Proveedor> proveedorRepo,
        IRepository<Catalog> catalogRepo,
        IRepository<Uom> uomRepo,
        IRepository<ProductStatus> productStatusRepo,
        IRepository<Product> productRepo,
        IRepository<CompanyProduct> companyProductRepo,
        IRepository<UsuarioSucursalAcceso> usuarioSucursalAccesoRepo,
        IParametroSistemaService parametroSistemaService,
        INumeracionDocumentoService numeracionDocumentoService,
        IConfiguracionInicialEmpresaService configuracionInicialEmpresaService,
        ICurrentUserService currentUser,
        IUnitOfWork uow)
    {
        _empresaRepo = empresaRepo;
        _usuarioRepo = usuarioRepo;
        _usuarioEmpresaRepo = usuarioEmpresaRepo;
        _sucursalRepo = sucursalRepo;
        _almacenRepo = almacenRepo;
        _clienteRepo = clienteRepo;
        _proveedorRepo = proveedorRepo;
        _catalogRepo = catalogRepo;
        _uomRepo = uomRepo;
        _productStatusRepo = productStatusRepo;
        _productRepo = productRepo;
        _companyProductRepo = companyProductRepo;
        _usuarioSucursalAccesoRepo = usuarioSucursalAccesoRepo;
        _parametroSistemaService = parametroSistemaService;
        _numeracionDocumentoService = numeracionDocumentoService;
        _configuracionInicialEmpresaService = configuracionInicialEmpresaService;
        _currentUser = currentUser;
        _uow = uow;
    }

    public async Task<Result<CrearEmpresaDemoCompletaResponseDto>> CrearCompletaAsync(
        CrearEmpresaDemoCompletaRequestDto request,
        CancellationToken ct = default)
    {
        if (!CanRunDemo())
            return Result<CrearEmpresaDemoCompletaResponseDto>.Failure("Solo PlatformAdmin/SuperAdmin puede ejecutar la creación DEMO.");

        var codigoDemo = NormalizeCodigoDemo(request.CodigoDemo);
        if (codigoDemo is null)
            return Result<CrearEmpresaDemoCompletaResponseDto>.Failure("El código demo debe tener formato DEMO## o DEMO###, por ejemplo DEMO01.");

        var nitObjetivo = BuildNit(request.Nit, codigoDemo);
        var nombreEmpresa = string.IsNullOrWhiteSpace(request.NombreEmpresa)
            ? $"AGORA DEMO ERP {codigoDemo} S.R.L."
            : request.NombreEmpresa.Trim();

        var response = new CrearEmpresaDemoCompletaResponseDto
        {
            CodigoDemo = codigoDemo,
            Nit = nitObjetivo,
            ResetSolicitado = request.ResetSiExiste,
            Advertencia = request.ResetSiExiste
                ? "Reset no implementado para proteger datos transaccionales. Se completaron datos faltantes."
                : null
        };

        if (!string.IsNullOrWhiteSpace(response.Advertencia))
            response.Advertencias.Add(response.Advertencia);

        return await _uow.ExecuteInTransactionAsync(async () =>
        {
            var empresaResult = await EnsureEmpresaAsync(codigoDemo, nombreEmpresa, nitObjetivo, response, ct);
            if (!empresaResult.IsSuccess)
                return Result<CrearEmpresaDemoCompletaResponseDto>.Failure(empresaResult.Error!);

            var empresa = empresaResult.Value!;
            response.EmpresaId = empresa.Id;
            response.NombreEmpresa = empresa.Nombre;
            response.Empresa = empresa.Nombre;
            response.Nit = empresa.NIT ?? nitObjetivo;

            var configResult = await _configuracionInicialEmpresaService.GenerarConfiguracionBasicaAsync(empresa.Id, ct);
            if (!configResult.IsSuccess)
                return Result<CrearEmpresaDemoCompletaResponseDto>.Failure(configResult.Error!);

            var sucursales = await EnsureSucursalesAsync(codigoDemo, empresa.Id, response, ct);
            await EnsureUsuariosAsync(codigoDemo, empresa.Id, sucursales, response, ct);
            await EnsureAlmacenesAsync(codigoDemo, empresa.Id, sucursales, response, ct);
            await EnsureClientesAsync(codigoDemo, empresa.Id, response, ct);
            await EnsureProveedoresAsync(codigoDemo, empresa.Id, response, ct);
            try
            {
                await EnsureProductosAsync(codigoDemo, empresa.Id, response, ct);
            }
            catch (Exception ex)
            {
                var advertencia = $"Productos no creados: {ex.Message}";
                response.Advertencia = advertencia;
                response.Advertencias.Add(advertencia);
            }

            await EnsureNumeracionesAsync(codigoDemo, sucursales, response, ct);

            response.Mensaje = $"Empresa demo {codigoDemo} procesada correctamente.";
            return Result<CrearEmpresaDemoCompletaResponseDto>.Success(response);
        }, ct);
    }

    private bool CanRunDemo()
    {
        var platformRole = _currentUser.PlatformRole;
        return string.Equals(platformRole, Roles.SuperAdmin, StringComparison.OrdinalIgnoreCase)
               || string.Equals(platformRole, Roles.SystemAdmin, StringComparison.OrdinalIgnoreCase);
    }

    private static string? NormalizeCodigoDemo(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return null;

        var normalized = raw.Trim().ToUpperInvariant();
        return CodigoDemoRegex.IsMatch(normalized) ? normalized : null;
    }

    private static string BuildNit(string? nitRequest, string codigoDemo)
    {
        if (!string.IsNullOrWhiteSpace(nitRequest))
            return nitRequest.Trim();

        var index = int.Parse(codigoDemo[4..], System.Globalization.CultureInfo.InvariantCulture);
        return $"990001{index:D3}";
    }

    private async Task<Result<Empresa>> EnsureEmpresaAsync(
        string codigoDemo,
        string nombreEmpresa,
        string nit,
        CrearEmpresaDemoCompletaResponseDto response,
        CancellationToken ct)
    {
        var empresas = await _empresaRepo.FindIgnoreQueryFiltersAsync(e => e.NIT == nit, ct);
        var existentePorNit = empresas.FirstOrDefault();

        if (existentePorNit is not null)
        {
            if (!ContainsCodigoDemo(existentePorNit.Nombre, codigoDemo))
            {
                return Result<Empresa>.Failure(
                    $"El NIT {nit} ya pertenece a otra empresa ({existentePorNit.Nombre}).");
            }

            if (!existentePorNit.Activo)
                existentePorNit.Activo = true;

            if (string.IsNullOrWhiteSpace(existentePorNit.Email))
                existentePorNit.Email = $"demo.{codigoDemo.ToLowerInvariant()}@agorahub360.com";

            if (string.IsNullOrWhiteSpace(existentePorNit.Telefono))
                existentePorNit.Telefono = "+591 70000000";

            if (string.IsNullOrWhiteSpace(existentePorNit.Direccion))
                existentePorNit.Direccion = $"{codigoDemo} - Dirección DEMO";

            if (string.IsNullOrWhiteSpace(existentePorNit.MonedaBaseId))
                existentePorNit.MonedaBaseId = "BOB";

            await _empresaRepo.UpdateAsync(existentePorNit, ct);
            await _uow.SaveChangesAsync(ct);

            response.EmpresaExistente = true;
            return Result<Empresa>.Success(existentePorNit);
        }

        var empresa = new Empresa
        {
            Nombre = nombreEmpresa,
            NIT = nit,
            Direccion = $"{codigoDemo} - Dirección DEMO",
            Telefono = "+591 70000000",
            Email = $"demo.{codigoDemo.ToLowerInvariant()}@agorahub360.com",
            MonedaBaseId = "BOB",
            Activo = true,
            PrefijoSku = codigoDemo,
            PermiteVariantes = true,
            PermiteServicios = true,
            AutoGeneraSku = true,
            MetodoCosteoDefault = 1,
            IndustriaId = 7
        };

        await _empresaRepo.AddAsync(empresa, ct);
        await _uow.SaveChangesAsync(ct);

        response.EmpresaCreada = true;
        return Result<Empresa>.Success(empresa);
    }

    private async Task EnsureUsuariosAsync(
        string codigoDemo,
        int empresaId,
        IReadOnlyList<Sucursal> sucursales,
        CrearEmpresaDemoCompletaResponseDto response,
        CancellationToken ct)
    {
        var lower = codigoDemo.ToLowerInvariant();

        var users = new[]
        {
            new UsuarioTemplate($"admindemo{lower[4..]}", $"admin.{lower}@agorahub360.com", $"Admin {codigoDemo}", Roles.AdminEmpresa, true, true),
            new UsuarioTemplate($"operadordemo{lower[4..]}", $"operador.{lower}@agorahub360.com", $"Operador {codigoDemo}", Roles.Operador, true, true),
            new UsuarioTemplate($"viewerdemo{lower[4..]}", $"viewer.{lower}@agorahub360.com", $"Viewer {codigoDemo}", Roles.Viewer, true, false),
        };

        var principalId = sucursales.FirstOrDefault(s => s.EsCentral)?.Id ?? sucursales.First().Id;

        foreach (var template in users)
        {
            var existing = (await _usuarioRepo.FindIgnoreQueryFiltersAsync(
                u => u.Email == template.Email || u.NombreUsuario == template.NombreUsuario,
                ct)).FirstOrDefault();

            if (existing is null)
            {
                existing = new Usuario
                {
                    NombreUsuario = template.NombreUsuario,
                    Email = template.Email,
                    NombreCompleto = template.NombreCompleto,
                    PasswordHash = HashPassword("Admin123"),
                    EmpresaActivaId = empresaId,
                    Activo = true
                };

                await _usuarioRepo.AddAsync(existing, ct);
                await _uow.SaveChangesAsync(ct);
                response.UsuariosCreados++;
                if (!response.Usuarios.Contains(existing.Email, StringComparer.OrdinalIgnoreCase))
                    response.Usuarios.Add(existing.Email);
            }
            else
            {
                if (!existing.Activo)
                    existing.Activo = true;

                if (!existing.EmpresaActivaId.HasValue)
                    existing.EmpresaActivaId = empresaId;

                await _usuarioRepo.UpdateAsync(existing, ct);
                await _uow.SaveChangesAsync(ct);
                response.UsuariosExistentes++;
                if (!response.Usuarios.Contains(existing.Email, StringComparer.OrdinalIgnoreCase))
                    response.Usuarios.Add(existing.Email);
            }

            var membership = (await _usuarioEmpresaRepo.FindIgnoreQueryFiltersAsync(
                ue => ue.UsuarioId == existing.Id && ue.EmpresaId == empresaId,
                ct)).FirstOrDefault();

            if (membership is null)
            {
                await _usuarioEmpresaRepo.AddAsync(new UsuarioEmpresa
                {
                    UsuarioId = existing.Id,
                    EmpresaId = empresaId,
                    Rol = template.Rol
                }, ct);
                await _uow.SaveChangesAsync(ct);
            }
            else if (!string.Equals(membership.Rol, template.Rol, StringComparison.OrdinalIgnoreCase))
            {
                membership.Rol = template.Rol;
                await _usuarioEmpresaRepo.UpdateAsync(membership, ct);
                await _uow.SaveChangesAsync(ct);
            }

            foreach (var sucursal in sucursales)
            {
                var acceso = (await _usuarioSucursalAccesoRepo.FindIgnoreQueryFiltersAsync(
                    usa => usa.EmpresaId == empresaId
                        && usa.UsuarioId == existing.Id
                        && usa.SucursalId == sucursal.Id,
                    ct)).FirstOrDefault();

                if (acceso is null)
                {
                    await _usuarioSucursalAccesoRepo.AddAsync(new UsuarioSucursalAcceso
                    {
                        EmpresaId = empresaId,
                        UsuarioId = existing.Id,
                        SucursalId = sucursal.Id,
                        EsPredeterminada = sucursal.Id == principalId,
                        PuedeConsultar = template.PuedeConsultar,
                        PuedeOperar = template.PuedeOperar,
                        Activo = true
                    }, ct);
                    await _uow.SaveChangesAsync(ct);
                }
                else
                {
                    acceso.EsPredeterminada = sucursal.Id == principalId;
                    acceso.PuedeConsultar = template.PuedeConsultar;
                    acceso.PuedeOperar = template.PuedeOperar;
                    acceso.Activo = true;
                    await _usuarioSucursalAccesoRepo.UpdateAsync(acceso, ct);
                    await _uow.SaveChangesAsync(ct);
                }
            }
        }
    }

    private async Task<IReadOnlyList<Sucursal>> EnsureSucursalesAsync(
        string codigoDemo,
        int empresaId,
        CrearEmpresaDemoCompletaResponseDto response,
        CancellationToken ct)
    {
        var required = new[]
        {
            new CrearSucursalDto
            {
                EmpresaId = empresaId,
                Nombre = $"{codigoDemo} - Sucursal Principal",
                Codigo = $"{codigoDemo}-PRINCIPAL",
                Sigla = $"{codigoDemo}P",
                PrefijoDocumental = codigoDemo,
                EsCentral = true,
                Activo = true
            },
            new CrearSucursalDto
            {
                EmpresaId = empresaId,
                Nombre = $"{codigoDemo} - Sucursal Norte",
                Codigo = $"{codigoDemo}-NORTE",
                Sigla = $"{codigoDemo}N",
                PrefijoDocumental = codigoDemo,
                EsCentral = false,
                Activo = true
            }
        };

        var createdOrExisting = new List<Sucursal>();

        foreach (var item in required)
        {
            var byCodigo = (await _sucursalRepo.FindIgnoreQueryFiltersAsync(
                s => s.EmpresaId == empresaId && s.Codigo == item.Codigo,
                ct)).FirstOrDefault();

            if (byCodigo is null)
            {
                byCodigo = new Sucursal
                {
                    EmpresaId = empresaId,
                    Nombre = item.Nombre,
                    Codigo = item.Codigo,
                    Sigla = item.Sigla,
                    PrefijoDocumental = item.PrefijoDocumental,
                    EsCentral = item.EsCentral,
                    Activo = true,
                    PermiteCompras = true,
                    PermiteVentas = true,
                    PermiteInventario = true,
                    PermiteDespacho = true,
                    PermiteFacturacion = true,
                    ManejaAlmacen = true
                };

                await _sucursalRepo.AddAsync(byCodigo, ct);
                await _uow.SaveChangesAsync(ct);
                response.SucursalesCreadas++;
                response.Sucursales.Add(byCodigo.Codigo ?? byCodigo.Nombre);
            }
            else
            {
                response.SucursalesExistentes++;
                if (!response.Sucursales.Contains(byCodigo.Codigo ?? byCodigo.Nombre, StringComparer.OrdinalIgnoreCase))
                    response.Sucursales.Add(byCodigo.Codigo ?? byCodigo.Nombre);
            }

            createdOrExisting.Add(byCodigo);
        }

        return createdOrExisting;
    }

    private async Task EnsureAlmacenesAsync(
        string codigoDemo,
        int empresaId,
        IReadOnlyList<Sucursal> sucursales,
        CrearEmpresaDemoCompletaResponseDto response,
        CancellationToken ct)
    {
        var map = new[]
        {
            new { Codigo = $"ALM-{codigoDemo}-PRINCIPAL", Nombre = $"{codigoDemo} - Almacén Principal", Sucursal = sucursales.FirstOrDefault(s => s.Nombre.Contains("Principal", StringComparison.OrdinalIgnoreCase)) },
            new { Codigo = $"ALM-{codigoDemo}-NORTE", Nombre = $"{codigoDemo} - Almacén Norte", Sucursal = sucursales.FirstOrDefault(s => s.Nombre.Contains("Norte", StringComparison.OrdinalIgnoreCase)) },
        };

        foreach (var item in map)
        {
            var existing = (await _almacenRepo.FindIgnoreQueryFiltersAsync(
                a => a.EmpresaId == empresaId && a.Codigo == item.Codigo,
                ct)).FirstOrDefault();

            if (existing is null)
            {
                await _almacenRepo.AddAsync(new Almacen
                {
                    EmpresaId = empresaId,
                    Codigo = item.Codigo,
                    Nombre = item.Nombre,
                    SucursalId = item.Sucursal?.Id,
                    Activo = true
                }, ct);
                await _uow.SaveChangesAsync(ct);
                response.AlmacenesCreados++;
                response.Almacenes.Add(item.Codigo);
            }
            else
            {
                response.AlmacenesExistentes++;
                if (!response.Almacenes.Contains(item.Codigo, StringComparer.OrdinalIgnoreCase))
                    response.Almacenes.Add(item.Codigo);
            }
        }
    }

    private async Task EnsureClientesAsync(
        string codigoDemo,
        int empresaId,
        CrearEmpresaDemoCompletaResponseDto response,
        CancellationToken ct)
    {
        var items = new[]
        {
            new CreateClienteDto { Codigo = $"{codigoDemo}-CLI-CONTADO", RazonSocial = $"{codigoDemo} - CLIENTE CONTADO", TipoCliente = "General", NIT = BuildPersonaNit(codigoDemo, "001") },
            new CreateClienteDto { Codigo = $"{codigoDemo}-CLI-EMPRESA", RazonSocial = $"{codigoDemo} - CLIENTE EMPRESA", TipoCliente = "Corporativo", NIT = BuildPersonaNit(codigoDemo, "002") },
            new CreateClienteDto { Codigo = $"{codigoDemo}-CLI-MAYORISTA", RazonSocial = $"{codigoDemo} - CLIENTE MAYORISTA", TipoCliente = "Mayorista", NIT = BuildPersonaNit(codigoDemo, "003") },
        };

        foreach (var item in items)
        {
            var existing = (await _clienteRepo.FindIgnoreQueryFiltersAsync(
                c => c.EmpresaId == empresaId && c.Codigo == item.Codigo,
                ct)).FirstOrDefault();

            if (existing is null)
            {
                await _clienteRepo.AddAsync(new Cliente
                {
                    EmpresaId = empresaId,
                    Codigo = item.Codigo,
                    RazonSocial = item.RazonSocial,
                    TipoCliente = item.TipoCliente,
                    NIT = item.NIT,
                    Activo = true
                }, ct);
                await _uow.SaveChangesAsync(ct);
                response.ClientesCreados++;
            }
            else
            {
                response.ClientesExistentes++;
            }
        }
    }

    private async Task EnsureProveedoresAsync(
        string codigoDemo,
        int empresaId,
        CrearEmpresaDemoCompletaResponseDto response,
        CancellationToken ct)
    {
        var items = new[]
        {
            new CreateProveedorDto { Codigo = $"{codigoDemo}-PRV-LOCAL", RazonSocial = $"{codigoDemo} - PROVEEDOR LOCAL", TipoProveedor = "Local", NIT = BuildPersonaNit(codigoDemo, "101") },
            new CreateProveedorDto { Codigo = $"{codigoDemo}-PRV-IMPORT", RazonSocial = $"{codigoDemo} - PROVEEDOR IMPORTACIÓN", TipoProveedor = "Internacional", NIT = BuildPersonaNit(codigoDemo, "102") },
        };

        foreach (var item in items)
        {
            var existing = (await _proveedorRepo.FindIgnoreQueryFiltersAsync(
                p => p.EmpresaId == empresaId && p.Codigo == item.Codigo,
                ct)).FirstOrDefault();

            if (existing is null)
            {
                await _proveedorRepo.AddAsync(new Proveedor
                {
                    EmpresaId = empresaId,
                    Codigo = item.Codigo,
                    RazonSocial = item.RazonSocial,
                    TipoProveedor = item.TipoProveedor,
                    NIT = item.NIT,
                    Activo = true
                }, ct);
                await _uow.SaveChangesAsync(ct);
                response.ProveedoresCreados++;
            }
            else
            {
                response.ProveedoresExistentes++;
            }
        }
    }

    private async Task EnsureProductosAsync(
        string codigoDemo,
        int empresaId,
        CrearEmpresaDemoCompletaResponseDto response,
        CancellationToken ct)
    {
        var catalog = await EnsureCatalogAsync(empresaId, codigoDemo, ct);
        var uom = await EnsureUomAsync(empresaId, ct);
        var status = await EnsureProductStatusAsync(empresaId, ct);

        var products = new[]
        {
            new { Generic = $"{codigoDemo} - CAFÉ MOLIDO", Commercial = $"{codigoDemo} - CAFÉ MOLIDO", Sku = $"SKU-{codigoDemo}-CAFE" },
            new { Generic = $"{codigoDemo} - EMPANADA", Commercial = $"{codigoDemo} - EMPANADA", Sku = $"SKU-{codigoDemo}-EMP" },
            new { Generic = $"{codigoDemo} - SERVICIO DELIVERY", Commercial = $"{codigoDemo} - SERVICIO DELIVERY", Sku = $"SKU-{codigoDemo}-DEL" },
        };

        foreach (var item in products)
        {
            var existingProduct = (await _productRepo.FindIgnoreQueryFiltersAsync(
                p => p.EmpresaId == empresaId && p.CommercialName == item.Commercial,
                ct)).FirstOrDefault();

            if (existingProduct is null)
            {
                existingProduct = new Product
                {
                    EmpresaId = empresaId,
                    CatalogId = catalog.CatalogId,
                    ProductKind = (byte)(item.Commercial.Contains("SERVICIO", StringComparison.OrdinalIgnoreCase) ? 2 : 1),
                    GenericName = item.Generic,
                    CommercialName = item.Commercial,
                    DefaultUomId = uom.UomId,
                    IsStockable = !item.Commercial.Contains("SERVICIO", StringComparison.OrdinalIgnoreCase),
                    IsSellable = true,
                    IsPurchasable = true,
                    LifecycleStatusId = status.ProductStatusId,
                    Activo = true
                };

                await _productRepo.AddAsync(existingProduct, ct);
                await _uow.SaveChangesAsync(ct);
                response.ProductosCreados++;
            }
            else
            {
                response.ProductosExistentes++;
            }

            var companyProduct = (await _companyProductRepo.FindIgnoreQueryFiltersAsync(
                cp => cp.EmpresaId == empresaId && cp.ProductId == existingProduct.ProductId,
                ct)).FirstOrDefault();

            if (companyProduct is null)
            {
                await _companyProductRepo.AddAsync(new CompanyProduct
                {
                    EmpresaId = empresaId,
                    ProductId = existingProduct.ProductId,
                    Sku = item.Sku,
                    CodigoInterno = item.Sku,
                    IsVisiblePOS = true,
                    IsVisibleB2B = true,
                    IsVisibleEcommerce = false,
                    AllowReturns = true,
                    Activo = true
                }, ct);
                await _uow.SaveChangesAsync(ct);
            }
        }
    }

    private async Task EnsureNumeracionesAsync(
        string codigoDemo,
        IReadOnlyList<Sucursal> sucursales,
        CrearEmpresaDemoCompletaResponseDto response,
        CancellationToken ct)
    {
        var principal = sucursales.FirstOrDefault(s => s.EsCentral) ?? sucursales.First();

        foreach (var tipo in TiposNumeracionBase)
        {
            var prefijo = $"{tipo}-{codigoDemo}-";
            var dto = new CrearNumeracionDocumentoRequestDto
            {
                TipoDocumento = tipo,
                Descripcion = $"Numeración {tipo} {codigoDemo}",
                SucursalId = principal.Id,
                Prefijo = prefijo.Length > 20 ? prefijo[..20] : prefijo,
                SiguienteNumero = 1,
                LongitudNumero = 6,
                Activo = true
            };

            var result = await _numeracionDocumentoService.CreateAsync(dto, ct);
            if (result.IsSuccess)
            {
                response.NumeracionesCreadas++;
            }
            else if (result.Error?.Contains("Ya existe una numeración", StringComparison.OrdinalIgnoreCase) == true)
            {
                response.NumeracionesExistentes++;
            }
            else
            {
                throw new InvalidOperationException(result.Error ?? $"No se pudo crear numeración {tipo}.");
            }
        }
    }

    private async Task<Catalog> EnsureCatalogAsync(int empresaId, string codigoDemo, CancellationToken ct)
    {
        var existing = (await _catalogRepo.FindIgnoreQueryFiltersAsync(
            c => c.EmpresaId == empresaId && c.IsDefault,
            ct)).FirstOrDefault();

        if (existing is not null)
            return existing;

        var catalog = new Catalog
        {
            EmpresaId = empresaId,
            Scope = 1,
            Name = $"Catálogo {codigoDemo}",
            IsDefault = true,
            Activo = true
        };

        await _catalogRepo.AddAsync(catalog, ct);
        await _uow.SaveChangesAsync(ct);
        return catalog;
    }

    private async Task<Uom> EnsureUomAsync(int empresaId, CancellationToken ct)
    {
        var existing = (await _uomRepo.FindIgnoreQueryFiltersAsync(
            u => u.EmpresaId == empresaId && u.Code == "UND",
            ct)).FirstOrDefault();

        if (existing is not null)
            return existing;

        var uom = new Uom
        {
            EmpresaId = empresaId,
            Code = "UND",
            Name = "Unidad",
            Activo = true
        };

        await _uomRepo.AddAsync(uom, ct);
        await _uow.SaveChangesAsync(ct);
        return uom;
    }

    private async Task<ProductStatus> EnsureProductStatusAsync(int empresaId, CancellationToken ct)
    {
        var existing = (await _productStatusRepo.FindIgnoreQueryFiltersAsync(
            s => s.EmpresaId == empresaId && s.Code == "ACTIVE",
            ct)).FirstOrDefault();

        if (existing is not null)
            return existing;

        var status = new ProductStatus
        {
            EmpresaId = empresaId,
            Code = "ACTIVE",
            Name = "Activo",
            IsDefault = true,
            Activo = true
        };

        await _productStatusRepo.AddAsync(status, ct);
        await _uow.SaveChangesAsync(ct);
        return status;
    }

    private static bool ContainsCodigoDemo(string? text, string codigoDemo)
    {
        if (string.IsNullOrWhiteSpace(text))
            return false;

        return text.Contains(codigoDemo, StringComparison.OrdinalIgnoreCase);
    }

    private static string BuildPersonaNit(string codigoDemo, string suffix)
    {
        var idx = int.Parse(codigoDemo[4..], System.Globalization.CultureInfo.InvariantCulture);
        return $"99{idx:D2}{suffix}";
    }

    private static string HashPassword(string password)
    {
        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }

    private sealed record UsuarioTemplate(
        string NombreUsuario,
        string Email,
        string NombreCompleto,
        string Rol,
        bool PuedeConsultar,
        bool PuedeOperar);
}
