using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgoraHub360.ERP.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSiatMetodosPagoCatalog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SiatMetodosPago",
                schema: "vta",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Codigo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ModoPago = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    EsPredeterminado = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModificadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    EmpresaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SiatMetodosPago", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SiatMetodosPago_EmpresaId",
                schema: "vta",
                table: "SiatMetodosPago",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_SiatMetodosPago_EmpresaId_Codigo",
                schema: "vta",
                table: "SiatMetodosPago",
                columns: new[] { "EmpresaId", "Codigo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SiatMetodosPago_EmpresaId_EsPredeterminado",
                schema: "vta",
                table: "SiatMetodosPago",
                columns: new[] { "EmpresaId", "EsPredeterminado" });

            migrationBuilder.CreateIndex(
                name: "IX_SiatMetodosPago_EmpresaId_ModoPago",
                schema: "vta",
                table: "SiatMetodosPago",
                columns: new[] { "EmpresaId", "ModoPago" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SiatMetodosPago",
                schema: "vta");
        }
    }
}
