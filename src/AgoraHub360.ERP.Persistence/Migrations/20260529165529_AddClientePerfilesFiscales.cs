using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgoraHub360.ERP.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddClientePerfilesFiscales : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ClientePerfilesFiscales",
                schema: "mdm",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClienteId = table.Column<int>(type: "int", nullable: false),
                    ClienteSucursalId = table.Column<int>(type: "int", nullable: true),
                    Alias = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    TipoDocumentoIdentidad = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    NumeroDocumento = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Complemento = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    RazonSocial = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TipoPersona = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    TipoPerfilFiscal = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    EmailFactura = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    TelefonoFactura = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    RequiereEmail = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    EsPredeterminado = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    ValidadoFacturacion = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CodigoClienteApi = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CodigoExternoFacturacion = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Observaciones = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModificadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    EmpresaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientePerfilesFiscales", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClientePerfilesFiscales_ClienteSucursales_ClienteSucursalId",
                        column: x => x.ClienteSucursalId,
                        principalSchema: "mdm",
                        principalTable: "ClienteSucursales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClientePerfilesFiscales_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalSchema: "mdm",
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClientePerfilesFiscales_ClienteId",
                schema: "mdm",
                table: "ClientePerfilesFiscales",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientePerfilesFiscales_ClienteSucursalId",
                schema: "mdm",
                table: "ClientePerfilesFiscales",
                column: "ClienteSucursalId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientePerfilesFiscales_EmpresaId",
                schema: "mdm",
                table: "ClientePerfilesFiscales",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientePerfilFiscal_Empresa_Cliente",
                schema: "mdm",
                table: "ClientePerfilesFiscales",
                columns: new[] { "EmpresaId", "ClienteId" });

            migrationBuilder.CreateIndex(
                name: "IX_ClientePerfilFiscal_Empresa_Cliente_Documento",
                schema: "mdm",
                table: "ClientePerfilesFiscales",
                columns: new[] { "EmpresaId", "ClienteId", "NumeroDocumento" });

            migrationBuilder.CreateIndex(
                name: "IX_ClientePerfilFiscal_Empresa_Cliente_EsPredeterminado",
                schema: "mdm",
                table: "ClientePerfilesFiscales",
                columns: new[] { "EmpresaId", "ClienteId", "EsPredeterminado" });

            migrationBuilder.CreateIndex(
                name: "IX_ClientePerfilFiscal_Empresa_Documento",
                schema: "mdm",
                table: "ClientePerfilesFiscales",
                columns: new[] { "EmpresaId", "NumeroDocumento" });

            migrationBuilder.CreateIndex(
                name: "UX_ClientePerfilFiscal_Empresa_Cliente_Documento_RazonSocial_Activo",
                schema: "mdm",
                table: "ClientePerfilesFiscales",
                columns: new[] { "EmpresaId", "ClienteId", "NumeroDocumento", "RazonSocial", "Activo" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClientePerfilesFiscales",
                schema: "mdm");
        }
    }
}
