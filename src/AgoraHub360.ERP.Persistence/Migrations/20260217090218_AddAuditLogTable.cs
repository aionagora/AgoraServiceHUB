using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgoraHub360.ERP.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditLogTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AuditLogs",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Entidad = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    EntidadId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Accion = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ValoresAnteriores = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ValoresNuevos = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CamposModificados = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    EmpresaId = table.Column<int>(type: "int", nullable: true),
                    Usuario = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    FechaHora = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_EmpresaId",
                schema: "core",
                table: "AuditLogs",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_Entidad",
                schema: "core",
                table: "AuditLogs",
                column: "Entidad");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_Entidad_EntidadId",
                schema: "core",
                table: "AuditLogs",
                columns: new[] { "Entidad", "EntidadId" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_FechaHora",
                schema: "core",
                table: "AuditLogs",
                column: "FechaHora");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditLogs",
                schema: "core");
        }
    }
}
