using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgoraHub360.ERP.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CMP_OrdenesCompra : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "cmp");

            migrationBuilder.CreateTable(
                name: "OrdenesCompra",
                schema: "cmp",
                columns: table => new
                {
                    OrdenCompraId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Numero = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    FechaEmision = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaEntregaEstimada = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ProveedorId = table.Column<int>(type: "int", nullable: false),
                    AlmacenDestinoId = table.Column<int>(type: "int", nullable: false),
                    MonedaId = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    TasaCambio = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    Estado = table.Column<byte>(type: "tinyint", nullable: false),
                    CondicionPago = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Observaciones = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ReferenciaExterna = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Subtotal = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Descuento = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Impuesto = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModificadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    EmpresaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrdenesCompra", x => x.OrdenCompraId);
                    table.ForeignKey(
                        name: "FK_OrdenesCompra_Almacenes_AlmacenDestinoId",
                        column: x => x.AlmacenDestinoId,
                        principalSchema: "mdm",
                        principalTable: "Almacenes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrdenesCompra_Proveedores_ProveedorId",
                        column: x => x.ProveedorId,
                        principalSchema: "mdm",
                        principalTable: "Proveedores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OrdenCompraLineas",
                schema: "cmp",
                columns: table => new
                {
                    OrdenCompraLineaId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrdenCompraId = table.Column<long>(type: "bigint", nullable: false),
                    NumeroLinea = table.Column<int>(type: "int", nullable: false),
                    CompanyProductId = table.Column<long>(type: "bigint", nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    UnidadMedida = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Cantidad = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    PrecioUnitario = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    PorcentajeDescuento = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    MontoDescuento = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Subtotal = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    PorcentajeImpuesto = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    MontoImpuesto = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    TotalLinea = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    CantidadRecepcionada = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModificadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrdenCompraLineas", x => x.OrdenCompraLineaId);
                    table.ForeignKey(
                        name: "FK_OrdenCompraLineas_CompanyProducts_CompanyProductId",
                        column: x => x.CompanyProductId,
                        principalSchema: "mdm",
                        principalTable: "CompanyProducts",
                        principalColumn: "CompanyProductId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrdenCompraLineas_OrdenesCompra_OrdenCompraId",
                        column: x => x.OrdenCompraId,
                        principalSchema: "cmp",
                        principalTable: "OrdenesCompra",
                        principalColumn: "OrdenCompraId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrdenCompraLineas_CompanyProductId",
                schema: "cmp",
                table: "OrdenCompraLineas",
                column: "CompanyProductId");

            migrationBuilder.CreateIndex(
                name: "IX_OrdenCompraLineas_OrdenCompraId_NumeroLinea",
                schema: "cmp",
                table: "OrdenCompraLineas",
                columns: new[] { "OrdenCompraId", "NumeroLinea" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrdenesCompra_AlmacenDestinoId",
                schema: "cmp",
                table: "OrdenesCompra",
                column: "AlmacenDestinoId");

            migrationBuilder.CreateIndex(
                name: "IX_OrdenesCompra_EmpresaId",
                schema: "cmp",
                table: "OrdenesCompra",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_OrdenesCompra_EmpresaId_Estado",
                schema: "cmp",
                table: "OrdenesCompra",
                columns: new[] { "EmpresaId", "Estado" });

            migrationBuilder.CreateIndex(
                name: "IX_OrdenesCompra_EmpresaId_FechaEmision",
                schema: "cmp",
                table: "OrdenesCompra",
                columns: new[] { "EmpresaId", "FechaEmision" });

            migrationBuilder.CreateIndex(
                name: "IX_OrdenesCompra_EmpresaId_Numero",
                schema: "cmp",
                table: "OrdenesCompra",
                columns: new[] { "EmpresaId", "Numero" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrdenesCompra_EmpresaId_ProveedorId",
                schema: "cmp",
                table: "OrdenesCompra",
                columns: new[] { "EmpresaId", "ProveedorId" });

            migrationBuilder.CreateIndex(
                name: "IX_OrdenesCompra_ProveedorId",
                schema: "cmp",
                table: "OrdenesCompra",
                column: "ProveedorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrdenCompraLineas",
                schema: "cmp");

            migrationBuilder.DropTable(
                name: "OrdenesCompra",
                schema: "cmp");
        }
    }
}
