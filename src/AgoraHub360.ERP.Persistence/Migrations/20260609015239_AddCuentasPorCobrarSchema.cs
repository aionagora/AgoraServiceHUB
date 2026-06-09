using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgoraHub360.ERP.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCuentasPorCobrarSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "cxc");

            migrationBuilder.CreateTable(
                name: "CuentasPorCobrar",
                schema: "cxc",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FacturaVentaId = table.Column<long>(type: "bigint", nullable: false),
                    ClienteId = table.Column<int>(type: "int", nullable: true),
                    ClienteNombre = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ClienteNit = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    NumeroFactura = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    NumeroVenta = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    FechaEmision = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaVencimiento = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MonedaCodigo = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    TipoCambio = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    TotalFactura = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalPagado = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    SaldoPendiente = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, computedColumnSql: "[TotalFactura] - [TotalPagado]", stored: true),
                    Estado = table.Column<byte>(type: "tinyint", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModificadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    EmpresaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CuentasPorCobrar", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CuentasPorCobrar_FacturasVenta_FacturaVentaId",
                        column: x => x.FacturaVentaId,
                        principalSchema: "vta",
                        principalTable: "FacturasVenta",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CuentasPorCobrar_EmpresaId",
                schema: "cxc",
                table: "CuentasPorCobrar",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_CuentasPorCobrar_EmpresaId_ClienteId",
                schema: "cxc",
                table: "CuentasPorCobrar",
                columns: new[] { "EmpresaId", "ClienteId" });

            migrationBuilder.CreateIndex(
                name: "IX_CuentasPorCobrar_EmpresaId_Estado",
                schema: "cxc",
                table: "CuentasPorCobrar",
                columns: new[] { "EmpresaId", "Estado" });

            migrationBuilder.CreateIndex(
                name: "IX_CuentasPorCobrar_EmpresaId_FacturaVentaId",
                schema: "cxc",
                table: "CuentasPorCobrar",
                columns: new[] { "EmpresaId", "FacturaVentaId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CuentasPorCobrar_EmpresaId_FechaVencimiento",
                schema: "cxc",
                table: "CuentasPorCobrar",
                columns: new[] { "EmpresaId", "FechaVencimiento" });

            migrationBuilder.CreateIndex(
                name: "IX_CuentasPorCobrar_FacturaVentaId",
                schema: "cxc",
                table: "CuentasPorCobrar",
                column: "FacturaVentaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CuentasPorCobrar",
                schema: "cxc");
        }
    }
}
