using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgoraHub360.ERP.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddFacturaVentaInternalModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FacturasVenta",
                schema: "vta",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VentaId = table.Column<long>(type: "bigint", nullable: false),
                    NumeroFactura = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    NumeroAutorizacion = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FechaEmision = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TipoDocumentoFactura = table.Column<byte>(type: "tinyint", nullable: false),
                    EstadoFactura = table.Column<byte>(type: "tinyint", nullable: false),
                    EstadoSiat = table.Column<byte>(type: "tinyint", nullable: false),
                    NitFactura = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Complemento = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    RazonSocialFactura = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    EmailFactura = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    TelefonoFactura = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    MonedaCodigo = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    TipoCambio = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    Subtotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    DescuentoTotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ImpuestoTotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Observaciones = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    MotivoAnulacion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    FechaAnulacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Cuf = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Cufd = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Cuis = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    CodigoControl = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    CodigoRecepcion = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    CodigoExcepcion = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Leyenda = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModificadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    EmpresaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FacturasVenta", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FacturasVenta_Ventas_VentaId",
                        column: x => x.VentaId,
                        principalSchema: "vta",
                        principalTable: "Ventas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FacturaVentaDetalles",
                schema: "vta",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FacturaVentaId = table.Column<long>(type: "bigint", nullable: false),
                    VentaDetalleId = table.Column<long>(type: "bigint", nullable: true),
                    TipoItemVenta = table.Column<byte>(type: "tinyint", nullable: false),
                    CodigoProducto = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Descripcion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Cantidad = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    UnidadMedida = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PrecioUnitario = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    DescuentoMonto = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ImpuestoMonto = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalLinea = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModificadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    EmpresaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FacturaVentaDetalles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FacturaVentaDetalles_FacturasVenta_FacturaVentaId",
                        column: x => x.FacturaVentaId,
                        principalSchema: "vta",
                        principalTable: "FacturasVenta",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FacturaVentaDetalles_VentaDetalles_VentaDetalleId",
                        column: x => x.VentaDetalleId,
                        principalSchema: "vta",
                        principalTable: "VentaDetalles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FacturasVenta_EmpresaId",
                schema: "vta",
                table: "FacturasVenta",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_FacturasVenta_EmpresaId_FechaEmision",
                schema: "vta",
                table: "FacturasVenta",
                columns: new[] { "EmpresaId", "FechaEmision" });

            migrationBuilder.CreateIndex(
                name: "IX_FacturasVenta_EmpresaId_NumeroFactura",
                schema: "vta",
                table: "FacturasVenta",
                columns: new[] { "EmpresaId", "NumeroFactura" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FacturasVenta_EmpresaId_VentaId",
                schema: "vta",
                table: "FacturasVenta",
                columns: new[] { "EmpresaId", "VentaId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FacturasVenta_EstadoFactura",
                schema: "vta",
                table: "FacturasVenta",
                column: "EstadoFactura");

            migrationBuilder.CreateIndex(
                name: "IX_FacturasVenta_NitFactura",
                schema: "vta",
                table: "FacturasVenta",
                column: "NitFactura");

            migrationBuilder.CreateIndex(
                name: "IX_FacturasVenta_VentaId",
                schema: "vta",
                table: "FacturasVenta",
                column: "VentaId");

            migrationBuilder.CreateIndex(
                name: "IX_FacturaVentaDetalles_EmpresaId",
                schema: "vta",
                table: "FacturaVentaDetalles",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_FacturaVentaDetalles_FacturaVentaId",
                schema: "vta",
                table: "FacturaVentaDetalles",
                column: "FacturaVentaId");

            migrationBuilder.CreateIndex(
                name: "IX_FacturaVentaDetalles_VentaDetalleId",
                schema: "vta",
                table: "FacturaVentaDetalles",
                column: "VentaDetalleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FacturaVentaDetalles",
                schema: "vta");

            migrationBuilder.DropTable(
                name: "FacturasVenta",
                schema: "vta");
        }
    }
}
