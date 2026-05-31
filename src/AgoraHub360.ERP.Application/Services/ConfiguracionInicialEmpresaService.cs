namespace AgoraHub360.ERP.Application.Services;

using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Domain.Entities.Core;
using AgoraHub360.ERP.Domain.Entities.VTA;
using AgoraHub360.ERP.Domain.Interfaces;
using AgoraHub360.ERP.Shared.Constants;
using AgoraHub360.ERP.Shared.DTOs.Empresa;

public class ConfiguracionInicialEmpresaService : IConfiguracionInicialEmpresaService
{
    private static readonly string[] TiposNumeracion = { "VTA", "FAC", "PED", "COT", "OC", "ING", "SAL", "TRF" };

    private static readonly ParametroDef[] ParametrosBase =
    {
        new("EMPRESA_MONEDA_BASE", "BOB", "Moneda base de la empresa", "General", "String"),
        new("EMPRESA_PAIS", "BO", "País de operación", "General", "String"),
        new("EMPRESA_ZONA_HORARIA", "America/La_Paz", "Zona horaria por defecto", "General", "String"),
        new("VTA_PERMITIR_STOCK_NEGATIVO", "false", "Permite vender con stock negativo", "Ventas", "Boolean"),
        new("VTA_CONFIRMAR_AUTOMATICAMENTE", "false", "Confirmación automática de ventas", "Ventas", "Boolean"),
        new("VTA_REQUIERE_CLIENTE", "true", "Requiere cliente en venta", "Ventas", "Boolean"),
        new("FAC_EMAIL_OBLIGATORIO", "false", "Email obligatorio para facturación", "Facturación", "Boolean"),
        new("FAC_GENERAR_AUTOMATICA", "false", "Generación automática de factura", "Facturación", "Boolean"),
        new("INV_METODO_COSTEO", "Promedio", "Método de costeo por defecto", "Inventario", "String"),
        new("INV_PERMITIR_NEGATIVO", "false", "Permite inventario negativo", "Inventario", "Boolean"),
        new("SIAT_MODO", "TEST", "Modo SIAT", "Facturación", "String"),
        new("SIAT_AMBIENTE", "1", "Ambiente SIAT", "Facturación", "Integer")
    };

    private static readonly SiatMetodoPagoDef[] MetodosPagoSiatBase =
    {
        new("1", "Efectivo", "Efectivo", true),
        new("2", "Tarjeta", "Tarjeta", false),
        new("3", "Cheque", "Cheque", false),
        new("4", "Transferencia / Depósito", "Transferencia", false),
        new("7", "QR", "Qr", false)
    };

    private readonly IRepository<Empresa> _empresaRepository;
    private readonly IRepository<ParametroSistema> _parametroRepository;
    private readonly IRepository<Sucursal> _sucursalRepository;
    private readonly IRepository<NumeracionDocumento> _numeracionRepository;
    private readonly IRepository<SiatMetodoPago> _siatMetodoPagoRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public ConfiguracionInicialEmpresaService(
        IRepository<Empresa> empresaRepository,
        IRepository<ParametroSistema> parametroRepository,
        IRepository<Sucursal> sucursalRepository,
        IRepository<NumeracionDocumento> numeracionRepository,
        IRepository<SiatMetodoPago> siatMetodoPagoRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _empresaRepository = empresaRepository;
        _parametroRepository = parametroRepository;
        _sucursalRepository = sucursalRepository;
        _numeracionRepository = numeracionRepository;
        _siatMetodoPagoRepository = siatMetodoPagoRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<ConfiguracionInicialEmpresaResultadoDto>> GenerarConfiguracionBasicaAsync(long empresaId, CancellationToken ct = default)
    {
        if (empresaId <= 0 || empresaId > int.MaxValue)
            return Result<ConfiguracionInicialEmpresaResultadoDto>.Failure("Empresa inválida para generar configuración inicial.");

        var empresaIdInt = (int)empresaId;
        var empresa = await _empresaRepository.GetByIdAsync(empresaIdInt, ct);
        if (empresa is null)
            return Result<ConfiguracionInicialEmpresaResultadoDto>.Failure("La empresa no existe.");

        if (!empresa.Activo)
            return Result<ConfiguracionInicialEmpresaResultadoDto>.Failure("La empresa está inactiva y no puede configurarse.");

        var esAdmin = _currentUserService.IsInRole(Roles.Admin);
        var empresaActivaUsuario = _currentUserService.EmpresaId;

        if (!esAdmin)
        {
            if (!empresaActivaUsuario.HasValue)
                return Result<ConfiguracionInicialEmpresaResultadoDto>.Failure("No se pudo determinar la empresa activa del usuario.");

            if (empresaActivaUsuario.Value != empresaIdInt)
                return Result<ConfiguracionInicialEmpresaResultadoDto>.Failure("No tiene permisos para configurar la empresa solicitada.");
        }

        return await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            var resultado = new ConfiguracionInicialEmpresaResultadoDto();

            await GenerarParametrosBaseAsync(empresaIdInt, resultado, ct);
            await GenerarNumeracionesBaseAsync(empresaIdInt, resultado, ct);
            await GenerarMetodosPagoSiatBaseAsync(empresaIdInt, resultado, ct);

            await _unitOfWork.SaveChangesAsync(ct);

            resultado.Mensaje =
                $"Configuración inicial generada. Parámetros: {resultado.ParametrosCreados} creados / {resultado.ParametrosExistentes} existentes. " +
                $"Numeraciones: {resultado.NumeracionesCreadas} creadas / {resultado.NumeracionesExistentes} existentes. " +
                $"Catálogos SIAT: {resultado.CatalogosCreados} creados / {resultado.CatalogosExistentes} existentes.";

            return Result<ConfiguracionInicialEmpresaResultadoDto>.Success(resultado);
        }, ct);
    }

    private async Task GenerarParametrosBaseAsync(int empresaId, ConfiguracionInicialEmpresaResultadoDto resultado, CancellationToken ct)
    {
        var existentes = await _parametroRepository.FindAsync(p => p.EmpresaId == empresaId, ct);
        var existentesPorClave = existentes.ToDictionary(p => p.Clave, StringComparer.OrdinalIgnoreCase);

        foreach (var parametro in ParametrosBase)
        {
            if (existentesPorClave.ContainsKey(parametro.Clave))
            {
                resultado.ParametrosExistentes++;
                continue;
            }

            await _parametroRepository.AddAsync(new ParametroSistema
            {
                EmpresaId = empresaId,
                Clave = parametro.Clave,
                Valor = parametro.Valor,
                Descripcion = parametro.Descripcion,
                Categoria = parametro.Categoria,
                TipoDato = parametro.TipoDato,
                Activo = true
            }, ct);

            resultado.ParametrosCreados++;
        }
    }

    private async Task GenerarNumeracionesBaseAsync(int empresaId, ConfiguracionInicialEmpresaResultadoDto resultado, CancellationToken ct)
    {
        var sucursalesActivas = await _sucursalRepository.FindAsync(s => s.EmpresaId == empresaId && s.Activo, ct);

        foreach (var sucursal in sucursalesActivas)
        {
            var codigoSucursal = ResolveCodigoSucursalDocumental(sucursal);

            foreach (var tipo in TiposNumeracion)
            {
                var existente = await _numeracionRepository.FindAsync(
                    n => n.EmpresaId == empresaId
                         && n.SucursalId == sucursal.Id
                         && n.TipoDocumento == tipo,
                    ct);

                if (existente.Any())
                {
                    resultado.NumeracionesExistentes++;
                    continue;
                }

                await _numeracionRepository.AddAsync(new NumeracionDocumento
                {
                    EmpresaId = empresaId,
                    SucursalId = sucursal.Id,
                    TipoDocumento = tipo,
                    Descripcion = $"Numeración {tipo} sucursal {codigoSucursal}",
                    Prefijo = BuildPrefijo(tipo, codigoSucursal),
                    SiguienteNumero = 1,
                    Digitos = 6,
                    Activo = true
                }, ct);

                resultado.NumeracionesCreadas++;
            }
        }
    }

    private async Task GenerarMetodosPagoSiatBaseAsync(int empresaId, ConfiguracionInicialEmpresaResultadoDto resultado, CancellationToken ct)
    {
        var existentes = await _siatMetodoPagoRepository.FindAsync(x => x.EmpresaId == empresaId, ct);
        var existentesPorCodigo = existentes.ToDictionary(x => x.Codigo, StringComparer.OrdinalIgnoreCase);
        var yaExistePredeterminado = existentes.Any(x => x.Activo && x.EsPredeterminado);

        foreach (var item in MetodosPagoSiatBase)
        {
            if (existentesPorCodigo.ContainsKey(item.Codigo))
            {
                resultado.CatalogosExistentes++;
                continue;
            }

            var nuevo = new SiatMetodoPago
            {
                EmpresaId = empresaId,
                Codigo = item.Codigo,
                Nombre = item.Nombre,
                ModoPago = item.ModoPago,
                EsPredeterminado = item.EsPredeterminado && !yaExistePredeterminado,
                Activo = true
            };

            if (nuevo.EsPredeterminado)
                yaExistePredeterminado = true;

            await _siatMetodoPagoRepository.AddAsync(nuevo, ct);
            resultado.CatalogosCreados++;
        }
    }

    private static string ResolveCodigoSucursalDocumental(Sucursal sucursal)
    {
        var codigoBruto = FirstNonEmpty(
                              sucursal.PrefijoDocumental,
                              sucursal.CodigoSucursalFiscal,
                              sucursal.Sigla,
                              sucursal.CodigoInterno,
                              sucursal.Codigo)
                          ?? "SUC00";

        var normalizado = codigoBruto
            .Trim()
            .Replace(" ", string.Empty, StringComparison.Ordinal)
            .ToUpperInvariant();

        if (string.IsNullOrWhiteSpace(normalizado))
            return "SUC00";

        return normalizado.Length > 10
            ? normalizado[..10]
            : normalizado;
    }

    private static string BuildPrefijo(string tipoDocumento, string codigoSucursal)
    {
        var prefijo = $"{tipoDocumento}-{codigoSucursal}";
        return prefijo.Length > 20
            ? prefijo[..20]
            : prefijo;
    }

    private static string? FirstNonEmpty(params string?[] values)
        => values.FirstOrDefault(v => !string.IsNullOrWhiteSpace(v));

    private sealed record ParametroDef(string Clave, string Valor, string Descripcion, string Categoria, string TipoDato);
    private sealed record SiatMetodoPagoDef(string Codigo, string Nombre, string ModoPago, bool EsPredeterminado);
}
