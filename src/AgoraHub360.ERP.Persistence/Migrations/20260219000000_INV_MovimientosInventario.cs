using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgoraHub360.ERP.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class INV_MovimientosInventario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "inv");

            migrationBuilder.CreateTable(
                name: "MovimientosInventario",
                schema: "inv",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Numero = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    TipoMovimiento = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FechaMovimiento = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProductoId = table.Column<int>(type: "int", nullable: false),
                    AlmacenId = table.Column<int>(type: "int", nullable: false),
                    AlmacenDestinoId = table.Column<int>(type: "int", nullable: true),
                    Cantidad = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    CostoUnitario = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    CostoTotal = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Referencia = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Observaciones = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    EmpresaId = table.Column<int>(type: "int", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModificadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovimientosInventario", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MovimientosInventario_Almacenes_AlmacenId",
                        column: x => x.AlmacenId,
                        principalSchema: "mdm",
                        principalTable: "Almacenes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MovimientosInventario_Almacenes_AlmacenDestinoId",
                        column: x => x.AlmacenDestinoId,
                        principalSchema: "mdm",
                        principalTable: "Almacenes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MovimientosInventario_Productos_ProductoId",
                        column: x => x.ProductoId,
                        principalSchema: "mdm",
                        principalTable: "Productos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StockProductos",
                schema: "inv",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductoId = table.Column<int>(type: "int", nullable: false),
                    AlmacenId = table.Column<int>(type: "int", nullable: false),
                    StockActual = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    CostoPromedio = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    UltimaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EmpresaId = table.Column<int>(type: "int", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModificadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockProductos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StockProductos_Almacenes_AlmacenId",
                        column: x => x.AlmacenId,
                        principalSchema: "mdm",
                        principalTable: "Almacenes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StockProductos_Productos_ProductoId",
                        column: x => x.ProductoId,
                        principalSchema: "mdm",
                        principalTable: "Productos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            // Índices MovimientosInventario
            migrationBuilder.CreateIndex(
                name: "IX_MovimientosInventario_EmpresaId",
                schema: "inv",
                table: "MovimientosInventario",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosInventario_EmpresaId_Numero",
                schema: "inv",
                table: "MovimientosInventario",
                columns: new[] { "EmpresaId", "Numero" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosInventario_EmpresaId_ProductoId_FechaMovimiento",
                schema: "inv",
                table: "MovimientosInventario",
                columns: new[] { "EmpresaId", "ProductoId", "FechaMovimiento" });

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosInventario_EmpresaId_AlmacenId",
                schema: "inv",
                table: "MovimientosInventario",
                columns: new[] { "EmpresaId", "AlmacenId" });

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosInventario_AlmacenDestinoId",
                schema: "inv",
                table: "MovimientosInventario",
                column: "AlmacenDestinoId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosInventario_ProductoId",
                schema: "inv",
                table: "MovimientosInventario",
                column: "ProductoId");

            // Índices StockProductos
            migrationBuilder.CreateIndex(
                name: "IX_StockProductos_EmpresaId",
                schema: "inv",
                table: "StockProductos",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_StockProductos_EmpresaId_ProductoId_AlmacenId",
                schema: "inv",
                table: "StockProductos",
                columns: new[] { "EmpresaId", "ProductoId", "AlmacenId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StockProductos_AlmacenId",
                schema: "inv",
                table: "StockProductos",
                column: "AlmacenId");

            migrationBuilder.CreateIndex(
                name: "IX_StockProductos_ProductoId",
                schema: "inv",
                table: "StockProductos",
                column: "ProductoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MovimientosInventario",
                schema: "inv");

            migrationBuilder.DropTable(
                name: "StockProductos",
                schema: "inv");
        }
    }
}
