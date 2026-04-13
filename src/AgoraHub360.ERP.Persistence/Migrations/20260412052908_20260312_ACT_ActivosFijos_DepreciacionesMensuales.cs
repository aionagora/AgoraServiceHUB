using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgoraHub360.ERP.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class _20260312_ACT_ActivosFijos_DepreciacionesMensuales : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "act");

            migrationBuilder.CreateTable(
                name: "ActivosFijos",
                schema: "act",
                columns: table => new
                {
                    ActivoFijoId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Codigo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    CategoriaActivo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CuentaContableId = table.Column<int>(type: "int", nullable: false),
                    CuentaDepreciacionId = table.Column<int>(type: "int", nullable: false),
                    CuentaGastoDepreciacionId = table.Column<int>(type: "int", nullable: false),
                    FechaAdquisicion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CostoAdquisicion = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ValorResidual = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TasaAnualDS24051 = table.Column<decimal>(type: "decimal(9,4)", precision: 9, scale: 4, nullable: false),
                    VidaUtilAnios = table.Column<int>(type: "int", nullable: false),
                    DepreciacionAcumulada = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    DepreciacionCompleta = table.Column<bool>(type: "bit", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModificadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    EmpresaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivosFijos", x => x.ActivoFijoId);
                    table.ForeignKey(
                        name: "FK_ActivosFijos_CuentasContables_CuentaContableId",
                        column: x => x.CuentaContableId,
                        principalSchema: "acc",
                        principalTable: "CuentasContables",
                        principalColumn: "CuentaContableId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ActivosFijos_CuentasContables_CuentaDepreciacionId",
                        column: x => x.CuentaDepreciacionId,
                        principalSchema: "acc",
                        principalTable: "CuentasContables",
                        principalColumn: "CuentaContableId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ActivosFijos_CuentasContables_CuentaGastoDepreciacionId",
                        column: x => x.CuentaGastoDepreciacionId,
                        principalSchema: "acc",
                        principalTable: "CuentasContables",
                        principalColumn: "CuentaContableId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DepreciacionesMensuales",
                schema: "act",
                columns: table => new
                {
                    DepreciacionMensualId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ActivoFijoId = table.Column<long>(type: "bigint", nullable: false),
                    PeriodoContableId = table.Column<int>(type: "int", nullable: false),
                    Monto = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AsientoContableId = table.Column<long>(type: "bigint", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModificadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DepreciacionesMensuales", x => x.DepreciacionMensualId);
                    table.ForeignKey(
                        name: "FK_DepreciacionesMensuales_ActivosFijos_ActivoFijoId",
                        column: x => x.ActivoFijoId,
                        principalSchema: "act",
                        principalTable: "ActivosFijos",
                        principalColumn: "ActivoFijoId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DepreciacionesMensuales_AsientosContables_AsientoContableId",
                        column: x => x.AsientoContableId,
                        principalSchema: "acc",
                        principalTable: "AsientosContables",
                        principalColumn: "AsientoContableId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DepreciacionesMensuales_PeriodosContables_PeriodoContableId",
                        column: x => x.PeriodoContableId,
                        principalSchema: "acc",
                        principalTable: "PeriodosContables",
                        principalColumn: "PeriodoContableId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ActivosFijos_Activo",
                schema: "act",
                table: "ActivosFijos",
                column: "Activo");

            migrationBuilder.CreateIndex(
                name: "IX_ActivosFijos_CuentaContableId",
                schema: "act",
                table: "ActivosFijos",
                column: "CuentaContableId");

            migrationBuilder.CreateIndex(
                name: "IX_ActivosFijos_CuentaDepreciacionId",
                schema: "act",
                table: "ActivosFijos",
                column: "CuentaDepreciacionId");

            migrationBuilder.CreateIndex(
                name: "IX_ActivosFijos_CuentaGastoDepreciacionId",
                schema: "act",
                table: "ActivosFijos",
                column: "CuentaGastoDepreciacionId");

            migrationBuilder.CreateIndex(
                name: "IX_ActivosFijos_EmpresaId",
                schema: "act",
                table: "ActivosFijos",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_ActivosFijos_EmpresaId_Codigo",
                schema: "act",
                table: "ActivosFijos",
                columns: new[] { "EmpresaId", "Codigo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DepreciacionesMensuales_Activo",
                schema: "act",
                table: "DepreciacionesMensuales",
                column: "Activo");

            migrationBuilder.CreateIndex(
                name: "IX_DepreciacionesMensuales_ActivoFijoId_PeriodoContableId",
                schema: "act",
                table: "DepreciacionesMensuales",
                columns: new[] { "ActivoFijoId", "PeriodoContableId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DepreciacionesMensuales_AsientoContableId",
                schema: "act",
                table: "DepreciacionesMensuales",
                column: "AsientoContableId");

            migrationBuilder.CreateIndex(
                name: "IX_DepreciacionesMensuales_PeriodoContableId",
                schema: "act",
                table: "DepreciacionesMensuales",
                column: "PeriodoContableId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DepreciacionesMensuales",
                schema: "act");

            migrationBuilder.DropTable(
                name: "ActivosFijos",
                schema: "act");
        }
    }
}
