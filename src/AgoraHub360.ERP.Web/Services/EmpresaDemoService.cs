namespace AgoraHub360.ERP.Web.Services;

using AgoraHub360.ERP.Shared.DTOs.Contabilidad;
using AgoraHub360.ERP.Shared.DTOs.Empresa;
using AgoraHub360.ERP.Shared.DTOs.Inventario;
using AgoraHub360.ERP.Shared.DTOs.MDM;
using ProveedorDtoNS = AgoraHub360.ERP.Shared.DTOs.Proveedor;
using AgoraHub360.ERP.Shared.DTOs.Usuario;

/// <summary>
/// Orquesta la creación de una empresa DEMO completa para Importadora de Muebles.
/// Ejecuta cada paso secuencialmente y reporta progreso via callback.
/// </summary>
public class EmpresaDemoService
{
    private readonly EmpresaHttpService        _empresaSvc;
    private readonly UsuarioHttpService        _usuarioSvc;
    private readonly AlmacenHttpService        _almacenSvc;
    private readonly ProveedorHttpService      _proveedorSvc;
    private readonly ClienteHttpService        _clienteSvc;
    private readonly CuentaContableHttpService _cuentaSvc;
    private readonly AsientoContableHttpService _asientoSvc;
    private readonly PeriodoContableHttpService _periodoSvc;
    private readonly MdmProductoGlobalHttpService _productoGlobalSvc;
    private readonly MdmCompanyProductHttpService _companyProductSvc;
    private readonly MovimientoInventarioHttpService _movimientoSvc;
    private readonly CatalogoHttpService       _catalogoSvc;
    private readonly UnidadMedidaHttpService    _unidadMedidaSvc;

    public EmpresaDemoService(
        EmpresaHttpService empresaSvc,
        UsuarioHttpService usuarioSvc,
        AlmacenHttpService almacenSvc,
        ProveedorHttpService proveedorSvc,
        ClienteHttpService clienteSvc,
        CuentaContableHttpService cuentaSvc,
        AsientoContableHttpService asientoSvc,
        PeriodoContableHttpService periodoSvc,
        MdmProductoGlobalHttpService productoGlobalSvc,
        MdmCompanyProductHttpService companyProductSvc,
        MovimientoInventarioHttpService movimientoSvc,
        CatalogoHttpService catalogoSvc,
        UnidadMedidaHttpService unidadMedidaSvc)
    {
        _empresaSvc       = empresaSvc;
        _usuarioSvc       = usuarioSvc;
        _almacenSvc       = almacenSvc;
        _proveedorSvc     = proveedorSvc;
        _clienteSvc       = clienteSvc;
        _cuentaSvc        = cuentaSvc;
        _asientoSvc       = asientoSvc;
        _periodoSvc       = periodoSvc;
        _productoGlobalSvc = productoGlobalSvc;
        _companyProductSvc = companyProductSvc;
        _movimientoSvc    = movimientoSvc;
        _catalogoSvc      = catalogoSvc;
        _unidadMedidaSvc  = unidadMedidaSvc;
    }

    // ?? Resultado público ????????????????????????????????????????????????????
    public DemoSeedResult Resultado { get; private set; } = new();

    // ?? Progreso ?????????????????????????????????????????????????????????????
    public int PasoActual     { get; private set; }
    public int TotalPasos     => _pasos.Count;
    public string MensajePaso { get; private set; } = "";
    public bool Completado    { get; private set; }
    public bool HayError      { get; private set; }
    public string? MensajeError { get; private set; }

    public event Action? OnProgreso;

    private readonly List<(string Nombre, Func<Task> Accion)> _pasos = new();

    // ?? Constructor de pasos ?????????????????????????????????????????????????
    public void ConfigurarPasos()
    {
        _pasos.Clear();
        _pasos.Add(("Creando empresa DEMO...",             CrearEmpresaAsync));
        _pasos.Add(("Generando usuarios y roles...",       CrearUsuariosAsync));
        _pasos.Add(("Configurando almacenes...",           CrearAlmacenesAsync));
        _pasos.Add(("Registrando proveedores...",          CrearProveedoresAsync));
        _pasos.Add(("Registrando clientes...",             CrearClientesAsync));
        _pasos.Add(("Generando plan de cuentas...",        CrearPlanCuentasAsync));
        _pasos.Add(("Creando períodos contables 2025...",  CrearPeriodosAsync));
        _pasos.Add(("Creando productos de mueblería...",   CrearProductosAsync));
        _pasos.Add(("Registrando stock inicial...",        CrearStockInicialAsync));
        _pasos.Add(("Generando asientos contables demo...",CrearAsientosAsync));
    }

    public async Task EjecutarAsync()
    {
        Resultado     = new DemoSeedResult();
        PasoActual    = 0;
        Completado    = false;
        HayError      = false;
        MensajeError  = null;

        foreach (var (nombre, accion) in _pasos)
        {
            PasoActual++;
            MensajePaso = nombre;
            NotificarProgreso();

            try
            {
                await accion();
            }
            catch (Exception ex)
            {
                HayError     = true;
                MensajeError = $"Error en paso {PasoActual} ({nombre}): {ex.Message}";
                NotificarProgreso();
                return;
            }
        }

        Completado = true;
        MensajePaso = "¡Empresa DEMO creada con éxito!";
        NotificarProgreso();
    }

    private void NotificarProgreso() => OnProgreso?.Invoke();

    // ????????????????????????????????????????????????????????????????????????
    // PASO 1 — Empresa
    // ????????????????????????????????????????????????????????????????????????
    private async Task CrearEmpresaAsync()
    {
        var dto = new CreateEmpresaDto
        {
            Nombre       = "MUEBLES ANDINOS IMPORT S.R.L. [DEMO]",
            NIT          = "1023456789",
            Direccion    = "Av. Arce 2345, Zona Sur, La Paz - Bolivia",
            Telefono     = "+591 2 2789100",
            Email        = "contacto@mueblesandinos-demo.bo",
            MonedaBaseId = "BOB"
        };
        var r = await _empresaSvc.CreateAsync(dto);
        if (!r.Success) throw new Exception(r.Message);
        Resultado.EmpresaId   = r.Data!.Id;
        Resultado.EmpresaNombre = r.Data.Nombre;
    }

    // ????????????????????????????????????????????????????????????????????????
    // PASO 2 — Usuarios
    // ????????????????????????????????????????????????????????????????????????
    private async Task CrearUsuariosAsync()
    {
        var usuarios = new[]
        {
            new CreateUsuarioDto { NombreUsuario="gerente.andinos",  Email="gerente@mueblesandinos-demo.bo",   Password="Demo2025!", NombreCompleto="Carlos Mamani Quispe",    EmpresaId=Resultado.EmpresaId, Rol="Manager" },
            new CreateUsuarioDto { NombreUsuario="contador.andinos", Email="contador@mueblesandinos-demo.bo",  Password="Demo2025!", NombreCompleto="Rosa Condori Limachi",    EmpresaId=Resultado.EmpresaId, Rol="User"    },
            new CreateUsuarioDto { NombreUsuario="compras.andinos",  Email="compras@mueblesandinos-demo.bo",   Password="Demo2025!", NombreCompleto="Hugo Apaza Flores",       EmpresaId=Resultado.EmpresaId, Rol="User"    },
            new CreateUsuarioDto { NombreUsuario="almacen.andinos",  Email="almacen@mueblesandinos-demo.bo",   Password="Demo2025!", NombreCompleto="Freddy Chura Alvarez",    EmpresaId=Resultado.EmpresaId, Rol="User"    },
            new CreateUsuarioDto { NombreUsuario="ventas.andinos",   Email="ventas@mueblesandinos-demo.bo",    Password="Demo2025!", NombreCompleto="Patricia Ticona Mamani",  EmpresaId=Resultado.EmpresaId, Rol="User"    },
        };
        foreach (var u in usuarios)
        {
            var r = await _usuarioSvc.CreateAsync(u);
            if (r.Success) Resultado.Usuarios.Add(u.NombreUsuario);
        }
    }

    // ????????????????????????????????????????????????????????????????????????
    // PASO 3 — Almacenes
    // ????????????????????????????????????????????????????????????????????????
    private async Task CrearAlmacenesAsync()
    {
        var almacenes = new[]
        {
            new CreateAlmacenDto { Codigo="ALM-LP-01",  Nombre="Almacén Central La Paz",        Direccion="Av. Periférica km 2.5, El Alto",      Responsable="Freddy Chura Alvarez",    Telefono="+591 2 2780001" },
            new CreateAlmacenDto { Codigo="ALM-LP-02",  Nombre="Showroom La Paz",               Direccion="Av. Arce 2345, Zona Sur, La Paz",     Responsable="Patricia Ticona Mamani",  Telefono="+591 2 2780002" },
            new CreateAlmacenDto { Codigo="ALM-SCZ-01", Nombre="Almacén Santa Cruz",            Direccion="Parque Industrial, Zona Franca SCZ",  Responsable="Jorge Mendez Vargas",     Telefono="+591 3 3360001" },
            new CreateAlmacenDto { Codigo="ALM-TRA-01", Nombre="Almacén Tránsito / Importación",Direccion="Aduana Interior La Paz",              Responsable="Hugo Apaza Flores",       Telefono="+591 2 2780003" },
        };
        foreach (var a in almacenes)
        {
            var r = await _almacenSvc.CreateAsync(a);
            if (r.Success)
            {
                Resultado.Almacenes.Add((r.Data!.Id, a.Nombre));
            }
        }
    }

    // ????????????????????????????????????????????????????????????????????????
    // PASO 4 — Proveedores
    // ????????????????????????????????????????????????????????????????????????
    private async Task CrearProveedoresAsync()
    {
        var proveedores = new AgoraHub360.ERP.Shared.DTOs.MDM.CreateProveedorDto[]
        {
            new() { RazonSocial="FURNITURE ASIA CO. LTD.",           NIT="CN-88012345",  Telefono="+86 21 6800 1234",  Email="sales@furnitureasiacn.com",      Direccion="No.188 Fumin Rd, Guangzhou, China",          TipoProveedor="Internacional" },
            new() { RazonSocial="TEAK MASTERS INDONESIA PT.",        NIT="ID-99023456",  Telefono="+62 21 5500 9876",  Email="export@teakmastersid.com",       Direccion="Jl. Raya Industri 45, Jakarta, Indonesia",   TipoProveedor="Internacional" },
            new() { RazonSocial="EURO MOBILI S.P.A.",                NIT="IT-77034567",  Telefono="+39 02 8800 4321",  Email="export@euromobili.it",           Direccion="Via Industriale 12, Milano, Italia",          TipoProveedor="Internacional" },
            new() { RazonSocial="COMERCIAL MUEBLERÍA ANDINA S.R.L.", NIT="1098765432",   Telefono="+591 2 2441100",    Email="ventas@muebleriaandina.bo",      Direccion="Calle Mercado 345, La Paz, Bolivia",          TipoProveedor="Local"         },
            new() { RazonSocial="MADERAS Y TAPIZADOS DEL NORTE",     NIT="2087654321",   Telefono="+591 2 2552200",    Email="contacto@maderasnorte.bo",       Direccion="Av. Montes 890, La Paz, Bolivia",             TipoProveedor="Local"         },
            new() { RazonSocial="AGENCIA ADUANERA BOLIVAR LTDA.",    NIT="3076543210",   Telefono="+591 2 2663300",    Email="aduana@bolivaragencia.bo",       Direccion="Puerto Seco La Paz, El Alto, Bolivia",        TipoProveedor="Servicio"      },
        };
        foreach (var p in proveedores)
        {
            var r = await _proveedorSvc.CreateAsync(p);
            if (r.Success) Resultado.Proveedores.Add(p.RazonSocial);
        }
    }

    // ????????????????????????????????????????????????????????????????????????
    // PASO 5 — Clientes
    // ????????????????????????????????????????????????????????????????????????
    private async Task CrearClientesAsync()
    {
        var clientes = new[]
        {
            new CreateClienteDto { Codigo="CLI-001", RazonSocial="Supermercados Del Valle S.A.",      NIT="5023001234", Telefono="+591 2 2441100", Email="compras@delvalle.bo",         Direccion="Av. Mcal. Santa Cruz 1250, La Paz",   NombreContacto="Pedro Mamani",  TipoCliente="Mayorista"    },
            new CreateClienteDto { Codigo="CLI-002", RazonSocial="Hotel Los Andes S.A.",              NIT="5034002345", Telefono="+591 2 2552210", Email="adquisiciones@losandes.bo",   Direccion="Av. 6 de Agosto 520, La Paz",         NombreContacto="Laura Chávez",  TipoCliente="Corporativo"  },
            new CreateClienteDto { Codigo="CLI-003", RazonSocial="Constructora Boliviana S.R.L.",     NIT="5045003456", Telefono="+591 4 4521100", Email="proyectos@constboliviana.bo", Direccion="Av. Heroínas 340, Cochabamba",         NombreContacto="Roberto Torres",TipoCliente="Corporativo"  },
            new CreateClienteDto { Codigo="CLI-004", RazonSocial="Tiendas Casa & Hogar Ltda.",        NIT="5056004567", Telefono="+591 3 3320050", Email="compras@casahogar.bo",        Direccion="Av. San Martín 650, Santa Cruz",       NombreContacto="Ana Flores",    TipoCliente="Mayorista"    },
            new CreateClienteDto { Codigo="CLI-005", RazonSocial="Oficinas Ejecutivas del Sur",       NIT="5067005678", Telefono="+591 2 2663300", Email="equipamiento@oesur.bo",       Direccion="Av. Calacoto 7890, Zona Sur, La Paz",  NombreContacto="Marco Vega",    TipoCliente="Corporativo"  },
            new CreateClienteDto { Codigo="CLI-006", RazonSocial="Distribuidora El Roble S.R.L.",     NIT="5078006789", Telefono="+591 2 2774400", Email="pedidos@elroble.bo",          Direccion="Calle Potosí 230, La Paz",             NombreContacto="Carmen Quispe", TipoCliente="Distribuidor" },
            new CreateClienteDto { Codigo="CLI-007", RazonSocial="Universidad Privada Los Andes",    NIT="5089007890", Telefono="+591 2 2885500", Email="logistica@uplosandes.bo",     Direccion="Calle 21 de Calacoto, La Paz",         NombreContacto="Luis Condori",  TipoCliente="Corporativo"  },
            new CreateClienteDto { Codigo="CLI-008", RazonSocial="Centro Comercial Plaza Mayor",     NIT="5090008901", Telefono="+591 3 3441160", Email="mall@plazamayor.bo",          Direccion="Av. Beni Km 3, Santa Cruz",            NombreContacto="Diana Roca",    TipoCliente="Mayorista"    },
        };
        foreach (var c in clientes)
        {
            var r = await _clienteSvc.CreateAsync(c);
            if (r.Success) Resultado.Clientes.Add(c.RazonSocial);
        }
    }

    // ????????????????????????????????????????????????????????????????????????
    // PASO 6 — Plan de Cuentas (rubros + cuentas de mueblería importadora)
    // ????????????????????????????????????????????????????????????????????????
    private async Task CrearPlanCuentasAsync()
    {
        // Seed automático del catálogo estándar
        await _asientoSvc.SeedCatalogosAsync();

        // Cuentas de mayor (Tipo: 1=Activo 2=Pasivo 3=Patrimonio 4=Ingreso 5=Egreso)
        // Naturaleza: 1=Deudora 2=Acreedora
        var cuentas = new[]
        {
            // ?? ACTIVO ??????????????????????????????????????????????????????
            new CreateCuentaContableDto { Codigo="1",        Nombre="ACTIVO",                               Tipo=1, Naturaleza=1, Nivel=1, PermiteMovimientos=false },
            new CreateCuentaContableDto { Codigo="1.1",      Nombre="ACTIVO CORRIENTE",                     Tipo=1, Naturaleza=1, Nivel=2, PermiteMovimientos=false },
            new CreateCuentaContableDto { Codigo="1.1.1",    Nombre="Caja",                                 Tipo=1, Naturaleza=1, Nivel=3, PermiteMovimientos=true,  Descripcion="Efectivo en caja general" },
            new CreateCuentaContableDto { Codigo="1.1.2",    Nombre="Caja Chica",                           Tipo=1, Naturaleza=1, Nivel=3, PermiteMovimientos=true,  Descripcion="Fondo fijo de caja chica" },
            new CreateCuentaContableDto { Codigo="1.1.3",    Nombre="Banco BNB Cta. Cte. Bs.",              Tipo=1, Naturaleza=1, Nivel=3, PermiteMovimientos=true,  Descripcion="Banco Nacional de Bolivia" },
            new CreateCuentaContableDto { Codigo="1.1.4",    Nombre="Banco BCP Cta. Cte. USD",              Tipo=1, Naturaleza=1, Nivel=3, PermiteMovimientos=true,  Descripcion="Banco de Crédito del Perú Bolivia" },
            new CreateCuentaContableDto { Codigo="1.1.5",    Nombre="Cuentas por Cobrar Clientes",          Tipo=1, Naturaleza=1, Nivel=3, PermiteMovimientos=true,  Descripcion="Saldo pendiente de cobro a clientes" },
            new CreateCuentaContableDto { Codigo="1.1.6",    Nombre="Anticipo a Proveedores",               Tipo=1, Naturaleza=1, Nivel=3, PermiteMovimientos=true,  Descripcion="Anticipos pagados a proveedores por importación" },
            new CreateCuentaContableDto { Codigo="1.1.7",    Nombre="Inventario de Mercaderías",            Tipo=1, Naturaleza=1, Nivel=3, PermiteMovimientos=true,  Descripcion="Muebles y accesorios en stock" },
            new CreateCuentaContableDto { Codigo="1.1.8",    Nombre="Mercadería en Tránsito",               Tipo=1, Naturaleza=1, Nivel=3, PermiteMovimientos=true,  Descripcion="Importaciones en proceso de internación" },
            new CreateCuentaContableDto { Codigo="1.1.9",    Nombre="IVA Crédito Fiscal",                   Tipo=1, Naturaleza=1, Nivel=3, PermiteMovimientos=true,  Descripcion="IVA pagado en compras" },
            new CreateCuentaContableDto { Codigo="1.1.10",   Nombre="Gastos Pagados por Anticipado",        Tipo=1, Naturaleza=1, Nivel=3, PermiteMovimientos=true,  Descripcion="Seguros y otros prepagados" },
            new CreateCuentaContableDto { Codigo="1.2",      Nombre="ACTIVO NO CORRIENTE",                  Tipo=1, Naturaleza=1, Nivel=2, PermiteMovimientos=false },
            new CreateCuentaContableDto { Codigo="1.2.1",    Nombre="Mobiliario y Equipos de Oficina",      Tipo=1, Naturaleza=1, Nivel=3, PermiteMovimientos=true,  Descripcion="Muebles y equipos propios" },
            new CreateCuentaContableDto { Codigo="1.2.2",    Nombre="Vehículos y Transporte",               Tipo=1, Naturaleza=1, Nivel=3, PermiteMovimientos=true,  Descripcion="Camiones y vehículos de reparto" },
            new CreateCuentaContableDto { Codigo="1.2.3",    Nombre="Dep. Acumulada Mobiliario",            Tipo=1, Naturaleza=2, Nivel=3, PermiteMovimientos=true,  Descripcion="(-) Depreciación acumulada" },
            // ?? PASIVO ??????????????????????????????????????????????????????
            new CreateCuentaContableDto { Codigo="2",        Nombre="PASIVO",                               Tipo=2, Naturaleza=2, Nivel=1, PermiteMovimientos=false },
            new CreateCuentaContableDto { Codigo="2.1",      Nombre="PASIVO CORRIENTE",                     Tipo=2, Naturaleza=2, Nivel=2, PermiteMovimientos=false },
            new CreateCuentaContableDto { Codigo="2.1.1",    Nombre="Cuentas por Pagar Proveedores",        Tipo=2, Naturaleza=2, Nivel=3, PermiteMovimientos=true,  Descripcion="Deudas con proveedores nacionales e internacionales" },
            new CreateCuentaContableDto { Codigo="2.1.2",    Nombre="Gastos Aduaneros por Pagar",           Tipo=2, Naturaleza=2, Nivel=3, PermiteMovimientos=true,  Descripcion="Aranceles y tributos aduaneros pendientes" },
            new CreateCuentaContableDto { Codigo="2.1.3",    Nombre="IVA Débito Fiscal",                    Tipo=2, Naturaleza=2, Nivel=3, PermiteMovimientos=true,  Descripcion="IVA cobrado en ventas" },
            new CreateCuentaContableDto { Codigo="2.1.4",    Nombre="IT por Pagar",                         Tipo=2, Naturaleza=2, Nivel=3, PermiteMovimientos=true,  Descripcion="Impuesto a las Transacciones" },
            new CreateCuentaContableDto { Codigo="2.1.5",    Nombre="Sueldos y Salarios por Pagar",         Tipo=2, Naturaleza=2, Nivel=3, PermiteMovimientos=true,  Descripcion="Planilla mensual" },
            new CreateCuentaContableDto { Codigo="2.1.6",    Nombre="Préstamo Bancario CP",                 Tipo=2, Naturaleza=2, Nivel=3, PermiteMovimientos=true,  Descripcion="Porción corriente de préstamos" },
            new CreateCuentaContableDto { Codigo="2.2",      Nombre="PASIVO NO CORRIENTE",                  Tipo=2, Naturaleza=2, Nivel=2, PermiteMovimientos=false },
            new CreateCuentaContableDto { Codigo="2.2.1",    Nombre="Préstamo Bancario LP",                 Tipo=2, Naturaleza=2, Nivel=3, PermiteMovimientos=true,  Descripcion="Porción largo plazo de préstamos" },
            // ?? PATRIMONIO ??????????????????????????????????????????????????
            new CreateCuentaContableDto { Codigo="3",        Nombre="PATRIMONIO NETO",                      Tipo=3, Naturaleza=2, Nivel=1, PermiteMovimientos=false },
            new CreateCuentaContableDto { Codigo="3.1",      Nombre="Capital Social",                       Tipo=3, Naturaleza=2, Nivel=2, PermiteMovimientos=true,  Descripcion="Aportes de socios" },
            new CreateCuentaContableDto { Codigo="3.2",      Nombre="Resultados Acumulados",                Tipo=3, Naturaleza=2, Nivel=2, PermiteMovimientos=true,  Descripcion="Utilidades retenidas" },
            new CreateCuentaContableDto { Codigo="3.3",      Nombre="Resultado del Ejercicio",              Tipo=3, Naturaleza=2, Nivel=2, PermiteMovimientos=true,  Descripcion="Ganancia/pérdida del período" },
            // ?? INGRESOS ?????????????????????????????????????????????????????
            new CreateCuentaContableDto { Codigo="4",        Nombre="INGRESOS",                             Tipo=4, Naturaleza=2, Nivel=1, PermiteMovimientos=false },
            new CreateCuentaContableDto { Codigo="4.1",      Nombre="Ventas de Muebles",                    Tipo=4, Naturaleza=2, Nivel=2, PermiteMovimientos=true,  Descripcion="Ingresos por venta de muebles" },
            new CreateCuentaContableDto { Codigo="4.1.1",    Nombre="Ventas - Muebles de Sala",             Tipo=4, Naturaleza=2, Nivel=3, PermiteMovimientos=true  },
            new CreateCuentaContableDto { Codigo="4.1.2",    Nombre="Ventas - Muebles de Dormitorio",       Tipo=4, Naturaleza=2, Nivel=3, PermiteMovimientos=true  },
            new CreateCuentaContableDto { Codigo="4.1.3",    Nombre="Ventas - Muebles de Oficina",          Tipo=4, Naturaleza=2, Nivel=3, PermiteMovimientos=true  },
            new CreateCuentaContableDto { Codigo="4.1.4",    Nombre="Ventas - Muebles de Cocina/Comedor",   Tipo=4, Naturaleza=2, Nivel=3, PermiteMovimientos=true  },
            new CreateCuentaContableDto { Codigo="4.2",      Nombre="Otros Ingresos",                       Tipo=4, Naturaleza=2, Nivel=2, PermiteMovimientos=true,  Descripcion="Servicios de instalación, reparación, etc." },
            // ?? EGRESOS ??????????????????????????????????????????????????????
            new CreateCuentaContableDto { Codigo="5",        Nombre="EGRESOS",                              Tipo=5, Naturaleza=1, Nivel=1, PermiteMovimientos=false },
            new CreateCuentaContableDto { Codigo="5.1",      Nombre="COSTO DE VENTAS",                      Tipo=5, Naturaleza=1, Nivel=2, PermiteMovimientos=false },
            new CreateCuentaContableDto { Codigo="5.1.1",    Nombre="Costo de Mercaderías Vendidas",        Tipo=5, Naturaleza=1, Nivel=3, PermiteMovimientos=true  },
            new CreateCuentaContableDto { Codigo="5.1.2",    Nombre="Fletes y Transporte de Importación",   Tipo=5, Naturaleza=1, Nivel=3, PermiteMovimientos=true  },
            new CreateCuentaContableDto { Codigo="5.1.3",    Nombre="Aranceles y Tributos Aduaneros",       Tipo=5, Naturaleza=1, Nivel=3, PermiteMovimientos=true  },
            new CreateCuentaContableDto { Codigo="5.2",      Nombre="GASTOS OPERATIVOS",                    Tipo=5, Naturaleza=1, Nivel=2, PermiteMovimientos=false },
            new CreateCuentaContableDto { Codigo="5.2.1",    Nombre="Sueldos y Salarios",                   Tipo=5, Naturaleza=1, Nivel=3, PermiteMovimientos=true  },
            new CreateCuentaContableDto { Codigo="5.2.2",    Nombre="Alquiler de Local y Showroom",         Tipo=5, Naturaleza=1, Nivel=3, PermiteMovimientos=true  },
            new CreateCuentaContableDto { Codigo="5.2.3",    Nombre="Servicios Básicos",                    Tipo=5, Naturaleza=1, Nivel=3, PermiteMovimientos=true  },
            new CreateCuentaContableDto { Codigo="5.2.4",    Nombre="Publicidad y Marketing",               Tipo=5, Naturaleza=1, Nivel=3, PermiteMovimientos=true  },
            new CreateCuentaContableDto { Codigo="5.2.5",    Nombre="Mantenimiento y Reparaciones",         Tipo=5, Naturaleza=1, Nivel=3, PermiteMovimientos=true  },
            new CreateCuentaContableDto { Codigo="5.2.6",    Nombre="Gastos de Importación y Aduana",       Tipo=5, Naturaleza=1, Nivel=3, PermiteMovimientos=true  },
            new CreateCuentaContableDto { Codigo="5.2.7",    Nombre="Depreciación del Ejercicio",           Tipo=5, Naturaleza=1, Nivel=3, PermiteMovimientos=true  },
            new CreateCuentaContableDto { Codigo="5.3",      Nombre="GASTOS FINANCIEROS",                   Tipo=5, Naturaleza=1, Nivel=2, PermiteMovimientos=false },
            new CreateCuentaContableDto { Codigo="5.3.1",    Nombre="Intereses Bancarios",                  Tipo=5, Naturaleza=1, Nivel=3, PermiteMovimientos=true  },
            new CreateCuentaContableDto { Codigo="5.3.2",    Nombre="Comisiones Bancarias",                 Tipo=5, Naturaleza=1, Nivel=3, PermiteMovimientos=true  },
        };

        foreach (var c in cuentas)
        {
            var r = await _cuentaSvc.CreateAsync(c);
            if (r.Success) Resultado.Cuentas.Add((r.Data!.CuentaContableId, c.Codigo, c.Nombre));
        }
    }

    // ????????????????????????????????????????????????????????????????????????
    // PASO 7 — Períodos contables 2025
    // ????????????????????????????????????????????????????????????????????????
    private async Task CrearPeriodosAsync()
    {
        var r = await _periodoSvc.GenerarPeriodosAsync(2025);
        if (r.Success) Resultado.PeriodosGenerados = r.Data;
    }

    // ????????????????????????????????????????????????????????????????????????
    // PASO 8 — Productos (catálogo global + activar en empresa)
    // ????????????????????????????????????????????????????????????????????????
    private async Task CrearProductosAsync()
    {
        // Obtener catálogos disponibles para la empresa actual
        var catalogos = await _catalogoSvc.GetAllAsync();
        if (!catalogos.Any())
            throw new Exception("No hay catálogos configurados. El seed de empresa debería haberlos creado.");
        var catalogoId = catalogos.First().CatalogId;

        // Obtener UOMs — usar la primera (UND) creada por el seed
        var uoms = await _unidadMedidaSvc.GetAllAsync();
        if (!uoms.Any())
            throw new Exception("No hay unidades de medida configuradas. El seed de empresa debería haberlas creado.");
        var defaultUomId = uoms.FirstOrDefault(u => u.Code == "UND")?.UomId ?? uoms.First().UomId;

        var productos = new[]
        {
            // Sala
            new CreateProductDto2 { CatalogId=catalogoId, GenericName="Sofá 3 Cuerpos",               CommercialName="Sofá Malibu 3C",             ShortDescription="Sofá tapizado en tela importada, 3 cuerpos",        DefaultUomId=defaultUomId, IsStockable=true, IsSellable=true, IsPurchasable=true },
            new CreateProductDto2 { CatalogId=catalogoId, GenericName="Sofá 2 Cuerpos",               CommercialName="Sofá Malibu 2C",             ShortDescription="Sofá tapizado en tela importada, 2 cuerpos",        DefaultUomId=defaultUomId, IsStockable=true, IsSellable=true, IsPurchasable=true },
            new CreateProductDto2 { CatalogId=catalogoId, GenericName="Sillón Individual",            CommercialName="Sillón Malibu 1C",           ShortDescription="Sillón tapizado a juego con sofá",                  DefaultUomId=defaultUomId, IsStockable=true, IsSellable=true, IsPurchasable=true },
            new CreateProductDto2 { CatalogId=catalogoId, GenericName="Mesa de Centro",               CommercialName="Mesa Centro Roble",          ShortDescription="Mesa de centro en madera roble 120x60 cm",          DefaultUomId=defaultUomId, IsStockable=true, IsSellable=true, IsPurchasable=true },
            new CreateProductDto2 { CatalogId=catalogoId, GenericName="Mueble TV",                    CommercialName="Mueble TV Milano 160",        ShortDescription="Mueble para televisor hasta 65 pulgadas",           DefaultUomId=defaultUomId, IsStockable=true, IsSellable=true, IsPurchasable=true },
            // Dormitorio
            new CreateProductDto2 { CatalogId=catalogoId, GenericName="Cama King Size",               CommercialName="Cama Venecia King",          ShortDescription="Cama King con cabecero tapizado, incluye tarima",    DefaultUomId=defaultUomId, IsStockable=true, IsSellable=true, IsPurchasable=true },
            new CreateProductDto2 { CatalogId=catalogoId, GenericName="Cama Queen Size",              CommercialName="Cama Venecia Queen",         ShortDescription="Cama Queen con cabecero tapizado, incluye tarima",  DefaultUomId=defaultUomId, IsStockable=true, IsSellable=true, IsPurchasable=true },
            new CreateProductDto2 { CatalogId=catalogoId, GenericName="Cómoda 6 Cajones",             CommercialName="Cómoda Roma 6C",             ShortDescription="Cómoda en MDF laqueado, 6 cajones con rieles",      DefaultUomId=defaultUomId, IsStockable=true, IsSellable=true, IsPurchasable=true },
            new CreateProductDto2 { CatalogId=catalogoId, GenericName="Ropero 4 Puertas",             CommercialName="Ropero Classic 4P",          ShortDescription="Armario con espejos y organizador interior",        DefaultUomId=defaultUomId, IsStockable=true, IsSellable=true, IsPurchasable=true },
            new CreateProductDto2 { CatalogId=catalogoId, GenericName="Mesa de Noche",                CommercialName="Nochero Roma",               ShortDescription="Mesa de noche a juego con dormitorio Roma",         DefaultUomId=defaultUomId, IsStockable=true, IsSellable=true, IsPurchasable=true },
            // Comedor
            new CreateProductDto2 { CatalogId=catalogoId, GenericName="Mesa Comedor 6 Personas",      CommercialName="Mesa Comedor Teka 6P",        ShortDescription="Mesa de teca sólida 160x90 cm, 6 personas",         DefaultUomId=defaultUomId, IsStockable=true, IsSellable=true, IsPurchasable=true },
            new CreateProductDto2 { CatalogId=catalogoId, GenericName="Silla Comedor",                CommercialName="Silla Teka Tapizada",        ShortDescription="Silla con asiento tapizado, pata de teka",          DefaultUomId=defaultUomId, IsStockable=true, IsSellable=true, IsPurchasable=true },
            new CreateProductDto2 { CatalogId=catalogoId, GenericName="Aparador / Buffet",            CommercialName="Buffet Colonial 3P",         ShortDescription="Aparador colonial 3 puertas madera sólida",         DefaultUomId=defaultUomId, IsStockable=true, IsSellable=true, IsPurchasable=true },
            // Oficina
            new CreateProductDto2 { CatalogId=catalogoId, GenericName="Escritorio Ejecutivo",         CommercialName="Escritorio Exec. Pro",       ShortDescription="Escritorio en L con cajones y porta CPU",           DefaultUomId=defaultUomId, IsStockable=true, IsSellable=true, IsPurchasable=true },
            new CreateProductDto2 { CatalogId=catalogoId, GenericName="Silla Ergonómica Ejecutiva",   CommercialName="Silla Ergo Elite",           ShortDescription="Silla con soporte lumbar y apoyabrazos ajustables",  DefaultUomId=defaultUomId, IsStockable=true, IsSellable=true, IsPurchasable=true },
            new CreateProductDto2 { CatalogId=catalogoId, GenericName="Estantería Modular",           CommercialName="Estante Modular 5N",         ShortDescription="Estantería 5 niveles, armable, carga 50kg/nivel",   DefaultUomId=defaultUomId, IsStockable=true, IsSellable=true, IsPurchasable=true },
            new CreateProductDto2 { CatalogId=catalogoId, GenericName="Mesa de Reuniones 8 Pers.",    CommercialName="Mesa Reunión Oval 8P",       ShortDescription="Mesa ovalada para sala de reuniones",               DefaultUomId=defaultUomId, IsStockable=true, IsSellable=true, IsPurchasable=true },
            // Accesorios
            new CreateProductDto2 { CatalogId=catalogoId, GenericName="Espejo Decorativo",            CommercialName="Espejo Marco Dorado",        ShortDescription="Espejo con marco decorativo 80x120 cm",             DefaultUomId=defaultUomId, IsStockable=true, IsSellable=true, IsPurchasable=true },
            new CreateProductDto2 { CatalogId=catalogoId, GenericName="Alfombra Importada",           CommercialName="Alfombra Persa 200x300",     ShortDescription="Alfombra 100% lana, diseño persa 200x300 cm",       DefaultUomId=defaultUomId, IsStockable=true, IsSellable=true, IsPurchasable=true },
            new CreateProductDto2 { CatalogId=catalogoId, GenericName="Lámpara de Pie",               CommercialName="Lámpara Arc Premium",        ShortDescription="Lámpara de pie arco, base mármol, altura 180 cm",   DefaultUomId=defaultUomId, IsStockable=true, IsSellable=true, IsPurchasable=true },
        };

        foreach (var p in productos)
        {
            var rProd = await _productoGlobalSvc.CreateAsync(p);
            if (!rProd.Success) continue;
            var productId = rProd.Data!.ProductId;

            // Activar en la empresa con SKU y precio
            var sku = "MUA-" + rProd.Data.GenericName.ToUpper().Replace(" ", "").Substring(0, Math.Min(6, rProd.Data.GenericName.Length));
            var rComp = await _companyProductSvc.CreateAsync(new CreateCompanyProductDto(
                ProductId: productId,
                Sku: sku,
                CodigoInterno: sku,
                IsVisiblePOS: true,
                IsVisibleEcommerce: false,
                IsVisibleB2B: true,
                AllowReturns: true,
                MinStock: 2,
                MaxStock: 50,
                ReorderPoint: 5
            ));
            if (rComp.Success)
                Resultado.Productos.Add((rComp.Data!.CompanyProductId, p.CommercialName, sku));
        }
    }

    // ????????????????????????????????????????????????????????????????????????
    // PASO 9 — Stock inicial (entradas de inventario)
    // ????????????????????????????????????????????????????????????????????????
    private async Task CrearStockInicialAsync()
    {
        if (!Resultado.Productos.Any() || !Resultado.Almacenes.Any()) return;

        var almacenCentralId = Resultado.Almacenes.FirstOrDefault().Id;
        if (almacenCentralId == 0) return;

        // Datos de stock por producto (cantidades y costos representativos)
        var stockData = new[]
        {
            (0,  12m, 2_850m,  "Importación China FAS-2025-001"),
            (1,  15m, 2_200m,  "Importación China FAS-2025-001"),
            (2,  20m, 1_450m,  "Importación China FAS-2025-001"),
            (3,  18m,   780m,  "Importación China FAS-2025-001"),
            (4,  14m,   920m,  "Importación China FAS-2025-001"),
            (5,   8m, 4_100m,  "Importación Indonesia TM-2025-002"),
            (6,  10m, 3_200m,  "Importación Indonesia TM-2025-002"),
            (7,  16m, 1_100m,  "Importación Indonesia TM-2025-002"),
            (8,   6m, 3_800m,  "Importación Indonesia TM-2025-002"),
            (9,  22m,   620m,  "Importación Indonesia TM-2025-002"),
            (10,  5m, 5_200m,  "Importación Italia EM-2025-003"),
            (11, 30m,   480m,  "Importación Italia EM-2025-003"),
            (12,  4m, 2_900m,  "Importación Italia EM-2025-003"),
            (13,  7m, 3_400m,  "Importación Italia EM-2025-003"),
            (14, 18m, 1_250m,  "Importación Italia EM-2025-003"),
            (15, 10m,   680m,  "Compra Local CMAN-2025-004"),
            (16,  3m, 6_100m,  "Compra Local CMAN-2025-004"),
            (17, 12m,   590m,  "Compra Local CMAN-2025-004"),
            (18,  8m, 1_380m,  "Compra Local CMAN-2025-004"),
            (19,  6m, 1_050m,  "Compra Local CMAN-2025-004"),
        };

        foreach (var (idx, qty, cost, refTxt) in stockData)
        {
            if (idx >= Resultado.Productos.Count) continue;
            var compProdId = Resultado.Productos[idx].Id;

            await _movimientoSvc.CreateAsync(new CreateMovimientoInventarioDto
            {
                MovementType     = "Receipt",
                MovementDate     = new DateTime(2025, 1, 15),
                CompanyProductId = compProdId,
                WarehouseId      = almacenCentralId,
                Quantity         = qty,
                UnitCost         = cost,
                Reference        = refTxt,
                Notes            = "Stock inicial DEMO"
            });
            Resultado.MovimientosStock++;
        }
    }

    // ????????????????????????????????????????????????????????????????????????
    // PASO 10 — Asientos contables demo
    // ????????????????????????????????????????????????????????????????????????
    private async Task CrearAsientosAsync()
    {
        var tiposComp = await _asientoSvc.GetTiposComprobanteAsync();
        var tcIngreso = tiposComp.FirstOrDefault(t => t.Codigo == "CI" || t.Codigo == "IN" || t.Nombre.Contains("Ingreso"))?.TipoComprobanteId ?? 1;
        var tcEgreso  = tiposComp.FirstOrDefault(t => t.Codigo == "CE" || t.Codigo == "EG" || t.Nombre.Contains("Egreso"))?.TipoComprobanteId  ?? 2;
        var tcGeneral = tiposComp.FirstOrDefault(t => t.Codigo == "CG" || t.Codigo == "GE" || t.Nombre.Contains("General"))?.TipoComprobanteId ?? (tiposComp.FirstOrDefault()?.TipoComprobanteId ?? 1);

        // Helper para obtener ID de cuenta por código
        int Cta(string codigo) =>
            Resultado.Cuentas.FirstOrDefault(c => c.Codigo == codigo).Id;

        var asientos = new[]
        {
            // 1 — Apertura / Capital inicial
            new CreateAsientoContableDto
            {
                TipoComprobanteId = tcGeneral,
                Fecha    = new DateTime(2025, 1, 2),
                Glosa    = "Apertura de gestión — Aporte de capital social",
                Concepto = "Capital inicial socios",
                Lineas   = new()
                {
                    new CreateAsientoLineaDto { CuentaContableId=Cta("1.1.3"), Debe=850_000m, Glosa="Banco BNB - aporte socios" },
                    new CreateAsientoLineaDto { CuentaContableId=Cta("1.1.4"), Debe=150_000m, Glosa="Banco BCP USD - aporte socios" },
                    new CreateAsientoLineaDto { CuentaContableId=Cta("3.1"),   Haber=1_000_000m, Glosa="Capital Social Muebles Andinos" },
                }
            },
            // 2 — Compra mercadería importada (anticipo)
            new CreateAsientoContableDto
            {
                TipoComprobanteId = tcEgreso,
                Fecha    = new DateTime(2025, 1, 10),
                Glosa    = "Anticipo 40% a FURNITURE ASIA CO. LTD. — OC-2025-001",
                Concepto = "Importación muebles sala y dormitorio",
                Lineas   = new()
                {
                    new CreateAsientoLineaDto { CuentaContableId=Cta("1.1.6"), Debe=120_000m, Glosa="Anticipo proveedor China 40%" },
                    new CreateAsientoLineaDto { CuentaContableId=Cta("1.1.4"), Haber=120_000m, Glosa="Pago BCP USD" },
                }
            },
            // 3 — Recepción mercadería + costos importación
            new CreateAsientoContableDto
            {
                TipoComprobanteId = tcIngreso,
                Fecha    = new DateTime(2025, 1, 20),
                Glosa    = "Recepción mercadería OC-2025-001 + liquidación importación",
                Concepto = "Ingreso al inventario — muebles sala y dormitorio",
                Lineas   = new()
                {
                    new CreateAsientoLineaDto { CuentaContableId=Cta("1.1.7"), Debe=320_000m, Glosa="Ingreso inventario muebles" },
                    new CreateAsientoLineaDto { CuentaContableId=Cta("1.1.6"), Haber=120_000m, Glosa="Aplicación anticipo" },
                    new CreateAsientoLineaDto { CuentaContableId=Cta("2.1.1"), Haber=180_000m, Glosa="Saldo OC por pagar proveedor" },
                    new CreateAsientoLineaDto { CuentaContableId=Cta("5.1.2"), Debe=18_500m,   Glosa="Flete marítimo + terrestre" },
                    new CreateAsientoLineaDto { CuentaContableId=Cta("5.1.3"), Debe=24_200m,   Glosa="Aranceles y tributos GAB" },
                    new CreateAsientoLineaDto { CuentaContableId=Cta("2.1.2"), Haber=24_200m,  Glosa="Obligación aduanera" },
                    new CreateAsientoLineaDto { CuentaContableId=Cta("2.1.1"), Haber=18_500m,  Glosa="Flete por pagar - Agencia Bolívar" },
                }
            },
            // 4 — Pago planilla enero
            new CreateAsientoContableDto
            {
                TipoComprobanteId = tcEgreso,
                Fecha    = new DateTime(2025, 1, 31),
                Glosa    = "Planilla sueldos enero 2025",
                Concepto = "Remuneraciones personal",
                Lineas   = new()
                {
                    new CreateAsientoLineaDto { CuentaContableId=Cta("5.2.1"), Debe=45_000m,  Glosa="Sueldos brutos enero" },
                    new CreateAsientoLineaDto { CuentaContableId=Cta("1.1.3"), Haber=38_250m, Glosa="Pago neto vía banco BNB" },
                    new CreateAsientoLineaDto { CuentaContableId=Cta("2.1.5"), Haber=6_750m,  Glosa="Retenciones AFP y otros" },
                }
            },
            // 5 — Venta al Hotel Los Andes
            new CreateAsientoContableDto
            {
                TipoComprobanteId = tcIngreso,
                Fecha    = new DateTime(2025, 2, 5),
                Glosa    = "Venta FAC-2025-0001 — Hotel Los Andes S.A.",
                Concepto = "Equipamiento habitaciones 101-120",
                Lineas   = new()
                {
                    new CreateAsientoLineaDto { CuentaContableId=Cta("1.1.5"), Debe=186_200m,  Glosa="CxC Hotel Los Andes FAC-2025-0001" },
                    new CreateAsientoLineaDto { CuentaContableId=Cta("4.1.2"), Haber=160_690m, Glosa="Venta muebles dormitorio" },
                    new CreateAsientoLineaDto { CuentaContableId=Cta("2.1.3"), Haber=20_890m,  Glosa="IVA Débito Fiscal 13%" },
                    new CreateAsientoLineaDto { CuentaContableId=Cta("2.1.4"), Haber=4_620m,   Glosa="IT 3%" },
                }
            },
            // 6 — Costo de venta correspondiente
            new CreateAsientoContableDto
            {
                TipoComprobanteId = tcEgreso,
                Fecha    = new DateTime(2025, 2, 5),
                Glosa    = "Costo de venta FAC-2025-0001 — Hotel Los Andes",
                Concepto = "Descargo inventario muebles dormitorio",
                Lineas   = new()
                {
                    new CreateAsientoLineaDto { CuentaContableId=Cta("5.1.1"), Debe=112_000m,  Glosa="CMV muebles dormitorio vendidos" },
                    new CreateAsientoLineaDto { CuentaContableId=Cta("1.1.7"), Haber=112_000m, Glosa="Descargo inventario" },
                }
            },
            // 7 — Cobro parcial
            new CreateAsientoContableDto
            {
                TipoComprobanteId = tcIngreso,
                Fecha    = new DateTime(2025, 2, 15),
                Glosa    = "Cobro parcial 50% FAC-2025-0001 — Hotel Los Andes",
                Concepto = "Transferencia bancaria 50%",
                Lineas   = new()
                {
                    new CreateAsientoLineaDto { CuentaContableId=Cta("1.1.3"), Debe=93_100m,  Glosa="Depósito BNB — Hotel Los Andes" },
                    new CreateAsientoLineaDto { CuentaContableId=Cta("1.1.5"), Haber=93_100m, Glosa="Aplicación CxC parcial" },
                }
            },
            // 8 — Venta Constructora Boliviana (muebles oficina)
            new CreateAsientoContableDto
            {
                TipoComprobanteId = tcIngreso,
                Fecha    = new DateTime(2025, 2, 20),
                Glosa    = "Venta FAC-2025-0002 — Constructora Boliviana S.R.L.",
                Concepto = "Equipamiento oficinas proyecto Torre Norte",
                Lineas   = new()
                {
                    new CreateAsientoLineaDto { CuentaContableId=Cta("1.1.5"), Debe=248_600m,  Glosa="CxC Constructora Boliviana FAC-2025-0002" },
                    new CreateAsientoLineaDto { CuentaContableId=Cta("4.1.3"), Haber=214_690m, Glosa="Venta muebles oficina" },
                    new CreateAsientoLineaDto { CuentaContableId=Cta("2.1.3"), Haber=27_910m,  Glosa="IVA Débito Fiscal" },
                    new CreateAsientoLineaDto { CuentaContableId=Cta("2.1.4"), Haber=6_000m,   Glosa="IT 3%" },
                }
            },
            // 9 — Alquiler showroom febrero
            new CreateAsientoContableDto
            {
                TipoComprobanteId = tcEgreso,
                Fecha    = new DateTime(2025, 2, 28),
                Glosa    = "Alquiler showroom y oficinas — Febrero 2025",
                Concepto = "Pago mensual arrendamiento",
                Lineas   = new()
                {
                    new CreateAsientoLineaDto { CuentaContableId=Cta("5.2.2"), Debe=18_000m,  Glosa="Arrendamiento Av. Arce 2345" },
                    new CreateAsientoLineaDto { CuentaContableId=Cta("1.1.3"), Haber=18_000m, Glosa="Pago banco BNB" },
                }
            },
            // 10 — Segunda importación
            new CreateAsientoContableDto
            {
                TipoComprobanteId = tcEgreso,
                Fecha    = new DateTime(2025, 3, 5),
                Glosa    = "OC-2025-002 TEAK MASTERS — Muebles comedor y accesorios",
                Concepto = "Anticipo 30% importación Indonesia",
                Lineas   = new()
                {
                    new CreateAsientoLineaDto { CuentaContableId=Cta("1.1.6"), Debe=85_000m,  Glosa="Anticipo 30% TM Indonesia" },
                    new CreateAsientoLineaDto { CuentaContableId=Cta("1.1.4"), Haber=85_000m, Glosa="Transferencia BCP USD" },
                }
            },
        };

        foreach (var a in asientos)
        {
            // Solo crear si tenemos cuentas válidas (IDs > 0)
            if (a.Lineas.Any(l => l.CuentaContableId == 0)) continue;

            var r = await _asientoSvc.CreateAsync(a);
            if (r.Success)
            {
                Resultado.Asientos.Add(r.Data!.Numero);
                // Contabilizar inmediatamente
                await _asientoSvc.ContabilizarAsync(r.Data.AsientoContableId);
            }
        }
    }
}

// ?? Resultado de la siembra ???????????????????????????????????????????????????
public class DemoSeedResult
{
    public int    EmpresaId        { get; set; }
    public string EmpresaNombre    { get; set; } = "";
    public List<string> Usuarios   { get; set; } = new();
    public List<(int Id, string Nombre)> Almacenes { get; set; } = new();
    public List<string> Proveedores { get; set; } = new();
    public List<string> Clientes   { get; set; } = new();
    public List<(int Id, string Codigo, string Nombre)> Cuentas { get; set; } = new();
    public int    PeriodosGenerados { get; set; }
    public List<(long Id, string Nombre, string Sku)> Productos { get; set; } = new();
    public int    MovimientosStock  { get; set; }
    public List<string> Asientos   { get; set; } = new();
}
