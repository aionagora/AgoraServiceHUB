using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgoraHub360.ERP.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AlignSucursalGeoIdsToGuid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF EXISTS (
    SELECT 1
    FROM core.Sucursales
    WHERE PaisId IS NOT NULL
       OR DepartamentoId IS NOT NULL
       OR ProvinciaId IS NOT NULL
       OR CiudadId IS NOT NULL
       OR ZonaId IS NOT NULL
)
BEGIN
    THROW 50001, 'No se puede migrar Sucursales geo IDs de int a uniqueidentifier porque existen datos no nulos. Requiere script de mapeo previo.', 1;
END
");

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
                name: "PaisId",
                schema: "core",
                table: "Sucursales");

            migrationBuilder.DropColumn(
                name: "DepartamentoId",
                schema: "core",
                table: "Sucursales");

            migrationBuilder.DropColumn(
                name: "ProvinciaId",
                schema: "core",
                table: "Sucursales");

            migrationBuilder.DropColumn(
                name: "CiudadId",
                schema: "core",
                table: "Sucursales");

            migrationBuilder.DropColumn(
                name: "ZonaId",
                schema: "core",
                table: "Sucursales");

            migrationBuilder.AddColumn<Guid>(
                name: "PaisId",
                schema: "core",
                table: "Sucursales",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DepartamentoId",
                schema: "core",
                table: "Sucursales",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ProvinciaId",
                schema: "core",
                table: "Sucursales",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CiudadId",
                schema: "core",
                table: "Sucursales",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ZonaId",
                schema: "core",
                table: "Sucursales",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Sucursales_EmpresaId_PaisId",
                schema: "core",
                table: "Sucursales",
                columns: new[] { "EmpresaId", "PaisId" });

            migrationBuilder.CreateIndex(
                name: "IX_Sucursales_EmpresaId_DepartamentoId",
                schema: "core",
                table: "Sucursales",
                columns: new[] { "EmpresaId", "DepartamentoId" });

            migrationBuilder.CreateIndex(
                name: "IX_Sucursales_EmpresaId_ProvinciaId",
                schema: "core",
                table: "Sucursales",
                columns: new[] { "EmpresaId", "ProvinciaId" });

            migrationBuilder.CreateIndex(
                name: "IX_Sucursales_EmpresaId_CiudadId",
                schema: "core",
                table: "Sucursales",
                columns: new[] { "EmpresaId", "CiudadId" });

            migrationBuilder.CreateIndex(
                name: "IX_Sucursales_EmpresaId_ZonaId",
                schema: "core",
                table: "Sucursales",
                columns: new[] { "EmpresaId", "ZonaId" });

            migrationBuilder.AddForeignKey(
                name: "FK_Sucursales_Paises_PaisId",
                schema: "core",
                table: "Sucursales",
                column: "PaisId",
                principalSchema: "core",
                principalTable: "Paises",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Sucursales_Departamentos_DepartamentoId",
                schema: "core",
                table: "Sucursales",
                column: "DepartamentoId",
                principalSchema: "core",
                principalTable: "Departamentos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Sucursales_Provincias_ProvinciaId",
                schema: "core",
                table: "Sucursales",
                column: "ProvinciaId",
                principalSchema: "core",
                principalTable: "Provincias",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Sucursales_Ciudades_CiudadId",
                schema: "core",
                table: "Sucursales",
                column: "CiudadId",
                principalSchema: "core",
                principalTable: "Ciudades",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Sucursales_Zonas_ZonaId",
                schema: "core",
                table: "Sucursales",
                column: "ZonaId",
                principalSchema: "core",
                principalTable: "Zonas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Sucursales_Paises_PaisId",
                schema: "core",
                table: "Sucursales");

            migrationBuilder.DropForeignKey(
                name: "FK_Sucursales_Departamentos_DepartamentoId",
                schema: "core",
                table: "Sucursales");

            migrationBuilder.DropForeignKey(
                name: "FK_Sucursales_Provincias_ProvinciaId",
                schema: "core",
                table: "Sucursales");

            migrationBuilder.DropForeignKey(
                name: "FK_Sucursales_Ciudades_CiudadId",
                schema: "core",
                table: "Sucursales");

            migrationBuilder.DropForeignKey(
                name: "FK_Sucursales_Zonas_ZonaId",
                schema: "core",
                table: "Sucursales");

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
                name: "PaisId",
                schema: "core",
                table: "Sucursales");

            migrationBuilder.DropColumn(
                name: "DepartamentoId",
                schema: "core",
                table: "Sucursales");

            migrationBuilder.DropColumn(
                name: "ProvinciaId",
                schema: "core",
                table: "Sucursales");

            migrationBuilder.DropColumn(
                name: "CiudadId",
                schema: "core",
                table: "Sucursales");

            migrationBuilder.DropColumn(
                name: "ZonaId",
                schema: "core",
                table: "Sucursales");

            migrationBuilder.AddColumn<int>(
                name: "PaisId",
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
                name: "ProvinciaId",
                schema: "core",
                table: "Sucursales",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CiudadId",
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
                name: "IX_Sucursales_EmpresaId_PaisId",
                schema: "core",
                table: "Sucursales",
                columns: new[] { "EmpresaId", "PaisId" });

            migrationBuilder.CreateIndex(
                name: "IX_Sucursales_EmpresaId_DepartamentoId",
                schema: "core",
                table: "Sucursales",
                columns: new[] { "EmpresaId", "DepartamentoId" });

            migrationBuilder.CreateIndex(
                name: "IX_Sucursales_EmpresaId_ProvinciaId",
                schema: "core",
                table: "Sucursales",
                columns: new[] { "EmpresaId", "ProvinciaId" });

            migrationBuilder.CreateIndex(
                name: "IX_Sucursales_EmpresaId_CiudadId",
                schema: "core",
                table: "Sucursales",
                columns: new[] { "EmpresaId", "CiudadId" });

            migrationBuilder.CreateIndex(
                name: "IX_Sucursales_EmpresaId_ZonaId",
                schema: "core",
                table: "Sucursales",
                columns: new[] { "EmpresaId", "ZonaId" });
        }
    }
}
