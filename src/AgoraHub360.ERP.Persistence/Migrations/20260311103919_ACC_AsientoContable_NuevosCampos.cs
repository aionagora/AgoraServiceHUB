using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgoraHub360.ERP.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ACC_AsientoContable_NuevosCampos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CerradoPor",
                schema: "acc",
                table: "PeriodosContables");

            migrationBuilder.DropColumn(
                name: "RegistradoPor",
                schema: "acc",
                table: "AsientosContables");

            migrationBuilder.AddColumn<int>(
                name: "CerradoPorId",
                schema: "acc",
                table: "PeriodosContables",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CerradoPorNombre",
                schema: "acc",
                table: "PeriodosContables",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RegistradoPorId",
                schema: "acc",
                table: "AsientosContables",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RegistradoPorNombre",
                schema: "acc",
                table: "AsientosContables",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PeriodosContables_CerradoPorId",
                schema: "acc",
                table: "PeriodosContables",
                column: "CerradoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_PeriodosContables_EmpresaId_CerradoPorId",
                schema: "acc",
                table: "PeriodosContables",
                columns: new[] { "EmpresaId", "CerradoPorId" });

            migrationBuilder.CreateIndex(
                name: "IX_AsientosContables_EmpresaId_RegistradoPorId",
                schema: "acc",
                table: "AsientosContables",
                columns: new[] { "EmpresaId", "RegistradoPorId" });

            migrationBuilder.CreateIndex(
                name: "IX_AsientosContables_RegistradoPorId",
                schema: "acc",
                table: "AsientosContables",
                column: "RegistradoPorId");

            migrationBuilder.AddForeignKey(
                name: "FK_AsientosContables_Usuarios_RegistradoPorId",
                schema: "acc",
                table: "AsientosContables",
                column: "RegistradoPorId",
                principalSchema: "core",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PeriodosContables_Usuarios_CerradoPorId",
                schema: "acc",
                table: "PeriodosContables",
                column: "CerradoPorId",
                principalSchema: "core",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AsientosContables_Usuarios_RegistradoPorId",
                schema: "acc",
                table: "AsientosContables");

            migrationBuilder.DropForeignKey(
                name: "FK_PeriodosContables_Usuarios_CerradoPorId",
                schema: "acc",
                table: "PeriodosContables");

            migrationBuilder.DropIndex(
                name: "IX_PeriodosContables_CerradoPorId",
                schema: "acc",
                table: "PeriodosContables");

            migrationBuilder.DropIndex(
                name: "IX_PeriodosContables_EmpresaId_CerradoPorId",
                schema: "acc",
                table: "PeriodosContables");

            migrationBuilder.DropIndex(
                name: "IX_AsientosContables_EmpresaId_RegistradoPorId",
                schema: "acc",
                table: "AsientosContables");

            migrationBuilder.DropIndex(
                name: "IX_AsientosContables_RegistradoPorId",
                schema: "acc",
                table: "AsientosContables");

            migrationBuilder.DropColumn(
                name: "CerradoPorId",
                schema: "acc",
                table: "PeriodosContables");

            migrationBuilder.DropColumn(
                name: "CerradoPorNombre",
                schema: "acc",
                table: "PeriodosContables");

            migrationBuilder.DropColumn(
                name: "RegistradoPorId",
                schema: "acc",
                table: "AsientosContables");

            migrationBuilder.DropColumn(
                name: "RegistradoPorNombre",
                schema: "acc",
                table: "AsientosContables");

            migrationBuilder.AddColumn<string>(
                name: "CerradoPor",
                schema: "acc",
                table: "PeriodosContables",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RegistradoPor",
                schema: "acc",
                table: "AsientosContables",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }
    }
}
