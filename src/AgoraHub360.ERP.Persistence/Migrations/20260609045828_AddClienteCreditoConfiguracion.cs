using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgoraHub360.ERP.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddClienteCreditoConfiguracion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ClienteCreditoConfiguraciones",
                schema: "cxc",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClienteId = table.Column<int>(type: "int", nullable: false),
                    CreditoHabilitado = table.Column<bool>(type: "bit", nullable: false),
                    DiasCredito = table.Column<int>(type: "int", nullable: false),
                    LimiteCredito = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Observaciones = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModificadoPor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    EmpresaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClienteCreditoConfiguraciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClienteCreditoConfiguraciones_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalSchema: "mdm",
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClienteCreditoConfiguraciones_ClienteId",
                schema: "cxc",
                table: "ClienteCreditoConfiguraciones",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_ClienteCreditoConfiguraciones_EmpresaId",
                schema: "cxc",
                table: "ClienteCreditoConfiguraciones",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_ClienteCreditoConfiguraciones_EmpresaId_ClienteId",
                schema: "cxc",
                table: "ClienteCreditoConfiguraciones",
                columns: new[] { "EmpresaId", "ClienteId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClienteCreditoConfiguraciones",
                schema: "cxc");
        }
    }
}
