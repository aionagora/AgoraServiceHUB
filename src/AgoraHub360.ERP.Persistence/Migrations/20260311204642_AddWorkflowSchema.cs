using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgoraHub360.ERP.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkflowSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "wf");

            migrationBuilder.CreateTable(
                name: "Tareas",
                schema: "wf",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EntityType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EntityId = table.Column<int>(type: "int", nullable: false),
                    Orden = table.Column<int>(type: "int", nullable: false),
                    Codigo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    FechaPlan = table.Column<DateOnly>(type: "date", nullable: true),
                    FechaReal = table.Column<DateOnly>(type: "date", nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "PENDIENTE"),
                    Completado = table.Column<bool>(type: "bit", nullable: false),
                    Responsable = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Observaciones = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    MetadataJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPor = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModificadoPor = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    EmpresaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tareas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tareas_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalSchema: "core",
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tareas_Codigo_UQ",
                schema: "wf",
                table: "Tareas",
                columns: new[] { "EmpresaId", "EntityType", "EntityId", "Codigo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tareas_EmpresaId",
                schema: "wf",
                table: "Tareas",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_Tareas_Entity",
                schema: "wf",
                table: "Tareas",
                columns: new[] { "EmpresaId", "EntityType", "EntityId", "Orden" })
                .Annotation("SqlServer:Include", new[] { "Estado", "Completado", "FechaReal" });

            migrationBuilder.CreateIndex(
                name: "IX_Tareas_Estado",
                schema: "wf",
                table: "Tareas",
                columns: new[] { "EmpresaId", "Estado", "EntityType" });

            migrationBuilder.CreateIndex(
                name: "IX_Tareas_Fechas",
                schema: "wf",
                table: "Tareas",
                columns: new[] { "EmpresaId", "FechaReal" })
                .Annotation("SqlServer:Include", new[] { "EntityType", "EntityId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Tareas",
                schema: "wf");
        }
    }
}
