using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgoraHub360.ERP.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class _20260312000000_TRB_RegistrosImpuesto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "trb");

            migrationBuilder.CreateTable(
                name: "RegistrosImpuesto",
                schema: "trb",
                columns: table => new
                {
                    RegistroImpuestoId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PeriodoContableId = table.Column<int>(type: "int", nullable: false),
                    TipoImpuesto = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    BaseImponible = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Tasa = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    MontoCalculado = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CreditoFiscal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    DebitoFiscal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    SaldoAFavor = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    MontoAPagar = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    NumeroCertificado = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FechaDeclaracion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AsientoContableId = table.Column<long>(type: "bigint", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModificadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    EmpresaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegistrosImpuesto", x => x.RegistroImpuestoId);
                    table.ForeignKey(
                        name: "FK_RegistrosImpuesto_AsientosContables_AsientoContableId",
                        column: x => x.AsientoContableId,
                        principalSchema: "acc",
                        principalTable: "AsientosContables",
                        principalColumn: "AsientoContableId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RegistrosImpuesto_PeriodosContables_PeriodoContableId",
                        column: x => x.PeriodoContableId,
                        principalSchema: "acc",
                        principalTable: "PeriodosContables",
                        principalColumn: "PeriodoContableId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RegistrosImpuesto_Activo",
                schema: "trb",
                table: "RegistrosImpuesto",
                column: "Activo");

            migrationBuilder.CreateIndex(
                name: "IX_RegistrosImpuesto_AsientoContableId",
                schema: "trb",
                table: "RegistrosImpuesto",
                column: "AsientoContableId");

            migrationBuilder.CreateIndex(
                name: "IX_RegistrosImpuesto_EmpresaId",
                schema: "trb",
                table: "RegistrosImpuesto",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_RegistrosImpuesto_EmpresaId_PeriodoContableId_TipoImpuesto",
                schema: "trb",
                table: "RegistrosImpuesto",
                columns: new[] { "EmpresaId", "PeriodoContableId", "TipoImpuesto" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RegistrosImpuesto_PeriodoContableId",
                schema: "trb",
                table: "RegistrosImpuesto",
                column: "PeriodoContableId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RegistrosImpuesto",
                schema: "trb");
        }
    }
}
