using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgoraHub360.ERP.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FE_CfgFacturacionElectronica : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "cfg");

            migrationBuilder.CreateTable(
                name: "AmbientesFacturacionElectronica",
                schema: "cfg",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Codigo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    EsProduccion = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModificadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AmbientesFacturacionElectronica", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AuditoriaFacturacion",
                schema: "cfg",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FacturaVentaId = table.Column<long>(type: "bigint", nullable: false),
                    ProveedorCodigo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ProveedorNombre = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    AmbienteCodigo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    AmbienteNombre = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    BillUuid = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Cuf = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    EstadoSiat = table.Column<byte>(type: "tinyint", nullable: false),
                    MensajeError = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    UsuarioId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FechaHora = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TiempoRespuestaMs = table.Column<long>(type: "bigint", nullable: false, defaultValue: 0L),
                    CodigoRespuestaProveedor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DescripcionRespuestaProveedor = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Exitoso = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModificadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    EmpresaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditoriaFacturacion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuditoriaFacturacion_FacturasVenta_FacturaVentaId",
                        column: x => x.FacturaVentaId,
                        principalSchema: "vta",
                        principalTable: "FacturasVenta",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProveedoresFacturacionElectronica",
                schema: "cfg",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Codigo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RequierePosToken = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    SoportaAnulacion = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    SoportaConsultaEstado = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    SoportaModoOffline = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    ClaseProvider = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModificadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProveedoresFacturacionElectronica", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ConfiguracionFacturacionElectronica",
                schema: "cfg",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NombreConfiguracion = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ProveedorFacturacionElectronicaId = table.Column<int>(type: "int", nullable: false),
                    AmbienteFacturacionElectronicaId = table.Column<int>(type: "int", nullable: false),
                    ClientId = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    ClientSecretEncrypted = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    TokenUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ApiManagementUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ApiBillingUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    PosTokenEncrypted = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    SucursalFiscal = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PuntoVentaFiscal = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ActivityCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    NitEmisor = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TimeoutSegundos = table.Column<int>(type: "int", nullable: false, defaultValue: 10),
                    EsConfiguracionActiva = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    Observaciones = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModificadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    EmpresaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfiguracionFacturacionElectronica", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConfiguracionFacturacionElectronica_AmbientesFacturacionElectronica_AmbienteFacturacionElectronicaId",
                        column: x => x.AmbienteFacturacionElectronicaId,
                        principalSchema: "cfg",
                        principalTable: "AmbientesFacturacionElectronica",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ConfiguracionFacturacionElectronica_ProveedoresFacturacionElectronica_ProveedorFacturacionElectronicaId",
                        column: x => x.ProveedorFacturacionElectronicaId,
                        principalSchema: "cfg",
                        principalTable: "ProveedoresFacturacionElectronica",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                schema: "cfg",
                table: "AmbientesFacturacionElectronica",
                columns: new[] { "Id", "Activo", "Codigo", "CreadoPor", "Descripcion", "FechaCreacion", "FechaModificacion", "ModificadoPor", "Nombre" },
                values: new object[] { 1, true, "TEST", "system", "Ambiente de pruebas para facturación electrónica", new DateTime(2026, 6, 10, 0, 0, 0, 0, DateTimeKind.Utc), null, null, "Pruebas" });

            migrationBuilder.InsertData(
                schema: "cfg",
                table: "AmbientesFacturacionElectronica",
                columns: new[] { "Id", "Activo", "Codigo", "CreadoPor", "Descripcion", "EsProduccion", "FechaCreacion", "FechaModificacion", "ModificadoPor", "Nombre" },
                values: new object[] { 2, true, "PRODUCCION", "system", "Ambiente de producción para facturación electrónica", true, new DateTime(2026, 6, 10, 0, 0, 0, 0, DateTimeKind.Utc), null, null, "Producción" });

            migrationBuilder.InsertData(
                schema: "cfg",
                table: "ProveedoresFacturacionElectronica",
                columns: new[] { "Id", "Activo", "ClaseProvider", "Codigo", "CreadoPor", "Descripcion", "FechaCreacion", "FechaModificacion", "ModificadoPor", "Nombre", "RequierePosToken", "SoportaAnulacion", "SoportaConsultaEstado" },
                values: new object[] { 1, true, "AgoraHub360.ERP.Infrastructure.Services.FE.CirrusFacturacionProvider", "CIRRUS", "system", "Proveedor de facturación electrónica Cirrus (SIAT Bolivia)", new DateTime(2026, 6, 10, 0, 0, 0, 0, DateTimeKind.Utc), null, null, "Cirrus", true, true, true });

            migrationBuilder.InsertData(
                schema: "cfg",
                table: "ProveedoresFacturacionElectronica",
                columns: new[] { "Id", "Activo", "ClaseProvider", "Codigo", "CreadoPor", "Descripcion", "FechaCreacion", "FechaModificacion", "ModificadoPor", "Nombre", "SoportaAnulacion", "SoportaConsultaEstado", "SoportaModoOffline" },
                values: new object[] { 2, true, "AgoraHub360.ERP.Infrastructure.Services.FE.AgoraFCProvider", "AGORAFC", "system", "Proveedor de facturación electrónica AgoraFC", new DateTime(2026, 6, 10, 0, 0, 0, 0, DateTimeKind.Utc), null, null, "AgoraFC", true, true, true });

            migrationBuilder.InsertData(
                schema: "cfg",
                table: "ProveedoresFacturacionElectronica",
                columns: new[] { "Id", "Activo", "ClaseProvider", "Codigo", "CreadoPor", "Descripcion", "FechaCreacion", "FechaModificacion", "ModificadoPor", "Nombre", "SoportaAnulacion", "SoportaConsultaEstado" },
                values: new object[] { 3, true, "AgoraHub360.ERP.Infrastructure.Services.FE.SIATDirectoProvider", "SIAT_DIRECTO", "system", "Integración directa con el SIAT de Bolivia (futuro)", new DateTime(2026, 6, 10, 0, 0, 0, 0, DateTimeKind.Utc), null, null, "SIAT Directo", true, true });

            migrationBuilder.CreateIndex(
                name: "IX_AmbientesFE_Activo",
                schema: "cfg",
                table: "AmbientesFacturacionElectronica",
                column: "Activo");

            migrationBuilder.CreateIndex(
                name: "UX_AmbientesFE_Codigo",
                schema: "cfg",
                table: "AmbientesFacturacionElectronica",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AuditoriaFacturacion_EmpresaId",
                schema: "cfg",
                table: "AuditoriaFacturacion",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditoriaFE_Empresa_Fecha",
                schema: "cfg",
                table: "AuditoriaFacturacion",
                columns: new[] { "EmpresaId", "FechaHora" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditoriaFE_Exitoso",
                schema: "cfg",
                table: "AuditoriaFacturacion",
                column: "Exitoso");

            migrationBuilder.CreateIndex(
                name: "IX_AuditoriaFE_FacturaVentaId",
                schema: "cfg",
                table: "AuditoriaFacturacion",
                column: "FacturaVentaId");

            migrationBuilder.CreateIndex(
                name: "IX_ConfigFE_Empresa_Activa",
                schema: "cfg",
                table: "ConfiguracionFacturacionElectronica",
                columns: new[] { "EmpresaId", "EsConfiguracionActiva" },
                filter: "[EsConfiguracionActiva] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_ConfigFE_Empresa_Proveedor",
                schema: "cfg",
                table: "ConfiguracionFacturacionElectronica",
                columns: new[] { "EmpresaId", "ProveedorFacturacionElectronicaId" });

            migrationBuilder.CreateIndex(
                name: "IX_ConfigFE_EmpresaId",
                schema: "cfg",
                table: "ConfiguracionFacturacionElectronica",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_ConfiguracionFacturacionElectronica_AmbienteFacturacionElectronicaId",
                schema: "cfg",
                table: "ConfiguracionFacturacionElectronica",
                column: "AmbienteFacturacionElectronicaId");

            migrationBuilder.CreateIndex(
                name: "IX_ConfiguracionFacturacionElectronica_ProveedorFacturacionElectronicaId",
                schema: "cfg",
                table: "ConfiguracionFacturacionElectronica",
                column: "ProveedorFacturacionElectronicaId");

            migrationBuilder.CreateIndex(
                name: "IX_ProveedoresFE_Activo",
                schema: "cfg",
                table: "ProveedoresFacturacionElectronica",
                column: "Activo");

            migrationBuilder.CreateIndex(
                name: "UX_ProveedoresFE_Codigo",
                schema: "cfg",
                table: "ProveedoresFacturacionElectronica",
                column: "Codigo",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditoriaFacturacion",
                schema: "cfg");

            migrationBuilder.DropTable(
                name: "ConfiguracionFacturacionElectronica",
                schema: "cfg");

            migrationBuilder.DropTable(
                name: "AmbientesFacturacionElectronica",
                schema: "cfg");

            migrationBuilder.DropTable(
                name: "ProveedoresFacturacionElectronica",
                schema: "cfg");
        }
    }
}
