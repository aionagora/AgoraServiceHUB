using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgoraHub360.ERP.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ACC_AsientosContables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AsientosContables",
                schema: "acc",
                columns: table => new
                {
                    AsientoContableId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Numero = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Glosa = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    OrigenTipo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    OrigenId = table.Column<long>(type: "bigint", nullable: true),
                    OrigenReferencia = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TotalDebe = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    TotalHaber = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModificadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    EmpresaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AsientosContables", x => x.AsientoContableId);
                });

            migrationBuilder.CreateTable(
                name: "AsientoContableLineas",
                schema: "acc",
                columns: table => new
                {
                    AsientoContableLineaId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AsientoContableId = table.Column<long>(type: "bigint", nullable: false),
                    NumeroLinea = table.Column<int>(type: "int", nullable: false),
                    CuentaContableId = table.Column<int>(type: "int", nullable: false),
                    Debe = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Haber = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Glosa = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Referencia = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModificadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AsientoContableLineas", x => x.AsientoContableLineaId);
                    table.ForeignKey(
                        name: "FK_AsientoContableLineas_AsientosContables_AsientoContableId",
                        column: x => x.AsientoContableId,
                        principalSchema: "acc",
                        principalTable: "AsientosContables",
                        principalColumn: "AsientoContableId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AsientoContableLineas_CuentasContables_CuentaContableId",
                        column: x => x.CuentaContableId,
                        principalSchema: "acc",
                        principalTable: "CuentasContables",
                        principalColumn: "CuentaContableId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AsientoContableLineas_AsientoContableId",
                schema: "acc",
                table: "AsientoContableLineas",
                column: "AsientoContableId");

            migrationBuilder.CreateIndex(
                name: "IX_AsientoContableLineas_CuentaContableId",
                schema: "acc",
                table: "AsientoContableLineas",
                column: "CuentaContableId");

            migrationBuilder.CreateIndex(
                name: "IX_AsientosContables_EmpresaId",
                schema: "acc",
                table: "AsientosContables",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_AsientosContables_EmpresaId_Estado",
                schema: "acc",
                table: "AsientosContables",
                columns: new[] { "EmpresaId", "Estado" });

            migrationBuilder.CreateIndex(
                name: "IX_AsientosContables_EmpresaId_Fecha",
                schema: "acc",
                table: "AsientosContables",
                columns: new[] { "EmpresaId", "Fecha" });

            migrationBuilder.CreateIndex(
                name: "IX_AsientosContables_EmpresaId_Numero",
                schema: "acc",
                table: "AsientosContables",
                columns: new[] { "EmpresaId", "Numero" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AsientosContables_OrigenTipo_OrigenId",
                schema: "acc",
                table: "AsientosContables",
                columns: new[] { "OrigenTipo", "OrigenId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AsientoContableLineas",
                schema: "acc");

            migrationBuilder.DropTable(
                name: "AsientosContables",
                schema: "acc");
        }
    }
}
