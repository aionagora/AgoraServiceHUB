using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgoraHub360.ERP.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CMP_FlujoImportacionCompleto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ActaDiferencias",
                schema: "cmp",
                table: "RecepcionesCompra",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "EnCuarentena",
                schema: "cmp",
                table: "RecepcionesCompra",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "NumeroReclamo",
                schema: "cmp",
                table: "RecepcionesCompra",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResultadoControlCalidad",
                schema: "cmp",
                table: "RecepcionesCompra",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "TieneDiferencias",
                schema: "cmp",
                table: "RecepcionesCompra",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "TipoDiferencia",
                schema: "cmp",
                table: "RecepcionesCompra",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UbicacionCuarentena",
                schema: "cmp",
                table: "RecepcionesCompra",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CantidadDañada",
                schema: "cmp",
                table: "RecepcionCompraLineas",
                type: "decimal(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "CantidadFaltante",
                schema: "cmp",
                table: "RecepcionCompraLineas",
                type: "decimal(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "CantidadSobrante",
                schema: "cmp",
                table: "RecepcionCompraLineas",
                type: "decimal(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<long>(
                name: "ExpedienteImportacionId",
                schema: "cmp",
                table: "OrdenesCompra",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MotivoRechazo",
                schema: "cmp",
                table: "OrdenesCompra",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "OrdenPedidoId",
                schema: "cmp",
                table: "OrdenesCompra",
                type: "bigint",
                nullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "OrdenCompraId",
                schema: "cmp",
                table: "HojasImportacion",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddColumn<long>(
                name: "ExpedienteImportacionId",
                schema: "cmp",
                table: "HojasImportacion",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ConfirmacionesProveedor",
                schema: "cmp",
                columns: table => new
                {
                    ConfirmacionProveedorId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrdenCompraId = table.Column<long>(type: "bigint", nullable: false),
                    NumeroProforma = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FechaConfirmacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CondicionesOK = table.Column<bool>(type: "bit", nullable: false),
                    ObservacionesProveedor = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    FechaEntregaComprometida = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Estado = table.Column<byte>(type: "tinyint", nullable: false),
                    Observaciones = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IteracionNegociacion = table.Column<int>(type: "int", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModificadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    EmpresaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfirmacionesProveedor", x => x.ConfirmacionProveedorId);
                    table.ForeignKey(
                        name: "FK_ConfirmacionesProveedor_OrdenesCompra_OrdenCompraId",
                        column: x => x.OrdenCompraId,
                        principalSchema: "cmp",
                        principalTable: "OrdenesCompra",
                        principalColumn: "OrdenCompraId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ExpedientesImportacion",
                schema: "cmp",
                columns: table => new
                {
                    ExpedienteImportacionId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Numero = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Estado = table.Column<byte>(type: "tinyint", nullable: false),
                    Incoterm = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    ModalidadTransporte = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    PaisOrigen = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PuertoOrigen = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    PuertoDestino = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Forwarder = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Aseguradora = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    NumeroPólizaSeguro = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    NumeroBLAWB = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ETD = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ATD = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ETA = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ATA = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NumeroDUIDIM = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Despachante = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    FechaPresentacionAduana = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TotalTributos = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    TuvoObservacionAduana = table.Column<bool>(type: "bit", nullable: false),
                    DetalleObservacionAduana = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    FechaLevante = table.Column<DateTime>(type: "datetime2", nullable: true),
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
                    table.PrimaryKey("PK_ExpedientesImportacion", x => x.ExpedienteImportacionId);
                });

            migrationBuilder.CreateTable(
                name: "OrdenesPedido",
                schema: "cmp",
                columns: table => new
                {
                    OrdenPedidoId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Numero = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    FechaEmision = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaRequerida = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Solicitante = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CentroCosto = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Urgencia = table.Column<byte>(type: "tinyint", nullable: false),
                    AlmacenDestinoId = table.Column<int>(type: "int", nullable: false),
                    Estado = table.Column<byte>(type: "tinyint", nullable: false),
                    Observaciones = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    MotivoRechazo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    StockCubre = table.Column<bool>(type: "bit", nullable: true),
                    ObservacionesRevisionStock = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModificadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    EmpresaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrdenesPedido", x => x.OrdenPedidoId);
                    table.ForeignKey(
                        name: "FK_OrdenesPedido_Almacenes_AlmacenDestinoId",
                        column: x => x.AlmacenDestinoId,
                        principalSchema: "mdm",
                        principalTable: "Almacenes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PagosOrdenCompra",
                schema: "cmp",
                columns: table => new
                {
                    PagoOrdenCompraId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrdenCompraId = table.Column<long>(type: "bigint", nullable: false),
                    TipoPago = table.Column<byte>(type: "tinyint", nullable: false),
                    MontoProgramado = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    MonedaId = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    TasaCambio = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    FechaProgramada = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaEjecucion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MontoEjecutado = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    Ejecutado = table.Column<bool>(type: "bit", nullable: false),
                    ReferenciaTransferencia = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
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
                    table.PrimaryKey("PK_PagosOrdenCompra", x => x.PagoOrdenCompraId);
                    table.ForeignKey(
                        name: "FK_PagosOrdenCompra_OrdenesCompra_OrdenCompraId",
                        column: x => x.OrdenCompraId,
                        principalSchema: "cmp",
                        principalTable: "OrdenesCompra",
                        principalColumn: "OrdenCompraId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HitosExpediente",
                schema: "cmp",
                columns: table => new
                {
                    HitoExpedienteId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExpedienteImportacionId = table.Column<long>(type: "bigint", nullable: false),
                    TipoHito = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FechaHito = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ReferenciaDocumento = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModificadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HitosExpediente", x => x.HitoExpedienteId);
                    table.ForeignKey(
                        name: "FK_HitosExpediente_ExpedientesImportacion_ExpedienteImportacionId",
                        column: x => x.ExpedienteImportacionId,
                        principalSchema: "cmp",
                        principalTable: "ExpedientesImportacion",
                        principalColumn: "ExpedienteImportacionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrdenPedidoLineas",
                schema: "cmp",
                columns: table => new
                {
                    OrdenPedidoLineaId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrdenPedidoId = table.Column<long>(type: "bigint", nullable: false),
                    NumeroLinea = table.Column<int>(type: "int", nullable: false),
                    CompanyProductId = table.Column<long>(type: "bigint", nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    UnidadMedida = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CantidadSolicitada = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    CantidadStockDisponible = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    CantidadEnTransito = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    CantidadAComprar = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    Notas = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModificadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrdenPedidoLineas", x => x.OrdenPedidoLineaId);
                    table.ForeignKey(
                        name: "FK_OrdenPedidoLineas_CompanyProducts_CompanyProductId",
                        column: x => x.CompanyProductId,
                        principalSchema: "mdm",
                        principalTable: "CompanyProducts",
                        principalColumn: "CompanyProductId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrdenPedidoLineas_OrdenesPedido_OrdenPedidoId",
                        column: x => x.OrdenPedidoId,
                        principalSchema: "cmp",
                        principalTable: "OrdenesPedido",
                        principalColumn: "OrdenPedidoId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrdenesCompra_EmpresaId_ExpedienteImportacionId",
                schema: "cmp",
                table: "OrdenesCompra",
                columns: new[] { "EmpresaId", "ExpedienteImportacionId" });

            migrationBuilder.CreateIndex(
                name: "IX_OrdenesCompra_EmpresaId_OrdenPedidoId",
                schema: "cmp",
                table: "OrdenesCompra",
                columns: new[] { "EmpresaId", "OrdenPedidoId" });

            migrationBuilder.CreateIndex(
                name: "IX_OrdenesCompra_ExpedienteImportacionId",
                schema: "cmp",
                table: "OrdenesCompra",
                column: "ExpedienteImportacionId");

            migrationBuilder.CreateIndex(
                name: "IX_OrdenesCompra_OrdenPedidoId",
                schema: "cmp",
                table: "OrdenesCompra",
                column: "OrdenPedidoId");

            migrationBuilder.CreateIndex(
                name: "IX_HojasImportacion_EmpresaId_ExpedienteImportacionId",
                schema: "cmp",
                table: "HojasImportacion",
                columns: new[] { "EmpresaId", "ExpedienteImportacionId" });

            migrationBuilder.CreateIndex(
                name: "IX_HojasImportacion_ExpedienteImportacionId",
                schema: "cmp",
                table: "HojasImportacion",
                column: "ExpedienteImportacionId");

            migrationBuilder.CreateIndex(
                name: "IX_ConfirmacionesProveedor_EmpresaId",
                schema: "cmp",
                table: "ConfirmacionesProveedor",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_ConfirmacionesProveedor_OrdenCompraId_IteracionNegociacion",
                schema: "cmp",
                table: "ConfirmacionesProveedor",
                columns: new[] { "OrdenCompraId", "IteracionNegociacion" });

            migrationBuilder.CreateIndex(
                name: "IX_ExpedientesImportacion_EmpresaId",
                schema: "cmp",
                table: "ExpedientesImportacion",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_ExpedientesImportacion_EmpresaId_Estado",
                schema: "cmp",
                table: "ExpedientesImportacion",
                columns: new[] { "EmpresaId", "Estado" });

            migrationBuilder.CreateIndex(
                name: "IX_ExpedientesImportacion_EmpresaId_Numero",
                schema: "cmp",
                table: "ExpedientesImportacion",
                columns: new[] { "EmpresaId", "Numero" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HitosExpediente_ExpedienteImportacionId_FechaHito",
                schema: "cmp",
                table: "HitosExpediente",
                columns: new[] { "ExpedienteImportacionId", "FechaHito" });

            migrationBuilder.CreateIndex(
                name: "IX_OrdenesPedido_AlmacenDestinoId",
                schema: "cmp",
                table: "OrdenesPedido",
                column: "AlmacenDestinoId");

            migrationBuilder.CreateIndex(
                name: "IX_OrdenesPedido_EmpresaId",
                schema: "cmp",
                table: "OrdenesPedido",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_OrdenesPedido_EmpresaId_Estado",
                schema: "cmp",
                table: "OrdenesPedido",
                columns: new[] { "EmpresaId", "Estado" });

            migrationBuilder.CreateIndex(
                name: "IX_OrdenesPedido_EmpresaId_FechaEmision",
                schema: "cmp",
                table: "OrdenesPedido",
                columns: new[] { "EmpresaId", "FechaEmision" });

            migrationBuilder.CreateIndex(
                name: "IX_OrdenesPedido_EmpresaId_Numero",
                schema: "cmp",
                table: "OrdenesPedido",
                columns: new[] { "EmpresaId", "Numero" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrdenPedidoLineas_CompanyProductId",
                schema: "cmp",
                table: "OrdenPedidoLineas",
                column: "CompanyProductId");

            migrationBuilder.CreateIndex(
                name: "IX_OrdenPedidoLineas_OrdenPedidoId_NumeroLinea",
                schema: "cmp",
                table: "OrdenPedidoLineas",
                columns: new[] { "OrdenPedidoId", "NumeroLinea" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PagosOrdenCompra_EmpresaId",
                schema: "cmp",
                table: "PagosOrdenCompra",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_PagosOrdenCompra_EmpresaId_Ejecutado",
                schema: "cmp",
                table: "PagosOrdenCompra",
                columns: new[] { "EmpresaId", "Ejecutado" });

            migrationBuilder.CreateIndex(
                name: "IX_PagosOrdenCompra_EmpresaId_OrdenCompraId",
                schema: "cmp",
                table: "PagosOrdenCompra",
                columns: new[] { "EmpresaId", "OrdenCompraId" });

            migrationBuilder.CreateIndex(
                name: "IX_PagosOrdenCompra_OrdenCompraId",
                schema: "cmp",
                table: "PagosOrdenCompra",
                column: "OrdenCompraId");

            migrationBuilder.AddForeignKey(
                name: "FK_HojasImportacion_ExpedientesImportacion_ExpedienteImportacionId",
                schema: "cmp",
                table: "HojasImportacion",
                column: "ExpedienteImportacionId",
                principalSchema: "cmp",
                principalTable: "ExpedientesImportacion",
                principalColumn: "ExpedienteImportacionId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_OrdenesCompra_ExpedientesImportacion_ExpedienteImportacionId",
                schema: "cmp",
                table: "OrdenesCompra",
                column: "ExpedienteImportacionId",
                principalSchema: "cmp",
                principalTable: "ExpedientesImportacion",
                principalColumn: "ExpedienteImportacionId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_OrdenesCompra_OrdenesPedido_OrdenPedidoId",
                schema: "cmp",
                table: "OrdenesCompra",
                column: "OrdenPedidoId",
                principalSchema: "cmp",
                principalTable: "OrdenesPedido",
                principalColumn: "OrdenPedidoId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HojasImportacion_ExpedientesImportacion_ExpedienteImportacionId",
                schema: "cmp",
                table: "HojasImportacion");

            migrationBuilder.DropForeignKey(
                name: "FK_OrdenesCompra_ExpedientesImportacion_ExpedienteImportacionId",
                schema: "cmp",
                table: "OrdenesCompra");

            migrationBuilder.DropForeignKey(
                name: "FK_OrdenesCompra_OrdenesPedido_OrdenPedidoId",
                schema: "cmp",
                table: "OrdenesCompra");

            migrationBuilder.DropTable(
                name: "ConfirmacionesProveedor",
                schema: "cmp");

            migrationBuilder.DropTable(
                name: "HitosExpediente",
                schema: "cmp");

            migrationBuilder.DropTable(
                name: "OrdenPedidoLineas",
                schema: "cmp");

            migrationBuilder.DropTable(
                name: "PagosOrdenCompra",
                schema: "cmp");

            migrationBuilder.DropTable(
                name: "ExpedientesImportacion",
                schema: "cmp");

            migrationBuilder.DropTable(
                name: "OrdenesPedido",
                schema: "cmp");

            migrationBuilder.DropIndex(
                name: "IX_OrdenesCompra_EmpresaId_ExpedienteImportacionId",
                schema: "cmp",
                table: "OrdenesCompra");

            migrationBuilder.DropIndex(
                name: "IX_OrdenesCompra_EmpresaId_OrdenPedidoId",
                schema: "cmp",
                table: "OrdenesCompra");

            migrationBuilder.DropIndex(
                name: "IX_OrdenesCompra_ExpedienteImportacionId",
                schema: "cmp",
                table: "OrdenesCompra");

            migrationBuilder.DropIndex(
                name: "IX_OrdenesCompra_OrdenPedidoId",
                schema: "cmp",
                table: "OrdenesCompra");

            migrationBuilder.DropIndex(
                name: "IX_HojasImportacion_EmpresaId_ExpedienteImportacionId",
                schema: "cmp",
                table: "HojasImportacion");

            migrationBuilder.DropIndex(
                name: "IX_HojasImportacion_ExpedienteImportacionId",
                schema: "cmp",
                table: "HojasImportacion");

            migrationBuilder.DropColumn(
                name: "ActaDiferencias",
                schema: "cmp",
                table: "RecepcionesCompra");

            migrationBuilder.DropColumn(
                name: "EnCuarentena",
                schema: "cmp",
                table: "RecepcionesCompra");

            migrationBuilder.DropColumn(
                name: "NumeroReclamo",
                schema: "cmp",
                table: "RecepcionesCompra");

            migrationBuilder.DropColumn(
                name: "ResultadoControlCalidad",
                schema: "cmp",
                table: "RecepcionesCompra");

            migrationBuilder.DropColumn(
                name: "TieneDiferencias",
                schema: "cmp",
                table: "RecepcionesCompra");

            migrationBuilder.DropColumn(
                name: "TipoDiferencia",
                schema: "cmp",
                table: "RecepcionesCompra");

            migrationBuilder.DropColumn(
                name: "UbicacionCuarentena",
                schema: "cmp",
                table: "RecepcionesCompra");

            migrationBuilder.DropColumn(
                name: "CantidadDañada",
                schema: "cmp",
                table: "RecepcionCompraLineas");

            migrationBuilder.DropColumn(
                name: "CantidadFaltante",
                schema: "cmp",
                table: "RecepcionCompraLineas");

            migrationBuilder.DropColumn(
                name: "CantidadSobrante",
                schema: "cmp",
                table: "RecepcionCompraLineas");

            migrationBuilder.DropColumn(
                name: "ExpedienteImportacionId",
                schema: "cmp",
                table: "OrdenesCompra");

            migrationBuilder.DropColumn(
                name: "MotivoRechazo",
                schema: "cmp",
                table: "OrdenesCompra");

            migrationBuilder.DropColumn(
                name: "OrdenPedidoId",
                schema: "cmp",
                table: "OrdenesCompra");

            migrationBuilder.DropColumn(
                name: "ExpedienteImportacionId",
                schema: "cmp",
                table: "HojasImportacion");

            migrationBuilder.AlterColumn<long>(
                name: "OrdenCompraId",
                schema: "cmp",
                table: "HojasImportacion",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);
        }
    }
}
