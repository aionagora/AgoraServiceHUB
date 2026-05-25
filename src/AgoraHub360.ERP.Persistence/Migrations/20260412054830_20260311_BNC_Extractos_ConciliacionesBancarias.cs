using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgoraHub360.ERP.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class _20260311_BNC_Extractos_ConciliacionesBancarias : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "bnc");

            migrationBuilder.CreateTable(
                name: "ConciliacionesBancarias",
                schema: "bnc",
                columns: table => new
                {
                    ConciliacionBancariaId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CuentaContableId = table.Column<int>(type: "int", nullable: false),
                    PeriodoContableId = table.Column<int>(type: "int", nullable: false),
                    SaldoExtracto = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    SaldoContable = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
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
                    table.PrimaryKey("PK_ConciliacionesBancarias", x => x.ConciliacionBancariaId);
                    table.ForeignKey(
                        name: "FK_ConciliacionesBancarias_CuentasContables_CuentaContableId",
                        column: x => x.CuentaContableId,
                        principalSchema: "acc",
                        principalTable: "CuentasContables",
                        principalColumn: "CuentaContableId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ConciliacionesBancarias_PeriodosContables_PeriodoContableId",
                        column: x => x.PeriodoContableId,
                        principalSchema: "acc",
                        principalTable: "PeriodosContables",
                        principalColumn: "PeriodoContableId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ExtractosBancarios",
                schema: "bnc",
                columns: table => new
                {
                    ExtractoBancarioId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CuentaContableId = table.Column<int>(type: "int", nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Monto = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    NumeroReferencia = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Conciliado = table.Column<bool>(type: "bit", nullable: false),
                    AsientoContableLineaId = table.Column<long>(type: "bigint", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModificadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    EmpresaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExtractosBancarios", x => x.ExtractoBancarioId);
                    table.ForeignKey(
                        name: "FK_ExtractosBancarios_AsientoContableLineas_AsientoContableLineaId",
                        column: x => x.AsientoContableLineaId,
                        principalSchema: "acc",
                        principalTable: "AsientoContableLineas",
                        principalColumn: "AsientoContableLineaId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExtractosBancarios_CuentasContables_CuentaContableId",
                        column: x => x.CuentaContableId,
                        principalSchema: "acc",
                        principalTable: "CuentasContables",
                        principalColumn: "CuentaContableId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PresupuestosContables",
                schema: "acc",
                columns: table => new
                {
                    PresupuestoContableId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Gestion = table.Column<int>(type: "int", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Observaciones = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModificadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    EmpresaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PresupuestosContables", x => x.PresupuestoContableId);
                });

            migrationBuilder.CreateTable(
                name: "PresupuestosContablesLineas",
                schema: "acc",
                columns: table => new
                {
                    PresupuestoContableLineaId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PresupuestoContableId = table.Column<int>(type: "int", nullable: false),
                    CuentaContableId = table.Column<int>(type: "int", nullable: false),
                    CentroCostoId = table.Column<int>(type: "int", nullable: true),
                    Mes = table.Column<int>(type: "int", nullable: false),
                    MontoPresupuestado = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModificadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PresupuestosContablesLineas", x => x.PresupuestoContableLineaId);
                    table.ForeignKey(
                        name: "FK_PresupuestosContablesLineas_CentrosCosto_CentroCostoId",
                        column: x => x.CentroCostoId,
                        principalSchema: "cst",
                        principalTable: "CentrosCosto",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PresupuestosContablesLineas_CuentasContables_CuentaContableId",
                        column: x => x.CuentaContableId,
                        principalSchema: "acc",
                        principalTable: "CuentasContables",
                        principalColumn: "CuentaContableId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PresupuestosContablesLineas_PresupuestosContables_PresupuestoContableId",
                        column: x => x.PresupuestoContableId,
                        principalSchema: "acc",
                        principalTable: "PresupuestosContables",
                        principalColumn: "PresupuestoContableId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConciliacionesBancarias_Activo",
                schema: "bnc",
                table: "ConciliacionesBancarias",
                column: "Activo");

            migrationBuilder.CreateIndex(
                name: "IX_ConciliacionesBancarias_CuentaContableId",
                schema: "bnc",
                table: "ConciliacionesBancarias",
                column: "CuentaContableId");

            migrationBuilder.CreateIndex(
                name: "IX_ConciliacionesBancarias_EmpresaId",
                schema: "bnc",
                table: "ConciliacionesBancarias",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_ConciliacionesBancarias_EmpresaId_CuentaContableId_PeriodoContableId",
                schema: "bnc",
                table: "ConciliacionesBancarias",
                columns: new[] { "EmpresaId", "CuentaContableId", "PeriodoContableId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ConciliacionesBancarias_PeriodoContableId",
                schema: "bnc",
                table: "ConciliacionesBancarias",
                column: "PeriodoContableId");

            migrationBuilder.CreateIndex(
                name: "IX_ExtractosBancarios_Activo",
                schema: "bnc",
                table: "ExtractosBancarios",
                column: "Activo");

            migrationBuilder.CreateIndex(
                name: "IX_ExtractosBancarios_AsientoContableLineaId",
                schema: "bnc",
                table: "ExtractosBancarios",
                column: "AsientoContableLineaId");

            migrationBuilder.CreateIndex(
                name: "IX_ExtractosBancarios_CuentaContableId",
                schema: "bnc",
                table: "ExtractosBancarios",
                column: "CuentaContableId");

            migrationBuilder.CreateIndex(
                name: "IX_ExtractosBancarios_EmpresaId",
                schema: "bnc",
                table: "ExtractosBancarios",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_ExtractosBancarios_EmpresaId_CuentaContableId_Fecha",
                schema: "bnc",
                table: "ExtractosBancarios",
                columns: new[] { "EmpresaId", "CuentaContableId", "Fecha" });

            migrationBuilder.CreateIndex(
                name: "IX_PresupuestosContables_Activo",
                schema: "acc",
                table: "PresupuestosContables",
                column: "Activo");

            migrationBuilder.CreateIndex(
                name: "IX_PresupuestosContables_EmpresaId",
                schema: "acc",
                table: "PresupuestosContables",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_PresupuestosContables_EmpresaId_Gestion_Estado",
                schema: "acc",
                table: "PresupuestosContables",
                columns: new[] { "EmpresaId", "Gestion", "Estado" });

            migrationBuilder.CreateIndex(
                name: "IX_PresupuestosContablesLineas_Activo",
                schema: "acc",
                table: "PresupuestosContablesLineas",
                column: "Activo");

            migrationBuilder.CreateIndex(
                name: "IX_PresupuestosContablesLineas_CentroCostoId",
                schema: "acc",
                table: "PresupuestosContablesLineas",
                column: "CentroCostoId");

            migrationBuilder.CreateIndex(
                name: "IX_PresupuestosContablesLineas_CuentaContableId",
                schema: "acc",
                table: "PresupuestosContablesLineas",
                column: "CuentaContableId");

            migrationBuilder.CreateIndex(
                name: "IX_PresupuestosContablesLineas_PresupuestoContableId_CuentaContableId_Mes_CentroCostoId",
                schema: "acc",
                table: "PresupuestosContablesLineas",
                columns: new[] { "PresupuestoContableId", "CuentaContableId", "Mes", "CentroCostoId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConciliacionesBancarias",
                schema: "bnc");

            migrationBuilder.DropTable(
                name: "ExtractosBancarios",
                schema: "bnc");

            migrationBuilder.DropTable(
                name: "PresupuestosContablesLineas",
                schema: "acc");

            migrationBuilder.DropTable(
                name: "PresupuestosContables",
                schema: "acc");
        }
    }
}
