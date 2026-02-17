using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgoraHub360.ERP.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDefaultAdminUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Insertar Empresa por defecto
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM [core].[Empresas] WHERE Id = 1)
                BEGIN
                    SET IDENTITY_INSERT [core].[Empresas] ON;
                    INSERT INTO [core].[Empresas] 
                        (Id, Nombre, NIT, Direccion, Telefono, Email, MonedaBaseId, FechaCreacion, Activo)
                    VALUES 
                        (1, 'AgoraHub360 - Empresa Demo', '1234567890', 'Av. Principal #123', '+591 12345678', 
                         'demo@agorahub360.com', 'BOB', GETDATE(), 1);
                    SET IDENTITY_INSERT [core].[Empresas] OFF;
                END
            ");

            // Insertar Usuario Administrador por defecto
            // Usuario: admin | Password: Admin123 | Hash: O2Esdae1BIpDX7bsgeUv+S1teVqLWpwXBw9qY8l6U7I=
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM [core].[Usuarios] WHERE NombreUsuario = 'admin')
                BEGIN
                    SET IDENTITY_INSERT [core].[Usuarios] ON;
                    INSERT INTO [core].[Usuarios] 
                        (Id, NombreUsuario, Email, PasswordHash, NombreCompleto, EmpresaActivaId, FechaCreacion, Activo)
                    VALUES 
                        (1, 'admin', 'admin@agorahub360.com', 'O2Esdae1BIpDX7bsgeUv+S1teVqLWpwXBw9qY8l6U7I=', 
                         'Administrador del Sistema', 1, GETDATE(), 1);
                    SET IDENTITY_INSERT [core].[Usuarios] OFF;
                END
            ");

            // Asignar rol Admin al usuario en la empresa demo
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM [core].[UsuarioEmpresas] WHERE UsuarioId = 1 AND EmpresaId = 1)
                BEGIN
                    INSERT INTO [core].[UsuarioEmpresas] (UsuarioId, EmpresaId, Rol)
                    VALUES (1, 1, 'Admin');
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Eliminar asignación de rol
            migrationBuilder.Sql("DELETE FROM [core].[UsuarioEmpresas] WHERE UsuarioId = 1 AND EmpresaId = 1");
            
            // Eliminar usuario admin
            migrationBuilder.Sql("DELETE FROM [core].[Usuarios] WHERE Id = 1");
            
            // Eliminar empresa demo
            migrationBuilder.Sql("DELETE FROM [core].[Empresas] WHERE Id = 1");
        }
    }
}
