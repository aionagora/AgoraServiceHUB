using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgoraHub360.ERP.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSucursalToNumeracionDocumento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_NumeracionesDocumento_EmpresaId_TipoDocumento",
                schema: "core",
                table: "NumeracionesDocumento");

            migrationBuilder.AddColumn<int>(
                name: "SucursalId",
                schema: "core",
                table: "NumeracionesDocumento",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_NumeracionesDocumento_EmpresaId_TipoDocumento_SucursalId",
                schema: "core",
                table: "NumeracionesDocumento",
                columns: new[] { "EmpresaId", "TipoDocumento", "SucursalId" },
                unique: true,
                filter: "[SucursalId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_NumeracionesDocumento_SucursalId",
                schema: "core",
                table: "NumeracionesDocumento",
                column: "SucursalId");

            migrationBuilder.AddForeignKey(
                name: "FK_NumeracionesDocumento_Sucursales_SucursalId",
                schema: "core",
                table: "NumeracionesDocumento",
                column: "SucursalId",
                principalSchema: "core",
                principalTable: "Sucursales",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NumeracionesDocumento_Sucursales_SucursalId",
                schema: "core",
                table: "NumeracionesDocumento");

            migrationBuilder.DropIndex(
                name: "IX_NumeracionesDocumento_EmpresaId_TipoDocumento_SucursalId",
                schema: "core",
                table: "NumeracionesDocumento");

            migrationBuilder.DropIndex(
                name: "IX_NumeracionesDocumento_SucursalId",
                schema: "core",
                table: "NumeracionesDocumento");

            migrationBuilder.DropColumn(
                name: "SucursalId",
                schema: "core",
                table: "NumeracionesDocumento");

            migrationBuilder.CreateIndex(
                name: "IX_NumeracionesDocumento_EmpresaId_TipoDocumento",
                schema: "core",
                table: "NumeracionesDocumento",
                columns: new[] { "EmpresaId", "TipoDocumento" },
                unique: true);
        }
    }
}
