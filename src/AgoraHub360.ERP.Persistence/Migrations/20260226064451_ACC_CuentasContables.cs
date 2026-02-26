using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgoraHub360.ERP.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ACC_CuentasContables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "acc");

            migrationBuilder.CreateTable(
                name: "CuentasContables",
                schema: "acc",
                columns: table => new
                {
                    CuentaContableId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Codigo = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Tipo = table.Column<byte>(type: "tinyint", nullable: false),
                    Naturaleza = table.Column<byte>(type: "tinyint", nullable: false),
                    Nivel = table.Column<int>(type: "int", nullable: false),
                    CuentaPadreId = table.Column<int>(type: "int", nullable: true),
                    PermiteMovimientos = table.Column<bool>(type: "bit", nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SaldoActual = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModificadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    EmpresaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CuentasContables", x => x.CuentaContableId);
                    table.ForeignKey(
                        name: "FK_CuentasContables_CuentasContables_CuentaPadreId",
                        column: x => x.CuentaPadreId,
                        principalSchema: "acc",
                        principalTable: "CuentasContables",
                        principalColumn: "CuentaContableId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CuentasContables_CuentaPadreId",
                schema: "acc",
                table: "CuentasContables",
                column: "CuentaPadreId");

            migrationBuilder.CreateIndex(
                name: "IX_CuentasContables_EmpresaId",
                schema: "acc",
                table: "CuentasContables",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_CuentasContables_EmpresaId_Codigo",
                schema: "acc",
                table: "CuentasContables",
                columns: new[] { "EmpresaId", "Codigo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CuentasContables_EmpresaId_CuentaPadreId",
                schema: "acc",
                table: "CuentasContables",
                columns: new[] { "EmpresaId", "CuentaPadreId" });

            migrationBuilder.CreateIndex(
                name: "IX_CuentasContables_EmpresaId_Tipo",
                schema: "acc",
                table: "CuentasContables",
                columns: new[] { "EmpresaId", "Tipo" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CuentasContables",
                schema: "acc");
        }
    }
}
