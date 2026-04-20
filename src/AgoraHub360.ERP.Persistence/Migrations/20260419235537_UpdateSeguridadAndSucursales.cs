using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgoraHub360.ERP.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSeguridadAndSucursales : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AliasComercial",
                schema: "mdm",
                table: "ClienteSucursales",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Celular",
                schema: "mdm",
                table: "ClienteSucursales",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                schema: "mdm",
                table: "ClienteSucursales",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "EsPuntoEntrega",
                schema: "mdm",
                table: "ClienteSucursales",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "RecibeFacturacion",
                schema: "mdm",
                table: "ClienteSucursales",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "RecibePedidos",
                schema: "mdm",
                table: "ClienteSucursales",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "ResponsableCargo",
                schema: "mdm",
                table: "ClienteSucursales",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResponsableNombre",
                schema: "mdm",
                table: "ClienteSucursales",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Telefono",
                schema: "mdm",
                table: "ClienteSucursales",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UrlMapa",
                schema: "mdm",
                table: "ClienteSucursales",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WhatsApp",
                schema: "mdm",
                table: "ClienteSucursales",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Contactos",
                schema: "mdm",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClienteId = table.Column<int>(type: "int", nullable: false),
                    ClienteSucursalId = table.Column<int>(type: "int", nullable: true),
                    Nombres = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Apellidos = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Cargo = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    Telefono = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Celular = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    WhatsApp = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    EsPrincipal = table.Column<bool>(type: "bit", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModificadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    EmpresaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contactos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Contactos_ClienteSucursales_ClienteSucursalId",
                        column: x => x.ClienteSucursalId,
                        principalSchema: "mdm",
                        principalTable: "ClienteSucursales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Contactos_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalSchema: "mdm",
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ModulosSistema",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Codigo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Icono = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Orden = table.Column<int>(type: "int", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModificadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModulosSistema", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PerfilesAcceso",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Codigo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    TipoUsuario = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Ambos"),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModificadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    EmpresaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PerfilesAcceso", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PerfilesAcceso_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalSchema: "core",
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UsuariosSucursalesAccesos",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    SucursalId = table.Column<int>(type: "int", nullable: false),
                    EsPredeterminada = table.Column<bool>(type: "bit", nullable: false),
                    PuedeConsultar = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    PuedeOperar = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModificadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    EmpresaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsuariosSucursalesAccesos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UsuariosSucursalesAccesos_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalSchema: "core",
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UsuariosSucursalesAccesos_Sucursales_SucursalId",
                        column: x => x.SucursalId,
                        principalSchema: "core",
                        principalTable: "Sucursales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UsuariosSucursalesAccesos_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalSchema: "core",
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FormulariosSistema",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ModuloSistemaId = table.Column<int>(type: "int", nullable: false),
                    Codigo = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Ruta = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Icono = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Orden = table.Column<int>(type: "int", nullable: false),
                    VisibleEnMenu = table.Column<bool>(type: "bit", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModificadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FormulariosSistema", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FormulariosSistema_ModulosSistema_ModuloSistemaId",
                        column: x => x.ModuloSistemaId,
                        principalSchema: "core",
                        principalTable: "ModulosSistema",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ContactosUsuariosAccesos",
                schema: "mdm",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ContactoId = table.Column<int>(type: "int", nullable: false),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    PerfilAccesoId = table.Column<int>(type: "int", nullable: true),
                    AccesoWeb = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    AccesoMovil = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModificadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    EmpresaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContactosUsuariosAccesos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContactosUsuariosAccesos_Contactos_ContactoId",
                        column: x => x.ContactoId,
                        principalSchema: "mdm",
                        principalTable: "Contactos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ContactosUsuariosAccesos_PerfilesAcceso_PerfilAccesoId",
                        column: x => x.PerfilAccesoId,
                        principalSchema: "core",
                        principalTable: "PerfilesAcceso",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ContactosUsuariosAccesos_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalSchema: "core",
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UsuariosPerfiles",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    PerfilAccesoId = table.Column<int>(type: "int", nullable: false),
                    VigenteDesde = table.Column<DateTime>(type: "datetime2", nullable: true),
                    VigenteHasta = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModificadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    EmpresaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsuariosPerfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UsuariosPerfiles_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalSchema: "core",
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UsuariosPerfiles_PerfilesAcceso_PerfilAccesoId",
                        column: x => x.PerfilAccesoId,
                        principalSchema: "core",
                        principalTable: "PerfilesAcceso",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UsuariosPerfiles_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalSchema: "core",
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AccionesSistema",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FormularioSistemaId = table.Column<int>(type: "int", nullable: false),
                    Codigo = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Orden = table.Column<int>(type: "int", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModificadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccionesSistema", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccionesSistema_FormulariosSistema_FormularioSistemaId",
                        column: x => x.FormularioSistemaId,
                        principalSchema: "core",
                        principalTable: "FormulariosSistema",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PerfilesPermisos",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PerfilAccesoId = table.Column<int>(type: "int", nullable: false),
                    FormularioSistemaId = table.Column<int>(type: "int", nullable: false),
                    AccionSistemaId = table.Column<int>(type: "int", nullable: true),
                    Permitido = table.Column<bool>(type: "bit", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModificadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    EmpresaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PerfilesPermisos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PerfilesPermisos_AccionesSistema_AccionSistemaId",
                        column: x => x.AccionSistemaId,
                        principalSchema: "core",
                        principalTable: "AccionesSistema",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PerfilesPermisos_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalSchema: "core",
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PerfilesPermisos_FormulariosSistema_FormularioSistemaId",
                        column: x => x.FormularioSistemaId,
                        principalSchema: "core",
                        principalTable: "FormulariosSistema",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PerfilesPermisos_PerfilesAcceso_PerfilAccesoId",
                        column: x => x.PerfilAccesoId,
                        principalSchema: "core",
                        principalTable: "PerfilesAcceso",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccionesSistema_FormularioSistemaId_Codigo",
                schema: "core",
                table: "AccionesSistema",
                columns: new[] { "FormularioSistemaId", "Codigo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AccionesSistema_FormularioSistemaId_Orden",
                schema: "core",
                table: "AccionesSistema",
                columns: new[] { "FormularioSistemaId", "Orden" });

            migrationBuilder.CreateIndex(
                name: "IX_Contacto_Empresa_Cliente_Email",
                schema: "mdm",
                table: "Contactos",
                columns: new[] { "EmpresaId", "ClienteId", "Email" });

            migrationBuilder.CreateIndex(
                name: "IX_Contacto_Empresa_Cliente_Principal",
                schema: "mdm",
                table: "Contactos",
                columns: new[] { "EmpresaId", "ClienteId", "EsPrincipal" },
                unique: true,
                filter: "[EsPrincipal] = 1 AND [Activo] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_Contactos_ClienteId",
                schema: "mdm",
                table: "Contactos",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_Contactos_ClienteSucursalId",
                schema: "mdm",
                table: "Contactos",
                column: "ClienteSucursalId");

            migrationBuilder.CreateIndex(
                name: "IX_Contactos_EmpresaId",
                schema: "mdm",
                table: "Contactos",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_ContactosUsuariosAccesos_ContactoId",
                schema: "mdm",
                table: "ContactosUsuariosAccesos",
                column: "ContactoId");

            migrationBuilder.CreateIndex(
                name: "IX_ContactosUsuariosAccesos_EmpresaId",
                schema: "mdm",
                table: "ContactosUsuariosAccesos",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_ContactosUsuariosAccesos_PerfilAccesoId",
                schema: "mdm",
                table: "ContactosUsuariosAccesos",
                column: "PerfilAccesoId");

            migrationBuilder.CreateIndex(
                name: "IX_ContactosUsuariosAccesos_UsuarioId",
                schema: "mdm",
                table: "ContactosUsuariosAccesos",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_ContactoUsuarioAcceso_Empresa_Contacto",
                schema: "mdm",
                table: "ContactosUsuariosAccesos",
                columns: new[] { "EmpresaId", "ContactoId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ContactoUsuarioAcceso_Empresa_Usuario",
                schema: "mdm",
                table: "ContactosUsuariosAccesos",
                columns: new[] { "EmpresaId", "UsuarioId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FormulariosSistema_Codigo",
                schema: "core",
                table: "FormulariosSistema",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FormulariosSistema_ModuloSistemaId_Orden",
                schema: "core",
                table: "FormulariosSistema",
                columns: new[] { "ModuloSistemaId", "Orden" });

            migrationBuilder.CreateIndex(
                name: "IX_ModulosSistema_Codigo",
                schema: "core",
                table: "ModulosSistema",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PerfilesAcceso_EmpresaId",
                schema: "core",
                table: "PerfilesAcceso",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_PerfilesAcceso_EmpresaId_Codigo",
                schema: "core",
                table: "PerfilesAcceso",
                columns: new[] { "EmpresaId", "Codigo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PerfilesPermisos_AccionSistemaId",
                schema: "core",
                table: "PerfilesPermisos",
                column: "AccionSistemaId");

            migrationBuilder.CreateIndex(
                name: "IX_PerfilesPermisos_EmpresaId",
                schema: "core",
                table: "PerfilesPermisos",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_PerfilesPermisos_FormularioSistemaId",
                schema: "core",
                table: "PerfilesPermisos",
                column: "FormularioSistemaId");

            migrationBuilder.CreateIndex(
                name: "IX_PerfilesPermisos_PerfilAccesoId",
                schema: "core",
                table: "PerfilesPermisos",
                column: "PerfilAccesoId");

            migrationBuilder.CreateIndex(
                name: "IX_PerfilesPermisos_UQ",
                schema: "core",
                table: "PerfilesPermisos",
                columns: new[] { "EmpresaId", "PerfilAccesoId", "FormularioSistemaId", "AccionSistemaId" },
                unique: true,
                filter: "[AccionSistemaId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_UsuariosPerfiles_EmpresaId",
                schema: "core",
                table: "UsuariosPerfiles",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_UsuariosPerfiles_PerfilAccesoId",
                schema: "core",
                table: "UsuariosPerfiles",
                column: "PerfilAccesoId");

            migrationBuilder.CreateIndex(
                name: "IX_UsuariosPerfiles_UQ",
                schema: "core",
                table: "UsuariosPerfiles",
                columns: new[] { "UsuarioId", "EmpresaId", "PerfilAccesoId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UsuariosSucursalesAccesos_EmpresaId",
                schema: "core",
                table: "UsuariosSucursalesAccesos",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_UsuariosSucursalesAccesos_SucursalId",
                schema: "core",
                table: "UsuariosSucursalesAccesos",
                column: "SucursalId");

            migrationBuilder.CreateIndex(
                name: "IX_UsuarioSucursalAcceso_Default",
                schema: "core",
                table: "UsuariosSucursalesAccesos",
                columns: new[] { "UsuarioId", "EmpresaId", "EsPredeterminada" },
                unique: true,
                filter: "[EsPredeterminada] = 1 AND [Activo] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_UsuarioSucursalAcceso_UQ",
                schema: "core",
                table: "UsuariosSucursalesAccesos",
                columns: new[] { "UsuarioId", "EmpresaId", "SucursalId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContactosUsuariosAccesos",
                schema: "mdm");

            migrationBuilder.DropTable(
                name: "PerfilesPermisos",
                schema: "core");

            migrationBuilder.DropTable(
                name: "UsuariosPerfiles",
                schema: "core");

            migrationBuilder.DropTable(
                name: "UsuariosSucursalesAccesos",
                schema: "core");

            migrationBuilder.DropTable(
                name: "Contactos",
                schema: "mdm");

            migrationBuilder.DropTable(
                name: "AccionesSistema",
                schema: "core");

            migrationBuilder.DropTable(
                name: "PerfilesAcceso",
                schema: "core");

            migrationBuilder.DropTable(
                name: "FormulariosSistema",
                schema: "core");

            migrationBuilder.DropTable(
                name: "ModulosSistema",
                schema: "core");

            migrationBuilder.DropColumn(
                name: "AliasComercial",
                schema: "mdm",
                table: "ClienteSucursales");

            migrationBuilder.DropColumn(
                name: "Celular",
                schema: "mdm",
                table: "ClienteSucursales");

            migrationBuilder.DropColumn(
                name: "Email",
                schema: "mdm",
                table: "ClienteSucursales");

            migrationBuilder.DropColumn(
                name: "EsPuntoEntrega",
                schema: "mdm",
                table: "ClienteSucursales");

            migrationBuilder.DropColumn(
                name: "RecibeFacturacion",
                schema: "mdm",
                table: "ClienteSucursales");

            migrationBuilder.DropColumn(
                name: "RecibePedidos",
                schema: "mdm",
                table: "ClienteSucursales");

            migrationBuilder.DropColumn(
                name: "ResponsableCargo",
                schema: "mdm",
                table: "ClienteSucursales");

            migrationBuilder.DropColumn(
                name: "ResponsableNombre",
                schema: "mdm",
                table: "ClienteSucursales");

            migrationBuilder.DropColumn(
                name: "Telefono",
                schema: "mdm",
                table: "ClienteSucursales");

            migrationBuilder.DropColumn(
                name: "UrlMapa",
                schema: "mdm",
                table: "ClienteSucursales");

            migrationBuilder.DropColumn(
                name: "WhatsApp",
                schema: "mdm",
                table: "ClienteSucursales");
        }
    }
}
