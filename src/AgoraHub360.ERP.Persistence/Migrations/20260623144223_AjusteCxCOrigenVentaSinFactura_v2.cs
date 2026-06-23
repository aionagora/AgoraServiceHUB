using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgoraHub360.ERP.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AjusteCxCOrigenVentaSinFactura_v2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CuentasPorCobrar_Ventas_VentaId",
                schema: "cxc",
                table: "CuentasPorCobrar");

            migrationBuilder.AlterColumn<string>(
                name: "TipoDocumentoOrigen",
                schema: "cxc",
                table: "CuentasPorCobrar",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "NumeroFactura",
                schema: "cxc",
                table: "CuentasPorCobrar",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(250)",
                oldMaxLength: 250);

            migrationBuilder.CreateIndex(
                name: "IX_CuentasPorCobrar_EmpresaId_VentaId_TipoDocumentoOrigen",
                schema: "cxc",
                table: "CuentasPorCobrar",
                columns: new[] { "EmpresaId", "VentaId", "TipoDocumentoOrigen" });

            migrationBuilder.AddForeignKey(
                name: "FK_CuentasPorCobrar_Ventas_VentaId",
                schema: "cxc",
                table: "CuentasPorCobrar",
                column: "VentaId",
                principalSchema: "vta",
                principalTable: "Ventas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CuentasPorCobrar_Ventas_VentaId",
                schema: "cxc",
                table: "CuentasPorCobrar");

            migrationBuilder.DropIndex(
                name: "IX_CuentasPorCobrar_EmpresaId_VentaId_TipoDocumentoOrigen",
                schema: "cxc",
                table: "CuentasPorCobrar");

            migrationBuilder.AlterColumn<string>(
                name: "TipoDocumentoOrigen",
                schema: "cxc",
                table: "CuentasPorCobrar",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "NumeroFactura",
                schema: "cxc",
                table: "CuentasPorCobrar",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(250)",
                oldMaxLength: 250,
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_CuentasPorCobrar_Ventas_VentaId",
                schema: "cxc",
                table: "CuentasPorCobrar",
                column: "VentaId",
                principalSchema: "vta",
                principalTable: "Ventas",
                principalColumn: "Id");
        }
    }
}
