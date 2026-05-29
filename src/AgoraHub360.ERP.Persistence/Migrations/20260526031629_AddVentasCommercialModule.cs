using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgoraHub360.ERP.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddVentasCommercialModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Ventas",
                schema: "vta",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SucursalId = table.Column<int>(type: "int", nullable: false),
                    AlmacenId = table.Column<int>(type: "int", nullable: true),
                    ClienteId = table.Column<int>(type: "int", nullable: true),
                    PedidoVentaId = table.Column<long>(type: "bigint", nullable: true),
                    NumeroVenta = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FechaVenta = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TipoVenta = table.Column<byte>(type: "tinyint", nullable: false),
                    EstadoVenta = table.Column<byte>(type: "tinyint", nullable: false),
                    EstadoPago = table.Column<byte>(type: "tinyint", nullable: false),
                    MonedaId = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: true),
                    MonedaCodigo = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    TipoCambio = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    Subtotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    DescuentoTotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ImpuestoTotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Observaciones = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    FacturaGenerada = table.Column<bool>(type: "bit", nullable: false),
                    InventarioDescontado = table.Column<bool>(type: "bit", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModificadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    EmpresaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ventas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Ventas_Almacenes_AlmacenId",
                        column: x => x.AlmacenId,
                        principalSchema: "mdm",
                        principalTable: "Almacenes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Ventas_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalSchema: "mdm",
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Ventas_Monedas_MonedaId",
                        column: x => x.MonedaId,
                        principalSchema: "core",
                        principalTable: "Monedas",
                        principalColumn: "Codigo",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Ventas_PedidosVenta_PedidoVentaId",
                        column: x => x.PedidoVentaId,
                        principalSchema: "vta",
                        principalTable: "PedidosVenta",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Ventas_Sucursales_SucursalId",
                        column: x => x.SucursalId,
                        principalSchema: "core",
                        principalTable: "Sucursales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "VentaDetalles",
                schema: "vta",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VentaId = table.Column<long>(type: "bigint", nullable: false),
                    TipoItemVenta = table.Column<byte>(type: "tinyint", nullable: false),
                    CompanyProductId = table.Column<long>(type: "bigint", nullable: true),
                    AlmacenId = table.Column<int>(type: "int", nullable: true),
                    Descripcion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Cantidad = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    UnidadMedidaId = table.Column<int>(type: "int", nullable: true),
                    PrecioUnitario = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    DescuentoPorcentaje = table.Column<decimal>(type: "decimal(9,4)", precision: 9, scale: 4, nullable: false),
                    DescuentoMonto = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ImpuestoMonto = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalLinea = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CostoUnitario = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    DescuentaInventario = table.Column<bool>(type: "bit", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModificadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    EmpresaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VentaDetalles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VentaDetalles_Almacenes_AlmacenId",
                        column: x => x.AlmacenId,
                        principalSchema: "mdm",
                        principalTable: "Almacenes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VentaDetalles_CompanyProducts_CompanyProductId",
                        column: x => x.CompanyProductId,
                        principalSchema: "mdm",
                        principalTable: "CompanyProducts",
                        principalColumn: "CompanyProductId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VentaDetalles_Uoms_UnidadMedidaId",
                        column: x => x.UnidadMedidaId,
                        principalSchema: "mdm",
                        principalTable: "Uoms",
                        principalColumn: "UomId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VentaDetalles_Ventas_VentaId",
                        column: x => x.VentaId,
                        principalSchema: "vta",
                        principalTable: "Ventas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VentaFacturacionDatos",
                schema: "vta",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VentaId = table.Column<long>(type: "bigint", nullable: false),
                    Facturar = table.Column<bool>(type: "bit", nullable: false),
                    FacturarAlMismoCliente = table.Column<bool>(type: "bit", nullable: false),
                    TipoDocumentoIdentidad = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    NitFactura = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Complemento = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    RazonSocialFactura = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    EmailFactura = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    TelefonoFactura = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    EstadoFactura = table.Column<byte>(type: "tinyint", nullable: false),
                    FacturaId = table.Column<long>(type: "bigint", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModificadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    EmpresaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VentaFacturacionDatos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VentaFacturacionDatos_Ventas_VentaId",
                        column: x => x.VentaId,
                        principalSchema: "vta",
                        principalTable: "Ventas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VentaPagos",
                schema: "vta",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VentaId = table.Column<long>(type: "bigint", nullable: false),
                    FechaPago = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TipoPago = table.Column<byte>(type: "tinyint", nullable: false),
                    ModoPago = table.Column<byte>(type: "tinyint", nullable: false),
                    CuentaCajaBancoId = table.Column<long>(type: "bigint", nullable: true),
                    Monto = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    MonedaId = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: true),
                    MonedaCodigo = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    TipoCambio = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    Referencia = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    EstadoPago = table.Column<byte>(type: "tinyint", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModificadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    EmpresaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VentaPagos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VentaPagos_Ventas_VentaId",
                        column: x => x.VentaId,
                        principalSchema: "vta",
                        principalTable: "Ventas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VentaDetalles_AlmacenId",
                schema: "vta",
                table: "VentaDetalles",
                column: "AlmacenId");

            migrationBuilder.CreateIndex(
                name: "IX_VentaDetalles_CompanyProductId",
                schema: "vta",
                table: "VentaDetalles",
                column: "CompanyProductId");

            migrationBuilder.CreateIndex(
                name: "IX_VentaDetalles_EmpresaId",
                schema: "vta",
                table: "VentaDetalles",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_VentaDetalles_UnidadMedidaId",
                schema: "vta",
                table: "VentaDetalles",
                column: "UnidadMedidaId");

            migrationBuilder.CreateIndex(
                name: "IX_VentaDetalles_VentaId",
                schema: "vta",
                table: "VentaDetalles",
                column: "VentaId");

            migrationBuilder.CreateIndex(
                name: "IX_VentaFacturacionDatos_EmpresaId",
                schema: "vta",
                table: "VentaFacturacionDatos",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_VentaFacturacionDatos_VentaId",
                schema: "vta",
                table: "VentaFacturacionDatos",
                column: "VentaId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VentaPagos_EmpresaId",
                schema: "vta",
                table: "VentaPagos",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_VentaPagos_VentaId",
                schema: "vta",
                table: "VentaPagos",
                column: "VentaId");

            migrationBuilder.CreateIndex(
                name: "IX_Ventas_AlmacenId",
                schema: "vta",
                table: "Ventas",
                column: "AlmacenId");

            migrationBuilder.CreateIndex(
                name: "IX_Ventas_ClienteId",
                schema: "vta",
                table: "Ventas",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_Ventas_EmpresaId",
                schema: "vta",
                table: "Ventas",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_Ventas_EmpresaId_ClienteId",
                schema: "vta",
                table: "Ventas",
                columns: new[] { "EmpresaId", "ClienteId" });

            migrationBuilder.CreateIndex(
                name: "IX_Ventas_EmpresaId_FechaVenta",
                schema: "vta",
                table: "Ventas",
                columns: new[] { "EmpresaId", "FechaVenta" });

            migrationBuilder.CreateIndex(
                name: "IX_Ventas_EmpresaId_NumeroVenta",
                schema: "vta",
                table: "Ventas",
                columns: new[] { "EmpresaId", "NumeroVenta" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Ventas_MonedaId",
                schema: "vta",
                table: "Ventas",
                column: "MonedaId");

            migrationBuilder.CreateIndex(
                name: "IX_Ventas_PedidoVentaId",
                schema: "vta",
                table: "Ventas",
                column: "PedidoVentaId");

            migrationBuilder.CreateIndex(
                name: "IX_Ventas_SucursalId",
                schema: "vta",
                table: "Ventas",
                column: "SucursalId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VentaDetalles",
                schema: "vta");

            migrationBuilder.DropTable(
                name: "VentaFacturacionDatos",
                schema: "vta");

            migrationBuilder.DropTable(
                name: "VentaPagos",
                schema: "vta");

            migrationBuilder.DropTable(
                name: "Ventas",
                schema: "vta");
        }
    }
}
