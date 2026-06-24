using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgoraHub360.ERP.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAnulacionToVentaPago : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Anulado",
                schema: "vta",
                table: "VentaPagos",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaAnulacion",
                schema: "vta",
                table: "VentaPagos",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MotivoAnulacion",
                schema: "vta",
                table: "VentaPagos",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UsuarioAnulacionId",
                schema: "vta",
                table: "VentaPagos",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Anulado",
                schema: "vta",
                table: "VentaPagos");

            migrationBuilder.DropColumn(
                name: "FechaAnulacion",
                schema: "vta",
                table: "VentaPagos");

            migrationBuilder.DropColumn(
                name: "MotivoAnulacion",
                schema: "vta",
                table: "VentaPagos");

            migrationBuilder.DropColumn(
                name: "UsuarioAnulacionId",
                schema: "vta",
                table: "VentaPagos");
        }
    }
}
