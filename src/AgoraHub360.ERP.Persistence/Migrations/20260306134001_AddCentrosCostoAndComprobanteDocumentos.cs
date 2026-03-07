using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgoraHub360.ERP.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCentrosCostoAndComprobanteDocumentos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CentroCostoId",
                schema: "acc",
                table: "AsientoContableLineas",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CentrosCosto",
                schema: "cst",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Codigo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ParentId = table.Column<int>(type: "int", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModificadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    EmpresaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CentrosCosto", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CentrosCosto_CentrosCosto_ParentId",
                        column: x => x.ParentId,
                        principalSchema: "cst",
                        principalTable: "CentrosCosto",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ComprobanteDocumentos",
                schema: "doc",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ComprobanteId = table.Column<long>(type: "bigint", nullable: false),
                    DocumentId = table.Column<long>(type: "bigint", nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModificadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    EmpresaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComprobanteDocumentos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComprobanteDocumentos_AsientosContables_ComprobanteId",
                        column: x => x.ComprobanteId,
                        principalSchema: "acc",
                        principalTable: "AsientosContables",
                        principalColumn: "AsientoContableId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ComprobanteDocumentos_Documents_DocumentId",
                        column: x => x.DocumentId,
                        principalSchema: "doc",
                        principalTable: "Documents",
                        principalColumn: "DocumentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AsientoContableLineas_CentroCostoId",
                schema: "acc",
                table: "AsientoContableLineas",
                column: "CentroCostoId");

            migrationBuilder.CreateIndex(
                name: "IX_CentrosCosto_EmpresaId",
                schema: "cst",
                table: "CentrosCosto",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_CentrosCosto_EmpresaId_Codigo",
                schema: "cst",
                table: "CentrosCosto",
                columns: new[] { "EmpresaId", "Codigo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CentrosCosto_ParentId",
                schema: "cst",
                table: "CentrosCosto",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_ComprobanteDocumentos_ComprobanteId",
                schema: "doc",
                table: "ComprobanteDocumentos",
                column: "ComprobanteId");

            migrationBuilder.CreateIndex(
                name: "IX_ComprobanteDocumentos_DocumentId",
                schema: "doc",
                table: "ComprobanteDocumentos",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_ComprobanteDocumentos_EmpresaId",
                schema: "doc",
                table: "ComprobanteDocumentos",
                column: "EmpresaId");

            migrationBuilder.AddForeignKey(
                name: "FK_AsientoContableLineas_CentrosCosto_CentroCostoId",
                schema: "acc",
                table: "AsientoContableLineas",
                column: "CentroCostoId",
                principalSchema: "cst",
                principalTable: "CentrosCosto",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AsientoContableLineas_CentrosCosto_CentroCostoId",
                schema: "acc",
                table: "AsientoContableLineas");

            migrationBuilder.DropTable(
                name: "CentrosCosto",
                schema: "cst");

            migrationBuilder.DropTable(
                name: "ComprobanteDocumentos",
                schema: "doc");

            migrationBuilder.DropIndex(
                name: "IX_AsientoContableLineas_CentroCostoId",
                schema: "acc",
                table: "AsientoContableLineas");

            migrationBuilder.DropColumn(
                name: "CentroCostoId",
                schema: "acc",
                table: "AsientoContableLineas");
        }
    }
}
