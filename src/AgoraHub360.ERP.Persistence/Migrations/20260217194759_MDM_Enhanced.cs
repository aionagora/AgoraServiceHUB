using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgoraHub360.ERP.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MDM_Enhanced : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CondicionPago",
                schema: "mdm",
                table: "Proveedores",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Pais",
                schema: "mdm",
                table: "Proveedores",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TipoProveedor",
                schema: "mdm",
                table: "Proveedores",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "ControlStock",
                schema: "mdm",
                table: "Productos",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "CostoBase",
                schema: "mdm",
                table: "Productos",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "TipoProducto",
                schema: "mdm",
                table: "Productos",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "Almacenes",
                schema: "mdm",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Codigo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Direccion = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Responsable = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Telefono = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModificadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    EmpresaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Almacenes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UbicacionesAlmacen",
                schema: "mdm",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AlmacenId = table.Column<int>(type: "int", nullable: false),
                    Codigo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModificadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    EmpresaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UbicacionesAlmacen", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Almacenes_EmpresaId",
                schema: "mdm",
                table: "Almacenes",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_Almacenes_EmpresaId_Codigo",
                schema: "mdm",
                table: "Almacenes",
                columns: new[] { "EmpresaId", "Codigo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UbicacionesAlmacen_AlmacenId",
                schema: "mdm",
                table: "UbicacionesAlmacen",
                column: "AlmacenId");

            migrationBuilder.CreateIndex(
                name: "IX_UbicacionesAlmacen_EmpresaId",
                schema: "mdm",
                table: "UbicacionesAlmacen",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_UbicacionesAlmacen_EmpresaId_AlmacenId_Codigo",
                schema: "mdm",
                table: "UbicacionesAlmacen",
                columns: new[] { "EmpresaId", "AlmacenId", "Codigo" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Almacenes",
                schema: "mdm");

            migrationBuilder.DropTable(
                name: "UbicacionesAlmacen",
                schema: "mdm");

            migrationBuilder.DropColumn(
                name: "CondicionPago",
                schema: "mdm",
                table: "Proveedores");

            migrationBuilder.DropColumn(
                name: "Pais",
                schema: "mdm",
                table: "Proveedores");

            migrationBuilder.DropColumn(
                name: "TipoProveedor",
                schema: "mdm",
                table: "Proveedores");

            migrationBuilder.DropColumn(
                name: "ControlStock",
                schema: "mdm",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "CostoBase",
                schema: "mdm",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "TipoProducto",
                schema: "mdm",
                table: "Productos");
        }
    }
}
