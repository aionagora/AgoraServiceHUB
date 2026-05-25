using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgoraHub360.ERP.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSucursalGeoIds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CiudadId",
                schema: "core",
                table: "Sucursales",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DepartamentoId",
                schema: "core",
                table: "Sucursales",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PaisId",
                schema: "core",
                table: "Sucursales",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProvinciaId",
                schema: "core",
                table: "Sucursales",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ZonaId",
                schema: "core",
                table: "Sucursales",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Sucursales_EmpresaId_CiudadId",
                schema: "core",
                table: "Sucursales",
                columns: new[] { "EmpresaId", "CiudadId" });

            migrationBuilder.CreateIndex(
                name: "IX_Sucursales_EmpresaId_DepartamentoId",
                schema: "core",
                table: "Sucursales",
                columns: new[] { "EmpresaId", "DepartamentoId" });

            migrationBuilder.CreateIndex(
                name: "IX_Sucursales_EmpresaId_PaisId",
                schema: "core",
                table: "Sucursales",
                columns: new[] { "EmpresaId", "PaisId" });

            migrationBuilder.CreateIndex(
                name: "IX_Sucursales_EmpresaId_ProvinciaId",
                schema: "core",
                table: "Sucursales",
                columns: new[] { "EmpresaId", "ProvinciaId" });

            migrationBuilder.CreateIndex(
                name: "IX_Sucursales_EmpresaId_ZonaId",
                schema: "core",
                table: "Sucursales",
                columns: new[] { "EmpresaId", "ZonaId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Sucursales_EmpresaId_CiudadId",
                schema: "core",
                table: "Sucursales");

            migrationBuilder.DropIndex(
                name: "IX_Sucursales_EmpresaId_DepartamentoId",
                schema: "core",
                table: "Sucursales");

            migrationBuilder.DropIndex(
                name: "IX_Sucursales_EmpresaId_PaisId",
                schema: "core",
                table: "Sucursales");

            migrationBuilder.DropIndex(
                name: "IX_Sucursales_EmpresaId_ProvinciaId",
                schema: "core",
                table: "Sucursales");

            migrationBuilder.DropIndex(
                name: "IX_Sucursales_EmpresaId_ZonaId",
                schema: "core",
                table: "Sucursales");

            migrationBuilder.DropColumn(
                name: "CiudadId",
                schema: "core",
                table: "Sucursales");

            migrationBuilder.DropColumn(
                name: "DepartamentoId",
                schema: "core",
                table: "Sucursales");

            migrationBuilder.DropColumn(
                name: "PaisId",
                schema: "core",
                table: "Sucursales");

            migrationBuilder.DropColumn(
                name: "ProvinciaId",
                schema: "core",
                table: "Sucursales");

            migrationBuilder.DropColumn(
                name: "ZonaId",
                schema: "core",
                table: "Sucursales");
        }
    }
}
