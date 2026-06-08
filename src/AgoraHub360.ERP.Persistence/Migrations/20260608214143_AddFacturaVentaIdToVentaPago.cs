using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgoraHub360.ERP.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddFacturaVentaIdToVentaPago : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "FacturaVentaId",
                schema: "vta",
                table: "VentaPagos",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_VentaPagos_FacturaVentaId",
                schema: "vta",
                table: "VentaPagos",
                column: "FacturaVentaId");

            migrationBuilder.AddForeignKey(
                name: "FK_VentaPagos_FacturasVenta_FacturaVentaId",
                schema: "vta",
                table: "VentaPagos",
                column: "FacturaVentaId",
                principalSchema: "vta",
                principalTable: "FacturasVenta",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VentaPagos_FacturasVenta_FacturaVentaId",
                schema: "vta",
                table: "VentaPagos");

            migrationBuilder.DropIndex(
                name: "IX_VentaPagos_FacturaVentaId",
                schema: "vta",
                table: "VentaPagos");

            migrationBuilder.DropColumn(
                name: "FacturaVentaId",
                schema: "vta",
                table: "VentaPagos");
        }
    }
}
