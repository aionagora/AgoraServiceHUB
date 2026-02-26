using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgoraHub360.ERP.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ACC_PlantillasContables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PlantillasContables",
                schema: "acc",
                columns: table => new
                {
                    PlantillaContableId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Codigo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TipoDocumento = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    GlosaPlantilla = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModificadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    EmpresaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlantillasContables", x => x.PlantillaContableId);
                });

            migrationBuilder.CreateTable(
                name: "PlantillaContableLineas",
                schema: "acc",
                columns: table => new
                {
                    PlantillaContableLineaId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PlantillaContableId = table.Column<int>(type: "int", nullable: false),
                    NumeroLinea = table.Column<int>(type: "int", nullable: false),
                    CuentaContableId = table.Column<int>(type: "int", nullable: false),
                    TipoMovimiento = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    CampoMonto = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Factor = table.Column<decimal>(type: "decimal(10,4)", nullable: false),
                    Glosa = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModificadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlantillaContableLineas", x => x.PlantillaContableLineaId);
                    table.ForeignKey(
                        name: "FK_PlantillaContableLineas_CuentasContables_CuentaContableId",
                        column: x => x.CuentaContableId,
                        principalSchema: "acc",
                        principalTable: "CuentasContables",
                        principalColumn: "CuentaContableId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PlantillaContableLineas_PlantillasContables_PlantillaContableId",
                        column: x => x.PlantillaContableId,
                        principalSchema: "acc",
                        principalTable: "PlantillasContables",
                        principalColumn: "PlantillaContableId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlantillaContableLineas_CuentaContableId",
                schema: "acc",
                table: "PlantillaContableLineas",
                column: "CuentaContableId");

            migrationBuilder.CreateIndex(
                name: "IX_PlantillaContableLineas_PlantillaContableId",
                schema: "acc",
                table: "PlantillaContableLineas",
                column: "PlantillaContableId");

            migrationBuilder.CreateIndex(
                name: "IX_PlantillasContables_EmpresaId",
                schema: "acc",
                table: "PlantillasContables",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_PlantillasContables_EmpresaId_Codigo",
                schema: "acc",
                table: "PlantillasContables",
                columns: new[] { "EmpresaId", "Codigo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlantillasContables_EmpresaId_TipoDocumento",
                schema: "acc",
                table: "PlantillasContables",
                columns: new[] { "EmpresaId", "TipoDocumento" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlantillaContableLineas",
                schema: "acc");

            migrationBuilder.DropTable(
                name: "PlantillasContables",
                schema: "acc");
        }
    }
}
