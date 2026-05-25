namespace AgoraHub360.ERP.Persistence.Configurations.WF;

using AgoraHub360.ERP.Domain.Entities.Workflow;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>
/// Configuración EF Core de PlantillaTarea (schema [wf]).
/// Incluye seed de plantillas globales (EmpresaId = null) para todos los tipos de documento.
/// </summary>
public class PlantillaTareaConfiguration : IEntityTypeConfiguration<PlantillaTarea>
{
    public void Configure(EntityTypeBuilder<PlantillaTarea> builder)
    {
        // ?? Tabla y PK ????????????????????????????????????????????????????????
        builder.ToTable("PlantillasTareas", "wf");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).UseIdentityColumn();

        // ?? Propiedades ???????????????????????????????????????????????????????
        builder.Property(p => p.EntityType)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(p => p.SubTipo)
            .HasMaxLength(50);

        builder.Property(p => p.Codigo)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(p => p.Descripcion)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.EsAutomatico)
            .HasDefaultValue(false);

        builder.Property(p => p.RolResponsable)
            .HasMaxLength(50);

        // Auditoría
        builder.Property(p => p.CreadoPor).HasMaxLength(120);
        builder.Property(p => p.ModificadoPor).HasMaxLength(120);

        // ?? Relaciones ????????????????????????????????????????????????????????
        builder.HasOne(p => p.Empresa)
            .WithMany()
            .HasForeignKey(p => p.EmpresaId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        // ?? Índices ???????????????????????????????????????????????????????????
        builder.HasIndex(p => new { p.EntityType, p.SubTipo, p.EmpresaId, p.Orden })
            .HasDatabaseName("IX_Plantillas_Query");

        // ?? Seed de plantillas globales ???????????????????????????????????????
        var fecha = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        builder.HasData(

            // ?? OrdenPedido / IMPORTACION ?????????????????????????????????????
            new PlantillaTarea { Id =  1, EntityType = "OrdenPedido", SubTipo = "IMPORTACION", Orden =  1, Codigo = "OP-CREADA",     Descripcion = "Orden de pedido creada en el sistema",          EsAutomatico = true,  RolResponsable = null,            EmpresaId = null, Activo = true, FechaCreacion = fecha },
            new PlantillaTarea { Id =  2, EntityType = "OrdenPedido", SubTipo = "IMPORTACION", Orden =  2, Codigo = "OC-EMITIDA",    Descripcion = "Orden de compra emitida al proveedor",           EsAutomatico = false, RolResponsable = "COMPRAS",       EmpresaId = null, Activo = true, FechaCreacion = fecha },
            new PlantillaTarea { Id =  3, EntityType = "OrdenPedido", SubTipo = "IMPORTACION", Orden =  3, Codigo = "ETD-CONF",      Descripcion = "ETD confirmado por proveedor / naviera",         EsAutomatico = false, RolResponsable = "COMPRAS",       EmpresaId = null, Activo = true, FechaCreacion = fecha },
            new PlantillaTarea { Id =  4, EntityType = "OrdenPedido", SubTipo = "IMPORTACION", Orden =  4, Codigo = "EMBARQUE",      Descripcion = "Embarque despachado (BL / AWB emitido)",          EsAutomatico = false, RolResponsable = "COMPRAS",       EmpresaId = null, Activo = true, FechaCreacion = fecha },
            new PlantillaTarea { Id =  5, EntityType = "OrdenPedido", SubTipo = "IMPORTACION", Orden =  5, Codigo = "ETA-CONF",      Descripcion = "ETA confirmado por forwarder / naviera",         EsAutomatico = false, RolResponsable = "COMPRAS",       EmpresaId = null, Activo = true, FechaCreacion = fecha },
            new PlantillaTarea { Id =  6, EntityType = "OrdenPedido", SubTipo = "IMPORTACION", Orden =  6, Codigo = "EN-ADUANA",     Descripcion = "Documentos presentados ante aduana",             EsAutomatico = false, RolResponsable = "AGENTE_ADUANA", EmpresaId = null, Activo = true, FechaCreacion = fecha },
            new PlantillaTarea { Id =  7, EntityType = "OrdenPedido", SubTipo = "IMPORTACION", Orden =  7, Codigo = "AFORO",         Descripcion = "Aforo / inspección aduanera realizada",          EsAutomatico = false, RolResponsable = "AGENTE_ADUANA", EmpresaId = null, Activo = true, FechaCreacion = fecha },
            new PlantillaTarea { Id =  8, EntityType = "OrdenPedido", SubTipo = "IMPORTACION", Orden =  8, Codigo = "DUI",           Descripcion = "DUI / DIM registrado y aprobado",                EsAutomatico = false, RolResponsable = "AGENTE_ADUANA", EmpresaId = null, Activo = true, FechaCreacion = fecha },
            new PlantillaTarea { Id =  9, EntityType = "OrdenPedido", SubTipo = "IMPORTACION", Orden =  9, Codigo = "LEVANTE",       Descripcion = "Levante de mercancía autorizado",                EsAutomatico = false, RolResponsable = "AGENTE_ADUANA", EmpresaId = null, Activo = true, FechaCreacion = fecha },
            new PlantillaTarea { Id = 10, EntityType = "OrdenPedido", SubTipo = "IMPORTACION", Orden = 10, Codigo = "TRANSPORTE",    Descripcion = "Transporte interno hacia almacén destino",       EsAutomatico = false, RolResponsable = "LOGISTICA",     EmpresaId = null, Activo = true, FechaCreacion = fecha },
            new PlantillaTarea { Id = 11, EntityType = "OrdenPedido", SubTipo = "IMPORTACION", Orden = 11, Codigo = "RECEPCION",     Descripcion = "Recepción física confirmada en almacén",         EsAutomatico = false, RolResponsable = "ALMACEN",       EmpresaId = null, Activo = true, FechaCreacion = fecha },
            new PlantillaTarea { Id = 12, EntityType = "OrdenPedido", SubTipo = "IMPORTACION", Orden = 12, Codigo = "LANDED-COST",   Descripcion = "Costo de importación calculado y contabilizado", EsAutomatico = false, RolResponsable = "FINANZAS",      EmpresaId = null, Activo = true, FechaCreacion = fecha },
            new PlantillaTarea { Id = 13, EntityType = "OrdenPedido", SubTipo = "IMPORTACION", Orden = 13, Codigo = "CIERRE",        Descripcion = "Expediente de importación cerrado",              EsAutomatico = true,  RolResponsable = null,            EmpresaId = null, Activo = true, FechaCreacion = fecha },

            // ?? OrdenPedido / TRASPASO_INTERNO ????????????????????????????????
            new PlantillaTarea { Id = 14, EntityType = "OrdenPedido", SubTipo = "TRASPASO_INTERNO", Orden = 1, Codigo = "OP-CREADA",    Descripcion = "Orden de pedido de traspaso creada",              EsAutomatico = true,  RolResponsable = null,         EmpresaId = null, Activo = true, FechaCreacion = fecha },
            new PlantillaTarea { Id = 15, EntityType = "OrdenPedido", SubTipo = "TRASPASO_INTERNO", Orden = 2, Codigo = "APROBACION",   Descripcion = "Traspaso aprobado por supervisor",                EsAutomatico = false, RolResponsable = "SUPERVISOR", EmpresaId = null, Activo = true, FechaCreacion = fecha },
            new PlantillaTarea { Id = 16, EntityType = "OrdenPedido", SubTipo = "TRASPASO_INTERNO", Orden = 3, Codigo = "PREPARACION",  Descripcion = "Mercancía preparada y verificada en origen",      EsAutomatico = false, RolResponsable = "ALMACEN",    EmpresaId = null, Activo = true, FechaCreacion = fecha },
            new PlantillaTarea { Id = 17, EntityType = "OrdenPedido", SubTipo = "TRASPASO_INTERNO", Orden = 4, Codigo = "DESPACHO",     Descripcion = "Mercancía despachada desde almacén origen",       EsAutomatico = false, RolResponsable = "LOGISTICA",  EmpresaId = null, Activo = true, FechaCreacion = fecha },
            new PlantillaTarea { Id = 18, EntityType = "OrdenPedido", SubTipo = "TRASPASO_INTERNO", Orden = 5, Codigo = "TRANSITO",     Descripcion = "Mercancía en tránsito hacia almacén destino",     EsAutomatico = false, RolResponsable = "LOGISTICA",  EmpresaId = null, Activo = true, FechaCreacion = fecha },
            new PlantillaTarea { Id = 19, EntityType = "OrdenPedido", SubTipo = "TRASPASO_INTERNO", Orden = 6, Codigo = "RECEPCION",    Descripcion = "Recepción confirmada en almacén destino",         EsAutomatico = false, RolResponsable = "ALMACEN",    EmpresaId = null, Activo = true, FechaCreacion = fecha },
            new PlantillaTarea { Id = 20, EntityType = "OrdenPedido", SubTipo = "TRASPASO_INTERNO", Orden = 7, Codigo = "CONFIRMACION", Descripcion = "Traspaso confirmado y diferencias registradas",   EsAutomatico = false, RolResponsable = "SUPERVISOR", EmpresaId = null, Activo = true, FechaCreacion = fecha },
            new PlantillaTarea { Id = 21, EntityType = "OrdenPedido", SubTipo = "TRASPASO_INTERNO", Orden = 8, Codigo = "CIERRE",       Descripcion = "Traspaso cerrado y stock actualizado",            EsAutomatico = true,  RolResponsable = null,         EmpresaId = null, Activo = true, FechaCreacion = fecha },

            // ?? OrdenCompra / (todos los sub-tipos) ???????????????????????????
            new PlantillaTarea { Id = 22, EntityType = "OrdenCompra", SubTipo = null, Orden =  1, Codigo = "OC-CREADA",          Descripcion = "Orden de compra generada en el sistema",          EsAutomatico = true,  RolResponsable = null,         EmpresaId = null, Activo = true, FechaCreacion = fecha },
            new PlantillaTarea { Id = 23, EntityType = "OrdenCompra", SubTipo = null, Orden =  2, Codigo = "APROBACION-L1",      Descripcion = "Aprobación de primer nivel (supervisor)",         EsAutomatico = false, RolResponsable = "SUPERVISOR", EmpresaId = null, Activo = true, FechaCreacion = fecha },
            new PlantillaTarea { Id = 24, EntityType = "OrdenCompra", SubTipo = null, Orden =  3, Codigo = "APROBACION-L2",      Descripcion = "Aprobación de segundo nivel (gerencia)",          EsAutomatico = false, RolResponsable = "GERENCIA",   EmpresaId = null, Activo = true, FechaCreacion = fecha },
            new PlantillaTarea { Id = 25, EntityType = "OrdenCompra", SubTipo = null, Orden =  4, Codigo = "ENVIADA-PROVEEDOR",  Descripcion = "Orden enviada al proveedor",                      EsAutomatico = false, RolResponsable = "COMPRAS",    EmpresaId = null, Activo = true, FechaCreacion = fecha },
            new PlantillaTarea { Id = 26, EntityType = "OrdenCompra", SubTipo = null, Orden =  5, Codigo = "CONF-PROVEEDOR",     Descripcion = "Orden confirmada por el proveedor",               EsAutomatico = false, RolResponsable = "COMPRAS",    EmpresaId = null, Activo = true, FechaCreacion = fecha },
            new PlantillaTarea { Id = 27, EntityType = "OrdenCompra", SubTipo = null, Orden =  6, Codigo = "PRODUCCION",         Descripcion = "Mercancía en producción / preparación",           EsAutomatico = false, RolResponsable = "COMPRAS",    EmpresaId = null, Activo = true, FechaCreacion = fecha },
            new PlantillaTarea { Id = 28, EntityType = "OrdenCompra", SubTipo = null, Orden =  7, Codigo = "ETD",                Descripcion = "Fecha estimada de despacho confirmada",           EsAutomatico = false, RolResponsable = "COMPRAS",    EmpresaId = null, Activo = true, FechaCreacion = fecha },
            new PlantillaTarea { Id = 29, EntityType = "OrdenCompra", SubTipo = null, Orden =  8, Codigo = "EMBARQUE",           Descripcion = "Embarque efectuado (BL / AWB recibido)",          EsAutomatico = false, RolResponsable = "COMPRAS",    EmpresaId = null, Activo = true, FechaCreacion = fecha },
            new PlantillaTarea { Id = 30, EntityType = "OrdenCompra", SubTipo = null, Orden =  9, Codigo = "RECEPCION",          Descripcion = "Mercancía recepcionada en almacén",               EsAutomatico = false, RolResponsable = "ALMACEN",    EmpresaId = null, Activo = true, FechaCreacion = fecha },
            new PlantillaTarea { Id = 31, EntityType = "OrdenCompra", SubTipo = null, Orden = 10, Codigo = "PAGADA",             Descripcion = "Pago al proveedor registrado y confirmado",       EsAutomatico = false, RolResponsable = "FINANZAS",   EmpresaId = null, Activo = true, FechaCreacion = fecha },

            // ?? OrdenVenta / (todos los sub-tipos) ????????????????????????????
            new PlantillaTarea { Id = 32, EntityType = "OrdenVenta", SubTipo = null, Orden = 1, Codigo = "OV-CREADA",     Descripcion = "Orden de venta registrada en el sistema",           EsAutomatico = true,  RolResponsable = null,         EmpresaId = null, Activo = true, FechaCreacion = fecha },
            new PlantillaTarea { Id = 33, EntityType = "OrdenVenta", SubTipo = null, Orden = 2, Codigo = "APROBACION",    Descripcion = "Orden de venta aprobada por supervisor",            EsAutomatico = false, RolResponsable = "SUPERVISOR", EmpresaId = null, Activo = true, FechaCreacion = fecha },
            new PlantillaTarea { Id = 34, EntityType = "OrdenVenta", SubTipo = null, Orden = 3, Codigo = "RESERVA-STOCK", Descripcion = "Stock reservado para la orden de venta",            EsAutomatico = false, RolResponsable = "ALMACEN",    EmpresaId = null, Activo = true, FechaCreacion = fecha },
            new PlantillaTarea { Id = 35, EntityType = "OrdenVenta", SubTipo = null, Orden = 4, Codigo = "PREPARACION",   Descripcion = "Pedido preparado y empacado en almacén",            EsAutomatico = false, RolResponsable = "ALMACEN",    EmpresaId = null, Activo = true, FechaCreacion = fecha },
            new PlantillaTarea { Id = 36, EntityType = "OrdenVenta", SubTipo = null, Orden = 5, Codigo = "FACTURADA",     Descripcion = "Factura emitida y registrada en contabilidad",      EsAutomatico = false, RolResponsable = "FINANZAS",   EmpresaId = null, Activo = true, FechaCreacion = fecha },
            new PlantillaTarea { Id = 37, EntityType = "OrdenVenta", SubTipo = null, Orden = 6, Codigo = "DESPACHO",      Descripcion = "Mercancía despachada hacia el cliente",             EsAutomatico = false, RolResponsable = "LOGISTICA",  EmpresaId = null, Activo = true, FechaCreacion = fecha },
            new PlantillaTarea { Id = 38, EntityType = "OrdenVenta", SubTipo = null, Orden = 7, Codigo = "ENTREGADA",     Descripcion = "Entrega al cliente confirmada con firma",           EsAutomatico = false, RolResponsable = "LOGISTICA",  EmpresaId = null, Activo = true, FechaCreacion = fecha },
            new PlantillaTarea { Id = 39, EntityType = "OrdenVenta", SubTipo = null, Orden = 8, Codigo = "COBRADA",       Descripcion = "Pago del cliente recibido y conciliado",            EsAutomatico = false, RolResponsable = "FINANZAS",   EmpresaId = null, Activo = true, FechaCreacion = fecha },

            // ?? HojaRuta / IMPORTACION ????????????????????????????????????????
            new PlantillaTarea { Id = 40, EntityType = "HojaRuta", SubTipo = "IMPORTACION",    Orden =  1, Codigo = "HR-CREADA",    Descripcion = "Hoja de ruta creada en el sistema",              EsAutomatico = true,  RolResponsable = null,            EmpresaId = null, Activo = true, FechaCreacion = fecha },
            new PlantillaTarea { Id = 41, EntityType = "HojaRuta", SubTipo = "IMPORTACION",    Orden =  2, Codigo = "DESPACHO-EXT", Descripcion = "Despacho en origen confirmado",                  EsAutomatico = false, RolResponsable = "COMPRAS",       EmpresaId = null, Activo = true, FechaCreacion = fecha },
            new PlantillaTarea { Id = 42, EntityType = "HojaRuta", SubTipo = "IMPORTACION",    Orden =  3, Codigo = "EN-TRANSITO",  Descripcion = "Carga en tránsito internacional",                EsAutomatico = false, RolResponsable = "COMPRAS",       EmpresaId = null, Activo = true, FechaCreacion = fecha },
            new PlantillaTarea { Id = 43, EntityType = "HojaRuta", SubTipo = "IMPORTACION",    Orden =  4, Codigo = "ADUANA",       Descripcion = "Trámite aduanero iniciado",                      EsAutomatico = false, RolResponsable = "AGENTE_ADUANA", EmpresaId = null, Activo = true, FechaCreacion = fecha },
            new PlantillaTarea { Id = 44, EntityType = "HojaRuta", SubTipo = "IMPORTACION",    Orden =  5, Codigo = "LEVANTE",      Descripcion = "Levante de aduana autorizado",                   EsAutomatico = false, RolResponsable = "AGENTE_ADUANA", EmpresaId = null, Activo = true, FechaCreacion = fecha },
            new PlantillaTarea { Id = 45, EntityType = "HojaRuta", SubTipo = "IMPORTACION",    Orden =  6, Codigo = "TRANSPORTE",   Descripcion = "Transporte hacia almacén destino",               EsAutomatico = false, RolResponsable = "LOGISTICA",     EmpresaId = null, Activo = true, FechaCreacion = fecha },
            new PlantillaTarea { Id = 46, EntityType = "HojaRuta", SubTipo = "IMPORTACION",    Orden =  7, Codigo = "RECEPCION",    Descripcion = "Recepción física en almacén confirmada",         EsAutomatico = false, RolResponsable = "ALMACEN",       EmpresaId = null, Activo = true, FechaCreacion = fecha },
            new PlantillaTarea { Id = 47, EntityType = "HojaRuta", SubTipo = "IMPORTACION",    Orden =  8, Codigo = "CIERRE-HR",    Descripcion = "Hoja de ruta cerrada",                           EsAutomatico = true,  RolResponsable = null,            EmpresaId = null, Activo = true, FechaCreacion = fecha },

            // ?? HojaRuta / TRASPASO_INTERNO ???????????????????????????????????
            new PlantillaTarea { Id = 48, EntityType = "HojaRuta", SubTipo = "TRASPASO_INTERNO", Orden = 1, Codigo = "HR-CREADA",   Descripcion = "Hoja de ruta de traspaso creada",               EsAutomatico = true,  RolResponsable = null,        EmpresaId = null, Activo = true, FechaCreacion = fecha },
            new PlantillaTarea { Id = 49, EntityType = "HojaRuta", SubTipo = "TRASPASO_INTERNO", Orden = 2, Codigo = "PREPARACION", Descripcion = "Preparación de mercancía en almacén origen",    EsAutomatico = false, RolResponsable = "ALMACEN",   EmpresaId = null, Activo = true, FechaCreacion = fecha },
            new PlantillaTarea { Id = 50, EntityType = "HojaRuta", SubTipo = "TRASPASO_INTERNO", Orden = 3, Codigo = "DESPACHO",    Descripcion = "Despacho desde almacén origen",                 EsAutomatico = false, RolResponsable = "LOGISTICA", EmpresaId = null, Activo = true, FechaCreacion = fecha },
            new PlantillaTarea { Id = 51, EntityType = "HojaRuta", SubTipo = "TRASPASO_INTERNO", Orden = 4, Codigo = "RECEPCION",   Descripcion = "Recepción en almacén destino",                  EsAutomatico = false, RolResponsable = "ALMACEN",   EmpresaId = null, Activo = true, FechaCreacion = fecha },
            new PlantillaTarea { Id = 52, EntityType = "HojaRuta", SubTipo = "TRASPASO_INTERNO", Orden = 5, Codigo = "CIERRE-HR",   Descripcion = "Hoja de ruta de traspaso cerrada",              EsAutomatico = true,  RolResponsable = null,        EmpresaId = null, Activo = true, FechaCreacion = fecha },

            // ?? HojaRuta / ENTREGA (clientes) ?????????????????????????????????
            new PlantillaTarea { Id = 53, EntityType = "HojaRuta", SubTipo = "ENTREGA",           Orden = 1, Codigo = "HR-CREADA",   Descripcion = "Hoja de ruta de entrega creada",                EsAutomatico = true,  RolResponsable = null,        EmpresaId = null, Activo = true, FechaCreacion = fecha },
            new PlantillaTarea { Id = 54, EntityType = "HojaRuta", SubTipo = "ENTREGA",           Orden = 2, Codigo = "PREPARACION", Descripcion = "Pedido preparado para entrega al cliente",      EsAutomatico = false, RolResponsable = "ALMACEN",   EmpresaId = null, Activo = true, FechaCreacion = fecha },
            new PlantillaTarea { Id = 55, EntityType = "HojaRuta", SubTipo = "ENTREGA",           Orden = 3, Codigo = "DESPACHO",    Descripcion = "Vehículo de reparto despachado",                EsAutomatico = false, RolResponsable = "LOGISTICA", EmpresaId = null, Activo = true, FechaCreacion = fecha },
            new PlantillaTarea { Id = 56, EntityType = "HojaRuta", SubTipo = "ENTREGA",           Orden = 4, Codigo = "ENTREGADA",   Descripcion = "Entrega al cliente confirmada",                 EsAutomatico = false, RolResponsable = "LOGISTICA", EmpresaId = null, Activo = true, FechaCreacion = fecha },
            new PlantillaTarea { Id = 57, EntityType = "HojaRuta", SubTipo = "ENTREGA",           Orden = 5, Codigo = "CIERRE-HR",   Descripcion = "Hoja de ruta cerrada",                          EsAutomatico = true,  RolResponsable = null,        EmpresaId = null, Activo = true, FechaCreacion = fecha }
        );
    }
}
