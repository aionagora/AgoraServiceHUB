using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgoraHub360.ERP.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddParametroSistemaNumeracionDocumento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NumeracionesDocumento",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TipoDocumento = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Prefijo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SiguienteNumero = table.Column<int>(type: "int", nullable: false),
                    Digitos = table.Column<int>(type: "int", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModificadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    EmpresaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NumeracionesDocumento", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ParametrosSistema",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Clave = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Valor = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Categoria = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TipoDato = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModificadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    EmpresaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParametrosSistema", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NumeracionesDocumento_EmpresaId",
                schema: "core",
                table: "NumeracionesDocumento",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_NumeracionesDocumento_EmpresaId_TipoDocumento",
                schema: "core",
                table: "NumeracionesDocumento",
                columns: new[] { "EmpresaId", "TipoDocumento" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ParametrosSistema_EmpresaId",
                schema: "core",
                table: "ParametrosSistema",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_ParametrosSistema_EmpresaId_Clave",
                schema: "core",
                table: "ParametrosSistema",
                columns: new[] { "EmpresaId", "Clave" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NumeracionesDocumento",
                schema: "core");

            migrationBuilder.DropTable(
                name: "ParametrosSistema",
                schema: "core");
        }
    }
}
