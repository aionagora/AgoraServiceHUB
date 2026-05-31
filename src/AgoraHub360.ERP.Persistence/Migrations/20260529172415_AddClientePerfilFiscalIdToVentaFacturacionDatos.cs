using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgoraHub360.ERP.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddClientePerfilFiscalIdToVentaFacturacionDatos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "ClientePerfilFiscalId",
                schema: "vta",
                table: "VentaFacturacionDatos",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_VentaFacturacionDatos_ClientePerfilFiscalId",
                schema: "vta",
                table: "VentaFacturacionDatos",
                column: "ClientePerfilFiscalId");

            migrationBuilder.AddForeignKey(
                name: "FK_VentaFacturacionDatos_ClientePerfilesFiscales_ClientePerfilFiscalId",
                schema: "vta",
                table: "VentaFacturacionDatos",
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
                name: "FK_VentaFacturacionDatos_ClientePerfilesFiscales_ClientePerfilFiscalId",
                schema: "vta",
                table: "VentaFacturacionDatos");

            migrationBuilder.DropIndex(
                name: "IX_VentaFacturacionDatos_ClientePerfilFiscalId",
                schema: "vta",
                table: "VentaFacturacionDatos");

            migrationBuilder.DropColumn(
                name: "ClientePerfilFiscalId",
                schema: "vta",
                table: "VentaFacturacionDatos");
        }
    }
}
