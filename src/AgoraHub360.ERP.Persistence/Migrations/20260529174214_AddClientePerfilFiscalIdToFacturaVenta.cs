using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgoraHub360.ERP.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddClientePerfilFiscalIdToFacturaVenta : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "ClientePerfilFiscalId",
                schema: "vta",
                table: "FacturasVenta",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_FacturasVenta_ClientePerfilFiscalId",
                schema: "vta",
                table: "FacturasVenta",
                column: "ClientePerfilFiscalId");

            migrationBuilder.AddForeignKey(
                name: "FK_FacturasVenta_ClientePerfilesFiscales_ClientePerfilFiscalId",
                schema: "vta",
                table: "FacturasVenta",
                column: "ClientePerfilFiscalId",
                principalSchema: "mdm",
                principalTable: "ClientePerfilesFiscales",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FacturasVenta_ClientePerfilesFiscales_ClientePerfilFiscalId",
                schema: "vta",
                table: "FacturasVenta");

            migrationBuilder.DropIndex(
                name: "IX_FacturasVenta_ClientePerfilFiscalId",
                schema: "vta",
                table: "FacturasVenta");

            migrationBuilder.DropColumn(
                name: "ClientePerfilFiscalId",
                schema: "vta",
                table: "FacturasVenta");
        }
    }
}
