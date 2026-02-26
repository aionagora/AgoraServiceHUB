using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgoraHub360.ERP.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CMP_Importaciones : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HojasImportacion",
                schema: "cmp",
                columns: table => new
                {
                    HojaImportacionId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Numero = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    OrdenCompraId = table.Column<long>(type: "bigint", nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReferenciaAduanera = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Observaciones = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    MetodoDistribucion = table.Column<byte>(type: "tinyint", nullable: false),
                    TotalGastos = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Liquidada = table.Column<bool>(type: "bit", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModificadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    EmpresaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HojasImportacion", x => x.HojaImportacionId);
                    table.ForeignKey(
                        name: "FK_HojasImportacion_OrdenesCompra_OrdenCompraId",
                        column: x => x.OrdenCompraId,
                        principalSchema: "cmp",
                        principalTable: "OrdenesCompra",
                        principalColumn: "OrdenCompraId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GastosImportacion",
                schema: "cmp",
                columns: table => new
                {
                    GastoImportacionId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HojaImportacionId = table.Column<long>(type: "bigint", nullable: false),
                    TipoGasto = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Monto = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    MonedaId = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    TasaCambio = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    MontoBase = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Referencia = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModificadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GastosImportacion", x => x.GastoImportacionId);
                    table.ForeignKey(
                        name: "FK_GastosImportacion_HojasImportacion_HojaImportacionId",
                        column: x => x.HojaImportacionId,
                        principalSchema: "cmp",
                        principalTable: "HojasImportacion",
                        principalColumn: "HojaImportacionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ImportacionLineas",
                schema: "cmp",
                columns: table => new
                {
                    ImportacionLineaId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HojaImportacionId = table.Column<long>(type: "bigint", nullable: false),
                    OrdenCompraLineaId = table.Column<long>(type: "bigint", nullable: false),
                    CostoFobUnitario = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    CostoFobTotal = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    FactorDistribucion = table.Column<decimal>(type: "decimal(18,8)", nullable: false),
                    GastoAsignado = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    CostoLandedUnitario = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    CostoLandedTotal = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModificadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImportacionLineas", x => x.ImportacionLineaId);
                    table.ForeignKey(
                        name: "FK_ImportacionLineas_HojasImportacion_HojaImportacionId",
                        column: x => x.HojaImportacionId,
                        principalSchema: "cmp",
                        principalTable: "HojasImportacion",
                        principalColumn: "HojaImportacionId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ImportacionLineas_OrdenCompraLineas_OrdenCompraLineaId",
                        column: x => x.OrdenCompraLineaId,
                        principalSchema: "cmp",
                        principalTable: "OrdenCompraLineas",
                        principalColumn: "OrdenCompraLineaId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GastosImportacion_HojaImportacionId",
                schema: "cmp",
                table: "GastosImportacion",
                column: "HojaImportacionId");

            migrationBuilder.CreateIndex(
                name: "IX_HojasImportacion_EmpresaId",
                schema: "cmp",
                table: "HojasImportacion",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_HojasImportacion_EmpresaId_Numero",
                schema: "cmp",
                table: "HojasImportacion",
                columns: new[] { "EmpresaId", "Numero" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HojasImportacion_EmpresaId_OrdenCompraId",
                schema: "cmp",
                table: "HojasImportacion",
                columns: new[] { "EmpresaId", "OrdenCompraId" });

            migrationBuilder.CreateIndex(
                name: "IX_HojasImportacion_OrdenCompraId",
                schema: "cmp",
                table: "HojasImportacion",
                column: "OrdenCompraId");

            migrationBuilder.CreateIndex(
                name: "IX_ImportacionLineas_HojaImportacionId",
                schema: "cmp",
                table: "ImportacionLineas",
                column: "HojaImportacionId");

            migrationBuilder.CreateIndex(
                name: "IX_ImportacionLineas_OrdenCompraLineaId",
                schema: "cmp",
                table: "ImportacionLineas",
                column: "OrdenCompraLineaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GastosImportacion",
                schema: "cmp");

            migrationBuilder.DropTable(
                name: "ImportacionLineas",
                schema: "cmp");

            migrationBuilder.DropTable(
                name: "HojasImportacion",
                schema: "cmp");
        }
    }
}
