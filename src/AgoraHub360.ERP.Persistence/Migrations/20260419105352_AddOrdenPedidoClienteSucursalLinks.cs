using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgoraHub360.ERP.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddOrdenPedidoClienteSucursalLinks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ClienteId",
                schema: "cmp",
                table: "OrdenesPedido",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ClienteSucursalId",
                schema: "cmp",
                table: "OrdenesPedido",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SucursalId",
                schema: "cmp",
                table: "OrdenesPedido",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrdenesPedido_ClienteId",
                schema: "cmp",
                table: "OrdenesPedido",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_OrdenesPedido_ClienteSucursalId",
                schema: "cmp",
                table: "OrdenesPedido",
                column: "ClienteSucursalId");

            migrationBuilder.CreateIndex(
                name: "IX_OrdenesPedido_EmpresaId_ClienteId",
                schema: "cmp",
                table: "OrdenesPedido",
                columns: new[] { "EmpresaId", "ClienteId" });

            migrationBuilder.CreateIndex(
                name: "IX_OrdenesPedido_EmpresaId_ClienteSucursalId",
                schema: "cmp",
                table: "OrdenesPedido",
                columns: new[] { "EmpresaId", "ClienteSucursalId" });

            migrationBuilder.CreateIndex(
                name: "IX_OrdenesPedido_EmpresaId_SucursalId",
                schema: "cmp",
                table: "OrdenesPedido",
                columns: new[] { "EmpresaId", "SucursalId" });

            migrationBuilder.CreateIndex(
                name: "IX_OrdenesPedido_SucursalId",
                schema: "cmp",
                table: "OrdenesPedido",
                column: "SucursalId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrdenesPedido_ClienteSucursales_ClienteSucursalId",
                schema: "cmp",
                table: "OrdenesPedido",
                column: "ClienteSucursalId",
                principalSchema: "mdm",
                principalTable: "ClienteSucursales",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrdenesPedido_Clientes_ClienteId",
                schema: "cmp",
                table: "OrdenesPedido",
                column: "ClienteId",
                principalSchema: "mdm",
                principalTable: "Clientes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrdenesPedido_Sucursales_SucursalId",
                schema: "cmp",
                table: "OrdenesPedido",
                column: "SucursalId",
                principalSchema: "core",
                principalTable: "Sucursales",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrdenesPedido_ClienteSucursales_ClienteSucursalId",
                schema: "cmp",
                table: "OrdenesPedido");

            migrationBuilder.DropForeignKey(
                name: "FK_OrdenesPedido_Clientes_ClienteId",
                schema: "cmp",
                table: "OrdenesPedido");

            migrationBuilder.DropForeignKey(
                name: "FK_OrdenesPedido_Sucursales_SucursalId",
                schema: "cmp",
                table: "OrdenesPedido");

            migrationBuilder.DropIndex(
                name: "IX_OrdenesPedido_ClienteId",
                schema: "cmp",
                table: "OrdenesPedido");

            migrationBuilder.DropIndex(
                name: "IX_OrdenesPedido_ClienteSucursalId",
                schema: "cmp",
                table: "OrdenesPedido");

            migrationBuilder.DropIndex(
                name: "IX_OrdenesPedido_EmpresaId_ClienteId",
                schema: "cmp",
                table: "OrdenesPedido");

            migrationBuilder.DropIndex(
                name: "IX_OrdenesPedido_EmpresaId_ClienteSucursalId",
                schema: "cmp",
                table: "OrdenesPedido");

            migrationBuilder.DropIndex(
                name: "IX_OrdenesPedido_EmpresaId_SucursalId",
                schema: "cmp",
                table: "OrdenesPedido");

            migrationBuilder.DropIndex(
                name: "IX_OrdenesPedido_SucursalId",
                schema: "cmp",
                table: "OrdenesPedido");

            migrationBuilder.DropColumn(
                name: "ClienteId",
                schema: "cmp",
                table: "OrdenesPedido");

            migrationBuilder.DropColumn(
                name: "ClienteSucursalId",
                schema: "cmp",
                table: "OrdenesPedido");

            migrationBuilder.DropColumn(
                name: "SucursalId",
                schema: "cmp",
                table: "OrdenesPedido");
        }
    }
}
