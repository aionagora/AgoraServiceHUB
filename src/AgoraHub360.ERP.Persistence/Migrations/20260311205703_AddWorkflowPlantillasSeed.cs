using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AgoraHub360.ERP.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkflowPlantillasSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PlantillasTareas",
                schema: "wf",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EntityType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SubTipo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Orden = table.Column<int>(type: "int", nullable: false),
                    Codigo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    EsAutomatico = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    RolResponsable = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    EmpresaId = table.Column<int>(type: "int", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPor = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModificadoPor = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlantillasTareas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlantillasTareas_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalSchema: "core",
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                schema: "wf",
                table: "PlantillasTareas",
                columns: new[] { "Id", "Activo", "Codigo", "CreadoPor", "Descripcion", "EmpresaId", "EntityType", "EsAutomatico", "FechaCreacion", "FechaModificacion", "ModificadoPor", "Orden", "RolResponsable", "SubTipo" },
                values: new object[] { 1, true, "OP-CREADA", null, "Orden de pedido creada en el sistema", null, "OrdenPedido", true, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 1, null, "IMPORTACION" });

            migrationBuilder.InsertData(
                schema: "wf",
                table: "PlantillasTareas",
                columns: new[] { "Id", "Activo", "Codigo", "CreadoPor", "Descripcion", "EmpresaId", "EntityType", "FechaCreacion", "FechaModificacion", "ModificadoPor", "Orden", "RolResponsable", "SubTipo" },
                values: new object[,]
                {
                    { 2, true, "OC-EMITIDA", null, "Orden de compra emitida al proveedor", null, "OrdenPedido", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 2, "COMPRAS", "IMPORTACION" },
                    { 3, true, "ETD-CONF", null, "ETD confirmado por proveedor / naviera", null, "OrdenPedido", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 3, "COMPRAS", "IMPORTACION" },
                    { 4, true, "EMBARQUE", null, "Embarque despachado (BL / AWB emitido)", null, "OrdenPedido", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 4, "COMPRAS", "IMPORTACION" },
                    { 5, true, "ETA-CONF", null, "ETA confirmado por forwarder / naviera", null, "OrdenPedido", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 5, "COMPRAS", "IMPORTACION" },
                    { 6, true, "EN-ADUANA", null, "Documentos presentados ante aduana", null, "OrdenPedido", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 6, "AGENTE_ADUANA", "IMPORTACION" },
                    { 7, true, "AFORO", null, "Aforo / inspección aduanera realizada", null, "OrdenPedido", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 7, "AGENTE_ADUANA", "IMPORTACION" },
                    { 8, true, "DUI", null, "DUI / DIM registrado y aprobado", null, "OrdenPedido", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 8, "AGENTE_ADUANA", "IMPORTACION" },
                    { 9, true, "LEVANTE", null, "Levante de mercancía autorizado", null, "OrdenPedido", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 9, "AGENTE_ADUANA", "IMPORTACION" },
                    { 10, true, "TRANSPORTE", null, "Transporte interno hacia almacén destino", null, "OrdenPedido", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 10, "LOGISTICA", "IMPORTACION" },
                    { 11, true, "RECEPCION", null, "Recepción física confirmada en almacén", null, "OrdenPedido", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 11, "ALMACEN", "IMPORTACION" },
                    { 12, true, "LANDED-COST", null, "Costo de importación calculado y contabilizado", null, "OrdenPedido", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 12, "FINANZAS", "IMPORTACION" }
                });

            migrationBuilder.InsertData(
                schema: "wf",
                table: "PlantillasTareas",
                columns: new[] { "Id", "Activo", "Codigo", "CreadoPor", "Descripcion", "EmpresaId", "EntityType", "EsAutomatico", "FechaCreacion", "FechaModificacion", "ModificadoPor", "Orden", "RolResponsable", "SubTipo" },
                values: new object[,]
                {
                    { 13, true, "CIERRE", null, "Expediente de importación cerrado", null, "OrdenPedido", true, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 13, null, "IMPORTACION" },
                    { 14, true, "OP-CREADA", null, "Orden de pedido de traspaso creada", null, "OrdenPedido", true, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 1, null, "TRASPASO_INTERNO" }
                });

            migrationBuilder.InsertData(
                schema: "wf",
                table: "PlantillasTareas",
                columns: new[] { "Id", "Activo", "Codigo", "CreadoPor", "Descripcion", "EmpresaId", "EntityType", "FechaCreacion", "FechaModificacion", "ModificadoPor", "Orden", "RolResponsable", "SubTipo" },
                values: new object[,]
                {
                    { 15, true, "APROBACION", null, "Traspaso aprobado por supervisor", null, "OrdenPedido", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 2, "SUPERVISOR", "TRASPASO_INTERNO" },
                    { 16, true, "PREPARACION", null, "Mercancía preparada y verificada en origen", null, "OrdenPedido", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 3, "ALMACEN", "TRASPASO_INTERNO" },
                    { 17, true, "DESPACHO", null, "Mercancía despachada desde almacén origen", null, "OrdenPedido", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 4, "LOGISTICA", "TRASPASO_INTERNO" },
                    { 18, true, "TRANSITO", null, "Mercancía en tránsito hacia almacén destino", null, "OrdenPedido", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 5, "LOGISTICA", "TRASPASO_INTERNO" },
                    { 19, true, "RECEPCION", null, "Recepción confirmada en almacén destino", null, "OrdenPedido", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 6, "ALMACEN", "TRASPASO_INTERNO" },
                    { 20, true, "CONFIRMACION", null, "Traspaso confirmado y diferencias registradas", null, "OrdenPedido", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 7, "SUPERVISOR", "TRASPASO_INTERNO" }
                });

            migrationBuilder.InsertData(
                schema: "wf",
                table: "PlantillasTareas",
                columns: new[] { "Id", "Activo", "Codigo", "CreadoPor", "Descripcion", "EmpresaId", "EntityType", "EsAutomatico", "FechaCreacion", "FechaModificacion", "ModificadoPor", "Orden", "RolResponsable", "SubTipo" },
                values: new object[,]
                {
                    { 21, true, "CIERRE", null, "Traspaso cerrado y stock actualizado", null, "OrdenPedido", true, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 8, null, "TRASPASO_INTERNO" },
                    { 22, true, "OC-CREADA", null, "Orden de compra generada en el sistema", null, "OrdenCompra", true, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 1, null, null }
                });

            migrationBuilder.InsertData(
                schema: "wf",
                table: "PlantillasTareas",
                columns: new[] { "Id", "Activo", "Codigo", "CreadoPor", "Descripcion", "EmpresaId", "EntityType", "FechaCreacion", "FechaModificacion", "ModificadoPor", "Orden", "RolResponsable", "SubTipo" },
                values: new object[,]
                {
                    { 23, true, "APROBACION-L1", null, "Aprobación de primer nivel (supervisor)", null, "OrdenCompra", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 2, "SUPERVISOR", null },
                    { 24, true, "APROBACION-L2", null, "Aprobación de segundo nivel (gerencia)", null, "OrdenCompra", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 3, "GERENCIA", null },
                    { 25, true, "ENVIADA-PROVEEDOR", null, "Orden enviada al proveedor", null, "OrdenCompra", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 4, "COMPRAS", null },
                    { 26, true, "CONF-PROVEEDOR", null, "Orden confirmada por el proveedor", null, "OrdenCompra", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 5, "COMPRAS", null },
                    { 27, true, "PRODUCCION", null, "Mercancía en producción / preparación", null, "OrdenCompra", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 6, "COMPRAS", null },
                    { 28, true, "ETD", null, "Fecha estimada de despacho confirmada", null, "OrdenCompra", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 7, "COMPRAS", null },
                    { 29, true, "EMBARQUE", null, "Embarque efectuado (BL / AWB recibido)", null, "OrdenCompra", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 8, "COMPRAS", null },
                    { 30, true, "RECEPCION", null, "Mercancía recepcionada en almacén", null, "OrdenCompra", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 9, "ALMACEN", null },
                    { 31, true, "PAGADA", null, "Pago al proveedor registrado y confirmado", null, "OrdenCompra", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 10, "FINANZAS", null }
                });

            migrationBuilder.InsertData(
                schema: "wf",
                table: "PlantillasTareas",
                columns: new[] { "Id", "Activo", "Codigo", "CreadoPor", "Descripcion", "EmpresaId", "EntityType", "EsAutomatico", "FechaCreacion", "FechaModificacion", "ModificadoPor", "Orden", "RolResponsable", "SubTipo" },
                values: new object[] { 32, true, "OV-CREADA", null, "Orden de venta registrada en el sistema", null, "OrdenVenta", true, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 1, null, null });

            migrationBuilder.InsertData(
                schema: "wf",
                table: "PlantillasTareas",
                columns: new[] { "Id", "Activo", "Codigo", "CreadoPor", "Descripcion", "EmpresaId", "EntityType", "FechaCreacion", "FechaModificacion", "ModificadoPor", "Orden", "RolResponsable", "SubTipo" },
                values: new object[,]
                {
                    { 33, true, "APROBACION", null, "Orden de venta aprobada por supervisor", null, "OrdenVenta", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 2, "SUPERVISOR", null },
                    { 34, true, "RESERVA-STOCK", null, "Stock reservado para la orden de venta", null, "OrdenVenta", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 3, "ALMACEN", null },
                    { 35, true, "PREPARACION", null, "Pedido preparado y empacado en almacén", null, "OrdenVenta", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 4, "ALMACEN", null },
                    { 36, true, "FACTURADA", null, "Factura emitida y registrada en contabilidad", null, "OrdenVenta", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 5, "FINANZAS", null },
                    { 37, true, "DESPACHO", null, "Mercancía despachada hacia el cliente", null, "OrdenVenta", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 6, "LOGISTICA", null },
                    { 38, true, "ENTREGADA", null, "Entrega al cliente confirmada con firma", null, "OrdenVenta", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 7, "LOGISTICA", null },
                    { 39, true, "COBRADA", null, "Pago del cliente recibido y conciliado", null, "OrdenVenta", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 8, "FINANZAS", null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Plantillas_Query",
                schema: "wf",
                table: "PlantillasTareas",
                columns: new[] { "EntityType", "SubTipo", "EmpresaId", "Orden" });

            migrationBuilder.CreateIndex(
                name: "IX_PlantillasTareas_EmpresaId",
                schema: "wf",
                table: "PlantillasTareas",
                column: "EmpresaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlantillasTareas",
                schema: "wf");
        }
    }
}
