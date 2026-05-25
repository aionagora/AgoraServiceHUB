using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgoraHub360.ERP.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ExpandSucursalFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Celular",
                schema: "core",
                table: "Sucursales",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CodigoInterno",
                schema: "core",
                table: "Sucursales",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CodigoSucursalFiscal",
                schema: "core",
                table: "Sucursales",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Departamento",
                schema: "core",
                table: "Sucursales",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Descripcion",
                schema: "core",
                table: "Sucursales",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmailAlternativo",
                schema: "core",
                table: "Sucursales",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Latitud",
                schema: "core",
                table: "Sucursales",
                type: "decimal(11,8)",
                precision: 11,
                scale: 8,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Longitud",
                schema: "core",
                table: "Sucursales",
                type: "decimal(11,8)",
                precision: 11,
                scale: 8,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ManejaAlmacen",
                schema: "core",
                table: "Sucursales",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "Observaciones",
                schema: "core",
                table: "Sucursales",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Pais",
                schema: "core",
                table: "Sucursales",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "PermiteCompras",
                schema: "core",
                table: "Sucursales",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "PermiteDespacho",
                schema: "core",
                table: "Sucursales",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "PermiteFacturacion",
                schema: "core",
                table: "Sucursales",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "PermiteInventario",
                schema: "core",
                table: "Sucursales",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "PermiteVentas",
                schema: "core",
                table: "Sucursales",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "PrefijoDocumental",
                schema: "core",
                table: "Sucursales",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Provincia",
                schema: "core",
                table: "Sucursales",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Referencia",
                schema: "core",
                table: "Sucursales",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResponsableCargo",
                schema: "core",
                table: "Sucursales",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResponsableNombre",
                schema: "core",
                table: "Sucursales",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Sigla",
                schema: "core",
                table: "Sucursales",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UrlMapa",
                schema: "core",
                table: "Sucursales",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WhatsApp",
                schema: "core",
                table: "Sucursales",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Zona",
                schema: "core",
                table: "Sucursales",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Celular",
                schema: "core",
                table: "Sucursales");

            migrationBuilder.DropColumn(
                name: "CodigoInterno",
                schema: "core",
                table: "Sucursales");

            migrationBuilder.DropColumn(
                name: "CodigoSucursalFiscal",
                schema: "core",
                table: "Sucursales");

            migrationBuilder.DropColumn(
                name: "Departamento",
                schema: "core",
                table: "Sucursales");

            migrationBuilder.DropColumn(
                name: "Descripcion",
                schema: "core",
                table: "Sucursales");

            migrationBuilder.DropColumn(
                name: "EmailAlternativo",
                schema: "core",
                table: "Sucursales");

            migrationBuilder.DropColumn(
                name: "Latitud",
                schema: "core",
                table: "Sucursales");

            migrationBuilder.DropColumn(
                name: "Longitud",
                schema: "core",
                table: "Sucursales");

            migrationBuilder.DropColumn(
                name: "ManejaAlmacen",
                schema: "core",
                table: "Sucursales");

            migrationBuilder.DropColumn(
                name: "Observaciones",
                schema: "core",
                table: "Sucursales");

            migrationBuilder.DropColumn(
                name: "Pais",
                schema: "core",
                table: "Sucursales");

            migrationBuilder.DropColumn(
                name: "PermiteCompras",
                schema: "core",
                table: "Sucursales");

            migrationBuilder.DropColumn(
                name: "PermiteDespacho",
                schema: "core",
                table: "Sucursales");

            migrationBuilder.DropColumn(
                name: "PermiteFacturacion",
                schema: "core",
                table: "Sucursales");

            migrationBuilder.DropColumn(
                name: "PermiteInventario",
                schema: "core",
                table: "Sucursales");

            migrationBuilder.DropColumn(
                name: "PermiteVentas",
                schema: "core",
                table: "Sucursales");

            migrationBuilder.DropColumn(
                name: "PrefijoDocumental",
                schema: "core",
                table: "Sucursales");

            migrationBuilder.DropColumn(
                name: "Provincia",
                schema: "core",
                table: "Sucursales");

            migrationBuilder.DropColumn(
                name: "Referencia",
                schema: "core",
                table: "Sucursales");

            migrationBuilder.DropColumn(
                name: "ResponsableCargo",
                schema: "core",
                table: "Sucursales");

            migrationBuilder.DropColumn(
                name: "ResponsableNombre",
                schema: "core",
                table: "Sucursales");

            migrationBuilder.DropColumn(
                name: "Sigla",
                schema: "core",
                table: "Sucursales");

            migrationBuilder.DropColumn(
                name: "UrlMapa",
                schema: "core",
                table: "Sucursales");

            migrationBuilder.DropColumn(
                name: "WhatsApp",
                schema: "core",
                table: "Sucursales");

            migrationBuilder.DropColumn(
                name: "Zona",
                schema: "core",
                table: "Sucursales");
        }
    }
}
