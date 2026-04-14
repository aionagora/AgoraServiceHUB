using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgoraHub360.ERP.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RefactorNumeracionComprobantes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AsientosContables_EmpresaId_Numero",
                schema: "acc",
                table: "AsientosContables");

            migrationBuilder.CreateIndex(
                name: "IX_AsientosContables_EmpresaId_Gestion_Numero",
                schema: "acc",
                table: "AsientosContables",
                columns: new[] { "EmpresaId", "Gestion", "Numero" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AsientosContables_EmpresaId_Gestion_Numero",
                schema: "acc",
                table: "AsientosContables");

            migrationBuilder.CreateIndex(
                name: "IX_AsientosContables_EmpresaId_Numero",
                schema: "acc",
                table: "AsientosContables",
                columns: new[] { "EmpresaId", "Numero" },
                unique: true);
        }
    }
}
