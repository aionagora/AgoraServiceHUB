using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgoraHub360.ERP.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CMP_OrdenPedido_SolicitanteId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Solicitante",
                schema: "cmp",
                table: "OrdenesPedido");

            migrationBuilder.EnsureSchema(
                name: "log");

            migrationBuilder.AddColumn<int>(
                name: "SolicitanteId",
                schema: "cmp",
                table: "OrdenesPedido",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "HojasRuta",
                schema: "log",
                columns: table => new
                {
                    HojaRutaId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NumeroHojaRuta = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TipoOP = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SubTipo = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    AlmacenOrigenId = table.Column<int>(type: "int", nullable: false),
                    AlmacenDestinoId = table.Column<int>(type: "int", nullable: true),
                    ProveedorCliente = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    DireccionEntrega = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    ContactoCliente = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    ResponsableUsuario = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaDocumento = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ETA = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SubEstado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Observaciones = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModificadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    EmpresaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HojasRuta", x => x.HojaRutaId);
                });

            migrationBuilder.CreateTable(
                name: "HojaRutaHistorial",
                schema: "log",
                columns: table => new
                {
                    HojaRutaHistorialId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HojaRutaId = table.Column<long>(type: "bigint", nullable: false),
                    EstadoAnterior = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SubEstadoAnterior = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    EstadoNuevo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SubEstadoNuevo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FechaCambio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Usuario = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Observaciones = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModificadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HojaRutaHistorial", x => x.HojaRutaHistorialId);
                    table.ForeignKey(
                        name: "FK_HojaRutaHistorial_HojasRuta_HojaRutaId",
                        column: x => x.HojaRutaId,
                        principalSchema: "log",
                        principalTable: "HojasRuta",
                        principalColumn: "HojaRutaId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrdenesPedido_EmpresaId_SolicitanteId",
                schema: "cmp",
                table: "OrdenesPedido",
                columns: new[] { "EmpresaId", "SolicitanteId" });

            migrationBuilder.CreateIndex(
                name: "IX_OrdenesPedido_SolicitanteId",
                schema: "cmp",
                table: "OrdenesPedido",
                column: "SolicitanteId");

            migrationBuilder.CreateIndex(
                name: "IX_HojaRutaHistorial_HojaRutaId",
                schema: "log",
                table: "HojaRutaHistorial",
                column: "HojaRutaId");

            migrationBuilder.CreateIndex(
                name: "IX_HojasRuta_EmpresaId",
                schema: "log",
                table: "HojasRuta",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_HojasRuta_EmpresaId_Estado",
                schema: "log",
                table: "HojasRuta",
                columns: new[] { "EmpresaId", "Estado" });

            migrationBuilder.CreateIndex(
                name: "IX_HojasRuta_EmpresaId_FechaDocumento",
                schema: "log",
                table: "HojasRuta",
                columns: new[] { "EmpresaId", "FechaDocumento" });

            migrationBuilder.CreateIndex(
                name: "IX_HojasRuta_EmpresaId_NumeroHojaRuta",
                schema: "log",
                table: "HojasRuta",
                columns: new[] { "EmpresaId", "NumeroHojaRuta" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HojasRuta_EmpresaId_TipoOP",
                schema: "log",
                table: "HojasRuta",
                columns: new[] { "EmpresaId", "TipoOP" });

            migrationBuilder.AddForeignKey(
                name: "FK_OrdenesPedido_Usuarios_SolicitanteId",
                schema: "cmp",
                table: "OrdenesPedido",
                column: "SolicitanteId",
                principalSchema: "core",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrdenesPedido_Usuarios_SolicitanteId",
                schema: "cmp",
                table: "OrdenesPedido");

            migrationBuilder.DropTable(
                name: "HojaRutaHistorial",
                schema: "log");

            migrationBuilder.DropTable(
                name: "HojasRuta",
                schema: "log");

            migrationBuilder.DropIndex(
                name: "IX_OrdenesPedido_EmpresaId_SolicitanteId",
                schema: "cmp",
                table: "OrdenesPedido");

            migrationBuilder.DropIndex(
                name: "IX_OrdenesPedido_SolicitanteId",
                schema: "cmp",
                table: "OrdenesPedido");

            migrationBuilder.DropColumn(
                name: "SolicitanteId",
                schema: "cmp",
                table: "OrdenesPedido");

            migrationBuilder.AddColumn<string>(
                name: "Solicitante",
                schema: "cmp",
                table: "OrdenesPedido",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");
        }
    }
}
