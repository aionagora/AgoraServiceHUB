using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgoraHub360.ERP.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AjusteCxCOrigenVentaSinFactura : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CuentasPorCobrar_EmpresaId_FacturaVentaId",
                schema: "cxc",
                table: "CuentasPorCobrar");

            migrationBuilder.AlterColumn<long>(
                name: "FacturaVentaId",
                schema: "cxc",
                table: "CuentasPorCobrar",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddColumn<string>(
                name: "TipoDocumentoOrigen",
                schema: "cxc",
                table: "CuentasPorCobrar",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "VentaId",
                schema: "cxc",
                table: "CuentasPorCobrar",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CuentasPorCobrar_EmpresaId_FacturaVentaId",
                schema: "cxc",
                table: "CuentasPorCobrar",
                columns: new[] { "EmpresaId", "FacturaVentaId" },
                unique: true,
                filter: "[FacturaVentaId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CuentasPorCobrar_VentaId",
                schema: "cxc",
                table: "CuentasPorCobrar",
                column: "VentaId");

            migrationBuilder.AddForeignKey(
                name: "FK_CuentasPorCobrar_Ventas_VentaId",
                schema: "cxc",
                table: "CuentasPorCobrar",
                column: "VentaId",
                principalSchema: "vta",
                principalTable: "Ventas",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CuentasPorCobrar_Ventas_VentaId",
                schema: "cxc",
                table: "CuentasPorCobrar");

            migrationBuilder.DropIndex(
                name: "IX_CuentasPorCobrar_EmpresaId_FacturaVentaId",
                schema: "cxc",
                table: "CuentasPorCobrar");

            migrationBuilder.DropIndex(
                name: "IX_CuentasPorCobrar_VentaId",
                schema: "cxc",
                table: "CuentasPorCobrar");

            migrationBuilder.DropColumn(
                name: "TipoDocumentoOrigen",
                schema: "cxc",
                table: "CuentasPorCobrar");

            migrationBuilder.DropColumn(
                name: "VentaId",
                schema: "cxc",
                table: "CuentasPorCobrar");

            migrationBuilder.AlterColumn<long>(
                name: "FacturaVentaId",
                schema: "cxc",
                table: "CuentasPorCobrar",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CuentasPorCobrar_EmpresaId_FacturaVentaId",
                schema: "cxc",
                table: "CuentasPorCobrar",
                columns: new[] { "EmpresaId", "FacturaVentaId" },
                unique: true);
        }
    }
}
