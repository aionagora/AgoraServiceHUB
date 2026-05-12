using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgoraHub360.ERP.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class VTA_PedidosVent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "vta");

            migrationBuilder.AddColumn<int>(
                name: "SucursalId",
                schema: "mdm",
                table: "Almacenes",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PedidosVenta",
                schema: "vta",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Numero = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FechaEmision = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaEntregaEsperada = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SucursalId = table.Column<int>(type: "int", nullable: false),
                    AlmacenId = table.Column<int>(type: "int", nullable: false),
                    VendedorId = table.Column<int>(type: "int", nullable: false),
                    ClienteId = table.Column<int>(type: "int", nullable: false),
                    ClienteSucursalId = table.Column<int>(type: "int", nullable: true),
                    UsuarioClienteId = table.Column<int>(type: "int", nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Subtotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Impuestos = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Observaciones = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModificadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    EmpresaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PedidosVenta", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PedidosVenta_Almacenes_AlmacenId",
                        column: x => x.AlmacenId,
                        principalSchema: "mdm",
                        principalTable: "Almacenes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PedidosVenta_ClienteSucursales_ClienteSucursalId",
                        column: x => x.ClienteSucursalId,
                        principalSchema: "mdm",
                        principalTable: "ClienteSucursales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PedidosVenta_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalSchema: "mdm",
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PedidosVenta_Sucursales_SucursalId",
                        column: x => x.SucursalId,
                        principalSchema: "core",
                        principalTable: "Sucursales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PedidosVenta_Usuarios_UsuarioClienteId",
                        column: x => x.UsuarioClienteId,
                        principalSchema: "core",
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PedidosVenta_Usuarios_VendedorId",
                        column: x => x.VendedorId,
                        principalSchema: "core",
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PedidoVentaDetalles",
                schema: "vta",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PedidoVentaId = table.Column<long>(type: "bigint", nullable: false),
                    CompanyProductId = table.Column<long>(type: "bigint", nullable: false),
                    CantidadSolicitada = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    CantidadConfirmada = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    CantidadDespachada = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    PrecioUnitario = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Subtotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Impuestos = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
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
                    table.PrimaryKey("PK_PedidoVentaDetalles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PedidoVentaDetalles_CompanyProducts_CompanyProductId",
                        column: x => x.CompanyProductId,
                        principalSchema: "mdm",
                        principalTable: "CompanyProducts",
                        principalColumn: "CompanyProductId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PedidoVentaDetalles_PedidosVenta_PedidoVentaId",
                        column: x => x.PedidoVentaId,
                        principalSchema: "vta",
                        principalTable: "PedidosVenta",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Almacenes_SucursalId",
                schema: "mdm",
                table: "Almacenes",
                column: "SucursalId");

            migrationBuilder.CreateIndex(
                name: "IX_PedidosVenta_AlmacenId",
                schema: "vta",
                table: "PedidosVenta",
                column: "AlmacenId");

            migrationBuilder.CreateIndex(
                name: "IX_PedidosVenta_ClienteId",
                schema: "vta",
                table: "PedidosVenta",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_PedidosVenta_ClienteSucursalId",
                schema: "vta",
                table: "PedidosVenta",
                column: "ClienteSucursalId");

            migrationBuilder.CreateIndex(
                name: "IX_PedidosVenta_EmpresaId",
                schema: "vta",
                table: "PedidosVenta",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_PedidosVenta_EmpresaId_Numero",
                schema: "vta",
                table: "PedidosVenta",
                columns: new[] { "EmpresaId", "Numero" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PedidosVenta_SucursalId",
                schema: "vta",
                table: "PedidosVenta",
                column: "SucursalId");

            migrationBuilder.CreateIndex(
                name: "IX_PedidosVenta_UsuarioClienteId",
                schema: "vta",
                table: "PedidosVenta",
                column: "UsuarioClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_PedidosVenta_VendedorId",
                schema: "vta",
                table: "PedidosVenta",
                column: "VendedorId");

            migrationBuilder.CreateIndex(
                name: "IX_PedidoVentaDetalles_CompanyProductId",
                schema: "vta",
                table: "PedidoVentaDetalles",
                column: "CompanyProductId");

            migrationBuilder.CreateIndex(
                name: "IX_PedidoVentaDetalles_EmpresaId",
                schema: "vta",
                table: "PedidoVentaDetalles",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_PedidoVentaDetalles_PedidoVentaId",
                schema: "vta",
                table: "PedidoVentaDetalles",
                column: "PedidoVentaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Almacenes_Sucursales_SucursalId",
                schema: "mdm",
                table: "Almacenes",
                column: "SucursalId",
                principalSchema: "core",
                principalTable: "Sucursales",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Almacenes_Sucursales_SucursalId",
                schema: "mdm",
                table: "Almacenes");

            migrationBuilder.DropTable(
                name: "PedidoVentaDetalles",
                schema: "vta");

            migrationBuilder.DropTable(
                name: "PedidosVenta",
                schema: "vta");

            migrationBuilder.DropIndex(
                name: "IX_Almacenes_SucursalId",
                schema: "mdm",
                table: "Almacenes");

            migrationBuilder.DropColumn(
                name: "SucursalId",
                schema: "mdm",
                table: "Almacenes");
        }
    }
}
