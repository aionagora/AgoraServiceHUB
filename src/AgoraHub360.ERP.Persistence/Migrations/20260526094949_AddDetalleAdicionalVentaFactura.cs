using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgoraHub360.ERP.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDetalleAdicionalVentaFactura : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DetalleAdicional",
                schema: "vta",
                table: "VentaDetalles",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DetalleAdicional",
                schema: "vta",
                table: "FacturaVentaDetalles",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DetalleAdicional",
                schema: "vta",
                table: "VentaDetalles");

            migrationBuilder.DropColumn(
                name: "DetalleAdicional",
                schema: "vta",
                table: "FacturaVentaDetalles");
        }
    }
}
