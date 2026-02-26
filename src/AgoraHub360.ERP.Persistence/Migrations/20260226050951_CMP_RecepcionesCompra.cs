using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgoraHub360.ERP.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CMP_RecepcionesCompra : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RecepcionesCompra",
                schema: "cmp",
                columns: table => new
                {
                    RecepcionCompraId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Numero = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    OrdenCompraId = table.Column<long>(type: "bigint", nullable: false),
                    FechaRecepcion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AlmacenId = table.Column<int>(type: "int", nullable: false),
                    DocumentoProveedor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Observaciones = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Confirmada = table.Column<bool>(type: "bit", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModificadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    EmpresaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecepcionesCompra", x => x.RecepcionCompraId);
                    table.ForeignKey(
                        name: "FK_RecepcionesCompra_Almacenes_AlmacenId",
                        column: x => x.AlmacenId,
                        principalSchema: "mdm",
                        principalTable: "Almacenes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RecepcionesCompra_OrdenesCompra_OrdenCompraId",
                        column: x => x.OrdenCompraId,
                        principalSchema: "cmp",
                        principalTable: "OrdenesCompra",
                        principalColumn: "OrdenCompraId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RecepcionCompraLineas",
                schema: "cmp",
                columns: table => new
                {
                    RecepcionCompraLineaId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RecepcionCompraId = table.Column<long>(type: "bigint", nullable: false),
                    OrdenCompraLineaId = table.Column<long>(type: "bigint", nullable: false),
                    CantidadRecibida = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    CostoUnitario = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Notas = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModificadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecepcionCompraLineas", x => x.RecepcionCompraLineaId);
                    table.ForeignKey(
                        name: "FK_RecepcionCompraLineas_OrdenCompraLineas_OrdenCompraLineaId",
                        column: x => x.OrdenCompraLineaId,
                        principalSchema: "cmp",
                        principalTable: "OrdenCompraLineas",
                        principalColumn: "OrdenCompraLineaId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RecepcionCompraLineas_RecepcionesCompra_RecepcionCompraId",
                        column: x => x.RecepcionCompraId,
                        principalSchema: "cmp",
                        principalTable: "RecepcionesCompra",
                        principalColumn: "RecepcionCompraId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RecepcionCompraLineas_OrdenCompraLineaId",
                schema: "cmp",
                table: "RecepcionCompraLineas",
                column: "OrdenCompraLineaId");

            migrationBuilder.CreateIndex(
                name: "IX_RecepcionCompraLineas_RecepcionCompraId",
                schema: "cmp",
                table: "RecepcionCompraLineas",
                column: "RecepcionCompraId");

            migrationBuilder.CreateIndex(
                name: "IX_RecepcionesCompra_AlmacenId",
                schema: "cmp",
                table: "RecepcionesCompra",
                column: "AlmacenId");

            migrationBuilder.CreateIndex(
                name: "IX_RecepcionesCompra_EmpresaId",
                schema: "cmp",
                table: "RecepcionesCompra",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_RecepcionesCompra_EmpresaId_Numero",
                schema: "cmp",
                table: "RecepcionesCompra",
                columns: new[] { "EmpresaId", "Numero" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RecepcionesCompra_EmpresaId_OrdenCompraId",
                schema: "cmp",
                table: "RecepcionesCompra",
                columns: new[] { "EmpresaId", "OrdenCompraId" });

            migrationBuilder.CreateIndex(
                name: "IX_RecepcionesCompra_OrdenCompraId",
                schema: "cmp",
                table: "RecepcionesCompra",
                column: "OrdenCompraId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RecepcionCompraLineas",
                schema: "cmp");

            migrationBuilder.DropTable(
                name: "RecepcionesCompra",
                schema: "cmp");
        }
    }
}
