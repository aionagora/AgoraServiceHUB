using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgoraHub360.ERP.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSalesReservationFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "ReservedStock",
                schema: "inv",
                table: "StockProductos",
                type: "decimal(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "CantidadReservada",
                schema: "vta",
                table: "PedidoVentaDetalles",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaAnulacion",
                schema: "vta",
                table: "PedidosVenta",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaConfirmacion",
                schema: "vta",
                table: "PedidosVenta",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaDespacho",
                schema: "vta",
                table: "PedidosVenta",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "InventarioDescontado",
                schema: "vta",
                table: "PedidosVenta",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ReservaAplicada",
                schema: "vta",
                table: "PedidosVenta",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReservedStock",
                schema: "inv",
                table: "StockProductos");

            migrationBuilder.DropColumn(
                name: "CantidadReservada",
                schema: "vta",
                table: "PedidoVentaDetalles");

            migrationBuilder.DropColumn(
                name: "FechaAnulacion",
                schema: "vta",
                table: "PedidosVenta");

            migrationBuilder.DropColumn(
                name: "FechaConfirmacion",
                schema: "vta",
                table: "PedidosVenta");

            migrationBuilder.DropColumn(
                name: "FechaDespacho",
                schema: "vta",
                table: "PedidosVenta");

            migrationBuilder.DropColumn(
                name: "InventarioDescontado",
                schema: "vta",
                table: "PedidosVenta");

            migrationBuilder.DropColumn(
                name: "ReservaAplicada",
                schema: "vta",
                table: "PedidosVenta");
        }
    }
}
