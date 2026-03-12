using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgoraHub360.ERP.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class LOG_HojaRuta_OrdenPedidoId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "OrdenPedidoId",
                schema: "log",
                table: "HojasRuta",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_HojasRuta_OrdenPedidoId",
                schema: "log",
                table: "HojasRuta",
                columns: new[] { "EmpresaId", "OrdenPedidoId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_HojasRuta_OrdenPedidoId",
                schema: "log",
                table: "HojasRuta");

            migrationBuilder.DropColumn(
                name: "OrdenPedidoId",
                schema: "log",
                table: "HojasRuta");
        }
    }
}
