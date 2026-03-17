using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AgoraHub360.ERP.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkflowHojaRutaPlantillas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                schema: "wf",
                table: "PlantillasTareas",
                columns: new[] { "Id", "Activo", "Codigo", "CreadoPor", "Descripcion", "EmpresaId", "EntityType", "EsAutomatico", "FechaCreacion", "FechaModificacion", "ModificadoPor", "Orden", "RolResponsable", "SubTipo" },
                values: new object[] { 40, true, "HR-CREADA", null, "Hoja de ruta creada en el sistema", null, "HojaRuta", true, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 1, null, "IMPORTACION" });

            migrationBuilder.InsertData(
                schema: "wf",
                table: "PlantillasTareas",
                columns: new[] { "Id", "Activo", "Codigo", "CreadoPor", "Descripcion", "EmpresaId", "EntityType", "FechaCreacion", "FechaModificacion", "ModificadoPor", "Orden", "RolResponsable", "SubTipo" },
                values: new object[,]
                {
                    { 41, true, "DESPACHO-EXT", null, "Despacho en origen confirmado", null, "HojaRuta", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 2, "COMPRAS", "IMPORTACION" },
                    { 42, true, "EN-TRANSITO", null, "Carga en tránsito internacional", null, "HojaRuta", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 3, "COMPRAS", "IMPORTACION" },
                    { 43, true, "ADUANA", null, "Trámite aduanero iniciado", null, "HojaRuta", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 4, "AGENTE_ADUANA", "IMPORTACION" },
                    { 44, true, "LEVANTE", null, "Levante de aduana autorizado", null, "HojaRuta", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 5, "AGENTE_ADUANA", "IMPORTACION" },
                    { 45, true, "TRANSPORTE", null, "Transporte hacia almacén destino", null, "HojaRuta", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 6, "LOGISTICA", "IMPORTACION" },
                    { 46, true, "RECEPCION", null, "Recepción física en almacén confirmada", null, "HojaRuta", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 7, "ALMACEN", "IMPORTACION" }
                });

            migrationBuilder.InsertData(
                schema: "wf",
                table: "PlantillasTareas",
                columns: new[] { "Id", "Activo", "Codigo", "CreadoPor", "Descripcion", "EmpresaId", "EntityType", "EsAutomatico", "FechaCreacion", "FechaModificacion", "ModificadoPor", "Orden", "RolResponsable", "SubTipo" },
                values: new object[,]
                {
                    { 47, true, "CIERRE-HR", null, "Hoja de ruta cerrada", null, "HojaRuta", true, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 8, null, "IMPORTACION" },
                    { 48, true, "HR-CREADA", null, "Hoja de ruta de traspaso creada", null, "HojaRuta", true, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 1, null, "TRASPASO_INTERNO" }
                });

            migrationBuilder.InsertData(
                schema: "wf",
                table: "PlantillasTareas",
                columns: new[] { "Id", "Activo", "Codigo", "CreadoPor", "Descripcion", "EmpresaId", "EntityType", "FechaCreacion", "FechaModificacion", "ModificadoPor", "Orden", "RolResponsable", "SubTipo" },
                values: new object[,]
                {
                    { 49, true, "PREPARACION", null, "Preparación de mercancía en almacén origen", null, "HojaRuta", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 2, "ALMACEN", "TRASPASO_INTERNO" },
                    { 50, true, "DESPACHO", null, "Despacho desde almacén origen", null, "HojaRuta", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 3, "LOGISTICA", "TRASPASO_INTERNO" },
                    { 51, true, "RECEPCION", null, "Recepción en almacén destino", null, "HojaRuta", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 4, "ALMACEN", "TRASPASO_INTERNO" }
                });

            migrationBuilder.InsertData(
                schema: "wf",
                table: "PlantillasTareas",
                columns: new[] { "Id", "Activo", "Codigo", "CreadoPor", "Descripcion", "EmpresaId", "EntityType", "EsAutomatico", "FechaCreacion", "FechaModificacion", "ModificadoPor", "Orden", "RolResponsable", "SubTipo" },
                values: new object[,]
                {
                    { 52, true, "CIERRE-HR", null, "Hoja de ruta de traspaso cerrada", null, "HojaRuta", true, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 5, null, "TRASPASO_INTERNO" },
                    { 53, true, "HR-CREADA", null, "Hoja de ruta de entrega creada", null, "HojaRuta", true, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 1, null, "ENTREGA" }
                });

            migrationBuilder.InsertData(
                schema: "wf",
                table: "PlantillasTareas",
                columns: new[] { "Id", "Activo", "Codigo", "CreadoPor", "Descripcion", "EmpresaId", "EntityType", "FechaCreacion", "FechaModificacion", "ModificadoPor", "Orden", "RolResponsable", "SubTipo" },
                values: new object[,]
                {
                    { 54, true, "PREPARACION", null, "Pedido preparado para entrega al cliente", null, "HojaRuta", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 2, "ALMACEN", "ENTREGA" },
                    { 55, true, "DESPACHO", null, "Vehículo de reparto despachado", null, "HojaRuta", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 3, "LOGISTICA", "ENTREGA" },
                    { 56, true, "ENTREGADA", null, "Entrega al cliente confirmada", null, "HojaRuta", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 4, "LOGISTICA", "ENTREGA" }
                });

            migrationBuilder.InsertData(
                schema: "wf",
                table: "PlantillasTareas",
                columns: new[] { "Id", "Activo", "Codigo", "CreadoPor", "Descripcion", "EmpresaId", "EntityType", "EsAutomatico", "FechaCreacion", "FechaModificacion", "ModificadoPor", "Orden", "RolResponsable", "SubTipo" },
                values: new object[] { 57, true, "CIERRE-HR", null, "Hoja de ruta cerrada", null, "HojaRuta", true, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 5, null, "ENTREGA" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "wf",
                table: "PlantillasTareas",
                keyColumn: "Id",
                keyValue: 40);

            migrationBuilder.DeleteData(
                schema: "wf",
                table: "PlantillasTareas",
                keyColumn: "Id",
                keyValue: 41);

            migrationBuilder.DeleteData(
                schema: "wf",
                table: "PlantillasTareas",
                keyColumn: "Id",
                keyValue: 42);

            migrationBuilder.DeleteData(
                schema: "wf",
                table: "PlantillasTareas",
                keyColumn: "Id",
                keyValue: 43);

            migrationBuilder.DeleteData(
                schema: "wf",
                table: "PlantillasTareas",
                keyColumn: "Id",
                keyValue: 44);

            migrationBuilder.DeleteData(
                schema: "wf",
                table: "PlantillasTareas",
                keyColumn: "Id",
                keyValue: 45);

            migrationBuilder.DeleteData(
                schema: "wf",
                table: "PlantillasTareas",
                keyColumn: "Id",
                keyValue: 46);

            migrationBuilder.DeleteData(
                schema: "wf",
                table: "PlantillasTareas",
                keyColumn: "Id",
                keyValue: 47);

            migrationBuilder.DeleteData(
                schema: "wf",
                table: "PlantillasTareas",
                keyColumn: "Id",
                keyValue: 48);

            migrationBuilder.DeleteData(
                schema: "wf",
                table: "PlantillasTareas",
                keyColumn: "Id",
                keyValue: 49);

            migrationBuilder.DeleteData(
                schema: "wf",
                table: "PlantillasTareas",
                keyColumn: "Id",
                keyValue: 50);

            migrationBuilder.DeleteData(
                schema: "wf",
                table: "PlantillasTareas",
                keyColumn: "Id",
                keyValue: 51);

            migrationBuilder.DeleteData(
                schema: "wf",
                table: "PlantillasTareas",
                keyColumn: "Id",
                keyValue: 52);

            migrationBuilder.DeleteData(
                schema: "wf",
                table: "PlantillasTareas",
                keyColumn: "Id",
                keyValue: 53);

            migrationBuilder.DeleteData(
                schema: "wf",
                table: "PlantillasTareas",
                keyColumn: "Id",
                keyValue: 54);

            migrationBuilder.DeleteData(
                schema: "wf",
                table: "PlantillasTareas",
                keyColumn: "Id",
                keyValue: 55);

            migrationBuilder.DeleteData(
                schema: "wf",
                table: "PlantillasTareas",
                keyColumn: "Id",
                keyValue: 56);

            migrationBuilder.DeleteData(
                schema: "wf",
                table: "PlantillasTareas",
                keyColumn: "Id",
                keyValue: 57);
        }
    }
}
