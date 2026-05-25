IF OBJECT_ID(N'[core].[__EFMigrationsHistory]') IS NULL
BEGIN
    IF SCHEMA_ID(N'core') IS NULL EXEC(N'CREATE SCHEMA [core];');
    CREATE TABLE [core].[__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260217065412_BaseCore'
)
BEGIN
    IF SCHEMA_ID(N'core') IS NULL EXEC(N'CREATE SCHEMA [core];');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260217065412_BaseCore'
)
BEGIN
    CREATE TABLE [core].[Monedas] (
        [Codigo] nvarchar(3) NOT NULL,
        [Nombre] nvarchar(100) NOT NULL,
        [Simbolo] nvarchar(5) NOT NULL,
        [Decimales] int NOT NULL,
        [FechaCreacion] datetime2 NOT NULL,
        [CreadoPor] nvarchar(100) NULL,
        [FechaModificacion] datetime2 NULL,
        [ModificadoPor] nvarchar(100) NULL,
        [Activo] bit NOT NULL,
        CONSTRAINT [PK_Monedas] PRIMARY KEY ([Codigo])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260217065412_BaseCore'
)
BEGIN
    CREATE TABLE [core].[Empresas] (
        [Id] int NOT NULL IDENTITY,
        [Nombre] nvarchar(200) NOT NULL,
        [NIT] nvarchar(50) NULL,
        [Direccion] nvarchar(500) NULL,
        [Telefono] nvarchar(50) NULL,
        [Email] nvarchar(200) NULL,
        [MonedaBaseId] nvarchar(3) NULL,
        [FechaCreacion] datetime2 NOT NULL,
        [CreadoPor] nvarchar(100) NULL,
        [FechaModificacion] datetime2 NULL,
        [ModificadoPor] nvarchar(100) NULL,
        [Activo] bit NOT NULL,
        CONSTRAINT [PK_Empresas] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Empresas_Monedas_MonedaBaseId] FOREIGN KEY ([MonedaBaseId]) REFERENCES [core].[Monedas] ([Codigo]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260217065412_BaseCore'
)
BEGIN
    CREATE TABLE [core].[Usuarios] (
        [Id] int NOT NULL IDENTITY,
        [NombreUsuario] nvarchar(100) NOT NULL,
        [Email] nvarchar(200) NOT NULL,
        [PasswordHash] nvarchar(500) NOT NULL,
        [NombreCompleto] nvarchar(300) NOT NULL,
        [EmpresaActivaId] int NULL,
        [FechaCreacion] datetime2 NOT NULL,
        [CreadoPor] nvarchar(100) NULL,
        [FechaModificacion] datetime2 NULL,
        [ModificadoPor] nvarchar(100) NULL,
        [Activo] bit NOT NULL,
        CONSTRAINT [PK_Usuarios] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Usuarios_Empresas_EmpresaActivaId] FOREIGN KEY ([EmpresaActivaId]) REFERENCES [core].[Empresas] ([Id]) ON DELETE SET NULL
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260217065412_BaseCore'
)
BEGIN
    CREATE TABLE [core].[UsuarioEmpresas] (
        [UsuarioId] int NOT NULL,
        [EmpresaId] int NOT NULL,
        [Rol] nvarchar(50) NOT NULL DEFAULT N'Viewer',
        CONSTRAINT [PK_UsuarioEmpresas] PRIMARY KEY ([UsuarioId], [EmpresaId]),
        CONSTRAINT [FK_UsuarioEmpresas_Empresas_EmpresaId] FOREIGN KEY ([EmpresaId]) REFERENCES [core].[Empresas] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_UsuarioEmpresas_Usuarios_UsuarioId] FOREIGN KEY ([UsuarioId]) REFERENCES [core].[Usuarios] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260217065412_BaseCore'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Codigo', N'Activo', N'CreadoPor', N'Decimales', N'FechaCreacion', N'FechaModificacion', N'ModificadoPor', N'Nombre', N'Simbolo') AND [object_id] = OBJECT_ID(N'[core].[Monedas]'))
        SET IDENTITY_INSERT [core].[Monedas] ON;
    EXEC(N'INSERT INTO [core].[Monedas] ([Codigo], [Activo], [CreadoPor], [Decimales], [FechaCreacion], [FechaModificacion], [ModificadoPor], [Nombre], [Simbolo])
    VALUES (N''BOB'', CAST(1 AS bit), NULL, 2, ''0001-01-01T00:00:00.0000000'', NULL, NULL, N''Boliviano'', N''Bs''),
    (N''EUR'', CAST(1 AS bit), NULL, 2, ''0001-01-01T00:00:00.0000000'', NULL, NULL, N''Euro'', N''€''),
    (N''USD'', CAST(1 AS bit), NULL, 2, ''0001-01-01T00:00:00.0000000'', NULL, NULL, N''Dólar Estadounidense'', N''$'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Codigo', N'Activo', N'CreadoPor', N'Decimales', N'FechaCreacion', N'FechaModificacion', N'ModificadoPor', N'Nombre', N'Simbolo') AND [object_id] = OBJECT_ID(N'[core].[Monedas]'))
        SET IDENTITY_INSERT [core].[Monedas] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260217065412_BaseCore'
)
BEGIN
    CREATE INDEX [IX_Empresas_MonedaBaseId] ON [core].[Empresas] ([MonedaBaseId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260217065412_BaseCore'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Empresas_NIT] ON [core].[Empresas] ([NIT]) WHERE [NIT] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260217065412_BaseCore'
)
BEGIN
    CREATE INDEX [IX_Empresas_Nombre] ON [core].[Empresas] ([Nombre]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260217065412_BaseCore'
)
BEGIN
    CREATE INDEX [IX_UsuarioEmpresas_EmpresaId] ON [core].[UsuarioEmpresas] ([EmpresaId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260217065412_BaseCore'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Usuarios_Email] ON [core].[Usuarios] ([Email]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260217065412_BaseCore'
)
BEGIN
    CREATE INDEX [IX_Usuarios_EmpresaActivaId] ON [core].[Usuarios] ([EmpresaActivaId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260217065412_BaseCore'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Usuarios_NombreUsuario] ON [core].[Usuarios] ([NombreUsuario]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260217065412_BaseCore'
)
BEGIN
    INSERT INTO [core].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260217065412_BaseCore', N'8.0.12');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260217084352_AddRolesTable'
)
BEGIN
    CREATE TABLE [core].[Roles] (
        [Id] int NOT NULL IDENTITY,
        [Nombre] nvarchar(50) NOT NULL,
        [Descripcion] nvarchar(200) NULL,
        [FechaCreacion] datetime2 NOT NULL,
        [CreadoPor] nvarchar(100) NULL,
        [FechaModificacion] datetime2 NULL,
        [ModificadoPor] nvarchar(100) NULL,
        [Activo] bit NOT NULL,
        CONSTRAINT [PK_Roles] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260217084352_AddRolesTable'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Activo', N'CreadoPor', N'Descripcion', N'FechaCreacion', N'FechaModificacion', N'ModificadoPor', N'Nombre') AND [object_id] = OBJECT_ID(N'[core].[Roles]'))
        SET IDENTITY_INSERT [core].[Roles] ON;
    EXEC(N'INSERT INTO [core].[Roles] ([Id], [Activo], [CreadoPor], [Descripcion], [FechaCreacion], [FechaModificacion], [ModificadoPor], [Nombre])
    VALUES (1, CAST(1 AS bit), NULL, N''Administrador con acceso total'', ''0001-01-01T00:00:00.0000000'', NULL, NULL, N''Admin''),
    (2, CAST(1 AS bit), NULL, N''Gerente con acceso a reportes y aprobaciones'', ''0001-01-01T00:00:00.0000000'', NULL, NULL, N''Manager''),
    (3, CAST(1 AS bit), NULL, N''Usuario operativo con acceso a modulos asignados'', ''0001-01-01T00:00:00.0000000'', NULL, NULL, N''User''),
    (4, CAST(1 AS bit), NULL, N''Solo lectura'', ''0001-01-01T00:00:00.0000000'', NULL, NULL, N''Viewer'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Activo', N'CreadoPor', N'Descripcion', N'FechaCreacion', N'FechaModificacion', N'ModificadoPor', N'Nombre') AND [object_id] = OBJECT_ID(N'[core].[Roles]'))
        SET IDENTITY_INSERT [core].[Roles] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260217084352_AddRolesTable'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Roles_Nombre] ON [core].[Roles] ([Nombre]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260217084352_AddRolesTable'
)
BEGIN
    INSERT INTO [core].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260217084352_AddRolesTable', N'8.0.12');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260217090218_AddAuditLogTable'
)
BEGIN
    CREATE TABLE [core].[AuditLogs] (
        [Id] bigint NOT NULL IDENTITY,
        [Entidad] nvarchar(128) NOT NULL,
        [EntidadId] nvarchar(128) NOT NULL,
        [Accion] nvarchar(20) NOT NULL,
        [ValoresAnteriores] nvarchar(max) NULL,
        [ValoresNuevos] nvarchar(max) NULL,
        [CamposModificados] nvarchar(1000) NULL,
        [EmpresaId] int NULL,
        [Usuario] nvarchar(256) NULL,
        [FechaHora] datetime2 NOT NULL,
        CONSTRAINT [PK_AuditLogs] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260217090218_AddAuditLogTable'
)
BEGIN
    CREATE INDEX [IX_AuditLogs_EmpresaId] ON [core].[AuditLogs] ([EmpresaId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260217090218_AddAuditLogTable'
)
BEGIN
    CREATE INDEX [IX_AuditLogs_Entidad] ON [core].[AuditLogs] ([Entidad]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260217090218_AddAuditLogTable'
)
BEGIN
    CREATE INDEX [IX_AuditLogs_Entidad_EntidadId] ON [core].[AuditLogs] ([Entidad], [EntidadId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260217090218_AddAuditLogTable'
)
BEGIN
    CREATE INDEX [IX_AuditLogs_FechaHora] ON [core].[AuditLogs] ([FechaHora]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260217090218_AddAuditLogTable'
)
BEGIN
    INSERT INTO [core].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260217090218_AddAuditLogTable', N'8.0.12');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260217091857_AddParametroSistemaNumeracionDocumento'
)
BEGIN
    CREATE TABLE [core].[NumeracionesDocumento] (
        [Id] int NOT NULL IDENTITY,
        [TipoDocumento] nvarchar(20) NOT NULL,
        [Descripcion] nvarchar(100) NOT NULL,
        [Prefijo] nvarchar(20) NOT NULL,
        [SiguienteNumero] int NOT NULL,
        [Digitos] int NOT NULL,
        [FechaCreacion] datetime2 NOT NULL,
        [CreadoPor] nvarchar(100) NULL,
        [FechaModificacion] datetime2 NULL,
        [ModificadoPor] nvarchar(100) NULL,
        [Activo] bit NOT NULL,
        [EmpresaId] int NOT NULL,
        CONSTRAINT [PK_NumeracionesDocumento] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260217091857_AddParametroSistemaNumeracionDocumento'
)
BEGIN
    CREATE TABLE [core].[ParametrosSistema] (
        [Id] int NOT NULL IDENTITY,
        [Clave] nvarchar(100) NOT NULL,
        [Valor] nvarchar(500) NOT NULL,
        [Descripcion] nvarchar(500) NULL,
        [Categoria] nvarchar(50) NOT NULL,
        [TipoDato] nvarchar(20) NOT NULL,
        [FechaCreacion] datetime2 NOT NULL,
        [CreadoPor] nvarchar(100) NULL,
        [FechaModificacion] datetime2 NULL,
        [ModificadoPor] nvarchar(100) NULL,
        [Activo] bit NOT NULL,
        [EmpresaId] int NOT NULL,
        CONSTRAINT [PK_ParametrosSistema] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260217091857_AddParametroSistemaNumeracionDocumento'
)
BEGIN
    CREATE INDEX [IX_NumeracionesDocumento_EmpresaId] ON [core].[NumeracionesDocumento] ([EmpresaId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260217091857_AddParametroSistemaNumeracionDocumento'
)
BEGIN
    CREATE UNIQUE INDEX [IX_NumeracionesDocumento_EmpresaId_TipoDocumento] ON [core].[NumeracionesDocumento] ([EmpresaId], [TipoDocumento]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260217091857_AddParametroSistemaNumeracionDocumento'
)
BEGIN
    CREATE INDEX [IX_ParametrosSistema_EmpresaId] ON [core].[ParametrosSistema] ([EmpresaId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260217091857_AddParametroSistemaNumeracionDocumento'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ParametrosSistema_EmpresaId_Clave] ON [core].[ParametrosSistema] ([EmpresaId], [Clave]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260217091857_AddParametroSistemaNumeracionDocumento'
)
BEGIN
    INSERT INTO [core].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260217091857_AddParametroSistemaNumeracionDocumento', N'8.0.12');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260217153259_AddDefaultAdminUser'
)
BEGIN

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
                
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260217153259_AddDefaultAdminUser'
)
BEGIN

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
                
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260217153259_AddDefaultAdminUser'
)
BEGIN

                    IF NOT EXISTS (SELECT 1 FROM [core].[UsuarioEmpresas] WHERE UsuarioId = 1 AND EmpresaId = 1)
                    BEGIN
                        INSERT INTO [core].[UsuarioEmpresas] (UsuarioId, EmpresaId, Rol)
                        VALUES (1, 1, 'Admin');
                    END
                
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260217153259_AddDefaultAdminUser'
)
BEGIN
    INSERT INTO [core].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260217153259_AddDefaultAdminUser', N'8.0.12');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260217191851_AddMDMEntities'
)
BEGIN
    IF SCHEMA_ID(N'mdm') IS NULL EXEC(N'CREATE SCHEMA [mdm];');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260217191851_AddMDMEntities'
)
BEGIN
    CREATE TABLE [mdm].[CategoriasProducto] (
        [Id] int NOT NULL IDENTITY,
        [Nombre] nvarchar(100) NOT NULL,
        [Descripcion] nvarchar(500) NULL,
        [FechaCreacion] datetime2 NOT NULL,
        [CreadoPor] nvarchar(100) NULL,
        [FechaModificacion] datetime2 NULL,
        [ModificadoPor] nvarchar(100) NULL,
        [Activo] bit NOT NULL,
        [EmpresaId] int NOT NULL,
        CONSTRAINT [PK_CategoriasProducto] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260217191851_AddMDMEntities'
)
BEGIN
    CREATE TABLE [mdm].[Clientes] (
        [Id] int NOT NULL IDENTITY,
        [Codigo] nvarchar(50) NOT NULL,
        [RazonSocial] nvarchar(200) NOT NULL,
        [NIT] nvarchar(50) NULL,
        [Direccion] nvarchar(300) NULL,
        [Telefono] nvarchar(50) NULL,
        [Email] nvarchar(200) NULL,
        [NombreContacto] nvarchar(200) NULL,
        [TipoCliente] nvarchar(50) NOT NULL,
        [FechaCreacion] datetime2 NOT NULL,
        [CreadoPor] nvarchar(100) NULL,
        [FechaModificacion] datetime2 NULL,
        [ModificadoPor] nvarchar(100) NULL,
        [Activo] bit NOT NULL,
        [EmpresaId] int NOT NULL,
        CONSTRAINT [PK_Clientes] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260217191851_AddMDMEntities'
)
BEGIN
    CREATE TABLE [mdm].[Productos] (
        [Id] int NOT NULL IDENTITY,
        [Codigo] nvarchar(50) NOT NULL,
        [Nombre] nvarchar(200) NOT NULL,
        [Descripcion] nvarchar(500) NULL,
        [CategoriaProductoId] int NOT NULL,
        [UnidadMedidaId] int NOT NULL,
        [PrecioCompra] decimal(18,4) NOT NULL,
        [PrecioVenta] decimal(18,4) NOT NULL,
        [StockMinimo] decimal(18,4) NOT NULL,
        [Sku] nvarchar(50) NULL,
        [FechaCreacion] datetime2 NOT NULL,
        [CreadoPor] nvarchar(100) NULL,
        [FechaModificacion] datetime2 NULL,
        [ModificadoPor] nvarchar(100) NULL,
        [Activo] bit NOT NULL,
        [EmpresaId] int NOT NULL,
        CONSTRAINT [PK_Productos] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260217191851_AddMDMEntities'
)
BEGIN
    CREATE TABLE [mdm].[Proveedores] (
        [Id] int NOT NULL IDENTITY,
        [Codigo] nvarchar(50) NOT NULL,
        [RazonSocial] nvarchar(200) NOT NULL,
        [NIT] nvarchar(50) NULL,
        [Direccion] nvarchar(300) NULL,
        [Telefono] nvarchar(50) NULL,
        [Email] nvarchar(200) NULL,
        [NombreContacto] nvarchar(200) NULL,
        [FechaCreacion] datetime2 NOT NULL,
        [CreadoPor] nvarchar(100) NULL,
        [FechaModificacion] datetime2 NULL,
        [ModificadoPor] nvarchar(100) NULL,
        [Activo] bit NOT NULL,
        [EmpresaId] int NOT NULL,
        CONSTRAINT [PK_Proveedores] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260217191851_AddMDMEntities'
)
BEGIN
    CREATE TABLE [mdm].[UnidadesMedida] (
        [Id] int NOT NULL IDENTITY,
        [Nombre] nvarchar(100) NOT NULL,
        [Abreviatura] nvarchar(10) NOT NULL,
        [FechaCreacion] datetime2 NOT NULL,
        [CreadoPor] nvarchar(100) NULL,
        [FechaModificacion] datetime2 NULL,
        [ModificadoPor] nvarchar(100) NULL,
        [Activo] bit NOT NULL,
        [EmpresaId] int NOT NULL,
        CONSTRAINT [PK_UnidadesMedida] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260217191851_AddMDMEntities'
)
BEGIN
    CREATE INDEX [IX_CategoriasProducto_EmpresaId] ON [mdm].[CategoriasProducto] ([EmpresaId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260217191851_AddMDMEntities'
)
BEGIN
    CREATE UNIQUE INDEX [IX_CategoriasProducto_EmpresaId_Nombre] ON [mdm].[CategoriasProducto] ([EmpresaId], [Nombre]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260217191851_AddMDMEntities'
)
BEGIN
    CREATE INDEX [IX_Clientes_EmpresaId] ON [mdm].[Clientes] ([EmpresaId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260217191851_AddMDMEntities'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Clientes_EmpresaId_Codigo] ON [mdm].[Clientes] ([EmpresaId], [Codigo]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260217191851_AddMDMEntities'
)
BEGIN
    CREATE INDEX [IX_Productos_CategoriaProductoId] ON [mdm].[Productos] ([CategoriaProductoId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260217191851_AddMDMEntities'
)
BEGIN
    CREATE INDEX [IX_Productos_EmpresaId] ON [mdm].[Productos] ([EmpresaId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260217191851_AddMDMEntities'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Productos_EmpresaId_Codigo] ON [mdm].[Productos] ([EmpresaId], [Codigo]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260217191851_AddMDMEntities'
)
BEGIN
    CREATE INDEX [IX_Productos_UnidadMedidaId] ON [mdm].[Productos] ([UnidadMedidaId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260217191851_AddMDMEntities'
)
BEGIN
    CREATE INDEX [IX_Proveedores_EmpresaId] ON [mdm].[Proveedores] ([EmpresaId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260217191851_AddMDMEntities'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Proveedores_EmpresaId_Codigo] ON [mdm].[Proveedores] ([EmpresaId], [Codigo]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260217191851_AddMDMEntities'
)
BEGIN
    CREATE INDEX [IX_UnidadesMedida_EmpresaId] ON [mdm].[UnidadesMedida] ([EmpresaId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260217191851_AddMDMEntities'
)
BEGIN
    CREATE UNIQUE INDEX [IX_UnidadesMedida_EmpresaId_Abreviatura] ON [mdm].[UnidadesMedida] ([EmpresaId], [Abreviatura]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260217191851_AddMDMEntities'
)
BEGIN
    CREATE UNIQUE INDEX [IX_UnidadesMedida_EmpresaId_Nombre] ON [mdm].[UnidadesMedida] ([EmpresaId], [Nombre]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260217191851_AddMDMEntities'
)
BEGIN
    INSERT INTO [core].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260217191851_AddMDMEntities', N'8.0.12');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260217194759_MDM_Enhanced'
)
BEGIN
    ALTER TABLE [mdm].[Proveedores] ADD [CondicionPago] nvarchar(100) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260217194759_MDM_Enhanced'
)
BEGIN
    ALTER TABLE [mdm].[Proveedores] ADD [Pais] nvarchar(100) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260217194759_MDM_Enhanced'
)
BEGIN
    ALTER TABLE [mdm].[Proveedores] ADD [TipoProveedor] nvarchar(50) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260217194759_MDM_Enhanced'
)
BEGIN
    ALTER TABLE [mdm].[Productos] ADD [ControlStock] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260217194759_MDM_Enhanced'
)
BEGIN
    ALTER TABLE [mdm].[Productos] ADD [CostoBase] decimal(18,4) NOT NULL DEFAULT 0.0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260217194759_MDM_Enhanced'
)
BEGIN
    ALTER TABLE [mdm].[Productos] ADD [TipoProducto] nvarchar(50) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260217194759_MDM_Enhanced'
)
BEGIN
    CREATE TABLE [mdm].[Almacenes] (
        [Id] int NOT NULL IDENTITY,
        [Codigo] nvarchar(50) NOT NULL,
        [Nombre] nvarchar(200) NOT NULL,
        [Direccion] nvarchar(300) NULL,
        [Responsable] nvarchar(200) NULL,
        [Telefono] nvarchar(50) NULL,
        [FechaCreacion] datetime2 NOT NULL,
        [CreadoPor] nvarchar(100) NULL,
        [FechaModificacion] datetime2 NULL,
        [ModificadoPor] nvarchar(100) NULL,
        [Activo] bit NOT NULL,
        [EmpresaId] int NOT NULL,
        CONSTRAINT [PK_Almacenes] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260217194759_MDM_Enhanced'
)
BEGIN
    CREATE TABLE [mdm].[UbicacionesAlmacen] (
        [Id] int NOT NULL IDENTITY,
        [AlmacenId] int NOT NULL,
        [Codigo] nvarchar(50) NOT NULL,
        [Nombre] nvarchar(200) NOT NULL,
        [Descripcion] nvarchar(500) NULL,
        [FechaCreacion] datetime2 NOT NULL,
        [CreadoPor] nvarchar(100) NULL,
        [FechaModificacion] datetime2 NULL,
        [ModificadoPor] nvarchar(100) NULL,
        [Activo] bit NOT NULL,
        [EmpresaId] int NOT NULL,
        CONSTRAINT [PK_UbicacionesAlmacen] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260217194759_MDM_Enhanced'
)
BEGIN
    CREATE INDEX [IX_Almacenes_EmpresaId] ON [mdm].[Almacenes] ([EmpresaId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260217194759_MDM_Enhanced'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Almacenes_EmpresaId_Codigo] ON [mdm].[Almacenes] ([EmpresaId], [Codigo]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260217194759_MDM_Enhanced'
)
BEGIN
    CREATE INDEX [IX_UbicacionesAlmacen_AlmacenId] ON [mdm].[UbicacionesAlmacen] ([AlmacenId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260217194759_MDM_Enhanced'
)
BEGIN
    CREATE INDEX [IX_UbicacionesAlmacen_EmpresaId] ON [mdm].[UbicacionesAlmacen] ([EmpresaId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260217194759_MDM_Enhanced'
)
BEGIN
    CREATE UNIQUE INDEX [IX_UbicacionesAlmacen_EmpresaId_AlmacenId_Codigo] ON [mdm].[UbicacionesAlmacen] ([EmpresaId], [AlmacenId], [Codigo]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260217194759_MDM_Enhanced'
)
BEGIN
    INSERT INTO [core].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260217194759_MDM_Enhanced', N'8.0.12');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260218215253_MDM_Refinements'
)
BEGIN
    IF SCHEMA_ID(N'cst') IS NULL EXEC(N'CREATE SCHEMA [cst];');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260218215253_MDM_Refinements'
)
BEGIN
    IF SCHEMA_ID(N'doc') IS NULL EXEC(N'CREATE SCHEMA [doc];');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260218215253_MDM_Refinements'
)
BEGIN
    IF SCHEMA_ID(N'ver') IS NULL EXEC(N'CREATE SCHEMA [ver];');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260218215253_MDM_Refinements'
)
BEGIN
    IF SCHEMA_ID(N'rul') IS NULL EXEC(N'CREATE SCHEMA [rul];');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260218215253_MDM_Refinements'
)
BEGIN
    IF SCHEMA_ID(N'prc') IS NULL EXEC(N'CREATE SCHEMA [prc];');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260218215253_MDM_Refinements'
)
BEGIN
    CREATE TABLE [mdm].[AttributeDefinitions] (
        [AttributeId] bigint NOT NULL IDENTITY,
        [IndustryId] int NOT NULL,
        [Code] nvarchar(60) NOT NULL,
        [Name] nvarchar(120) NOT NULL,
        [DataType] tinyint NOT NULL,
        [IsRequired] bit NOT NULL,
        [IsSearchable] bit NOT NULL,
        [IsVariantAxis] bit NOT NULL,
        [ValidationRegex] nvarchar(200) NULL,
        [MinValue] decimal(18,4) NULL,
        [MaxValue] decimal(18,4) NULL,
        [UnitHint] nvarchar(20) NULL,
        [FechaCreacion] datetime2 NOT NULL,
        [CreadoPor] nvarchar(100) NULL,
        [FechaModificacion] datetime2 NULL,
        [ModificadoPor] nvarchar(100) NULL,
        [Activo] bit NOT NULL,
        CONSTRAINT [PK_AttributeDefinitions] PRIMARY KEY ([AttributeId])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260218215253_MDM_Refinements'
)
BEGIN
    CREATE TABLE [mdm].[Brands] (
        [BrandId] bigint NOT NULL IDENTITY,
        [Nombre] nvarchar(120) NOT NULL,
        [LogoUrl] nvarchar(500) NULL,
        [FechaCreacion] datetime2 NOT NULL,
        [CreadoPor] nvarchar(100) NULL,
        [FechaModificacion] datetime2 NULL,
        [ModificadoPor] nvarchar(100) NULL,
        [Activo] bit NOT NULL,
        CONSTRAINT [PK_Brands] PRIMARY KEY ([BrandId])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260218215253_MDM_Refinements'
)
BEGIN
    CREATE TABLE [mdm].[Catalogs] (
        [CatalogId] bigint NOT NULL IDENTITY,
        [Scope] tinyint NOT NULL,
        [EmpresaId] int NULL,
        [Nombre] nvarchar(120) NOT NULL,
        [IsDefault] bit NOT NULL,
        [FechaCreacion] datetime2 NOT NULL,
        [CreadoPor] nvarchar(100) NULL,
        [FechaModificacion] datetime2 NULL,
        [ModificadoPor] nvarchar(100) NULL,
        [Activo] bit NOT NULL,
        CONSTRAINT [PK_Catalogs] PRIMARY KEY ([CatalogId])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260218215253_MDM_Refinements'
)
BEGIN
    CREATE TABLE [cst].[CostingRules] (
        [EmpresaId] int NOT NULL,
        [ProductKind] tinyint NOT NULL,
        [DefaultMethod] tinyint NOT NULL,
        [FechaCreacion] datetime2 NOT NULL,
        [CreadoPor] nvarchar(100) NULL,
        [FechaModificacion] datetime2 NULL,
        [ModificadoPor] nvarchar(100) NULL,
        [Activo] bit NOT NULL,
        CONSTRAINT [PK_CostingRules] PRIMARY KEY ([EmpresaId], [ProductKind])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260218215253_MDM_Refinements'
)
BEGIN
    CREATE TABLE [doc].[Documents] (
        [DocumentId] bigint NOT NULL IDENTITY,
        [StorageProvider] tinyint NOT NULL,
        [FileName] nvarchar(160) NOT NULL,
        [MimeType] nvarchar(80) NOT NULL,
        [Url] nvarchar(500) NULL,
        [Hash] nvarchar(64) NULL,
        [SizeBytes] bigint NOT NULL,
        [FechaCreacion] datetime2 NOT NULL,
        [CreadoPor] nvarchar(100) NULL,
        [FechaModificacion] datetime2 NULL,
        [ModificadoPor] nvarchar(100) NULL,
        [Activo] bit NOT NULL,
        [EmpresaId] int NOT NULL,
        CONSTRAINT [PK_Documents] PRIMARY KEY ([DocumentId])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260218215253_MDM_Refinements'
)
BEGIN
    CREATE TABLE [ver].[EntityVersions] (
        [VersionId] bigint NOT NULL IDENTITY,
        [EntityName] nvarchar(80) NOT NULL,
        [EntityId] nvarchar(80) NOT NULL,
        [VersionNo] int NOT NULL,
        [ChangeType] tinyint NOT NULL,
        [SnapshotJson] nvarchar(max) NOT NULL,
        [ChangedBy] nvarchar(120) NULL,
        [ChangedAt] datetime2 NOT NULL,
        [EmpresaId] int NULL,
        CONSTRAINT [PK_EntityVersions] PRIMARY KEY ([VersionId])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260218215253_MDM_Refinements'
)
BEGIN
    CREATE TABLE [rul].[Industries] (
        [IndustryId] int NOT NULL IDENTITY,
        [Code] nvarchar(30) NOT NULL,
        [Name] nvarchar(80) NOT NULL,
        [FechaCreacion] datetime2 NOT NULL,
        [CreadoPor] nvarchar(100) NULL,
        [FechaModificacion] datetime2 NULL,
        [ModificadoPor] nvarchar(100) NULL,
        [Activo] bit NOT NULL,
        CONSTRAINT [PK_Industries] PRIMARY KEY ([IndustryId])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260218215253_MDM_Refinements'
)
BEGIN
    CREATE TABLE [cst].[LandedCostProfiles] (
        [ProfileId] bigint NOT NULL IDENTITY,
        [Name] nvarchar(60) NOT NULL,
        [AllocationMethod] tinyint NOT NULL,
        [IsDefault] bit NOT NULL,
        [FechaCreacion] datetime2 NOT NULL,
        [CreadoPor] nvarchar(100) NULL,
        [FechaModificacion] datetime2 NULL,
        [ModificadoPor] nvarchar(100) NULL,
        [Activo] bit NOT NULL,
        [EmpresaId] int NOT NULL,
        CONSTRAINT [PK_LandedCostProfiles] PRIMARY KEY ([ProfileId])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260218215253_MDM_Refinements'
)
BEGIN
    CREATE TABLE [mdm].[Manufacturers] (
        [ManufacturerId] bigint NOT NULL IDENTITY,
        [Nombre] nvarchar(160) NOT NULL,
        [Pais] nvarchar(80) NULL,
        [FechaCreacion] datetime2 NOT NULL,
        [CreadoPor] nvarchar(100) NULL,
        [FechaModificacion] datetime2 NULL,
        [ModificadoPor] nvarchar(100) NULL,
        [Activo] bit NOT NULL,
        CONSTRAINT [PK_Manufacturers] PRIMARY KEY ([ManufacturerId])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260218215253_MDM_Refinements'
)
BEGIN
    CREATE TABLE [prc].[PriceLists] (
        [PriceListId] bigint NOT NULL IDENTITY,
        [Code] nvarchar(30) NOT NULL,
        [Name] nvarchar(80) NOT NULL,
        [CurrencyId] nvarchar(3) NOT NULL,
        [ChannelId] int NULL,
        [ValidFrom] date NULL,
        [ValidTo] date NULL,
        [IsDefault] bit NOT NULL,
        [FechaCreacion] datetime2 NOT NULL,
        [CreadoPor] nvarchar(100) NULL,
        [FechaModificacion] datetime2 NULL,
        [ModificadoPor] nvarchar(100) NULL,
        [Activo] bit NOT NULL,
        [EmpresaId] int NOT NULL,
        CONSTRAINT [PK_PriceLists] PRIMARY KEY ([PriceListId])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260218215253_MDM_Refinements'
)
BEGIN
    CREATE TABLE [mdm].[ProductClassifications] (
        [ClassificationId] bigint NOT NULL IDENTITY,
        [CatalogId] bigint NOT NULL,
        [Type] tinyint NOT NULL,
        [Nombre] nvarchar(120) NOT NULL,
        [ParentId] bigint NULL,
        [FechaCreacion] datetime2 NOT NULL,
        [CreadoPor] nvarchar(100) NULL,
        [FechaModificacion] datetime2 NULL,
        [ModificadoPor] nvarchar(100) NULL,
        [Activo] bit NOT NULL,
        CONSTRAINT [PK_ProductClassifications] PRIMARY KEY ([ClassificationId]),
        CONSTRAINT [FK_ProductClassifications_ProductClassifications_ParentId] FOREIGN KEY ([ParentId]) REFERENCES [mdm].[ProductClassifications] ([ClassificationId]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260218215253_MDM_Refinements'
)
BEGIN
    CREATE TABLE [mdm].[ProductStatuses] (
        [ProductStatusId] int NOT NULL IDENTITY,
        [Code] nvarchar(30) NOT NULL,
        [Nombre] nvarchar(80) NOT NULL,
        [IsDefault] bit NOT NULL,
        [FechaCreacion] datetime2 NOT NULL,
        [CreadoPor] nvarchar(100) NULL,
        [FechaModificacion] datetime2 NULL,
        [ModificadoPor] nvarchar(100) NULL,
        [Activo] bit NOT NULL,
        CONSTRAINT [PK_ProductStatuses] PRIMARY KEY ([ProductStatusId])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260218215253_MDM_Refinements'
)
BEGIN
    CREATE TABLE [mdm].[Uoms] (
        [UomId] int NOT NULL IDENTITY,
        [Code] nvarchar(10) NOT NULL,
        [Nombre] nvarchar(40) NOT NULL,
        [FechaCreacion] datetime2 NOT NULL,
        [CreadoPor] nvarchar(100) NULL,
        [FechaModificacion] datetime2 NULL,
        [ModificadoPor] nvarchar(100) NULL,
        [Activo] bit NOT NULL,
        CONSTRAINT [PK_Uoms] PRIMARY KEY ([UomId])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260218215253_MDM_Refinements'
)
BEGIN
    CREATE TABLE [mdm].[AttributeOptions] (
        [OptionId] bigint NOT NULL IDENTITY,
        [AttributeId] bigint NOT NULL,
        [Value] nvarchar(120) NOT NULL,
        [SortOrder] int NOT NULL,
        [FechaCreacion] datetime2 NOT NULL,
        [CreadoPor] nvarchar(100) NULL,
        [FechaModificacion] datetime2 NULL,
        [ModificadoPor] nvarchar(100) NULL,
        [Activo] bit NOT NULL,
        CONSTRAINT [PK_AttributeOptions] PRIMARY KEY ([OptionId]),
        CONSTRAINT [FK_AttributeOptions_AttributeDefinitions_AttributeId] FOREIGN KEY ([AttributeId]) REFERENCES [mdm].[AttributeDefinitions] ([AttributeId]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260218215253_MDM_Refinements'
)
BEGIN
    CREATE TABLE [mdm].[Categories] (
        [CategoryId] bigint NOT NULL IDENTITY,
        [CatalogId] bigint NOT NULL,
        [ParentCategoryId] bigint NULL,
        [Nombre] nvarchar(120) NOT NULL,
        [Path] nvarchar(600) NULL,
        [SortOrder] int NOT NULL,
        [FechaCreacion] datetime2 NOT NULL,
        [CreadoPor] nvarchar(100) NULL,
        [FechaModificacion] datetime2 NULL,
        [ModificadoPor] nvarchar(100) NULL,
        [Activo] bit NOT NULL,
        CONSTRAINT [PK_Categories] PRIMARY KEY ([CategoryId]),
        CONSTRAINT [FK_Categories_Catalogs_CatalogId] FOREIGN KEY ([CatalogId]) REFERENCES [mdm].[Catalogs] ([CatalogId]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Categories_Categories_ParentCategoryId] FOREIGN KEY ([ParentCategoryId]) REFERENCES [mdm].[Categories] ([CategoryId]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260218215253_MDM_Refinements'
)
BEGIN
    CREATE TABLE [doc].[ProductDocuments] (
        [ProductId] bigint NOT NULL,
        [DocumentId] bigint NOT NULL,
        [DocType] tinyint NOT NULL,
        [SortOrder] int NOT NULL,
        CONSTRAINT [PK_ProductDocuments] PRIMARY KEY ([ProductId], [DocumentId]),
        CONSTRAINT [FK_ProductDocuments_Documents_DocumentId] FOREIGN KEY ([DocumentId]) REFERENCES [doc].[Documents] ([DocumentId]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260218215253_MDM_Refinements'
)
BEGIN
    CREATE TABLE [rul].[ProductIndustryRules] (
        [RuleId] bigint NOT NULL IDENTITY,
        [IndustryId] int NOT NULL,
        [ConditionJson] nvarchar(max) NOT NULL,
        [ActionsJson] nvarchar(max) NOT NULL,
        [Priority] int NOT NULL,
        [FechaCreacion] datetime2 NOT NULL,
        [CreadoPor] nvarchar(100) NULL,
        [FechaModificacion] datetime2 NULL,
        [ModificadoPor] nvarchar(100) NULL,
        [Activo] bit NOT NULL,
        CONSTRAINT [PK_ProductIndustryRules] PRIMARY KEY ([RuleId]),
        CONSTRAINT [FK_ProductIndustryRules_Industries_IndustryId] FOREIGN KEY ([IndustryId]) REFERENCES [rul].[Industries] ([IndustryId]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260218215253_MDM_Refinements'
)
BEGIN
    CREATE TABLE [prc].[PriceListItems] (
        [ItemId] bigint NOT NULL IDENTITY,
        [PriceListId] bigint NOT NULL,
        [CompanyProductId] bigint NULL,
        [VariantId] bigint NULL,
        [Price] decimal(18,4) NOT NULL,
        [MinQty] decimal(18,4) NULL,
        [DiscountPercent] decimal(9,4) NULL,
        [ValidFrom] date NULL,
        [ValidTo] date NULL,
        [FechaCreacion] datetime2 NOT NULL,
        [CreadoPor] nvarchar(100) NULL,
        [FechaModificacion] datetime2 NULL,
        [ModificadoPor] nvarchar(100) NULL,
        [Activo] bit NOT NULL,
        CONSTRAINT [PK_PriceListItems] PRIMARY KEY ([ItemId]),
        CONSTRAINT [FK_PriceListItems_PriceLists_PriceListId] FOREIGN KEY ([PriceListId]) REFERENCES [prc].[PriceLists] ([PriceListId]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260218215253_MDM_Refinements'
)
BEGIN
    CREATE TABLE [mdm].[Products] (
        [ProductId] bigint NOT NULL IDENTITY,
        [CatalogId] bigint NOT NULL,
        [ProductKind] tinyint NOT NULL,
        [NombreGenerico] nvarchar(180) NOT NULL,
        [NombreComercial] nvarchar(180) NOT NULL,
        [DescripcionCorta] nvarchar(300) NULL,
        [DescripcionLarga] nvarchar(max) NULL,
        [BrandId] bigint NULL,
        [ManufacturerId] bigint NULL,
        [DefaultUomId] int NOT NULL,
        [IsStockable] bit NOT NULL,
        [IsSellable] bit NOT NULL,
        [IsPurchasable] bit NOT NULL,
        [LifecycleStatusId] int NOT NULL,
        [FechaCreacion] datetime2 NOT NULL,
        [CreadoPor] nvarchar(100) NULL,
        [FechaModificacion] datetime2 NULL,
        [ModificadoPor] nvarchar(100) NULL,
        [Activo] bit NOT NULL,
        CONSTRAINT [PK_Products] PRIMARY KEY ([ProductId]),
        CONSTRAINT [FK_Products_Brands_BrandId] FOREIGN KEY ([BrandId]) REFERENCES [mdm].[Brands] ([BrandId]) ON DELETE SET NULL,
        CONSTRAINT [FK_Products_Catalogs_CatalogId] FOREIGN KEY ([CatalogId]) REFERENCES [mdm].[Catalogs] ([CatalogId]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Products_Manufacturers_ManufacturerId] FOREIGN KEY ([ManufacturerId]) REFERENCES [mdm].[Manufacturers] ([ManufacturerId]) ON DELETE SET NULL,
        CONSTRAINT [FK_Products_ProductStatuses_LifecycleStatusId] FOREIGN KEY ([LifecycleStatusId]) REFERENCES [mdm].[ProductStatuses] ([ProductStatusId]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Products_Uoms_DefaultUomId] FOREIGN KEY ([DefaultUomId]) REFERENCES [mdm].[Uoms] ([UomId]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260218215253_MDM_Refinements'
)
BEGIN
    CREATE TABLE [mdm].[CompanyProducts] (
        [CompanyProductId] bigint NOT NULL IDENTITY,
        [ProductId] bigint NOT NULL,
        [Sku] nvarchar(60) NOT NULL,
        [CodigoInterno] nvarchar(60) NULL,
        [ImpuestoProfileId] int NULL,
        [MonedaBaseId] nvarchar(3) NULL,
        [IsVisiblePOS] bit NOT NULL,
        [IsVisibleEcommerce] bit NOT NULL,
        [IsVisibleB2B] bit NOT NULL,
        [AllowReturns] bit NOT NULL,
        [WarrantyDays] int NULL,
        [MinStock] decimal(18,4) NULL,
        [MaxStock] decimal(18,4) NULL,
        [ReorderPoint] decimal(18,4) NULL,
        [CostingMethod] tinyint NULL,
        [FechaCreacion] datetime2 NOT NULL,
        [CreadoPor] nvarchar(100) NULL,
        [FechaModificacion] datetime2 NULL,
        [ModificadoPor] nvarchar(100) NULL,
        [Activo] bit NOT NULL,
        [EmpresaId] int NOT NULL,
        CONSTRAINT [PK_CompanyProducts] PRIMARY KEY ([CompanyProductId]),
        CONSTRAINT [FK_CompanyProducts_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [mdm].[Products] ([ProductId]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260218215253_MDM_Refinements'
)
BEGIN
    CREATE TABLE [mdm].[ProductAttributes] (
        [ProductAttributeId] bigint NOT NULL IDENTITY,
        [ProductId] bigint NOT NULL,
        [AttributeId] bigint NOT NULL,
        [ValueString] nvarchar(max) NULL,
        [ValueDecimal] decimal(18,4) NULL,
        [ValueInt] int NULL,
        [ValueBool] bit NULL,
        [ValueDate] date NULL,
        [ValueJson] nvarchar(max) NULL,
        [OptionId] bigint NULL,
        [ValidFrom] date NULL,
        [ValidTo] date NULL,
        [FechaCreacion] datetime2 NOT NULL,
        [CreadoPor] nvarchar(100) NULL,
        [FechaModificacion] datetime2 NULL,
        [ModificadoPor] nvarchar(100) NULL,
        [Activo] bit NOT NULL,
        CONSTRAINT [PK_ProductAttributes] PRIMARY KEY ([ProductAttributeId]),
        CONSTRAINT [FK_ProductAttributes_AttributeDefinitions_AttributeId] FOREIGN KEY ([AttributeId]) REFERENCES [mdm].[AttributeDefinitions] ([AttributeId]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ProductAttributes_AttributeOptions_OptionId] FOREIGN KEY ([OptionId]) REFERENCES [mdm].[AttributeOptions] ([OptionId]) ON DELETE SET NULL,
        CONSTRAINT [FK_ProductAttributes_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [mdm].[Products] ([ProductId]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260218215253_MDM_Refinements'
)
BEGIN
    CREATE TABLE [mdm].[ProductCategories] (
        [ProductId] bigint NOT NULL,
        [CategoryId] bigint NOT NULL,
        [IsPrimary] bit NOT NULL,
        CONSTRAINT [PK_ProductCategories] PRIMARY KEY ([ProductId], [CategoryId]),
        CONSTRAINT [FK_ProductCategories_Categories_CategoryId] FOREIGN KEY ([CategoryId]) REFERENCES [mdm].[Categories] ([CategoryId]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ProductCategories_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [mdm].[Products] ([ProductId]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260218215253_MDM_Refinements'
)
BEGIN
    CREATE TABLE [mdm].[ProductClassificationLinks] (
        [ProductId] bigint NOT NULL,
        [ClassificationId] bigint NOT NULL,
        CONSTRAINT [PK_ProductClassificationLinks] PRIMARY KEY ([ProductId], [ClassificationId]),
        CONSTRAINT [FK_ProductClassificationLinks_ProductClassifications_ClassificationId] FOREIGN KEY ([ClassificationId]) REFERENCES [mdm].[ProductClassifications] ([ClassificationId]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ProductClassificationLinks_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [mdm].[Products] ([ProductId]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260218215253_MDM_Refinements'
)
BEGIN
    CREATE TABLE [mdm].[ProductCodes] (
        [ProductCodeId] bigint NOT NULL IDENTITY,
        [EmpresaId] int NULL,
        [ProductId] bigint NOT NULL,
        [CodeType] tinyint NOT NULL,
        [Valor] nvarchar(120) NOT NULL,
        [ProviderId] bigint NULL,
        [CustomerId] bigint NULL,
        [ChannelId] int NULL,
        [ValidFrom] date NULL,
        [ValidTo] date NULL,
        [IsPrimary] bit NOT NULL,
        [IsActive] bit NOT NULL,
        [FechaCreacion] datetime2 NOT NULL,
        [CreadoPor] nvarchar(100) NULL,
        [FechaModificacion] datetime2 NULL,
        [ModificadoPor] nvarchar(100) NULL,
        [Activo] bit NOT NULL,
        CONSTRAINT [PK_ProductCodes] PRIMARY KEY ([ProductCodeId]),
        CONSTRAINT [FK_ProductCodes_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [mdm].[Products] ([ProductId]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260218215253_MDM_Refinements'
)
BEGIN
    CREATE TABLE [mdm].[ProductUoms] (
        [ProductUomId] bigint NOT NULL IDENTITY,
        [ProductId] bigint NOT NULL,
        [UomId] int NOT NULL,
        [IsBase] bit NOT NULL,
        [FactorToBase] decimal(18,6) NOT NULL,
        [Barcode] nvarchar(120) NULL,
        [FechaCreacion] datetime2 NOT NULL,
        [CreadoPor] nvarchar(100) NULL,
        [FechaModificacion] datetime2 NULL,
        [ModificadoPor] nvarchar(100) NULL,
        [Activo] bit NOT NULL,
        CONSTRAINT [PK_ProductUoms] PRIMARY KEY ([ProductUomId]),
        CONSTRAINT [FK_ProductUoms_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [mdm].[Products] ([ProductId]) ON DELETE CASCADE,
        CONSTRAINT [FK_ProductUoms_Uoms_UomId] FOREIGN KEY ([UomId]) REFERENCES [mdm].[Uoms] ([UomId]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260218215253_MDM_Refinements'
)
BEGIN
    CREATE TABLE [mdm].[ProductVariants] (
        [VariantId] bigint NOT NULL IDENTITY,
        [ParentProductId] bigint NOT NULL,
        [Sku] nvarchar(60) NOT NULL,
        [Barcode] nvarchar(120) NULL,
        [VariantName] nvarchar(180) NULL,
        [FechaCreacion] datetime2 NOT NULL,
        [CreadoPor] nvarchar(100) NULL,
        [FechaModificacion] datetime2 NULL,
        [ModificadoPor] nvarchar(100) NULL,
        [Activo] bit NOT NULL,
        [EmpresaId] int NOT NULL,
        CONSTRAINT [PK_ProductVariants] PRIMARY KEY ([VariantId]),
        CONSTRAINT [FK_ProductVariants_Products_ParentProductId] FOREIGN KEY ([ParentProductId]) REFERENCES [mdm].[Products] ([ProductId]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260218215253_MDM_Refinements'
)
BEGIN
    CREATE TABLE [mdm].[CompanyProductFeatures] (
        [EmpresaId] int NOT NULL,
        [CompanyProductId] bigint NOT NULL,
        [FeatureCode] nvarchar(40) NOT NULL,
        [IsEnabled] bit NOT NULL,
        CONSTRAINT [PK_CompanyProductFeatures] PRIMARY KEY ([EmpresaId], [CompanyProductId], [FeatureCode]),
        CONSTRAINT [FK_CompanyProductFeatures_CompanyProducts_CompanyProductId] FOREIGN KEY ([CompanyProductId]) REFERENCES [mdm].[CompanyProducts] ([CompanyProductId]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260218215253_MDM_Refinements'
)
BEGIN
    CREATE TABLE [mdm].[VariantAttributeValues] (
        [VariantId] bigint NOT NULL,
        [AttributeId] bigint NOT NULL,
        [OptionId] bigint NULL,
        [ValueString] nvarchar(120) NULL,
        CONSTRAINT [PK_VariantAttributeValues] PRIMARY KEY ([VariantId], [AttributeId]),
        CONSTRAINT [FK_VariantAttributeValues_AttributeDefinitions_AttributeId] FOREIGN KEY ([AttributeId]) REFERENCES [mdm].[AttributeDefinitions] ([AttributeId]) ON DELETE NO ACTION,
        CONSTRAINT [FK_VariantAttributeValues_AttributeOptions_OptionId] FOREIGN KEY ([OptionId]) REFERENCES [mdm].[AttributeOptions] ([OptionId]) ON DELETE SET NULL,
        CONSTRAINT [FK_VariantAttributeValues_ProductVariants_VariantId] FOREIGN KEY ([VariantId]) REFERENCES [mdm].[ProductVariants] ([VariantId]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260218215253_MDM_Refinements'
)
BEGIN
    CREATE UNIQUE INDEX [IX_AttributeDefinitions_IndustryId_Code] ON [mdm].[AttributeDefinitions] ([IndustryId], [Code]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260218215253_MDM_Refinements'
)
BEGIN
    CREATE INDEX [IX_AttributeDefinitions_IsVariantAxis] ON [mdm].[AttributeDefinitions] ([IsVariantAxis]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260218215253_MDM_Refinements'
)
BEGIN
    CREATE UNIQUE INDEX [IX_AttributeOptions_AttributeId_Value] ON [mdm].[AttributeOptions] ([AttributeId], [Value]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260218215253_MDM_Refinements'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Brands_Nombre] ON [mdm].[Brands] ([Nombre]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260218215253_MDM_Refinements'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Catalogs_Scope_EmpresaId_Nombre] ON [mdm].[Catalogs] ([Scope], [EmpresaId], [Nombre]) WHERE [EmpresaId] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260218215253_MDM_Refinements'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Categories_CatalogId_ParentCategoryId_Nombre] ON [mdm].[Categories] ([CatalogId], [ParentCategoryId], [Nombre]) WHERE [ParentCategoryId] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260218215253_MDM_Refinements'
)
BEGIN
    CREATE INDEX [IX_Categories_ParentCategoryId] ON [mdm].[Categories] ([ParentCategoryId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260218215253_MDM_Refinements'
)
BEGIN
    CREATE INDEX [IX_CompanyProductFeatures_CompanyProductId] ON [mdm].[CompanyProductFeatures] ([CompanyProductId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260218215253_MDM_Refinements'
)
BEGIN
    CREATE INDEX [IX_CompanyProducts_EmpresaId] ON [mdm].[CompanyProducts] ([EmpresaId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260218215253_MDM_Refinements'
)
BEGIN
    CREATE INDEX [IX_CompanyProducts_EmpresaId_IsVisiblePOS_IsVisibleEcommerce] ON [mdm].[CompanyProducts] ([EmpresaId], [IsVisiblePOS], [IsVisibleEcommerce]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260218215253_MDM_Refinements'
)
BEGIN
    CREATE UNIQUE INDEX [IX_CompanyProducts_EmpresaId_ProductId] ON [mdm].[CompanyProducts] ([EmpresaId], [ProductId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260218215253_MDM_Refinements'
)
BEGIN
    CREATE UNIQUE INDEX [IX_CompanyProducts_EmpresaId_Sku] ON [mdm].[CompanyProducts] ([EmpresaId], [Sku]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260218215253_MDM_Refinements'
)
BEGIN
    CREATE INDEX [IX_CompanyProducts_ProductId] ON [mdm].[CompanyProducts] ([ProductId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260218215253_MDM_Refinements'
)
BEGIN
    CREATE INDEX [IX_Documents_EmpresaId] ON [doc].[Documents] ([EmpresaId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260218215253_MDM_Refinements'
)
BEGIN
    CREATE INDEX [IX_EntityVersions_EmpresaId] ON [ver].[EntityVersions] ([EmpresaId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260218215253_MDM_Refinements'
)
BEGIN
    CREATE INDEX [IX_EntityVersions_EntityName_EntityId] ON [ver].[EntityVersions] ([EntityName], [EntityId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260218215253_MDM_Refinements'
)
BEGIN
    CREATE UNIQUE INDEX [IX_EntityVersions_EntityName_EntityId_VersionNo] ON [ver].[EntityVersions] ([EntityName], [EntityId], [VersionNo]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260218215253_MDM_Refinements'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Industries_Code] ON [rul].[Industries] ([Code]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260218215253_MDM_Refinements'
)
BEGIN
    CREATE INDEX [IX_LandedCostProfiles_EmpresaId] ON [cst].[LandedCostProfiles] ([EmpresaId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260218215253_MDM_Refinements'
)
BEGIN
    CREATE INDEX [IX_LandedCostProfiles_EmpresaId_IsDefault] ON [cst].[LandedCostProfiles] ([EmpresaId], [IsDefault]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260218215253_MDM_Refinements'
)
BEGIN
    CREATE INDEX [IX_Manufacturers_Nombre] ON [mdm].[Manufacturers] ([Nombre]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260218215253_MDM_Refinements'
)
BEGIN
    CREATE INDEX [IX_PriceListItems_PriceListId_CompanyProductId_MinQty_ValidFrom] ON [prc].[PriceListItems] ([PriceListId], [CompanyProductId], [MinQty], [ValidFrom]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260218215253_MDM_Refinements'
)
BEGIN
    CREATE INDEX [IX_PriceListItems_PriceListId_VariantId_MinQty_ValidFrom] ON [prc].[PriceListItems] ([PriceListId], [VariantId], [MinQty], [ValidFrom]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260218215253_MDM_Refinements'
)
BEGIN
    CREATE INDEX [IX_PriceLists_EmpresaId] ON [prc].[PriceLists] ([EmpresaId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260218215253_MDM_Refinements'
)
BEGIN
    CREATE UNIQUE INDEX [IX_PriceLists_EmpresaId_Code] ON [prc].[PriceLists] ([EmpresaId], [Code]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260218215253_MDM_Refinements'
)
BEGIN
    CREATE INDEX [IX_PriceLists_EmpresaId_IsDefault] ON [prc].[PriceLists] ([EmpresaId], [IsDefault]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260218215253_MDM_Refinements'
)
BEGIN
    CREATE INDEX [IX_ProductAttributes_AttributeId] ON [mdm].[ProductAttributes] ([AttributeId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260218215253_MDM_Refinements'
)
BEGIN
    CREATE INDEX [IX_ProductAttributes_OptionId] ON [mdm].[ProductAttributes] ([OptionId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260218215253_MDM_Refinements'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_ProductAttributes_ProductId_AttributeId_ValidFrom] ON [mdm].[ProductAttributes] ([ProductId], [AttributeId], [ValidFrom]) WHERE [ValidFrom] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260218215253_MDM_Refinements'
)
BEGIN
    CREATE INDEX [IX_ProductCategories_CategoryId] ON [mdm].[ProductCategories] ([CategoryId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260218215253_MDM_Refinements'
)
BEGIN
    CREATE INDEX [IX_ProductClassificationLinks_ClassificationId] ON [mdm].[ProductClassificationLinks] ([ClassificationId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260218215253_MDM_Refinements'
)
BEGIN
    CREATE INDEX [IX_ProductClassifications_CatalogId_Type_Nombre] ON [mdm].[ProductClassifications] ([CatalogId], [Type], [Nombre]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260218215253_MDM_Refinements'
)
BEGIN
    CREATE INDEX [IX_ProductClassifications_ParentId] ON [mdm].[ProductClassifications] ([ParentId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260218215253_MDM_Refinements'
)
BEGIN
    CREATE INDEX [IX_ProductCodes_EmpresaId_CodeType_Valor] ON [mdm].[ProductCodes] ([EmpresaId], [CodeType], [Valor]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260218215253_MDM_Refinements'
)
BEGIN
    CREATE INDEX [IX_ProductCodes_ProductId_CodeType_IsPrimary] ON [mdm].[ProductCodes] ([ProductId], [CodeType], [IsPrimary]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260218215253_MDM_Refinements'
)
BEGIN
    CREATE INDEX [IX_ProductDocuments_DocumentId] ON [doc].[ProductDocuments] ([DocumentId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260218215253_MDM_Refinements'
)
BEGIN
    CREATE INDEX [IX_ProductDocuments_ProductId_DocType] ON [doc].[ProductDocuments] ([ProductId], [DocType]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260218215253_MDM_Refinements'
)
BEGIN
    CREATE INDEX [IX_ProductIndustryRules_IndustryId_Priority] ON [rul].[ProductIndustryRules] ([IndustryId], [Priority]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260218215253_MDM_Refinements'
)
BEGIN
    CREATE INDEX [IX_Products_BrandId] ON [mdm].[Products] ([BrandId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260218215253_MDM_Refinements'
)
BEGIN
    CREATE INDEX [IX_Products_CatalogId_BrandId] ON [mdm].[Products] ([CatalogId], [BrandId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260218215253_MDM_Refinements'
)
BEGIN
    CREATE INDEX [IX_Products_CatalogId_NombreComercial] ON [mdm].[Products] ([CatalogId], [NombreComercial]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260218215253_MDM_Refinements'
)
BEGIN
    CREATE INDEX [IX_Products_DefaultUomId] ON [mdm].[Products] ([DefaultUomId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260218215253_MDM_Refinements'
)
BEGIN
    CREATE INDEX [IX_Products_LifecycleStatusId] ON [mdm].[Products] ([LifecycleStatusId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260218215253_MDM_Refinements'
)
BEGIN
    CREATE INDEX [IX_Products_ManufacturerId] ON [mdm].[Products] ([ManufacturerId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260218215253_MDM_Refinements'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ProductStatuses_Code] ON [mdm].[ProductStatuses] ([Code]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260218215253_MDM_Refinements'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ProductUoms_ProductId_UomId] ON [mdm].[ProductUoms] ([ProductId], [UomId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260218215253_MDM_Refinements'
)
BEGIN
    CREATE INDEX [IX_ProductUoms_UomId] ON [mdm].[ProductUoms] ([UomId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260218215253_MDM_Refinements'
)
BEGIN
    CREATE INDEX [IX_ProductVariants_EmpresaId] ON [mdm].[ProductVariants] ([EmpresaId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260218215253_MDM_Refinements'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ProductVariants_EmpresaId_Sku] ON [mdm].[ProductVariants] ([EmpresaId], [Sku]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260218215253_MDM_Refinements'
)
BEGIN
    CREATE INDEX [IX_ProductVariants_ParentProductId] ON [mdm].[ProductVariants] ([ParentProductId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260218215253_MDM_Refinements'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Uoms_Code] ON [mdm].[Uoms] ([Code]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260218215253_MDM_Refinements'
)
BEGIN
    CREATE INDEX [IX_VariantAttributeValues_AttributeId] ON [mdm].[VariantAttributeValues] ([AttributeId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260218215253_MDM_Refinements'
)
BEGIN
    CREATE INDEX [IX_VariantAttributeValues_OptionId] ON [mdm].[VariantAttributeValues] ([OptionId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260218215253_MDM_Refinements'
)
BEGIN
    INSERT INTO [core].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260218215253_MDM_Refinements', N'8.0.12');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260223085825_SeedProductStatus'
)
BEGIN
    DROP TABLE [mdm].[CategoriasProducto];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260223085825_SeedProductStatus'
)
BEGIN
    DROP TABLE [mdm].[Productos];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260223085825_SeedProductStatus'
)
BEGIN
    DROP TABLE [mdm].[UnidadesMedida];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260223085825_SeedProductStatus'
)
BEGIN
    IF SCHEMA_ID(N'inv') IS NULL EXEC(N'CREATE SCHEMA [inv];');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260223085825_SeedProductStatus'
)
BEGIN
    EXEC sp_rename N'[mdm].[Uoms].[Nombre]', N'Name', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260223085825_SeedProductStatus'
)
BEGIN
    EXEC sp_rename N'[mdm].[ProductStatuses].[Nombre]', N'Name', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260223085825_SeedProductStatus'
)
BEGIN
    EXEC sp_rename N'[mdm].[Products].[NombreGenerico]', N'GenericName', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260223085825_SeedProductStatus'
)
BEGIN
    EXEC sp_rename N'[mdm].[Products].[NombreComercial]', N'CommercialName', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260223085825_SeedProductStatus'
)
BEGIN
    EXEC sp_rename N'[mdm].[Products].[DescripcionLarga]', N'LongDescription', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260223085825_SeedProductStatus'
)
BEGIN
    EXEC sp_rename N'[mdm].[Products].[DescripcionCorta]', N'ShortDescription', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260223085825_SeedProductStatus'
)
BEGIN
    EXEC sp_rename N'[mdm].[Products].[IX_Products_CatalogId_NombreComercial]', N'IX_Products_CatalogId_CommercialName', N'INDEX';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260223085825_SeedProductStatus'
)
BEGIN
    EXEC sp_rename N'[mdm].[ProductClassifications].[Nombre]', N'Name', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260223085825_SeedProductStatus'
)
BEGIN
    EXEC sp_rename N'[mdm].[ProductClassifications].[IX_ProductClassifications_CatalogId_Type_Nombre]', N'IX_ProductClassifications_CatalogId_Type_Name', N'INDEX';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260223085825_SeedProductStatus'
)
BEGIN
    EXEC sp_rename N'[mdm].[Manufacturers].[Pais]', N'Country', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260223085825_SeedProductStatus'
)
BEGIN
    EXEC sp_rename N'[mdm].[Manufacturers].[Nombre]', N'Name', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260223085825_SeedProductStatus'
)
BEGIN
    EXEC sp_rename N'[mdm].[Manufacturers].[IX_Manufacturers_Nombre]', N'IX_Manufacturers_Name', N'INDEX';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260223085825_SeedProductStatus'
)
BEGIN
    EXEC sp_rename N'[mdm].[Categories].[Nombre]', N'Name', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260223085825_SeedProductStatus'
)
BEGIN
    EXEC sp_rename N'[mdm].[Categories].[IX_Categories_CatalogId_ParentCategoryId_Nombre]', N'IX_Categories_CatalogId_ParentCategoryId_Name', N'INDEX';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260223085825_SeedProductStatus'
)
BEGIN
    EXEC sp_rename N'[mdm].[Catalogs].[Nombre]', N'Name', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260223085825_SeedProductStatus'
)
BEGIN
    EXEC sp_rename N'[mdm].[Catalogs].[IX_Catalogs_Scope_EmpresaId_Nombre]', N'IX_Catalogs_Scope_EmpresaId_Name', N'INDEX';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260223085825_SeedProductStatus'
)
BEGIN
    EXEC sp_rename N'[mdm].[Brands].[Nombre]', N'Name', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260223085825_SeedProductStatus'
)
BEGIN
    EXEC sp_rename N'[mdm].[Brands].[IX_Brands_Nombre]', N'IX_Brands_Name', N'INDEX';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260223085825_SeedProductStatus'
)
BEGIN
    CREATE TABLE [inv].[MovimientosInventario] (
        [Id] int NOT NULL IDENTITY,
        [Number] nvarchar(30) NOT NULL,
        [MovementType] nvarchar(20) NOT NULL,
        [MovementDate] datetime2 NOT NULL,
        [CompanyProductId] bigint NOT NULL,
        [WarehouseId] int NOT NULL,
        [DestinationWarehouseId] int NULL,
        [Quantity] decimal(18,4) NOT NULL,
        [UnitCost] decimal(18,4) NOT NULL,
        [TotalCost] decimal(18,4) NOT NULL,
        [Reference] nvarchar(50) NULL,
        [Notes] nvarchar(500) NULL,
        [FechaCreacion] datetime2 NOT NULL,
        [CreadoPor] nvarchar(100) NULL,
        [FechaModificacion] datetime2 NULL,
        [ModificadoPor] nvarchar(100) NULL,
        [Activo] bit NOT NULL,
        [EmpresaId] int NOT NULL,
        CONSTRAINT [PK_MovimientosInventario] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_MovimientosInventario_Almacenes_DestinationWarehouseId] FOREIGN KEY ([DestinationWarehouseId]) REFERENCES [mdm].[Almacenes] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_MovimientosInventario_Almacenes_WarehouseId] FOREIGN KEY ([WarehouseId]) REFERENCES [mdm].[Almacenes] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_MovimientosInventario_CompanyProducts_CompanyProductId] FOREIGN KEY ([CompanyProductId]) REFERENCES [mdm].[CompanyProducts] ([CompanyProductId]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260223085825_SeedProductStatus'
)
BEGIN
    CREATE TABLE [inv].[StockProductos] (
        [Id] int NOT NULL IDENTITY,
        [CompanyProductId] bigint NOT NULL,
        [AlmacenId] int NOT NULL,
        [CurrentStock] decimal(18,4) NOT NULL,
        [AverageCost] decimal(18,4) NOT NULL,
        [LastUpdated] datetime2 NOT NULL,
        [FechaCreacion] datetime2 NOT NULL,
        [CreadoPor] nvarchar(100) NULL,
        [FechaModificacion] datetime2 NULL,
        [ModificadoPor] nvarchar(100) NULL,
        [Activo] bit NOT NULL,
        [EmpresaId] int NOT NULL,
        CONSTRAINT [PK_StockProductos] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_StockProductos_Almacenes_AlmacenId] FOREIGN KEY ([AlmacenId]) REFERENCES [mdm].[Almacenes] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_StockProductos_CompanyProducts_CompanyProductId] FOREIGN KEY ([CompanyProductId]) REFERENCES [mdm].[CompanyProducts] ([CompanyProductId]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260223085825_SeedProductStatus'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'ProductStatusId', N'Activo', N'Code', N'CreadoPor', N'FechaCreacion', N'FechaModificacion', N'IsDefault', N'ModificadoPor', N'Name') AND [object_id] = OBJECT_ID(N'[mdm].[ProductStatuses]'))
        SET IDENTITY_INSERT [mdm].[ProductStatuses] ON;
    EXEC(N'INSERT INTO [mdm].[ProductStatuses] ([ProductStatusId], [Activo], [Code], [CreadoPor], [FechaCreacion], [FechaModificacion], [IsDefault], [ModificadoPor], [Name])
    VALUES (1, CAST(1 AS bit), N''ACTIVE'', NULL, ''0001-01-01T00:00:00.0000000'', NULL, CAST(1 AS bit), NULL, N''Activo''),
    (2, CAST(1 AS bit), N''INACTIVE'', NULL, ''0001-01-01T00:00:00.0000000'', NULL, CAST(0 AS bit), NULL, N''Inactivo''),
    (3, CAST(1 AS bit), N''DRAFT'', NULL, ''0001-01-01T00:00:00.0000000'', NULL, CAST(0 AS bit), NULL, N''Borrador''),
    (4, CAST(1 AS bit), N''DISCONTINUED'', NULL, ''0001-01-01T00:00:00.0000000'', NULL, CAST(0 AS bit), NULL, N''Descontinuado'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'ProductStatusId', N'Activo', N'Code', N'CreadoPor', N'FechaCreacion', N'FechaModificacion', N'IsDefault', N'ModificadoPor', N'Name') AND [object_id] = OBJECT_ID(N'[mdm].[ProductStatuses]'))
        SET IDENTITY_INSERT [mdm].[ProductStatuses] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260223085825_SeedProductStatus'
)
BEGIN
    CREATE INDEX [IX_MovimientosInventario_CompanyProductId] ON [inv].[MovimientosInventario] ([CompanyProductId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260223085825_SeedProductStatus'
)
BEGIN
    CREATE INDEX [IX_MovimientosInventario_DestinationWarehouseId] ON [inv].[MovimientosInventario] ([DestinationWarehouseId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260223085825_SeedProductStatus'
)
BEGIN
    CREATE INDEX [IX_MovimientosInventario_EmpresaId] ON [inv].[MovimientosInventario] ([EmpresaId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260223085825_SeedProductStatus'
)
BEGIN
    CREATE INDEX [IX_MovimientosInventario_EmpresaId_CompanyProductId_MovementDate] ON [inv].[MovimientosInventario] ([EmpresaId], [CompanyProductId], [MovementDate]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260223085825_SeedProductStatus'
)
BEGIN
    CREATE UNIQUE INDEX [IX_MovimientosInventario_EmpresaId_Number] ON [inv].[MovimientosInventario] ([EmpresaId], [Number]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260223085825_SeedProductStatus'
)
BEGIN
    CREATE INDEX [IX_MovimientosInventario_EmpresaId_WarehouseId] ON [inv].[MovimientosInventario] ([EmpresaId], [WarehouseId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260223085825_SeedProductStatus'
)
BEGIN
    CREATE INDEX [IX_MovimientosInventario_WarehouseId] ON [inv].[MovimientosInventario] ([WarehouseId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260223085825_SeedProductStatus'
)
BEGIN
    CREATE INDEX [IX_StockProductos_AlmacenId] ON [inv].[StockProductos] ([AlmacenId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260223085825_SeedProductStatus'
)
BEGIN
    CREATE INDEX [IX_StockProductos_CompanyProductId] ON [inv].[StockProductos] ([CompanyProductId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260223085825_SeedProductStatus'
)
BEGIN
    CREATE INDEX [IX_StockProductos_EmpresaId] ON [inv].[StockProductos] ([EmpresaId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260223085825_SeedProductStatus'
)
BEGIN
    CREATE UNIQUE INDEX [IX_StockProductos_EmpresaId_CompanyProductId_AlmacenId] ON [inv].[StockProductos] ([EmpresaId], [CompanyProductId], [AlmacenId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260223085825_SeedProductStatus'
)
BEGIN
    INSERT INTO [core].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260223085825_SeedProductStatus', N'8.0.12');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260223091722_SeedDefaultCatalog'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'CatalogId', N'Activo', N'CreadoPor', N'EmpresaId', N'FechaCreacion', N'FechaModificacion', N'IsDefault', N'ModificadoPor', N'Name', N'Scope') AND [object_id] = OBJECT_ID(N'[mdm].[Catalogs]'))
        SET IDENTITY_INSERT [mdm].[Catalogs] ON;
    EXEC(N'INSERT INTO [mdm].[Catalogs] ([CatalogId], [Activo], [CreadoPor], [EmpresaId], [FechaCreacion], [FechaModificacion], [IsDefault], [ModificadoPor], [Name], [Scope])
    VALUES (CAST(1 AS bigint), CAST(1 AS bit), NULL, NULL, ''0001-01-01T00:00:00.0000000'', NULL, CAST(1 AS bit), NULL, N''Catálogo General'', CAST(1 AS tinyint))');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'CatalogId', N'Activo', N'CreadoPor', N'EmpresaId', N'FechaCreacion', N'FechaModificacion', N'IsDefault', N'ModificadoPor', N'Name', N'Scope') AND [object_id] = OBJECT_ID(N'[mdm].[Catalogs]'))
        SET IDENTITY_INSERT [mdm].[Catalogs] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260223091722_SeedDefaultCatalog'
)
BEGIN
    INSERT INTO [core].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260223091722_SeedDefaultCatalog', N'8.0.12');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260226043817_CMP_OrdenesCompra'
)
BEGIN
    IF SCHEMA_ID(N'cmp') IS NULL EXEC(N'CREATE SCHEMA [cmp];');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260226043817_CMP_OrdenesCompra'
)
BEGIN
    CREATE TABLE [cmp].[OrdenesCompra] (
        [OrdenCompraId] bigint NOT NULL IDENTITY,
        [Numero] nvarchar(30) NOT NULL,
        [FechaEmision] datetime2 NOT NULL,
        [FechaEntregaEstimada] datetime2 NULL,
        [ProveedorId] int NOT NULL,
        [AlmacenDestinoId] int NOT NULL,
        [MonedaId] nvarchar(3) NOT NULL,
        [TasaCambio] decimal(18,6) NOT NULL,
        [Estado] tinyint NOT NULL,
        [CondicionPago] nvarchar(100) NULL,
        [Observaciones] nvarchar(1000) NULL,
        [ReferenciaExterna] nvarchar(100) NULL,
        [Subtotal] decimal(18,4) NOT NULL,
        [Descuento] decimal(18,4) NOT NULL,
        [Impuesto] decimal(18,4) NOT NULL,
        [Total] decimal(18,4) NOT NULL,
        [FechaCreacion] datetime2 NOT NULL,
        [CreadoPor] nvarchar(100) NULL,
        [FechaModificacion] datetime2 NULL,
        [ModificadoPor] nvarchar(100) NULL,
        [Activo] bit NOT NULL,
        [EmpresaId] int NOT NULL,
        CONSTRAINT [PK_OrdenesCompra] PRIMARY KEY ([OrdenCompraId]),
        CONSTRAINT [FK_OrdenesCompra_Almacenes_AlmacenDestinoId] FOREIGN KEY ([AlmacenDestinoId]) REFERENCES [mdm].[Almacenes] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_OrdenesCompra_Proveedores_ProveedorId] FOREIGN KEY ([ProveedorId]) REFERENCES [mdm].[Proveedores] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260226043817_CMP_OrdenesCompra'
)
BEGIN
    CREATE TABLE [cmp].[OrdenCompraLineas] (
        [OrdenCompraLineaId] bigint NOT NULL IDENTITY,
        [OrdenCompraId] bigint NOT NULL,
        [NumeroLinea] int NOT NULL,
        [CompanyProductId] bigint NOT NULL,
        [Descripcion] nvarchar(300) NOT NULL,
        [UnidadMedida] nvarchar(20) NOT NULL,
        [Cantidad] decimal(18,4) NOT NULL,
        [PrecioUnitario] decimal(18,4) NOT NULL,
        [PorcentajeDescuento] decimal(5,2) NOT NULL,
        [MontoDescuento] decimal(18,4) NOT NULL,
        [Subtotal] decimal(18,4) NOT NULL,
        [PorcentajeImpuesto] decimal(5,2) NOT NULL,
        [MontoImpuesto] decimal(18,4) NOT NULL,
        [TotalLinea] decimal(18,4) NOT NULL,
        [CantidadRecepcionada] decimal(18,4) NOT NULL,
        [FechaCreacion] datetime2 NOT NULL,
        [CreadoPor] nvarchar(100) NULL,
        [FechaModificacion] datetime2 NULL,
        [ModificadoPor] nvarchar(100) NULL,
        [Activo] bit NOT NULL,
        CONSTRAINT [PK_OrdenCompraLineas] PRIMARY KEY ([OrdenCompraLineaId]),
        CONSTRAINT [FK_OrdenCompraLineas_CompanyProducts_CompanyProductId] FOREIGN KEY ([CompanyProductId]) REFERENCES [mdm].[CompanyProducts] ([CompanyProductId]) ON DELETE NO ACTION,
        CONSTRAINT [FK_OrdenCompraLineas_OrdenesCompra_OrdenCompraId] FOREIGN KEY ([OrdenCompraId]) REFERENCES [cmp].[OrdenesCompra] ([OrdenCompraId]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260226043817_CMP_OrdenesCompra'
)
BEGIN
    CREATE INDEX [IX_OrdenCompraLineas_CompanyProductId] ON [cmp].[OrdenCompraLineas] ([CompanyProductId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260226043817_CMP_OrdenesCompra'
)
BEGIN
    CREATE UNIQUE INDEX [IX_OrdenCompraLineas_OrdenCompraId_NumeroLinea] ON [cmp].[OrdenCompraLineas] ([OrdenCompraId], [NumeroLinea]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260226043817_CMP_OrdenesCompra'
)
BEGIN
    CREATE INDEX [IX_OrdenesCompra_AlmacenDestinoId] ON [cmp].[OrdenesCompra] ([AlmacenDestinoId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260226043817_CMP_OrdenesCompra'
)
BEGIN
    CREATE INDEX [IX_OrdenesCompra_EmpresaId] ON [cmp].[OrdenesCompra] ([EmpresaId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260226043817_CMP_OrdenesCompra'
)
BEGIN
    CREATE INDEX [IX_OrdenesCompra_EmpresaId_Estado] ON [cmp].[OrdenesCompra] ([EmpresaId], [Estado]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260226043817_CMP_OrdenesCompra'
)
BEGIN
    CREATE INDEX [IX_OrdenesCompra_EmpresaId_FechaEmision] ON [cmp].[OrdenesCompra] ([EmpresaId], [FechaEmision]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260226043817_CMP_OrdenesCompra'
)
BEGIN
    CREATE UNIQUE INDEX [IX_OrdenesCompra_EmpresaId_Numero] ON [cmp].[OrdenesCompra] ([EmpresaId], [Numero]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260226043817_CMP_OrdenesCompra'
)
BEGIN
    CREATE INDEX [IX_OrdenesCompra_EmpresaId_ProveedorId] ON [cmp].[OrdenesCompra] ([EmpresaId], [ProveedorId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260226043817_CMP_OrdenesCompra'
)
BEGIN
    CREATE INDEX [IX_OrdenesCompra_ProveedorId] ON [cmp].[OrdenesCompra] ([ProveedorId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260226043817_CMP_OrdenesCompra'
)
BEGIN
    INSERT INTO [core].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260226043817_CMP_OrdenesCompra', N'8.0.12');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260226050951_CMP_RecepcionesCompra'
)
BEGIN
    CREATE TABLE [cmp].[RecepcionesCompra] (
        [RecepcionCompraId] bigint NOT NULL IDENTITY,
        [Numero] nvarchar(30) NOT NULL,
        [OrdenCompraId] bigint NOT NULL,
        [FechaRecepcion] datetime2 NOT NULL,
        [AlmacenId] int NOT NULL,
        [DocumentoProveedor] nvarchar(100) NULL,
        [Observaciones] nvarchar(1000) NULL,
        [Confirmada] bit NOT NULL,
        [FechaCreacion] datetime2 NOT NULL,
        [CreadoPor] nvarchar(100) NULL,
        [FechaModificacion] datetime2 NULL,
        [ModificadoPor] nvarchar(100) NULL,
        [Activo] bit NOT NULL,
        [EmpresaId] int NOT NULL,
        CONSTRAINT [PK_RecepcionesCompra] PRIMARY KEY ([RecepcionCompraId]),
        CONSTRAINT [FK_RecepcionesCompra_Almacenes_AlmacenId] FOREIGN KEY ([AlmacenId]) REFERENCES [mdm].[Almacenes] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_RecepcionesCompra_OrdenesCompra_OrdenCompraId] FOREIGN KEY ([OrdenCompraId]) REFERENCES [cmp].[OrdenesCompra] ([OrdenCompraId]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260226050951_CMP_RecepcionesCompra'
)
BEGIN
    CREATE TABLE [cmp].[RecepcionCompraLineas] (
        [RecepcionCompraLineaId] bigint NOT NULL IDENTITY,
        [RecepcionCompraId] bigint NOT NULL,
        [OrdenCompraLineaId] bigint NOT NULL,
        [CantidadRecibida] decimal(18,4) NOT NULL,
        [CostoUnitario] decimal(18,4) NOT NULL,
        [Notas] nvarchar(500) NULL,
        [FechaCreacion] datetime2 NOT NULL,
        [CreadoPor] nvarchar(100) NULL,
        [FechaModificacion] datetime2 NULL,
        [ModificadoPor] nvarchar(100) NULL,
        [Activo] bit NOT NULL,
        CONSTRAINT [PK_RecepcionCompraLineas] PRIMARY KEY ([RecepcionCompraLineaId]),
        CONSTRAINT [FK_RecepcionCompraLineas_OrdenCompraLineas_OrdenCompraLineaId] FOREIGN KEY ([OrdenCompraLineaId]) REFERENCES [cmp].[OrdenCompraLineas] ([OrdenCompraLineaId]) ON DELETE NO ACTION,
        CONSTRAINT [FK_RecepcionCompraLineas_RecepcionesCompra_RecepcionCompraId] FOREIGN KEY ([RecepcionCompraId]) REFERENCES [cmp].[RecepcionesCompra] ([RecepcionCompraId]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260226050951_CMP_RecepcionesCompra'
)
BEGIN
    CREATE INDEX [IX_RecepcionCompraLineas_OrdenCompraLineaId] ON [cmp].[RecepcionCompraLineas] ([OrdenCompraLineaId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260226050951_CMP_RecepcionesCompra'
)
BEGIN
    CREATE INDEX [IX_RecepcionCompraLineas_RecepcionCompraId] ON [cmp].[RecepcionCompraLineas] ([RecepcionCompraId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260226050951_CMP_RecepcionesCompra'
)
BEGIN
    CREATE INDEX [IX_RecepcionesCompra_AlmacenId] ON [cmp].[RecepcionesCompra] ([AlmacenId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260226050951_CMP_RecepcionesCompra'
)
BEGIN
    CREATE INDEX [IX_RecepcionesCompra_EmpresaId] ON [cmp].[RecepcionesCompra] ([EmpresaId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260226050951_CMP_RecepcionesCompra'
)
BEGIN
    CREATE UNIQUE INDEX [IX_RecepcionesCompra_EmpresaId_Numero] ON [cmp].[RecepcionesCompra] ([EmpresaId], [Numero]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260226050951_CMP_RecepcionesCompra'
)
BEGIN
    CREATE INDEX [IX_RecepcionesCompra_EmpresaId_OrdenCompraId] ON [cmp].[RecepcionesCompra] ([EmpresaId], [OrdenCompraId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260226050951_CMP_RecepcionesCompra'
)
BEGIN
    CREATE INDEX [IX_RecepcionesCompra_OrdenCompraId] ON [cmp].[RecepcionesCompra] ([OrdenCompraId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260226050951_CMP_RecepcionesCompra'
)
BEGIN
    INSERT INTO [core].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260226050951_CMP_RecepcionesCompra', N'8.0.12');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260226053846_CMP_Importaciones'
)
BEGIN
    CREATE TABLE [cmp].[HojasImportacion] (
        [HojaImportacionId] bigint NOT NULL IDENTITY,
        [Numero] nvarchar(30) NOT NULL,
        [OrdenCompraId] bigint NOT NULL,
        [Fecha] datetime2 NOT NULL,
        [ReferenciaAduanera] nvarchar(100) NULL,
        [Observaciones] nvarchar(1000) NULL,
        [MetodoDistribucion] tinyint NOT NULL,
        [TotalGastos] decimal(18,4) NOT NULL,
        [Liquidada] bit NOT NULL,
        [FechaCreacion] datetime2 NOT NULL,
        [CreadoPor] nvarchar(100) NULL,
        [FechaModificacion] datetime2 NULL,
        [ModificadoPor] nvarchar(100) NULL,
        [Activo] bit NOT NULL,
        [EmpresaId] int NOT NULL,
        CONSTRAINT [PK_HojasImportacion] PRIMARY KEY ([HojaImportacionId]),
        CONSTRAINT [FK_HojasImportacion_OrdenesCompra_OrdenCompraId] FOREIGN KEY ([OrdenCompraId]) REFERENCES [cmp].[OrdenesCompra] ([OrdenCompraId]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260226053846_CMP_Importaciones'
)
BEGIN
    CREATE TABLE [cmp].[GastosImportacion] (
        [GastoImportacionId] bigint NOT NULL IDENTITY,
        [HojaImportacionId] bigint NOT NULL,
        [TipoGasto] nvarchar(50) NOT NULL,
        [Descripcion] nvarchar(300) NULL,
        [Monto] decimal(18,4) NOT NULL,
        [MonedaId] nvarchar(3) NOT NULL,
        [TasaCambio] decimal(18,6) NOT NULL,
        [MontoBase] decimal(18,4) NOT NULL,
        [Referencia] nvarchar(100) NULL,
        [FechaCreacion] datetime2 NOT NULL,
        [CreadoPor] nvarchar(100) NULL,
        [FechaModificacion] datetime2 NULL,
        [ModificadoPor] nvarchar(100) NULL,
        [Activo] bit NOT NULL,
        CONSTRAINT [PK_GastosImportacion] PRIMARY KEY ([GastoImportacionId]),
        CONSTRAINT [FK_GastosImportacion_HojasImportacion_HojaImportacionId] FOREIGN KEY ([HojaImportacionId]) REFERENCES [cmp].[HojasImportacion] ([HojaImportacionId]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260226053846_CMP_Importaciones'
)
BEGIN
    CREATE TABLE [cmp].[ImportacionLineas] (
        [ImportacionLineaId] bigint NOT NULL IDENTITY,
        [HojaImportacionId] bigint NOT NULL,
        [OrdenCompraLineaId] bigint NOT NULL,
        [CostoFobUnitario] decimal(18,4) NOT NULL,
        [CostoFobTotal] decimal(18,4) NOT NULL,
        [FactorDistribucion] decimal(18,8) NOT NULL,
        [GastoAsignado] decimal(18,4) NOT NULL,
        [CostoLandedUnitario] decimal(18,4) NOT NULL,
        [CostoLandedTotal] decimal(18,4) NOT NULL,
        [FechaCreacion] datetime2 NOT NULL,
        [CreadoPor] nvarchar(100) NULL,
        [FechaModificacion] datetime2 NULL,
        [ModificadoPor] nvarchar(100) NULL,
        [Activo] bit NOT NULL,
        CONSTRAINT [PK_ImportacionLineas] PRIMARY KEY ([ImportacionLineaId]),
        CONSTRAINT [FK_ImportacionLineas_HojasImportacion_HojaImportacionId] FOREIGN KEY ([HojaImportacionId]) REFERENCES [cmp].[HojasImportacion] ([HojaImportacionId]) ON DELETE CASCADE,
        CONSTRAINT [FK_ImportacionLineas_OrdenCompraLineas_OrdenCompraLineaId] FOREIGN KEY ([OrdenCompraLineaId]) REFERENCES [cmp].[OrdenCompraLineas] ([OrdenCompraLineaId]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260226053846_CMP_Importaciones'
)
BEGIN
    CREATE INDEX [IX_GastosImportacion_HojaImportacionId] ON [cmp].[GastosImportacion] ([HojaImportacionId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260226053846_CMP_Importaciones'
)
BEGIN
    CREATE INDEX [IX_HojasImportacion_EmpresaId] ON [cmp].[HojasImportacion] ([EmpresaId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260226053846_CMP_Importaciones'
)
BEGIN
    CREATE UNIQUE INDEX [IX_HojasImportacion_EmpresaId_Numero] ON [cmp].[HojasImportacion] ([EmpresaId], [Numero]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260226053846_CMP_Importaciones'
)
BEGIN
    CREATE INDEX [IX_HojasImportacion_EmpresaId_OrdenCompraId] ON [cmp].[HojasImportacion] ([EmpresaId], [OrdenCompraId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260226053846_CMP_Importaciones'
)
BEGIN
    CREATE INDEX [IX_HojasImportacion_OrdenCompraId] ON [cmp].[HojasImportacion] ([OrdenCompraId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260226053846_CMP_Importaciones'
)
BEGIN
    CREATE INDEX [IX_ImportacionLineas_HojaImportacionId] ON [cmp].[ImportacionLineas] ([HojaImportacionId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260226053846_CMP_Importaciones'
)
BEGIN
    CREATE INDEX [IX_ImportacionLineas_OrdenCompraLineaId] ON [cmp].[ImportacionLineas] ([OrdenCompraLineaId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260226053846_CMP_Importaciones'
)
BEGIN
    INSERT INTO [core].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260226053846_CMP_Importaciones', N'8.0.12');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260226064451_ACC_CuentasContables'
)
BEGIN
    IF SCHEMA_ID(N'acc') IS NULL EXEC(N'CREATE SCHEMA [acc];');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260226064451_ACC_CuentasContables'
)
BEGIN
    CREATE TABLE [acc].[CuentasContables] (
        [CuentaContableId] int NOT NULL IDENTITY,
        [Codigo] nvarchar(30) NOT NULL,
        [Nombre] nvarchar(200) NOT NULL,
        [Tipo] tinyint NOT NULL,
        [Naturaleza] tinyint NOT NULL,
        [Nivel] int NOT NULL,
        [CuentaPadreId] int NULL,
        [PermiteMovimientos] bit NOT NULL,
        [Descripcion] nvarchar(500) NULL,
        [SaldoActual] decimal(18,4) NOT NULL,
        [FechaCreacion] datetime2 NOT NULL,
        [CreadoPor] nvarchar(100) NULL,
        [FechaModificacion] datetime2 NULL,
        [ModificadoPor] nvarchar(100) NULL,
        [Activo] bit NOT NULL,
        [EmpresaId] int NOT NULL,
        CONSTRAINT [PK_CuentasContables] PRIMARY KEY ([CuentaContableId]),
        CONSTRAINT [FK_CuentasContables_CuentasContables_CuentaPadreId] FOREIGN KEY ([CuentaPadreId]) REFERENCES [acc].[CuentasContables] ([CuentaContableId]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260226064451_ACC_CuentasContables'
)
BEGIN
    CREATE INDEX [IX_CuentasContables_CuentaPadreId] ON [acc].[CuentasContables] ([CuentaPadreId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260226064451_ACC_CuentasContables'
)
BEGIN
    CREATE INDEX [IX_CuentasContables_EmpresaId] ON [acc].[CuentasContables] ([EmpresaId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260226064451_ACC_CuentasContables'
)
BEGIN
    CREATE UNIQUE INDEX [IX_CuentasContables_EmpresaId_Codigo] ON [acc].[CuentasContables] ([EmpresaId], [Codigo]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260226064451_ACC_CuentasContables'
)
BEGIN
    CREATE INDEX [IX_CuentasContables_EmpresaId_CuentaPadreId] ON [acc].[CuentasContables] ([EmpresaId], [CuentaPadreId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260226064451_ACC_CuentasContables'
)
BEGIN
    CREATE INDEX [IX_CuentasContables_EmpresaId_Tipo] ON [acc].[CuentasContables] ([EmpresaId], [Tipo]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260226064451_ACC_CuentasContables'
)
BEGIN
    INSERT INTO [core].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260226064451_ACC_CuentasContables', N'8.0.12');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260226065319_ACC_AsientosContables'
)
BEGIN
    CREATE TABLE [acc].[AsientosContables] (
        [AsientoContableId] bigint NOT NULL IDENTITY,
        [Numero] nvarchar(30) NOT NULL,
        [Fecha] datetime2 NOT NULL,
        [Tipo] nvarchar(20) NOT NULL,
        [Glosa] nvarchar(500) NOT NULL,
        [Estado] nvarchar(20) NOT NULL,
        [OrigenTipo] nvarchar(50) NULL,
        [OrigenId] bigint NULL,
        [OrigenReferencia] nvarchar(100) NULL,
        [TotalDebe] decimal(18,4) NOT NULL,
        [TotalHaber] decimal(18,4) NOT NULL,
        [FechaCreacion] datetime2 NOT NULL,
        [CreadoPor] nvarchar(100) NULL,
        [FechaModificacion] datetime2 NULL,
        [ModificadoPor] nvarchar(100) NULL,
        [Activo] bit NOT NULL,
        [EmpresaId] int NOT NULL,
        CONSTRAINT [PK_AsientosContables] PRIMARY KEY ([AsientoContableId])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260226065319_ACC_AsientosContables'
)
BEGIN
    CREATE TABLE [acc].[AsientoContableLineas] (
        [AsientoContableLineaId] bigint NOT NULL IDENTITY,
        [AsientoContableId] bigint NOT NULL,
        [NumeroLinea] int NOT NULL,
        [CuentaContableId] int NOT NULL,
        [Debe] decimal(18,4) NOT NULL,
        [Haber] decimal(18,4) NOT NULL,
        [Glosa] nvarchar(300) NULL,
        [Referencia] nvarchar(100) NULL,
        [FechaCreacion] datetime2 NOT NULL,
        [CreadoPor] nvarchar(100) NULL,
        [FechaModificacion] datetime2 NULL,
        [ModificadoPor] nvarchar(100) NULL,
        [Activo] bit NOT NULL,
        CONSTRAINT [PK_AsientoContableLineas] PRIMARY KEY ([AsientoContableLineaId]),
        CONSTRAINT [FK_AsientoContableLineas_AsientosContables_AsientoContableId] FOREIGN KEY ([AsientoContableId]) REFERENCES [acc].[AsientosContables] ([AsientoContableId]) ON DELETE CASCADE,
        CONSTRAINT [FK_AsientoContableLineas_CuentasContables_CuentaContableId] FOREIGN KEY ([CuentaContableId]) REFERENCES [acc].[CuentasContables] ([CuentaContableId]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260226065319_ACC_AsientosContables'
)
BEGIN
    CREATE INDEX [IX_AsientoContableLineas_AsientoContableId] ON [acc].[AsientoContableLineas] ([AsientoContableId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260226065319_ACC_AsientosContables'
)
BEGIN
    CREATE INDEX [IX_AsientoContableLineas_CuentaContableId] ON [acc].[AsientoContableLineas] ([CuentaContableId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260226065319_ACC_AsientosContables'
)
BEGIN
    CREATE INDEX [IX_AsientosContables_EmpresaId] ON [acc].[AsientosContables] ([EmpresaId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260226065319_ACC_AsientosContables'
)
BEGIN
    CREATE INDEX [IX_AsientosContables_EmpresaId_Estado] ON [acc].[AsientosContables] ([EmpresaId], [Estado]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260226065319_ACC_AsientosContables'
)
BEGIN
    CREATE INDEX [IX_AsientosContables_EmpresaId_Fecha] ON [acc].[AsientosContables] ([EmpresaId], [Fecha]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260226065319_ACC_AsientosContables'
)
BEGIN
    CREATE UNIQUE INDEX [IX_AsientosContables_EmpresaId_Numero] ON [acc].[AsientosContables] ([EmpresaId], [Numero]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260226065319_ACC_AsientosContables'
)
BEGIN
    CREATE INDEX [IX_AsientosContables_OrigenTipo_OrigenId] ON [acc].[AsientosContables] ([OrigenTipo], [OrigenId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260226065319_ACC_AsientosContables'
)
BEGIN
    INSERT INTO [core].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260226065319_ACC_AsientosContables', N'8.0.12');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260226070740_ACC_PeriodosContables'
)
BEGIN
    CREATE TABLE [acc].[PeriodosContables] (
        [PeriodoContableId] int NOT NULL IDENTITY,
        [Anio] int NOT NULL,
        [Mes] int NOT NULL,
        [Nombre] nvarchar(50) NOT NULL,
        [Estado] nvarchar(20) NOT NULL,
        [FechaCierre] datetime2 NULL,
        [CerradoPor] nvarchar(100) NULL,
        [FechaCreacion] datetime2 NOT NULL,
        [CreadoPor] nvarchar(100) NULL,
        [FechaModificacion] datetime2 NULL,
        [ModificadoPor] nvarchar(100) NULL,
        [Activo] bit NOT NULL,
        [EmpresaId] int NOT NULL,
        CONSTRAINT [PK_PeriodosContables] PRIMARY KEY ([PeriodoContableId])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260226070740_ACC_PeriodosContables'
)
BEGIN
    CREATE INDEX [IX_PeriodosContables_EmpresaId] ON [acc].[PeriodosContables] ([EmpresaId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260226070740_ACC_PeriodosContables'
)
BEGIN
    CREATE UNIQUE INDEX [IX_PeriodosContables_EmpresaId_Anio_Mes] ON [acc].[PeriodosContables] ([EmpresaId], [Anio], [Mes]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260226070740_ACC_PeriodosContables'
)
BEGIN
    CREATE INDEX [IX_PeriodosContables_EmpresaId_Estado] ON [acc].[PeriodosContables] ([EmpresaId], [Estado]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260226070740_ACC_PeriodosContables'
)
BEGIN
    INSERT INTO [core].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260226070740_ACC_PeriodosContables', N'8.0.12');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260226072414_ACC_PlantillasContables'
)
BEGIN
    CREATE TABLE [acc].[PlantillasContables] (
        [PlantillaContableId] int NOT NULL IDENTITY,
        [Codigo] nvarchar(20) NOT NULL,
        [Nombre] nvarchar(200) NOT NULL,
        [TipoDocumento] nvarchar(50) NOT NULL,
        [Descripcion] nvarchar(500) NULL,
        [GlosaPlantilla] nvarchar(500) NOT NULL,
        [FechaCreacion] datetime2 NOT NULL,
        [CreadoPor] nvarchar(100) NULL,
        [FechaModificacion] datetime2 NULL,
        [ModificadoPor] nvarchar(100) NULL,
        [Activo] bit NOT NULL,
        [EmpresaId] int NOT NULL,
        CONSTRAINT [PK_PlantillasContables] PRIMARY KEY ([PlantillaContableId])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260226072414_ACC_PlantillasContables'
)
BEGIN
    CREATE TABLE [acc].[PlantillaContableLineas] (
        [PlantillaContableLineaId] int NOT NULL IDENTITY,
        [PlantillaContableId] int NOT NULL,
        [NumeroLinea] int NOT NULL,
        [CuentaContableId] int NOT NULL,
        [TipoMovimiento] nvarchar(10) NOT NULL,
        [CampoMonto] nvarchar(50) NOT NULL,
        [Factor] decimal(10,4) NOT NULL,
        [Glosa] nvarchar(300) NULL,
        [FechaCreacion] datetime2 NOT NULL,
        [CreadoPor] nvarchar(100) NULL,
        [FechaModificacion] datetime2 NULL,
        [ModificadoPor] nvarchar(100) NULL,
        [Activo] bit NOT NULL,
        CONSTRAINT [PK_PlantillaContableLineas] PRIMARY KEY ([PlantillaContableLineaId]),
        CONSTRAINT [FK_PlantillaContableLineas_CuentasContables_CuentaContableId] FOREIGN KEY ([CuentaContableId]) REFERENCES [acc].[CuentasContables] ([CuentaContableId]) ON DELETE NO ACTION,
        CONSTRAINT [FK_PlantillaContableLineas_PlantillasContables_PlantillaContableId] FOREIGN KEY ([PlantillaContableId]) REFERENCES [acc].[PlantillasContables] ([PlantillaContableId]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260226072414_ACC_PlantillasContables'
)
BEGIN
    CREATE INDEX [IX_PlantillaContableLineas_CuentaContableId] ON [acc].[PlantillaContableLineas] ([CuentaContableId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260226072414_ACC_PlantillasContables'
)
BEGIN
    CREATE INDEX [IX_PlantillaContableLineas_PlantillaContableId] ON [acc].[PlantillaContableLineas] ([PlantillaContableId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260226072414_ACC_PlantillasContables'
)
BEGIN
    CREATE INDEX [IX_PlantillasContables_EmpresaId] ON [acc].[PlantillasContables] ([EmpresaId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260226072414_ACC_PlantillasContables'
)
BEGIN
    CREATE UNIQUE INDEX [IX_PlantillasContables_EmpresaId_Codigo] ON [acc].[PlantillasContables] ([EmpresaId], [Codigo]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260226072414_ACC_PlantillasContables'
)
BEGIN
    CREATE INDEX [IX_PlantillasContables_EmpresaId_TipoDocumento] ON [acc].[PlantillasContables] ([EmpresaId], [TipoDocumento]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260226072414_ACC_PlantillasContables'
)
BEGIN
    INSERT INTO [core].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260226072414_ACC_PlantillasContables', N'8.0.12');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260226081631_ACC_ComprobantesContables'
)
BEGIN
    EXEC sp_rename N'[acc].[AsientosContables].[Tipo]', N'TipoRegistro', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260226081631_ACC_ComprobantesContables'
)
BEGIN
    ALTER TABLE [acc].[AsientosContables] ADD [Concepto] nvarchar(300) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260226081631_ACC_ComprobantesContables'
)
BEGIN
    ALTER TABLE [acc].[AsientosContables] ADD [Gestion] int NOT NULL DEFAULT 0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260226081631_ACC_ComprobantesContables'
)
BEGIN
    ALTER TABLE [acc].[AsientosContables] ADD [NumeroDocumentoPago] nvarchar(100) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260226081631_ACC_ComprobantesContables'
)
BEGIN
    ALTER TABLE [acc].[AsientosContables] ADD [RegistradoPor] nvarchar(100) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260226081631_ACC_ComprobantesContables'
)
BEGIN
    ALTER TABLE [acc].[AsientosContables] ADD [TipoCambioId] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260226081631_ACC_ComprobantesContables'
)
BEGIN
    ALTER TABLE [acc].[AsientosContables] ADD [TipoComprobanteId] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260226081631_ACC_ComprobantesContables'
)
BEGIN
    ALTER TABLE [acc].[AsientosContables] ADD [TipoPagoId] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260226081631_ACC_ComprobantesContables'
)
BEGIN
    ALTER TABLE [acc].[AsientosContables] ADD [ValorTipoCambio] decimal(18,6) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260226081631_ACC_ComprobantesContables'
)
BEGIN
    CREATE TABLE [acc].[TiposCambio] (
        [TipoCambioId] int NOT NULL IDENTITY,
        [Moneda] nvarchar(10) NOT NULL,
        [Nombre] nvarchar(100) NOT NULL,
        [Simbolo] nvarchar(10) NOT NULL,
        [TasaCompra] decimal(18,6) NOT NULL,
        [TasaVenta] decimal(18,6) NOT NULL,
        [FechaVigencia] datetime2 NOT NULL,
        [FechaCreacion] datetime2 NOT NULL,
        [CreadoPor] nvarchar(100) NULL,
        [FechaModificacion] datetime2 NULL,
        [ModificadoPor] nvarchar(100) NULL,
        [Activo] bit NOT NULL,
        [EmpresaId] int NOT NULL,
        CONSTRAINT [PK_TiposCambio] PRIMARY KEY ([TipoCambioId])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260226081631_ACC_ComprobantesContables'
)
BEGIN
    CREATE TABLE [acc].[TiposComprobante] (
        [TipoComprobanteId] int NOT NULL IDENTITY,
        [Codigo] nvarchar(10) NOT NULL,
        [Nombre] nvarchar(100) NOT NULL,
        [Prefijo] nvarchar(10) NOT NULL,
        [Orden] int NOT NULL,
        [FechaCreacion] datetime2 NOT NULL,
        [CreadoPor] nvarchar(100) NULL,
        [FechaModificacion] datetime2 NULL,
        [ModificadoPor] nvarchar(100) NULL,
        [Activo] bit NOT NULL,
        [EmpresaId] int NOT NULL,
        CONSTRAINT [PK_TiposComprobante] PRIMARY KEY ([TipoComprobanteId])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260226081631_ACC_ComprobantesContables'
)
BEGIN
    CREATE TABLE [acc].[TiposPago] (
        [TipoPagoId] int NOT NULL IDENTITY,
        [Codigo] nvarchar(10) NOT NULL,
        [Nombre] nvarchar(100) NOT NULL,
        [RequiereReferencia] bit NOT NULL,
        [Orden] int NOT NULL,
        [FechaCreacion] datetime2 NOT NULL,
        [CreadoPor] nvarchar(100) NULL,
        [FechaModificacion] datetime2 NULL,
        [ModificadoPor] nvarchar(100) NULL,
        [Activo] bit NOT NULL,
        [EmpresaId] int NOT NULL,
        CONSTRAINT [PK_TiposPago] PRIMARY KEY ([TipoPagoId])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260226081631_ACC_ComprobantesContables'
)
BEGIN
    CREATE INDEX [IX_AsientosContables_EmpresaId_Gestion] ON [acc].[AsientosContables] ([EmpresaId], [Gestion]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260226081631_ACC_ComprobantesContables'
)
BEGIN
    CREATE INDEX [IX_AsientosContables_TipoCambioId] ON [acc].[AsientosContables] ([TipoCambioId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260226081631_ACC_ComprobantesContables'
)
BEGIN
    CREATE INDEX [IX_AsientosContables_TipoComprobanteId] ON [acc].[AsientosContables] ([TipoComprobanteId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260226081631_ACC_ComprobantesContables'
)
BEGIN
    CREATE INDEX [IX_AsientosContables_TipoPagoId] ON [acc].[AsientosContables] ([TipoPagoId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260226081631_ACC_ComprobantesContables'
)
BEGIN
    CREATE INDEX [IX_TiposCambio_EmpresaId] ON [acc].[TiposCambio] ([EmpresaId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260226081631_ACC_ComprobantesContables'
)
BEGIN
    CREATE INDEX [IX_TiposCambio_EmpresaId_Moneda_FechaVigencia] ON [acc].[TiposCambio] ([EmpresaId], [Moneda], [FechaVigencia]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260226081631_ACC_ComprobantesContables'
)
BEGIN
    CREATE INDEX [IX_TiposComprobante_EmpresaId] ON [acc].[TiposComprobante] ([EmpresaId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260226081631_ACC_ComprobantesContables'
)
BEGIN
    CREATE UNIQUE INDEX [IX_TiposComprobante_EmpresaId_Codigo] ON [acc].[TiposComprobante] ([EmpresaId], [Codigo]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260226081631_ACC_ComprobantesContables'
)
BEGIN
    CREATE INDEX [IX_TiposPago_EmpresaId] ON [acc].[TiposPago] ([EmpresaId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260226081631_ACC_ComprobantesContables'
)
BEGIN
    CREATE UNIQUE INDEX [IX_TiposPago_EmpresaId_Codigo] ON [acc].[TiposPago] ([EmpresaId], [Codigo]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260226081631_ACC_ComprobantesContables'
)
BEGIN
    ALTER TABLE [acc].[AsientosContables] ADD CONSTRAINT [FK_AsientosContables_TiposCambio_TipoCambioId] FOREIGN KEY ([TipoCambioId]) REFERENCES [acc].[TiposCambio] ([TipoCambioId]) ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260226081631_ACC_ComprobantesContables'
)
BEGIN
    ALTER TABLE [acc].[AsientosContables] ADD CONSTRAINT [FK_AsientosContables_TiposComprobante_TipoComprobanteId] FOREIGN KEY ([TipoComprobanteId]) REFERENCES [acc].[TiposComprobante] ([TipoComprobanteId]) ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260226081631_ACC_ComprobantesContables'
)
BEGIN
    ALTER TABLE [acc].[AsientosContables] ADD CONSTRAINT [FK_AsientosContables_TiposPago_TipoPagoId] FOREIGN KEY ([TipoPagoId]) REFERENCES [acc].[TiposPago] ([TipoPagoId]) ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260226081631_ACC_ComprobantesContables'
)
BEGIN
    INSERT INTO [core].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260226081631_ACC_ComprobantesContables', N'8.0.12');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260305065107_CMP_FlujoImportacionCompleto'
)
BEGIN
    ALTER TABLE [cmp].[RecepcionesCompra] ADD [ActaDiferencias] nvarchar(2000) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260305065107_CMP_FlujoImportacionCompleto'
)
BEGIN
    ALTER TABLE [cmp].[RecepcionesCompra] ADD [EnCuarentena] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260305065107_CMP_FlujoImportacionCompleto'
)
BEGIN
    ALTER TABLE [cmp].[RecepcionesCompra] ADD [NumeroReclamo] nvarchar(100) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260305065107_CMP_FlujoImportacionCompleto'
)
BEGIN
    ALTER TABLE [cmp].[RecepcionesCompra] ADD [ResultadoControlCalidad] nvarchar(50) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260305065107_CMP_FlujoImportacionCompleto'
)
BEGIN
    ALTER TABLE [cmp].[RecepcionesCompra] ADD [TieneDiferencias] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260305065107_CMP_FlujoImportacionCompleto'
)
BEGIN
    ALTER TABLE [cmp].[RecepcionesCompra] ADD [TipoDiferencia] nvarchar(50) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260305065107_CMP_FlujoImportacionCompleto'
)
BEGIN
    ALTER TABLE [cmp].[RecepcionesCompra] ADD [UbicacionCuarentena] nvarchar(200) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260305065107_CMP_FlujoImportacionCompleto'
)
BEGIN
    ALTER TABLE [cmp].[RecepcionCompraLineas] ADD [CantidadDañada] decimal(18,4) NOT NULL DEFAULT 0.0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260305065107_CMP_FlujoImportacionCompleto'
)
BEGIN
    ALTER TABLE [cmp].[RecepcionCompraLineas] ADD [CantidadFaltante] decimal(18,4) NOT NULL DEFAULT 0.0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260305065107_CMP_FlujoImportacionCompleto'
)
BEGIN
    ALTER TABLE [cmp].[RecepcionCompraLineas] ADD [CantidadSobrante] decimal(18,4) NOT NULL DEFAULT 0.0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260305065107_CMP_FlujoImportacionCompleto'
)
BEGIN
    ALTER TABLE [cmp].[OrdenesCompra] ADD [ExpedienteImportacionId] bigint NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260305065107_CMP_FlujoImportacionCompleto'
)
BEGIN
    ALTER TABLE [cmp].[OrdenesCompra] ADD [MotivoRechazo] nvarchar(500) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260305065107_CMP_FlujoImportacionCompleto'
)
BEGIN
    ALTER TABLE [cmp].[OrdenesCompra] ADD [OrdenPedidoId] bigint NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260305065107_CMP_FlujoImportacionCompleto'
)
BEGIN
    DECLARE @var0 sysname;
    SELECT @var0 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[cmp].[HojasImportacion]') AND [c].[name] = N'OrdenCompraId');
    IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [cmp].[HojasImportacion] DROP CONSTRAINT [' + @var0 + '];');
    ALTER TABLE [cmp].[HojasImportacion] ALTER COLUMN [OrdenCompraId] bigint NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260305065107_CMP_FlujoImportacionCompleto'
)
BEGIN
    ALTER TABLE [cmp].[HojasImportacion] ADD [ExpedienteImportacionId] bigint NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260305065107_CMP_FlujoImportacionCompleto'
)
BEGIN
    CREATE TABLE [cmp].[ConfirmacionesProveedor] (
        [ConfirmacionProveedorId] bigint NOT NULL IDENTITY,
        [OrdenCompraId] bigint NOT NULL,
        [NumeroProforma] nvarchar(100) NULL,
        [FechaConfirmacion] datetime2 NOT NULL,
        [CondicionesOK] bit NOT NULL,
        [ObservacionesProveedor] nvarchar(2000) NULL,
        [FechaEntregaComprometida] datetime2 NULL,
        [Estado] tinyint NOT NULL,
        [Observaciones] nvarchar(1000) NULL,
        [IteracionNegociacion] int NOT NULL,
        [FechaCreacion] datetime2 NOT NULL,
        [CreadoPor] nvarchar(100) NULL,
        [FechaModificacion] datetime2 NULL,
        [ModificadoPor] nvarchar(100) NULL,
        [Activo] bit NOT NULL,
        [EmpresaId] int NOT NULL,
        CONSTRAINT [PK_ConfirmacionesProveedor] PRIMARY KEY ([ConfirmacionProveedorId]),
        CONSTRAINT [FK_ConfirmacionesProveedor_OrdenesCompra_OrdenCompraId] FOREIGN KEY ([OrdenCompraId]) REFERENCES [cmp].[OrdenesCompra] ([OrdenCompraId]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260305065107_CMP_FlujoImportacionCompleto'
)
BEGIN
    CREATE TABLE [cmp].[ExpedientesImportacion] (
        [ExpedienteImportacionId] bigint NOT NULL IDENTITY,
        [Numero] nvarchar(30) NOT NULL,
        [Estado] tinyint NOT NULL,
        [Incoterm] nvarchar(10) NULL,
        [ModalidadTransporte] nvarchar(30) NULL,
        [PaisOrigen] nvarchar(100) NULL,
        [PuertoOrigen] nvarchar(200) NULL,
        [PuertoDestino] nvarchar(200) NULL,
        [Forwarder] nvarchar(200) NULL,
        [Aseguradora] nvarchar(200) NULL,
        [NumeroPólizaSeguro] nvarchar(100) NULL,
        [NumeroBLAWB] nvarchar(100) NULL,
        [ETD] datetime2 NULL,
        [ATD] datetime2 NULL,
        [ETA] datetime2 NULL,
        [ATA] datetime2 NULL,
        [NumeroDUIDIM] nvarchar(100) NULL,
        [Despachante] nvarchar(200) NULL,
        [FechaPresentacionAduana] datetime2 NULL,
        [TotalTributos] decimal(18,4) NULL,
        [TuvoObservacionAduana] bit NOT NULL,
        [DetalleObservacionAduana] nvarchar(2000) NULL,
        [FechaLevante] datetime2 NULL,
        [Observaciones] nvarchar(1000) NULL,
        [FechaCreacion] datetime2 NOT NULL,
        [CreadoPor] nvarchar(100) NULL,
        [FechaModificacion] datetime2 NULL,
        [ModificadoPor] nvarchar(100) NULL,
        [Activo] bit NOT NULL,
        [EmpresaId] int NOT NULL,
        CONSTRAINT [PK_ExpedientesImportacion] PRIMARY KEY ([ExpedienteImportacionId])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260305065107_CMP_FlujoImportacionCompleto'
)
BEGIN
    CREATE TABLE [cmp].[OrdenesPedido] (
        [OrdenPedidoId] bigint NOT NULL IDENTITY,
        [Numero] nvarchar(30) NOT NULL,
        [FechaEmision] datetime2 NOT NULL,
        [FechaRequerida] datetime2 NULL,
        [Solicitante] nvarchar(200) NOT NULL,
        [CentroCosto] nvarchar(100) NULL,
        [Urgencia] tinyint NOT NULL,
        [AlmacenDestinoId] int NOT NULL,
        [Estado] tinyint NOT NULL,
        [Observaciones] nvarchar(1000) NULL,
        [MotivoRechazo] nvarchar(500) NULL,
        [StockCubre] bit NULL,
        [ObservacionesRevisionStock] nvarchar(1000) NULL,
        [FechaCreacion] datetime2 NOT NULL,
        [CreadoPor] nvarchar(100) NULL,
        [FechaModificacion] datetime2 NULL,
        [ModificadoPor] nvarchar(100) NULL,
        [Activo] bit NOT NULL,
        [EmpresaId] int NOT NULL,
        CONSTRAINT [PK_OrdenesPedido] PRIMARY KEY ([OrdenPedidoId]),
        CONSTRAINT [FK_OrdenesPedido_Almacenes_AlmacenDestinoId] FOREIGN KEY ([AlmacenDestinoId]) REFERENCES [mdm].[Almacenes] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260305065107_CMP_FlujoImportacionCompleto'
)
BEGIN
    CREATE TABLE [cmp].[PagosOrdenCompra] (
        [PagoOrdenCompraId] bigint NOT NULL IDENTITY,
        [OrdenCompraId] bigint NOT NULL,
        [TipoPago] tinyint NOT NULL,
        [MontoProgramado] decimal(18,4) NOT NULL,
        [MonedaId] nvarchar(3) NOT NULL,
        [TasaCambio] decimal(18,6) NOT NULL,
        [FechaProgramada] datetime2 NOT NULL,
        [FechaEjecucion] datetime2 NULL,
        [MontoEjecutado] decimal(18,4) NULL,
        [Ejecutado] bit NOT NULL,
        [ReferenciaTransferencia] nvarchar(200) NULL,
        [Observaciones] nvarchar(1000) NULL,
        [FechaCreacion] datetime2 NOT NULL,
        [CreadoPor] nvarchar(100) NULL,
        [FechaModificacion] datetime2 NULL,
        [ModificadoPor] nvarchar(100) NULL,
        [Activo] bit NOT NULL,
        [EmpresaId] int NOT NULL,
        CONSTRAINT [PK_PagosOrdenCompra] PRIMARY KEY ([PagoOrdenCompraId]),
        CONSTRAINT [FK_PagosOrdenCompra_OrdenesCompra_OrdenCompraId] FOREIGN KEY ([OrdenCompraId]) REFERENCES [cmp].[OrdenesCompra] ([OrdenCompraId]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260305065107_CMP_FlujoImportacionCompleto'
)
BEGIN
    CREATE TABLE [cmp].[HitosExpediente] (
        [HitoExpedienteId] bigint NOT NULL IDENTITY,
        [ExpedienteImportacionId] bigint NOT NULL,
        [TipoHito] nvarchar(50) NOT NULL,
        [FechaHito] datetime2 NOT NULL,
        [Descripcion] nvarchar(1000) NULL,
        [ReferenciaDocumento] nvarchar(200) NULL,
        [FechaCreacion] datetime2 NOT NULL,
        [CreadoPor] nvarchar(100) NULL,
        [FechaModificacion] datetime2 NULL,
        [ModificadoPor] nvarchar(100) NULL,
        [Activo] bit NOT NULL,
        CONSTRAINT [PK_HitosExpediente] PRIMARY KEY ([HitoExpedienteId]),
        CONSTRAINT [FK_HitosExpediente_ExpedientesImportacion_ExpedienteImportacionId] FOREIGN KEY ([ExpedienteImportacionId]) REFERENCES [cmp].[ExpedientesImportacion] ([ExpedienteImportacionId]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260305065107_CMP_FlujoImportacionCompleto'
)
BEGIN
    CREATE TABLE [cmp].[OrdenPedidoLineas] (
        [OrdenPedidoLineaId] bigint NOT NULL IDENTITY,
        [OrdenPedidoId] bigint NOT NULL,
        [NumeroLinea] int NOT NULL,
        [CompanyProductId] bigint NOT NULL,
        [Descripcion] nvarchar(500) NOT NULL,
        [UnidadMedida] nvarchar(20) NOT NULL,
        [CantidadSolicitada] decimal(18,4) NOT NULL,
        [CantidadStockDisponible] decimal(18,4) NULL,
        [CantidadEnTransito] decimal(18,4) NULL,
        [CantidadAComprar] decimal(18,4) NULL,
        [Notas] nvarchar(500) NULL,
        [FechaCreacion] datetime2 NOT NULL,
        [CreadoPor] nvarchar(100) NULL,
        [FechaModificacion] datetime2 NULL,
        [ModificadoPor] nvarchar(100) NULL,
        [Activo] bit NOT NULL,
        CONSTRAINT [PK_OrdenPedidoLineas] PRIMARY KEY ([OrdenPedidoLineaId]),
        CONSTRAINT [FK_OrdenPedidoLineas_CompanyProducts_CompanyProductId] FOREIGN KEY ([CompanyProductId]) REFERENCES [mdm].[CompanyProducts] ([CompanyProductId]) ON DELETE NO ACTION,
        CONSTRAINT [FK_OrdenPedidoLineas_OrdenesPedido_OrdenPedidoId] FOREIGN KEY ([OrdenPedidoId]) REFERENCES [cmp].[OrdenesPedido] ([OrdenPedidoId]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260305065107_CMP_FlujoImportacionCompleto'
)
BEGIN
    CREATE INDEX [IX_OrdenesCompra_EmpresaId_ExpedienteImportacionId] ON [cmp].[OrdenesCompra] ([EmpresaId], [ExpedienteImportacionId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260305065107_CMP_FlujoImportacionCompleto'
)
BEGIN
    CREATE INDEX [IX_OrdenesCompra_EmpresaId_OrdenPedidoId] ON [cmp].[OrdenesCompra] ([EmpresaId], [OrdenPedidoId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260305065107_CMP_FlujoImportacionCompleto'
)
BEGIN
    CREATE INDEX [IX_OrdenesCompra_ExpedienteImportacionId] ON [cmp].[OrdenesCompra] ([ExpedienteImportacionId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260305065107_CMP_FlujoImportacionCompleto'
)
BEGIN
    CREATE INDEX [IX_OrdenesCompra_OrdenPedidoId] ON [cmp].[OrdenesCompra] ([OrdenPedidoId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260305065107_CMP_FlujoImportacionCompleto'
)
BEGIN
    CREATE INDEX [IX_HojasImportacion_EmpresaId_ExpedienteImportacionId] ON [cmp].[HojasImportacion] ([EmpresaId], [ExpedienteImportacionId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260305065107_CMP_FlujoImportacionCompleto'
)
BEGIN
    CREATE INDEX [IX_HojasImportacion_ExpedienteImportacionId] ON [cmp].[HojasImportacion] ([ExpedienteImportacionId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260305065107_CMP_FlujoImportacionCompleto'
)
BEGIN
    CREATE INDEX [IX_ConfirmacionesProveedor_EmpresaId] ON [cmp].[ConfirmacionesProveedor] ([EmpresaId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260305065107_CMP_FlujoImportacionCompleto'
)
BEGIN
    CREATE INDEX [IX_ConfirmacionesProveedor_OrdenCompraId_IteracionNegociacion] ON [cmp].[ConfirmacionesProveedor] ([OrdenCompraId], [IteracionNegociacion]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260305065107_CMP_FlujoImportacionCompleto'
)
BEGIN
    CREATE INDEX [IX_ExpedientesImportacion_EmpresaId] ON [cmp].[ExpedientesImportacion] ([EmpresaId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260305065107_CMP_FlujoImportacionCompleto'
)
BEGIN
    CREATE INDEX [IX_ExpedientesImportacion_EmpresaId_Estado] ON [cmp].[ExpedientesImportacion] ([EmpresaId], [Estado]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260305065107_CMP_FlujoImportacionCompleto'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ExpedientesImportacion_EmpresaId_Numero] ON [cmp].[ExpedientesImportacion] ([EmpresaId], [Numero]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260305065107_CMP_FlujoImportacionCompleto'
)
BEGIN
    CREATE INDEX [IX_HitosExpediente_ExpedienteImportacionId_FechaHito] ON [cmp].[HitosExpediente] ([ExpedienteImportacionId], [FechaHito]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260305065107_CMP_FlujoImportacionCompleto'
)
BEGIN
    CREATE INDEX [IX_OrdenesPedido_AlmacenDestinoId] ON [cmp].[OrdenesPedido] ([AlmacenDestinoId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260305065107_CMP_FlujoImportacionCompleto'
)
BEGIN
    CREATE INDEX [IX_OrdenesPedido_EmpresaId] ON [cmp].[OrdenesPedido] ([EmpresaId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260305065107_CMP_FlujoImportacionCompleto'
)
BEGIN
    CREATE INDEX [IX_OrdenesPedido_EmpresaId_Estado] ON [cmp].[OrdenesPedido] ([EmpresaId], [Estado]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260305065107_CMP_FlujoImportacionCompleto'
)
BEGIN
    CREATE INDEX [IX_OrdenesPedido_EmpresaId_FechaEmision] ON [cmp].[OrdenesPedido] ([EmpresaId], [FechaEmision]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260305065107_CMP_FlujoImportacionCompleto'
)
BEGIN
    CREATE UNIQUE INDEX [IX_OrdenesPedido_EmpresaId_Numero] ON [cmp].[OrdenesPedido] ([EmpresaId], [Numero]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260305065107_CMP_FlujoImportacionCompleto'
)
BEGIN
    CREATE INDEX [IX_OrdenPedidoLineas_CompanyProductId] ON [cmp].[OrdenPedidoLineas] ([CompanyProductId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260305065107_CMP_FlujoImportacionCompleto'
)
BEGIN
    CREATE UNIQUE INDEX [IX_OrdenPedidoLineas_OrdenPedidoId_NumeroLinea] ON [cmp].[OrdenPedidoLineas] ([OrdenPedidoId], [NumeroLinea]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260305065107_CMP_FlujoImportacionCompleto'
)
BEGIN
    CREATE INDEX [IX_PagosOrdenCompra_EmpresaId] ON [cmp].[PagosOrdenCompra] ([EmpresaId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260305065107_CMP_FlujoImportacionCompleto'
)
BEGIN
    CREATE INDEX [IX_PagosOrdenCompra_EmpresaId_Ejecutado] ON [cmp].[PagosOrdenCompra] ([EmpresaId], [Ejecutado]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260305065107_CMP_FlujoImportacionCompleto'
)
BEGIN
    CREATE INDEX [IX_PagosOrdenCompra_EmpresaId_OrdenCompraId] ON [cmp].[PagosOrdenCompra] ([EmpresaId], [OrdenCompraId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260305065107_CMP_FlujoImportacionCompleto'
)
BEGIN
    CREATE INDEX [IX_PagosOrdenCompra_OrdenCompraId] ON [cmp].[PagosOrdenCompra] ([OrdenCompraId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260305065107_CMP_FlujoImportacionCompleto'
)
BEGIN
    ALTER TABLE [cmp].[HojasImportacion] ADD CONSTRAINT [FK_HojasImportacion_ExpedientesImportacion_ExpedienteImportacionId] FOREIGN KEY ([ExpedienteImportacionId]) REFERENCES [cmp].[ExpedientesImportacion] ([ExpedienteImportacionId]) ON DELETE SET NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260305065107_CMP_FlujoImportacionCompleto'
)
BEGIN
    ALTER TABLE [cmp].[OrdenesCompra] ADD CONSTRAINT [FK_OrdenesCompra_ExpedientesImportacion_ExpedienteImportacionId] FOREIGN KEY ([ExpedienteImportacionId]) REFERENCES [cmp].[ExpedientesImportacion] ([ExpedienteImportacionId]) ON DELETE SET NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260305065107_CMP_FlujoImportacionCompleto'
)
BEGIN
    ALTER TABLE [cmp].[OrdenesCompra] ADD CONSTRAINT [FK_OrdenesCompra_OrdenesPedido_OrdenPedidoId] FOREIGN KEY ([OrdenPedidoId]) REFERENCES [cmp].[OrdenesPedido] ([OrdenPedidoId]) ON DELETE SET NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260305065107_CMP_FlujoImportacionCompleto'
)
BEGIN
    INSERT INTO [core].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260305065107_CMP_FlujoImportacionCompleto', N'8.0.12');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260306134001_AddCentrosCostoAndComprobanteDocumentos'
)
BEGIN
    ALTER TABLE [acc].[AsientoContableLineas] ADD [CentroCostoId] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260306134001_AddCentrosCostoAndComprobanteDocumentos'
)
BEGIN
    CREATE TABLE [cst].[CentrosCosto] (
        [Id] int NOT NULL IDENTITY,
        [Codigo] nvarchar(20) NOT NULL,
        [Nombre] nvarchar(120) NOT NULL,
        [Descripcion] nvarchar(500) NULL,
        [ParentId] int NULL,
        [FechaCreacion] datetime2 NOT NULL,
        [CreadoPor] nvarchar(100) NULL,
        [FechaModificacion] datetime2 NULL,
        [ModificadoPor] nvarchar(100) NULL,
        [Activo] bit NOT NULL,
        [EmpresaId] int NOT NULL,
        CONSTRAINT [PK_CentrosCosto] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_CentrosCosto_CentrosCosto_ParentId] FOREIGN KEY ([ParentId]) REFERENCES [cst].[CentrosCosto] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260306134001_AddCentrosCostoAndComprobanteDocumentos'
)
BEGIN
    CREATE TABLE [doc].[ComprobanteDocumentos] (
        [Id] int NOT NULL IDENTITY,
        [ComprobanteId] bigint NOT NULL,
        [DocumentId] bigint NOT NULL,
        [Descripcion] nvarchar(200) NULL,
        [FechaCreacion] datetime2 NOT NULL,
        [CreadoPor] nvarchar(100) NULL,
        [FechaModificacion] datetime2 NULL,
        [ModificadoPor] nvarchar(100) NULL,
        [Activo] bit NOT NULL,
        [EmpresaId] int NOT NULL,
        CONSTRAINT [PK_ComprobanteDocumentos] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ComprobanteDocumentos_AsientosContables_ComprobanteId] FOREIGN KEY ([ComprobanteId]) REFERENCES [acc].[AsientosContables] ([AsientoContableId]) ON DELETE CASCADE,
        CONSTRAINT [FK_ComprobanteDocumentos_Documents_DocumentId] FOREIGN KEY ([DocumentId]) REFERENCES [doc].[Documents] ([DocumentId]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260306134001_AddCentrosCostoAndComprobanteDocumentos'
)
BEGIN
    CREATE INDEX [IX_AsientoContableLineas_CentroCostoId] ON [acc].[AsientoContableLineas] ([CentroCostoId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260306134001_AddCentrosCostoAndComprobanteDocumentos'
)
BEGIN
    CREATE INDEX [IX_CentrosCosto_EmpresaId] ON [cst].[CentrosCosto] ([EmpresaId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260306134001_AddCentrosCostoAndComprobanteDocumentos'
)
BEGIN
    CREATE UNIQUE INDEX [IX_CentrosCosto_EmpresaId_Codigo] ON [cst].[CentrosCosto] ([EmpresaId], [Codigo]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260306134001_AddCentrosCostoAndComprobanteDocumentos'
)
BEGIN
    CREATE INDEX [IX_CentrosCosto_ParentId] ON [cst].[CentrosCosto] ([ParentId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260306134001_AddCentrosCostoAndComprobanteDocumentos'
)
BEGIN
    CREATE INDEX [IX_ComprobanteDocumentos_ComprobanteId] ON [doc].[ComprobanteDocumentos] ([ComprobanteId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260306134001_AddCentrosCostoAndComprobanteDocumentos'
)
BEGIN
    CREATE INDEX [IX_ComprobanteDocumentos_DocumentId] ON [doc].[ComprobanteDocumentos] ([DocumentId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260306134001_AddCentrosCostoAndComprobanteDocumentos'
)
BEGIN
    CREATE INDEX [IX_ComprobanteDocumentos_EmpresaId] ON [doc].[ComprobanteDocumentos] ([EmpresaId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260306134001_AddCentrosCostoAndComprobanteDocumentos'
)
BEGIN
    ALTER TABLE [acc].[AsientoContableLineas] ADD CONSTRAINT [FK_AsientoContableLineas_CentrosCosto_CentroCostoId] FOREIGN KEY ([CentroCostoId]) REFERENCES [cst].[CentrosCosto] ([Id]) ON DELETE SET NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260306134001_AddCentrosCostoAndComprobanteDocumentos'
)
BEGIN
    INSERT INTO [core].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260306134001_AddCentrosCostoAndComprobanteDocumentos', N'8.0.12');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260309033747_TenantScopeMDM'
)
BEGIN

                    IF COL_LENGTH('mdm.Products','EmpresaId') IS NULL
                        ALTER TABLE mdm.Products ADD EmpresaId INT NOT NULL CONSTRAINT DF_Products_EmpresaId DEFAULT(0);
                    IF COL_LENGTH('mdm.Brands','EmpresaId') IS NULL
                        ALTER TABLE mdm.Brands ADD EmpresaId INT NOT NULL CONSTRAINT DF_Brands_EmpresaId DEFAULT(0);
                    IF COL_LENGTH('mdm.Manufacturers','EmpresaId') IS NULL
                        ALTER TABLE mdm.Manufacturers ADD EmpresaId INT NOT NULL CONSTRAINT DF_Manufacturers_EmpresaId DEFAULT(0);
                    IF COL_LENGTH('mdm.Categories','EmpresaId') IS NULL
                        ALTER TABLE mdm.Categories ADD EmpresaId INT NOT NULL CONSTRAINT DF_Categories_EmpresaId DEFAULT(0);
                    IF COL_LENGTH('mdm.Uoms','EmpresaId') IS NULL
                        ALTER TABLE mdm.Uoms ADD EmpresaId INT NOT NULL CONSTRAINT DF_Uoms_EmpresaId DEFAULT(0);
                    IF COL_LENGTH('mdm.ProductStatuses','EmpresaId') IS NULL
                        ALTER TABLE mdm.ProductStatuses ADD EmpresaId INT NOT NULL CONSTRAINT DF_ProductStatuses_EmpresaId DEFAULT(0);
                    IF COL_LENGTH('mdm.ProductAttributes','EmpresaId') IS NULL
                        ALTER TABLE mdm.ProductAttributes ADD EmpresaId INT NOT NULL CONSTRAINT DF_ProductAttributes_EmpresaId DEFAULT(0);
                    IF COL_LENGTH('mdm.ProductUoms','EmpresaId') IS NULL
                        ALTER TABLE mdm.ProductUoms ADD EmpresaId INT NOT NULL CONSTRAINT DF_ProductUoms_EmpresaId DEFAULT(0);
                    IF COL_LENGTH('mdm.ProductClassifications','EmpresaId') IS NULL
                        ALTER TABLE mdm.ProductClassifications ADD EmpresaId INT NOT NULL CONSTRAINT DF_ProductClassifications_EmpresaId DEFAULT(0);
                    IF COL_LENGTH('mdm.AttributeDefinitions','EmpresaId') IS NULL
                        ALTER TABLE mdm.AttributeDefinitions ADD EmpresaId INT NOT NULL CONSTRAINT DF_AttributeDefinitions_EmpresaId DEFAULT(0);
                    IF COL_LENGTH('mdm.AttributeOptions','EmpresaId') IS NULL
                        ALTER TABLE mdm.AttributeOptions ADD EmpresaId INT NOT NULL CONSTRAINT DF_AttributeOptions_EmpresaId DEFAULT(0);
                
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260309033747_TenantScopeMDM'
)
BEGIN

                    DECLARE @EmpId INT = (SELECT TOP 1 Id FROM core.Empresas ORDER BY Id);
                    IF @EmpId IS NULL SET @EmpId = 1;

                    UPDATE mdm.Products SET EmpresaId = @EmpId WHERE EmpresaId = 0;
                    UPDATE mdm.Brands SET EmpresaId = @EmpId WHERE EmpresaId = 0;
                    UPDATE mdm.Manufacturers SET EmpresaId = @EmpId WHERE EmpresaId = 0;
                    UPDATE mdm.Categories SET EmpresaId = @EmpId WHERE EmpresaId = 0;
                    UPDATE mdm.Uoms SET EmpresaId = @EmpId WHERE EmpresaId = 0;
                    UPDATE mdm.ProductStatuses SET EmpresaId = @EmpId WHERE EmpresaId = 0;
                    UPDATE mdm.ProductAttributes SET EmpresaId = @EmpId WHERE EmpresaId = 0;
                    UPDATE mdm.ProductUoms SET EmpresaId = @EmpId WHERE EmpresaId = 0;
                    UPDATE mdm.ProductClassifications SET EmpresaId = @EmpId WHERE EmpresaId = 0;
                    UPDATE mdm.AttributeDefinitions SET EmpresaId = @EmpId WHERE EmpresaId = 0;
                    UPDATE mdm.AttributeOptions SET EmpresaId = @EmpId WHERE EmpresaId = 0;

                    -- Catalogs (already has EmpresaId as nullable)
                    UPDATE mdm.Catalogs SET EmpresaId = @EmpId WHERE EmpresaId IS NULL OR EmpresaId = 0;
                    -- ProductCodes (already has EmpresaId as nullable)
                    UPDATE mdm.ProductCodes SET EmpresaId = @EmpId WHERE EmpresaId IS NULL OR EmpresaId = 0;
                
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260309033747_TenantScopeMDM'
)
BEGIN

                    IF EXISTS(SELECT 1 FROM sys.indexes WHERE name='IX_Uoms_Code' AND object_id=OBJECT_ID('mdm.Uoms'))
                        DROP INDEX IX_Uoms_Code ON mdm.Uoms;
                    IF EXISTS(SELECT 1 FROM sys.indexes WHERE name='IX_ProductStatuses_Code' AND object_id=OBJECT_ID('mdm.ProductStatuses'))
                        DROP INDEX IX_ProductStatuses_Code ON mdm.ProductStatuses;
                    IF EXISTS(SELECT 1 FROM sys.indexes WHERE name='IX_Manufacturers_Name' AND object_id=OBJECT_ID('mdm.Manufacturers'))
                        DROP INDEX IX_Manufacturers_Name ON mdm.Manufacturers;
                    IF EXISTS(SELECT 1 FROM sys.indexes WHERE name='IX_Categories_CatalogId_ParentCategoryId_Name' AND object_id=OBJECT_ID('mdm.Categories'))
                        DROP INDEX IX_Categories_CatalogId_ParentCategoryId_Name ON mdm.Categories;
                    IF EXISTS(SELECT 1 FROM sys.indexes WHERE name='IX_Catalogs_Scope_EmpresaId_Name' AND object_id=OBJECT_ID('mdm.Catalogs'))
                        DROP INDEX IX_Catalogs_Scope_EmpresaId_Name ON mdm.Catalogs;
                    IF EXISTS(SELECT 1 FROM sys.indexes WHERE name='IX_Brands_Name' AND object_id=OBJECT_ID('mdm.Brands'))
                        DROP INDEX IX_Brands_Name ON mdm.Brands;
                    IF EXISTS(SELECT 1 FROM sys.indexes WHERE name='IX_AttributeDefinitions_IndustryId_Code' AND object_id=OBJECT_ID('mdm.AttributeDefinitions'))
                        DROP INDEX IX_AttributeDefinitions_IndustryId_Code ON mdm.AttributeDefinitions;
                    -- Also drop ProductCodes index if it uses EmpresaId
                    IF EXISTS(SELECT 1 FROM sys.indexes WHERE name='IX_ProductCodes_EmpresaId_CodeType_Valor' AND object_id=OBJECT_ID('mdm.ProductCodes'))
                        DROP INDEX IX_ProductCodes_EmpresaId_CodeType_Valor ON mdm.ProductCodes;
                
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260309033747_TenantScopeMDM'
)
BEGIN
    ALTER TABLE mdm.Catalogs ALTER COLUMN EmpresaId INT NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260309033747_TenantScopeMDM'
)
BEGIN
    ALTER TABLE mdm.ProductCodes ALTER COLUMN EmpresaId INT NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260309033747_TenantScopeMDM'
)
BEGIN

                    IF COL_LENGTH('core.Empresas','IndustriaId') IS NULL
                        ALTER TABLE core.Empresas ADD IndustriaId TINYINT NOT NULL CONSTRAINT DF_Empresas_IndustriaId DEFAULT(7);
                    IF COL_LENGTH('core.Empresas','MetodoCosteoDefault') IS NULL
                        ALTER TABLE core.Empresas ADD MetodoCosteoDefault TINYINT NOT NULL CONSTRAINT DF_Empresas_MetodoCosteoDefault DEFAULT(1);
                    IF COL_LENGTH('core.Empresas','PermiteVariantes') IS NULL
                        ALTER TABLE core.Empresas ADD PermiteVariantes BIT NOT NULL CONSTRAINT DF_Empresas_PermiteVariantes DEFAULT(1);
                    IF COL_LENGTH('core.Empresas','PermiteLotes') IS NULL
                        ALTER TABLE core.Empresas ADD PermiteLotes BIT NOT NULL CONSTRAINT DF_Empresas_PermiteLotes DEFAULT(0);
                    IF COL_LENGTH('core.Empresas','PermiteServicios') IS NULL
                        ALTER TABLE core.Empresas ADD PermiteServicios BIT NOT NULL CONSTRAINT DF_Empresas_PermiteServicios DEFAULT(1);
                    IF COL_LENGTH('core.Empresas','AutoGeneraSku') IS NULL
                        ALTER TABLE core.Empresas ADD AutoGeneraSku BIT NOT NULL CONSTRAINT DF_Empresas_AutoGeneraSku DEFAULT(1);
                    IF COL_LENGTH('core.Empresas','PrefijoSku') IS NULL
                        ALTER TABLE core.Empresas ADD PrefijoSku NVARCHAR(20) NULL;
                
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260309033747_TenantScopeMDM'
)
BEGIN

                    -- Uoms
                    IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name='IX_Uoms_EmpresaId' AND object_id=OBJECT_ID('mdm.Uoms'))
                        CREATE INDEX IX_Uoms_EmpresaId ON mdm.Uoms(EmpresaId);
                    IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name='IX_Uoms_EmpresaId_Code' AND object_id=OBJECT_ID('mdm.Uoms'))
                        CREATE UNIQUE INDEX IX_Uoms_EmpresaId_Code ON mdm.Uoms(EmpresaId, Code);

                    -- ProductStatuses
                    IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name='IX_ProductStatuses_EmpresaId' AND object_id=OBJECT_ID('mdm.ProductStatuses'))
                        CREATE INDEX IX_ProductStatuses_EmpresaId ON mdm.ProductStatuses(EmpresaId);
                    IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name='IX_ProductStatuses_EmpresaId_Code' AND object_id=OBJECT_ID('mdm.ProductStatuses'))
                        CREATE UNIQUE INDEX IX_ProductStatuses_EmpresaId_Code ON mdm.ProductStatuses(EmpresaId, Code);

                    -- Products
                    IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name='IX_Products_EmpresaId' AND object_id=OBJECT_ID('mdm.Products'))
                        CREATE INDEX IX_Products_EmpresaId ON mdm.Products(EmpresaId);

                    -- ProductCodes
                    IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name='IX_ProductCodes_EmpresaId' AND object_id=OBJECT_ID('mdm.ProductCodes'))
                        CREATE INDEX IX_ProductCodes_EmpresaId ON mdm.ProductCodes(EmpresaId);

                    -- ProductClassifications
                    IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name='IX_ProductClassifications_EmpresaId' AND object_id=OBJECT_ID('mdm.ProductClassifications'))
                        CREATE INDEX IX_ProductClassifications_EmpresaId ON mdm.ProductClassifications(EmpresaId);

                    -- ProductAttributes
                    IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name='IX_ProductAttributes_EmpresaId' AND object_id=OBJECT_ID('mdm.ProductAttributes'))
                        CREATE INDEX IX_ProductAttributes_EmpresaId ON mdm.ProductAttributes(EmpresaId);

                    -- ProductUoms
                    IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name='IX_ProductUoms_EmpresaId' AND object_id=OBJECT_ID('mdm.ProductUoms'))
                        CREATE INDEX IX_ProductUoms_EmpresaId ON mdm.ProductUoms(EmpresaId);

                    -- Manufacturers
                    IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name='IX_Manufacturers_EmpresaId' AND object_id=OBJECT_ID('mdm.Manufacturers'))
                        CREATE INDEX IX_Manufacturers_EmpresaId ON mdm.Manufacturers(EmpresaId);
                    IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name='IX_Manufacturers_EmpresaId_Name' AND object_id=OBJECT_ID('mdm.Manufacturers'))
                        CREATE UNIQUE INDEX IX_Manufacturers_EmpresaId_Name ON mdm.Manufacturers(EmpresaId, Name);

                    -- Categories
                    IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name='IX_Categories_EmpresaId' AND object_id=OBJECT_ID('mdm.Categories'))
                        CREATE INDEX IX_Categories_EmpresaId ON mdm.Categories(EmpresaId);
                    IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name='IX_Categories_EmpresaId_CatalogId_ParentCategoryId_Name' AND object_id=OBJECT_ID('mdm.Categories'))
                        CREATE UNIQUE INDEX IX_Categories_EmpresaId_CatalogId_ParentCategoryId_Name ON mdm.Categories(EmpresaId, CatalogId, ParentCategoryId, Name) WHERE ParentCategoryId IS NOT NULL;

                    -- Catalogs
                    IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name='IX_Catalogs_EmpresaId' AND object_id=OBJECT_ID('mdm.Catalogs'))
                        CREATE INDEX IX_Catalogs_EmpresaId ON mdm.Catalogs(EmpresaId);
                    IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name='IX_Catalogs_EmpresaId_Name' AND object_id=OBJECT_ID('mdm.Catalogs'))
                        CREATE UNIQUE INDEX IX_Catalogs_EmpresaId_Name ON mdm.Catalogs(EmpresaId, Name);

                    -- Brands
                    IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name='IX_Brands_EmpresaId' AND object_id=OBJECT_ID('mdm.Brands'))
                        CREATE INDEX IX_Brands_EmpresaId ON mdm.Brands(EmpresaId);
                    IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name='IX_Brands_EmpresaId_Name' AND object_id=OBJECT_ID('mdm.Brands'))
                        CREATE UNIQUE INDEX IX_Brands_EmpresaId_Name ON mdm.Brands(EmpresaId, Name);

                    -- AttributeOptions
                    IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name='IX_AttributeOptions_EmpresaId' AND object_id=OBJECT_ID('mdm.AttributeOptions'))
                        CREATE INDEX IX_AttributeOptions_EmpresaId ON mdm.AttributeOptions(EmpresaId);

                    -- AttributeDefinitions
                    IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name='IX_AttributeDefinitions_EmpresaId' AND object_id=OBJECT_ID('mdm.AttributeDefinitions'))
                        CREATE INDEX IX_AttributeDefinitions_EmpresaId ON mdm.AttributeDefinitions(EmpresaId);
                    IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name='IX_AttributeDefinitions_EmpresaId_IndustryId_Code' AND object_id=OBJECT_ID('mdm.AttributeDefinitions'))
                        CREATE UNIQUE INDEX IX_AttributeDefinitions_EmpresaId_IndustryId_Code ON mdm.AttributeDefinitions(EmpresaId, IndustryId, Code);
                
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260309033747_TenantScopeMDM'
)
BEGIN

                    IF NOT EXISTS(SELECT 1 FROM sys.foreign_keys WHERE name='FK_CentrosCosto_Empresas_EmpresaId')
                        ALTER TABLE cst.CentrosCosto ADD CONSTRAINT FK_CentrosCosto_Empresas_EmpresaId
                            FOREIGN KEY (EmpresaId) REFERENCES core.Empresas(Id);
                
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260309033747_TenantScopeMDM'
)
BEGIN

                    IF EXISTS(SELECT 1 FROM sys.default_constraints WHERE name='DF_Products_EmpresaId') ALTER TABLE mdm.Products DROP CONSTRAINT DF_Products_EmpresaId;
                    IF EXISTS(SELECT 1 FROM sys.default_constraints WHERE name='DF_Brands_EmpresaId') ALTER TABLE mdm.Brands DROP CONSTRAINT DF_Brands_EmpresaId;
                    IF EXISTS(SELECT 1 FROM sys.default_constraints WHERE name='DF_Manufacturers_EmpresaId') ALTER TABLE mdm.Manufacturers DROP CONSTRAINT DF_Manufacturers_EmpresaId;
                    IF EXISTS(SELECT 1 FROM sys.default_constraints WHERE name='DF_Categories_EmpresaId') ALTER TABLE mdm.Categories DROP CONSTRAINT DF_Categories_EmpresaId;
                    IF EXISTS(SELECT 1 FROM sys.default_constraints WHERE name='DF_Uoms_EmpresaId') ALTER TABLE mdm.Uoms DROP CONSTRAINT DF_Uoms_EmpresaId;
                    IF EXISTS(SELECT 1 FROM sys.default_constraints WHERE name='DF_ProductStatuses_EmpresaId') ALTER TABLE mdm.ProductStatuses DROP CONSTRAINT DF_ProductStatuses_EmpresaId;
                    IF EXISTS(SELECT 1 FROM sys.default_constraints WHERE name='DF_ProductAttributes_EmpresaId') ALTER TABLE mdm.ProductAttributes DROP CONSTRAINT DF_ProductAttributes_EmpresaId;
                    IF EXISTS(SELECT 1 FROM sys.default_constraints WHERE name='DF_ProductUoms_EmpresaId') ALTER TABLE mdm.ProductUoms DROP CONSTRAINT DF_ProductUoms_EmpresaId;
                    IF EXISTS(SELECT 1 FROM sys.default_constraints WHERE name='DF_ProductClassifications_EmpresaId') ALTER TABLE mdm.ProductClassifications DROP CONSTRAINT DF_ProductClassifications_EmpresaId;
                    IF EXISTS(SELECT 1 FROM sys.default_constraints WHERE name='DF_AttributeDefinitions_EmpresaId') ALTER TABLE mdm.AttributeDefinitions DROP CONSTRAINT DF_AttributeDefinitions_EmpresaId;
                    IF EXISTS(SELECT 1 FROM sys.default_constraints WHERE name='DF_AttributeOptions_EmpresaId') ALTER TABLE mdm.AttributeOptions DROP CONSTRAINT DF_AttributeOptions_EmpresaId;
                
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260309033747_TenantScopeMDM'
)
BEGIN
    INSERT INTO [core].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260309033747_TenantScopeMDM', N'8.0.12');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260311012127_CMP_OrdenPedido_SolicitanteId'
)
BEGIN
    DECLARE @var1 sysname;
    SELECT @var1 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[cmp].[OrdenesPedido]') AND [c].[name] = N'Solicitante');
    IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [cmp].[OrdenesPedido] DROP CONSTRAINT [' + @var1 + '];');
    ALTER TABLE [cmp].[OrdenesPedido] DROP COLUMN [Solicitante];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260311012127_CMP_OrdenPedido_SolicitanteId'
)
BEGIN
    IF SCHEMA_ID(N'log') IS NULL EXEC(N'CREATE SCHEMA [log];');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260311012127_CMP_OrdenPedido_SolicitanteId'
)
BEGIN
    ALTER TABLE [cmp].[OrdenesPedido] ADD [SolicitanteId] int NOT NULL DEFAULT 0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260311012127_CMP_OrdenPedido_SolicitanteId'
)
BEGIN
    CREATE TABLE [log].[HojasRuta] (
        [HojaRutaId] bigint NOT NULL IDENTITY,
        [NumeroHojaRuta] nvarchar(20) NOT NULL,
        [TipoOP] nvarchar(20) NOT NULL,
        [SubTipo] nvarchar(30) NULL,
        [AlmacenOrigenId] int NOT NULL,
        [AlmacenDestinoId] int NULL,
        [ProveedorCliente] nvarchar(200) NULL,
        [DireccionEntrega] nvarchar(300) NULL,
        [ContactoCliente] nvarchar(150) NULL,
        [ResponsableUsuario] nvarchar(100) NOT NULL,
        [FechaRegistro] datetime2 NOT NULL,
        [FechaDocumento] datetime2 NOT NULL,
        [ETA] datetime2 NULL,
        [Estado] nvarchar(20) NOT NULL,
        [SubEstado] nvarchar(20) NOT NULL,
        [Observaciones] nvarchar(2000) NULL,
        [FechaCreacion] datetime2 NOT NULL,
        [CreadoPor] nvarchar(100) NULL,
        [FechaModificacion] datetime2 NULL,
        [ModificadoPor] nvarchar(100) NULL,
        [Activo] bit NOT NULL,
        [EmpresaId] int NOT NULL,
        CONSTRAINT [PK_HojasRuta] PRIMARY KEY ([HojaRutaId])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260311012127_CMP_OrdenPedido_SolicitanteId'
)
BEGIN
    CREATE TABLE [log].[HojaRutaHistorial] (
        [HojaRutaHistorialId] bigint NOT NULL IDENTITY,
        [HojaRutaId] bigint NOT NULL,
        [EstadoAnterior] nvarchar(20) NOT NULL,
        [SubEstadoAnterior] nvarchar(20) NOT NULL,
        [EstadoNuevo] nvarchar(20) NOT NULL,
        [SubEstadoNuevo] nvarchar(20) NOT NULL,
        [FechaCambio] datetime2 NOT NULL,
        [Usuario] nvarchar(100) NOT NULL,
        [Observaciones] nvarchar(2000) NULL,
        [FechaCreacion] datetime2 NOT NULL,
        [CreadoPor] nvarchar(100) NULL,
        [FechaModificacion] datetime2 NULL,
        [ModificadoPor] nvarchar(100) NULL,
        [Activo] bit NOT NULL,
        CONSTRAINT [PK_HojaRutaHistorial] PRIMARY KEY ([HojaRutaHistorialId]),
        CONSTRAINT [FK_HojaRutaHistorial_HojasRuta_HojaRutaId] FOREIGN KEY ([HojaRutaId]) REFERENCES [log].[HojasRuta] ([HojaRutaId]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260311012127_CMP_OrdenPedido_SolicitanteId'
)
BEGIN
    CREATE INDEX [IX_OrdenesPedido_EmpresaId_SolicitanteId] ON [cmp].[OrdenesPedido] ([EmpresaId], [SolicitanteId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260311012127_CMP_OrdenPedido_SolicitanteId'
)
BEGIN
    CREATE INDEX [IX_OrdenesPedido_SolicitanteId] ON [cmp].[OrdenesPedido] ([SolicitanteId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260311012127_CMP_OrdenPedido_SolicitanteId'
)
BEGIN
    CREATE INDEX [IX_HojaRutaHistorial_HojaRutaId] ON [log].[HojaRutaHistorial] ([HojaRutaId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260311012127_CMP_OrdenPedido_SolicitanteId'
)
BEGIN
    CREATE INDEX [IX_HojasRuta_EmpresaId] ON [log].[HojasRuta] ([EmpresaId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260311012127_CMP_OrdenPedido_SolicitanteId'
)
BEGIN
    CREATE INDEX [IX_HojasRuta_EmpresaId_Estado] ON [log].[HojasRuta] ([EmpresaId], [Estado]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260311012127_CMP_OrdenPedido_SolicitanteId'
)
BEGIN
    CREATE INDEX [IX_HojasRuta_EmpresaId_FechaDocumento] ON [log].[HojasRuta] ([EmpresaId], [FechaDocumento]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260311012127_CMP_OrdenPedido_SolicitanteId'
)
BEGIN
    CREATE UNIQUE INDEX [IX_HojasRuta_EmpresaId_NumeroHojaRuta] ON [log].[HojasRuta] ([EmpresaId], [NumeroHojaRuta]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260311012127_CMP_OrdenPedido_SolicitanteId'
)
BEGIN
    CREATE INDEX [IX_HojasRuta_EmpresaId_TipoOP] ON [log].[HojasRuta] ([EmpresaId], [TipoOP]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260311012127_CMP_OrdenPedido_SolicitanteId'
)
BEGIN
    ALTER TABLE [cmp].[OrdenesPedido] ADD CONSTRAINT [FK_OrdenesPedido_Usuarios_SolicitanteId] FOREIGN KEY ([SolicitanteId]) REFERENCES [core].[Usuarios] ([Id]) ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260311012127_CMP_OrdenPedido_SolicitanteId'
)
BEGIN
    INSERT INTO [core].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260311012127_CMP_OrdenPedido_SolicitanteId', N'8.0.12');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260311103919_ACC_AsientoContable_NuevosCampos'
)
BEGIN
    DECLARE @var2 sysname;
    SELECT @var2 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[acc].[PeriodosContables]') AND [c].[name] = N'CerradoPor');
    IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [acc].[PeriodosContables] DROP CONSTRAINT [' + @var2 + '];');
    ALTER TABLE [acc].[PeriodosContables] DROP COLUMN [CerradoPor];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260311103919_ACC_AsientoContable_NuevosCampos'
)
BEGIN
    DECLARE @var3 sysname;
    SELECT @var3 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[acc].[AsientosContables]') AND [c].[name] = N'RegistradoPor');
    IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [acc].[AsientosContables] DROP CONSTRAINT [' + @var3 + '];');
    ALTER TABLE [acc].[AsientosContables] DROP COLUMN [RegistradoPor];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260311103919_ACC_AsientoContable_NuevosCampos'
)
BEGIN
    ALTER TABLE [acc].[PeriodosContables] ADD [CerradoPorId] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260311103919_ACC_AsientoContable_NuevosCampos'
)
BEGIN
    ALTER TABLE [acc].[PeriodosContables] ADD [CerradoPorNombre] nvarchar(300) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260311103919_ACC_AsientoContable_NuevosCampos'
)
BEGIN
    ALTER TABLE [acc].[AsientosContables] ADD [RegistradoPorId] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260311103919_ACC_AsientoContable_NuevosCampos'
)
BEGIN
    ALTER TABLE [acc].[AsientosContables] ADD [RegistradoPorNombre] nvarchar(300) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260311103919_ACC_AsientoContable_NuevosCampos'
)
BEGIN
    CREATE INDEX [IX_PeriodosContables_CerradoPorId] ON [acc].[PeriodosContables] ([CerradoPorId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260311103919_ACC_AsientoContable_NuevosCampos'
)
BEGIN
    CREATE INDEX [IX_PeriodosContables_EmpresaId_CerradoPorId] ON [acc].[PeriodosContables] ([EmpresaId], [CerradoPorId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260311103919_ACC_AsientoContable_NuevosCampos'
)
BEGIN
    CREATE INDEX [IX_AsientosContables_EmpresaId_RegistradoPorId] ON [acc].[AsientosContables] ([EmpresaId], [RegistradoPorId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260311103919_ACC_AsientoContable_NuevosCampos'
)
BEGIN
    CREATE INDEX [IX_AsientosContables_RegistradoPorId] ON [acc].[AsientosContables] ([RegistradoPorId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260311103919_ACC_AsientoContable_NuevosCampos'
)
BEGIN
    ALTER TABLE [acc].[AsientosContables] ADD CONSTRAINT [FK_AsientosContables_Usuarios_RegistradoPorId] FOREIGN KEY ([RegistradoPorId]) REFERENCES [core].[Usuarios] ([Id]) ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260311103919_ACC_AsientoContable_NuevosCampos'
)
BEGIN
    ALTER TABLE [acc].[PeriodosContables] ADD CONSTRAINT [FK_PeriodosContables_Usuarios_CerradoPorId] FOREIGN KEY ([CerradoPorId]) REFERENCES [core].[Usuarios] ([Id]) ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260311103919_ACC_AsientoContable_NuevosCampos'
)
BEGIN
    INSERT INTO [core].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260311103919_ACC_AsientoContable_NuevosCampos', N'8.0.12');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260311204642_AddWorkflowSchema'
)
BEGIN
    IF SCHEMA_ID(N'wf') IS NULL EXEC(N'CREATE SCHEMA [wf];');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260311204642_AddWorkflowSchema'
)
BEGIN
    CREATE TABLE [wf].[Tareas] (
        [Id] int NOT NULL IDENTITY,
        [EntityType] nvarchar(50) NOT NULL,
        [EntityId] int NOT NULL,
        [Orden] int NOT NULL,
        [Codigo] nvarchar(20) NOT NULL,
        [Descripcion] nvarchar(200) NOT NULL,
        [FechaPlan] date NULL,
        [FechaReal] date NULL,
        [Estado] nvarchar(20) NOT NULL DEFAULT N'PENDIENTE',
        [Completado] bit NOT NULL,
        [Responsable] nvarchar(100) NULL,
        [Observaciones] nvarchar(500) NULL,
        [MetadataJson] nvarchar(max) NULL,
        [FechaCreacion] datetime2 NOT NULL,
        [CreadoPor] nvarchar(120) NULL,
        [FechaModificacion] datetime2 NULL,
        [ModificadoPor] nvarchar(120) NULL,
        [Activo] bit NOT NULL,
        [EmpresaId] int NOT NULL,
        CONSTRAINT [PK_Tareas] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Tareas_Empresas_EmpresaId] FOREIGN KEY ([EmpresaId]) REFERENCES [core].[Empresas] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260311204642_AddWorkflowSchema'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Tareas_Codigo_UQ] ON [wf].[Tareas] ([EmpresaId], [EntityType], [EntityId], [Codigo]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260311204642_AddWorkflowSchema'
)
BEGIN
    CREATE INDEX [IX_Tareas_EmpresaId] ON [wf].[Tareas] ([EmpresaId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260311204642_AddWorkflowSchema'
)
BEGIN
    CREATE INDEX [IX_Tareas_Entity] ON [wf].[Tareas] ([EmpresaId], [EntityType], [EntityId], [Orden]) INCLUDE ([Estado], [Completado], [FechaReal]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260311204642_AddWorkflowSchema'
)
BEGIN
    CREATE INDEX [IX_Tareas_Estado] ON [wf].[Tareas] ([EmpresaId], [Estado], [EntityType]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260311204642_AddWorkflowSchema'
)
BEGIN
    CREATE INDEX [IX_Tareas_Fechas] ON [wf].[Tareas] ([EmpresaId], [FechaReal]) INCLUDE ([EntityType], [EntityId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260311204642_AddWorkflowSchema'
)
BEGIN
    INSERT INTO [core].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260311204642_AddWorkflowSchema', N'8.0.12');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260311205703_AddWorkflowPlantillasSeed'
)
BEGIN
    CREATE TABLE [wf].[PlantillasTareas] (
        [Id] int NOT NULL IDENTITY,
        [EntityType] nvarchar(50) NOT NULL,
        [SubTipo] nvarchar(50) NULL,
        [Orden] int NOT NULL,
        [Codigo] nvarchar(20) NOT NULL,
        [Descripcion] nvarchar(200) NOT NULL,
        [EsAutomatico] bit NOT NULL DEFAULT CAST(0 AS bit),
        [RolResponsable] nvarchar(50) NULL,
        [EmpresaId] int NULL,
        [FechaCreacion] datetime2 NOT NULL,
        [CreadoPor] nvarchar(120) NULL,
        [FechaModificacion] datetime2 NULL,
        [ModificadoPor] nvarchar(120) NULL,
        [Activo] bit NOT NULL,
        CONSTRAINT [PK_PlantillasTareas] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PlantillasTareas_Empresas_EmpresaId] FOREIGN KEY ([EmpresaId]) REFERENCES [core].[Empresas] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260311205703_AddWorkflowPlantillasSeed'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Activo', N'Codigo', N'CreadoPor', N'Descripcion', N'EmpresaId', N'EntityType', N'EsAutomatico', N'FechaCreacion', N'FechaModificacion', N'ModificadoPor', N'Orden', N'RolResponsable', N'SubTipo') AND [object_id] = OBJECT_ID(N'[wf].[PlantillasTareas]'))
        SET IDENTITY_INSERT [wf].[PlantillasTareas] ON;
    EXEC(N'INSERT INTO [wf].[PlantillasTareas] ([Id], [Activo], [Codigo], [CreadoPor], [Descripcion], [EmpresaId], [EntityType], [EsAutomatico], [FechaCreacion], [FechaModificacion], [ModificadoPor], [Orden], [RolResponsable], [SubTipo])
    VALUES (1, CAST(1 AS bit), N''OP-CREADA'', NULL, N''Orden de pedido creada en el sistema'', NULL, N''OrdenPedido'', CAST(1 AS bit), ''2025-01-01T00:00:00.0000000Z'', NULL, NULL, 1, NULL, N''IMPORTACION'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Activo', N'Codigo', N'CreadoPor', N'Descripcion', N'EmpresaId', N'EntityType', N'EsAutomatico', N'FechaCreacion', N'FechaModificacion', N'ModificadoPor', N'Orden', N'RolResponsable', N'SubTipo') AND [object_id] = OBJECT_ID(N'[wf].[PlantillasTareas]'))
        SET IDENTITY_INSERT [wf].[PlantillasTareas] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260311205703_AddWorkflowPlantillasSeed'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Activo', N'Codigo', N'CreadoPor', N'Descripcion', N'EmpresaId', N'EntityType', N'FechaCreacion', N'FechaModificacion', N'ModificadoPor', N'Orden', N'RolResponsable', N'SubTipo') AND [object_id] = OBJECT_ID(N'[wf].[PlantillasTareas]'))
        SET IDENTITY_INSERT [wf].[PlantillasTareas] ON;
    EXEC(N'INSERT INTO [wf].[PlantillasTareas] ([Id], [Activo], [Codigo], [CreadoPor], [Descripcion], [EmpresaId], [EntityType], [FechaCreacion], [FechaModificacion], [ModificadoPor], [Orden], [RolResponsable], [SubTipo])
    VALUES (2, CAST(1 AS bit), N''OC-EMITIDA'', NULL, N''Orden de compra emitida al proveedor'', NULL, N''OrdenPedido'', ''2025-01-01T00:00:00.0000000Z'', NULL, NULL, 2, N''COMPRAS'', N''IMPORTACION''),
    (3, CAST(1 AS bit), N''ETD-CONF'', NULL, N''ETD confirmado por proveedor / naviera'', NULL, N''OrdenPedido'', ''2025-01-01T00:00:00.0000000Z'', NULL, NULL, 3, N''COMPRAS'', N''IMPORTACION''),
    (4, CAST(1 AS bit), N''EMBARQUE'', NULL, N''Embarque despachado (BL / AWB emitido)'', NULL, N''OrdenPedido'', ''2025-01-01T00:00:00.0000000Z'', NULL, NULL, 4, N''COMPRAS'', N''IMPORTACION''),
    (5, CAST(1 AS bit), N''ETA-CONF'', NULL, N''ETA confirmado por forwarder / naviera'', NULL, N''OrdenPedido'', ''2025-01-01T00:00:00.0000000Z'', NULL, NULL, 5, N''COMPRAS'', N''IMPORTACION''),
    (6, CAST(1 AS bit), N''EN-ADUANA'', NULL, N''Documentos presentados ante aduana'', NULL, N''OrdenPedido'', ''2025-01-01T00:00:00.0000000Z'', NULL, NULL, 6, N''AGENTE_ADUANA'', N''IMPORTACION''),
    (7, CAST(1 AS bit), N''AFORO'', NULL, N''Aforo / inspección aduanera realizada'', NULL, N''OrdenPedido'', ''2025-01-01T00:00:00.0000000Z'', NULL, NULL, 7, N''AGENTE_ADUANA'', N''IMPORTACION''),
    (8, CAST(1 AS bit), N''DUI'', NULL, N''DUI / DIM registrado y aprobado'', NULL, N''OrdenPedido'', ''2025-01-01T00:00:00.0000000Z'', NULL, NULL, 8, N''AGENTE_ADUANA'', N''IMPORTACION''),
    (9, CAST(1 AS bit), N''LEVANTE'', NULL, N''Levante de mercancía autorizado'', NULL, N''OrdenPedido'', ''2025-01-01T00:00:00.0000000Z'', NULL, NULL, 9, N''AGENTE_ADUANA'', N''IMPORTACION''),
    (10, CAST(1 AS bit), N''TRANSPORTE'', NULL, N''Transporte interno hacia almacén destino'', NULL, N''OrdenPedido'', ''2025-01-01T00:00:00.0000000Z'', NULL, NULL, 10, N''LOGISTICA'', N''IMPORTACION''),
    (11, CAST(1 AS bit), N''RECEPCION'', NULL, N''Recepción física confirmada en almacén'', NULL, N''OrdenPedido'', ''2025-01-01T00:00:00.0000000Z'', NULL, NULL, 11, N''ALMACEN'', N''IMPORTACION''),
    (12, CAST(1 AS bit), N''LANDED-COST'', NULL, N''Costo de importación calculado y contabilizado'', NULL, N''OrdenPedido'', ''2025-01-01T00:00:00.0000000Z'', NULL, NULL, 12, N''FINANZAS'', N''IMPORTACION'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Activo', N'Codigo', N'CreadoPor', N'Descripcion', N'EmpresaId', N'EntityType', N'FechaCreacion', N'FechaModificacion', N'ModificadoPor', N'Orden', N'RolResponsable', N'SubTipo') AND [object_id] = OBJECT_ID(N'[wf].[PlantillasTareas]'))
        SET IDENTITY_INSERT [wf].[PlantillasTareas] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260311205703_AddWorkflowPlantillasSeed'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Activo', N'Codigo', N'CreadoPor', N'Descripcion', N'EmpresaId', N'EntityType', N'EsAutomatico', N'FechaCreacion', N'FechaModificacion', N'ModificadoPor', N'Orden', N'RolResponsable', N'SubTipo') AND [object_id] = OBJECT_ID(N'[wf].[PlantillasTareas]'))
        SET IDENTITY_INSERT [wf].[PlantillasTareas] ON;
    EXEC(N'INSERT INTO [wf].[PlantillasTareas] ([Id], [Activo], [Codigo], [CreadoPor], [Descripcion], [EmpresaId], [EntityType], [EsAutomatico], [FechaCreacion], [FechaModificacion], [ModificadoPor], [Orden], [RolResponsable], [SubTipo])
    VALUES (13, CAST(1 AS bit), N''CIERRE'', NULL, N''Expediente de importación cerrado'', NULL, N''OrdenPedido'', CAST(1 AS bit), ''2025-01-01T00:00:00.0000000Z'', NULL, NULL, 13, NULL, N''IMPORTACION''),
    (14, CAST(1 AS bit), N''OP-CREADA'', NULL, N''Orden de pedido de traspaso creada'', NULL, N''OrdenPedido'', CAST(1 AS bit), ''2025-01-01T00:00:00.0000000Z'', NULL, NULL, 1, NULL, N''TRASPASO_INTERNO'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Activo', N'Codigo', N'CreadoPor', N'Descripcion', N'EmpresaId', N'EntityType', N'EsAutomatico', N'FechaCreacion', N'FechaModificacion', N'ModificadoPor', N'Orden', N'RolResponsable', N'SubTipo') AND [object_id] = OBJECT_ID(N'[wf].[PlantillasTareas]'))
        SET IDENTITY_INSERT [wf].[PlantillasTareas] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260311205703_AddWorkflowPlantillasSeed'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Activo', N'Codigo', N'CreadoPor', N'Descripcion', N'EmpresaId', N'EntityType', N'FechaCreacion', N'FechaModificacion', N'ModificadoPor', N'Orden', N'RolResponsable', N'SubTipo') AND [object_id] = OBJECT_ID(N'[wf].[PlantillasTareas]'))
        SET IDENTITY_INSERT [wf].[PlantillasTareas] ON;
    EXEC(N'INSERT INTO [wf].[PlantillasTareas] ([Id], [Activo], [Codigo], [CreadoPor], [Descripcion], [EmpresaId], [EntityType], [FechaCreacion], [FechaModificacion], [ModificadoPor], [Orden], [RolResponsable], [SubTipo])
    VALUES (15, CAST(1 AS bit), N''APROBACION'', NULL, N''Traspaso aprobado por supervisor'', NULL, N''OrdenPedido'', ''2025-01-01T00:00:00.0000000Z'', NULL, NULL, 2, N''SUPERVISOR'', N''TRASPASO_INTERNO''),
    (16, CAST(1 AS bit), N''PREPARACION'', NULL, N''Mercancía preparada y verificada en origen'', NULL, N''OrdenPedido'', ''2025-01-01T00:00:00.0000000Z'', NULL, NULL, 3, N''ALMACEN'', N''TRASPASO_INTERNO''),
    (17, CAST(1 AS bit), N''DESPACHO'', NULL, N''Mercancía despachada desde almacén origen'', NULL, N''OrdenPedido'', ''2025-01-01T00:00:00.0000000Z'', NULL, NULL, 4, N''LOGISTICA'', N''TRASPASO_INTERNO''),
    (18, CAST(1 AS bit), N''TRANSITO'', NULL, N''Mercancía en tránsito hacia almacén destino'', NULL, N''OrdenPedido'', ''2025-01-01T00:00:00.0000000Z'', NULL, NULL, 5, N''LOGISTICA'', N''TRASPASO_INTERNO''),
    (19, CAST(1 AS bit), N''RECEPCION'', NULL, N''Recepción confirmada en almacén destino'', NULL, N''OrdenPedido'', ''2025-01-01T00:00:00.0000000Z'', NULL, NULL, 6, N''ALMACEN'', N''TRASPASO_INTERNO''),
    (20, CAST(1 AS bit), N''CONFIRMACION'', NULL, N''Traspaso confirmado y diferencias registradas'', NULL, N''OrdenPedido'', ''2025-01-01T00:00:00.0000000Z'', NULL, NULL, 7, N''SUPERVISOR'', N''TRASPASO_INTERNO'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Activo', N'Codigo', N'CreadoPor', N'Descripcion', N'EmpresaId', N'EntityType', N'FechaCreacion', N'FechaModificacion', N'ModificadoPor', N'Orden', N'RolResponsable', N'SubTipo') AND [object_id] = OBJECT_ID(N'[wf].[PlantillasTareas]'))
        SET IDENTITY_INSERT [wf].[PlantillasTareas] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260311205703_AddWorkflowPlantillasSeed'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Activo', N'Codigo', N'CreadoPor', N'Descripcion', N'EmpresaId', N'EntityType', N'EsAutomatico', N'FechaCreacion', N'FechaModificacion', N'ModificadoPor', N'Orden', N'RolResponsable', N'SubTipo') AND [object_id] = OBJECT_ID(N'[wf].[PlantillasTareas]'))
        SET IDENTITY_INSERT [wf].[PlantillasTareas] ON;
    EXEC(N'INSERT INTO [wf].[PlantillasTareas] ([Id], [Activo], [Codigo], [CreadoPor], [Descripcion], [EmpresaId], [EntityType], [EsAutomatico], [FechaCreacion], [FechaModificacion], [ModificadoPor], [Orden], [RolResponsable], [SubTipo])
    VALUES (21, CAST(1 AS bit), N''CIERRE'', NULL, N''Traspaso cerrado y stock actualizado'', NULL, N''OrdenPedido'', CAST(1 AS bit), ''2025-01-01T00:00:00.0000000Z'', NULL, NULL, 8, NULL, N''TRASPASO_INTERNO''),
    (22, CAST(1 AS bit), N''OC-CREADA'', NULL, N''Orden de compra generada en el sistema'', NULL, N''OrdenCompra'', CAST(1 AS bit), ''2025-01-01T00:00:00.0000000Z'', NULL, NULL, 1, NULL, NULL)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Activo', N'Codigo', N'CreadoPor', N'Descripcion', N'EmpresaId', N'EntityType', N'EsAutomatico', N'FechaCreacion', N'FechaModificacion', N'ModificadoPor', N'Orden', N'RolResponsable', N'SubTipo') AND [object_id] = OBJECT_ID(N'[wf].[PlantillasTareas]'))
        SET IDENTITY_INSERT [wf].[PlantillasTareas] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260311205703_AddWorkflowPlantillasSeed'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Activo', N'Codigo', N'CreadoPor', N'Descripcion', N'EmpresaId', N'EntityType', N'FechaCreacion', N'FechaModificacion', N'ModificadoPor', N'Orden', N'RolResponsable', N'SubTipo') AND [object_id] = OBJECT_ID(N'[wf].[PlantillasTareas]'))
        SET IDENTITY_INSERT [wf].[PlantillasTareas] ON;
    EXEC(N'INSERT INTO [wf].[PlantillasTareas] ([Id], [Activo], [Codigo], [CreadoPor], [Descripcion], [EmpresaId], [EntityType], [FechaCreacion], [FechaModificacion], [ModificadoPor], [Orden], [RolResponsable], [SubTipo])
    VALUES (23, CAST(1 AS bit), N''APROBACION-L1'', NULL, N''Aprobación de primer nivel (supervisor)'', NULL, N''OrdenCompra'', ''2025-01-01T00:00:00.0000000Z'', NULL, NULL, 2, N''SUPERVISOR'', NULL),
    (24, CAST(1 AS bit), N''APROBACION-L2'', NULL, N''Aprobación de segundo nivel (gerencia)'', NULL, N''OrdenCompra'', ''2025-01-01T00:00:00.0000000Z'', NULL, NULL, 3, N''GERENCIA'', NULL),
    (25, CAST(1 AS bit), N''ENVIADA-PROVEEDOR'', NULL, N''Orden enviada al proveedor'', NULL, N''OrdenCompra'', ''2025-01-01T00:00:00.0000000Z'', NULL, NULL, 4, N''COMPRAS'', NULL),
    (26, CAST(1 AS bit), N''CONF-PROVEEDOR'', NULL, N''Orden confirmada por el proveedor'', NULL, N''OrdenCompra'', ''2025-01-01T00:00:00.0000000Z'', NULL, NULL, 5, N''COMPRAS'', NULL),
    (27, CAST(1 AS bit), N''PRODUCCION'', NULL, N''Mercancía en producción / preparación'', NULL, N''OrdenCompra'', ''2025-01-01T00:00:00.0000000Z'', NULL, NULL, 6, N''COMPRAS'', NULL),
    (28, CAST(1 AS bit), N''ETD'', NULL, N''Fecha estimada de despacho confirmada'', NULL, N''OrdenCompra'', ''2025-01-01T00:00:00.0000000Z'', NULL, NULL, 7, N''COMPRAS'', NULL),
    (29, CAST(1 AS bit), N''EMBARQUE'', NULL, N''Embarque efectuado (BL / AWB recibido)'', NULL, N''OrdenCompra'', ''2025-01-01T00:00:00.0000000Z'', NULL, NULL, 8, N''COMPRAS'', NULL),
    (30, CAST(1 AS bit), N''RECEPCION'', NULL, N''Mercancía recepcionada en almacén'', NULL, N''OrdenCompra'', ''2025-01-01T00:00:00.0000000Z'', NULL, NULL, 9, N''ALMACEN'', NULL),
    (31, CAST(1 AS bit), N''PAGADA'', NULL, N''Pago al proveedor registrado y confirmado'', NULL, N''OrdenCompra'', ''2025-01-01T00:00:00.0000000Z'', NULL, NULL, 10, N''FINANZAS'', NULL)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Activo', N'Codigo', N'CreadoPor', N'Descripcion', N'EmpresaId', N'EntityType', N'FechaCreacion', N'FechaModificacion', N'ModificadoPor', N'Orden', N'RolResponsable', N'SubTipo') AND [object_id] = OBJECT_ID(N'[wf].[PlantillasTareas]'))
        SET IDENTITY_INSERT [wf].[PlantillasTareas] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260311205703_AddWorkflowPlantillasSeed'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Activo', N'Codigo', N'CreadoPor', N'Descripcion', N'EmpresaId', N'EntityType', N'EsAutomatico', N'FechaCreacion', N'FechaModificacion', N'ModificadoPor', N'Orden', N'RolResponsable', N'SubTipo') AND [object_id] = OBJECT_ID(N'[wf].[PlantillasTareas]'))
        SET IDENTITY_INSERT [wf].[PlantillasTareas] ON;
    EXEC(N'INSERT INTO [wf].[PlantillasTareas] ([Id], [Activo], [Codigo], [CreadoPor], [Descripcion], [EmpresaId], [EntityType], [EsAutomatico], [FechaCreacion], [FechaModificacion], [ModificadoPor], [Orden], [RolResponsable], [SubTipo])
    VALUES (32, CAST(1 AS bit), N''OV-CREADA'', NULL, N''Orden de venta registrada en el sistema'', NULL, N''OrdenVenta'', CAST(1 AS bit), ''2025-01-01T00:00:00.0000000Z'', NULL, NULL, 1, NULL, NULL)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Activo', N'Codigo', N'CreadoPor', N'Descripcion', N'EmpresaId', N'EntityType', N'EsAutomatico', N'FechaCreacion', N'FechaModificacion', N'ModificadoPor', N'Orden', N'RolResponsable', N'SubTipo') AND [object_id] = OBJECT_ID(N'[wf].[PlantillasTareas]'))
        SET IDENTITY_INSERT [wf].[PlantillasTareas] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260311205703_AddWorkflowPlantillasSeed'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Activo', N'Codigo', N'CreadoPor', N'Descripcion', N'EmpresaId', N'EntityType', N'FechaCreacion', N'FechaModificacion', N'ModificadoPor', N'Orden', N'RolResponsable', N'SubTipo') AND [object_id] = OBJECT_ID(N'[wf].[PlantillasTareas]'))
        SET IDENTITY_INSERT [wf].[PlantillasTareas] ON;
    EXEC(N'INSERT INTO [wf].[PlantillasTareas] ([Id], [Activo], [Codigo], [CreadoPor], [Descripcion], [EmpresaId], [EntityType], [FechaCreacion], [FechaModificacion], [ModificadoPor], [Orden], [RolResponsable], [SubTipo])
    VALUES (33, CAST(1 AS bit), N''APROBACION'', NULL, N''Orden de venta aprobada por supervisor'', NULL, N''OrdenVenta'', ''2025-01-01T00:00:00.0000000Z'', NULL, NULL, 2, N''SUPERVISOR'', NULL),
    (34, CAST(1 AS bit), N''RESERVA-STOCK'', NULL, N''Stock reservado para la orden de venta'', NULL, N''OrdenVenta'', ''2025-01-01T00:00:00.0000000Z'', NULL, NULL, 3, N''ALMACEN'', NULL),
    (35, CAST(1 AS bit), N''PREPARACION'', NULL, N''Pedido preparado y empacado en almacén'', NULL, N''OrdenVenta'', ''2025-01-01T00:00:00.0000000Z'', NULL, NULL, 4, N''ALMACEN'', NULL),
    (36, CAST(1 AS bit), N''FACTURADA'', NULL, N''Factura emitida y registrada en contabilidad'', NULL, N''OrdenVenta'', ''2025-01-01T00:00:00.0000000Z'', NULL, NULL, 5, N''FINANZAS'', NULL),
    (37, CAST(1 AS bit), N''DESPACHO'', NULL, N''Mercancía despachada hacia el cliente'', NULL, N''OrdenVenta'', ''2025-01-01T00:00:00.0000000Z'', NULL, NULL, 6, N''LOGISTICA'', NULL),
    (38, CAST(1 AS bit), N''ENTREGADA'', NULL, N''Entrega al cliente confirmada con firma'', NULL, N''OrdenVenta'', ''2025-01-01T00:00:00.0000000Z'', NULL, NULL, 7, N''LOGISTICA'', NULL),
    (39, CAST(1 AS bit), N''COBRADA'', NULL, N''Pago del cliente recibido y conciliado'', NULL, N''OrdenVenta'', ''2025-01-01T00:00:00.0000000Z'', NULL, NULL, 8, N''FINANZAS'', NULL)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Activo', N'Codigo', N'CreadoPor', N'Descripcion', N'EmpresaId', N'EntityType', N'FechaCreacion', N'FechaModificacion', N'ModificadoPor', N'Orden', N'RolResponsable', N'SubTipo') AND [object_id] = OBJECT_ID(N'[wf].[PlantillasTareas]'))
        SET IDENTITY_INSERT [wf].[PlantillasTareas] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260311205703_AddWorkflowPlantillasSeed'
)
BEGIN
    CREATE INDEX [IX_Plantillas_Query] ON [wf].[PlantillasTareas] ([EntityType], [SubTipo], [EmpresaId], [Orden]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260311205703_AddWorkflowPlantillasSeed'
)
BEGIN
    CREATE INDEX [IX_PlantillasTareas_EmpresaId] ON [wf].[PlantillasTareas] ([EmpresaId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260311205703_AddWorkflowPlantillasSeed'
)
BEGIN
    INSERT INTO [core].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260311205703_AddWorkflowPlantillasSeed', N'8.0.12');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260312001834_AddWorkflowHojaRutaPlantillas'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Activo', N'Codigo', N'CreadoPor', N'Descripcion', N'EmpresaId', N'EntityType', N'EsAutomatico', N'FechaCreacion', N'FechaModificacion', N'ModificadoPor', N'Orden', N'RolResponsable', N'SubTipo') AND [object_id] = OBJECT_ID(N'[wf].[PlantillasTareas]'))
        SET IDENTITY_INSERT [wf].[PlantillasTareas] ON;
    EXEC(N'INSERT INTO [wf].[PlantillasTareas] ([Id], [Activo], [Codigo], [CreadoPor], [Descripcion], [EmpresaId], [EntityType], [EsAutomatico], [FechaCreacion], [FechaModificacion], [ModificadoPor], [Orden], [RolResponsable], [SubTipo])
    VALUES (40, CAST(1 AS bit), N''HR-CREADA'', NULL, N''Hoja de ruta creada en el sistema'', NULL, N''HojaRuta'', CAST(1 AS bit), ''2025-01-01T00:00:00.0000000Z'', NULL, NULL, 1, NULL, N''IMPORTACION'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Activo', N'Codigo', N'CreadoPor', N'Descripcion', N'EmpresaId', N'EntityType', N'EsAutomatico', N'FechaCreacion', N'FechaModificacion', N'ModificadoPor', N'Orden', N'RolResponsable', N'SubTipo') AND [object_id] = OBJECT_ID(N'[wf].[PlantillasTareas]'))
        SET IDENTITY_INSERT [wf].[PlantillasTareas] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260312001834_AddWorkflowHojaRutaPlantillas'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Activo', N'Codigo', N'CreadoPor', N'Descripcion', N'EmpresaId', N'EntityType', N'FechaCreacion', N'FechaModificacion', N'ModificadoPor', N'Orden', N'RolResponsable', N'SubTipo') AND [object_id] = OBJECT_ID(N'[wf].[PlantillasTareas]'))
        SET IDENTITY_INSERT [wf].[PlantillasTareas] ON;
    EXEC(N'INSERT INTO [wf].[PlantillasTareas] ([Id], [Activo], [Codigo], [CreadoPor], [Descripcion], [EmpresaId], [EntityType], [FechaCreacion], [FechaModificacion], [ModificadoPor], [Orden], [RolResponsable], [SubTipo])
    VALUES (41, CAST(1 AS bit), N''DESPACHO-EXT'', NULL, N''Despacho en origen confirmado'', NULL, N''HojaRuta'', ''2025-01-01T00:00:00.0000000Z'', NULL, NULL, 2, N''COMPRAS'', N''IMPORTACION''),
    (42, CAST(1 AS bit), N''EN-TRANSITO'', NULL, N''Carga en tránsito internacional'', NULL, N''HojaRuta'', ''2025-01-01T00:00:00.0000000Z'', NULL, NULL, 3, N''COMPRAS'', N''IMPORTACION''),
    (43, CAST(1 AS bit), N''ADUANA'', NULL, N''Trámite aduanero iniciado'', NULL, N''HojaRuta'', ''2025-01-01T00:00:00.0000000Z'', NULL, NULL, 4, N''AGENTE_ADUANA'', N''IMPORTACION''),
    (44, CAST(1 AS bit), N''LEVANTE'', NULL, N''Levante de aduana autorizado'', NULL, N''HojaRuta'', ''2025-01-01T00:00:00.0000000Z'', NULL, NULL, 5, N''AGENTE_ADUANA'', N''IMPORTACION''),
    (45, CAST(1 AS bit), N''TRANSPORTE'', NULL, N''Transporte hacia almacén destino'', NULL, N''HojaRuta'', ''2025-01-01T00:00:00.0000000Z'', NULL, NULL, 6, N''LOGISTICA'', N''IMPORTACION''),
    (46, CAST(1 AS bit), N''RECEPCION'', NULL, N''Recepción física en almacén confirmada'', NULL, N''HojaRuta'', ''2025-01-01T00:00:00.0000000Z'', NULL, NULL, 7, N''ALMACEN'', N''IMPORTACION'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Activo', N'Codigo', N'CreadoPor', N'Descripcion', N'EmpresaId', N'EntityType', N'FechaCreacion', N'FechaModificacion', N'ModificadoPor', N'Orden', N'RolResponsable', N'SubTipo') AND [object_id] = OBJECT_ID(N'[wf].[PlantillasTareas]'))
        SET IDENTITY_INSERT [wf].[PlantillasTareas] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260312001834_AddWorkflowHojaRutaPlantillas'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Activo', N'Codigo', N'CreadoPor', N'Descripcion', N'EmpresaId', N'EntityType', N'EsAutomatico', N'FechaCreacion', N'FechaModificacion', N'ModificadoPor', N'Orden', N'RolResponsable', N'SubTipo') AND [object_id] = OBJECT_ID(N'[wf].[PlantillasTareas]'))
        SET IDENTITY_INSERT [wf].[PlantillasTareas] ON;
    EXEC(N'INSERT INTO [wf].[PlantillasTareas] ([Id], [Activo], [Codigo], [CreadoPor], [Descripcion], [EmpresaId], [EntityType], [EsAutomatico], [FechaCreacion], [FechaModificacion], [ModificadoPor], [Orden], [RolResponsable], [SubTipo])
    VALUES (47, CAST(1 AS bit), N''CIERRE-HR'', NULL, N''Hoja de ruta cerrada'', NULL, N''HojaRuta'', CAST(1 AS bit), ''2025-01-01T00:00:00.0000000Z'', NULL, NULL, 8, NULL, N''IMPORTACION''),
    (48, CAST(1 AS bit), N''HR-CREADA'', NULL, N''Hoja de ruta de traspaso creada'', NULL, N''HojaRuta'', CAST(1 AS bit), ''2025-01-01T00:00:00.0000000Z'', NULL, NULL, 1, NULL, N''TRASPASO_INTERNO'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Activo', N'Codigo', N'CreadoPor', N'Descripcion', N'EmpresaId', N'EntityType', N'EsAutomatico', N'FechaCreacion', N'FechaModificacion', N'ModificadoPor', N'Orden', N'RolResponsable', N'SubTipo') AND [object_id] = OBJECT_ID(N'[wf].[PlantillasTareas]'))
        SET IDENTITY_INSERT [wf].[PlantillasTareas] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260312001834_AddWorkflowHojaRutaPlantillas'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Activo', N'Codigo', N'CreadoPor', N'Descripcion', N'EmpresaId', N'EntityType', N'FechaCreacion', N'FechaModificacion', N'ModificadoPor', N'Orden', N'RolResponsable', N'SubTipo') AND [object_id] = OBJECT_ID(N'[wf].[PlantillasTareas]'))
        SET IDENTITY_INSERT [wf].[PlantillasTareas] ON;
    EXEC(N'INSERT INTO [wf].[PlantillasTareas] ([Id], [Activo], [Codigo], [CreadoPor], [Descripcion], [EmpresaId], [EntityType], [FechaCreacion], [FechaModificacion], [ModificadoPor], [Orden], [RolResponsable], [SubTipo])
    VALUES (49, CAST(1 AS bit), N''PREPARACION'', NULL, N''Preparación de mercancía en almacén origen'', NULL, N''HojaRuta'', ''2025-01-01T00:00:00.0000000Z'', NULL, NULL, 2, N''ALMACEN'', N''TRASPASO_INTERNO''),
    (50, CAST(1 AS bit), N''DESPACHO'', NULL, N''Despacho desde almacén origen'', NULL, N''HojaRuta'', ''2025-01-01T00:00:00.0000000Z'', NULL, NULL, 3, N''LOGISTICA'', N''TRASPASO_INTERNO''),
    (51, CAST(1 AS bit), N''RECEPCION'', NULL, N''Recepción en almacén destino'', NULL, N''HojaRuta'', ''2025-01-01T00:00:00.0000000Z'', NULL, NULL, 4, N''ALMACEN'', N''TRASPASO_INTERNO'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Activo', N'Codigo', N'CreadoPor', N'Descripcion', N'EmpresaId', N'EntityType', N'FechaCreacion', N'FechaModificacion', N'ModificadoPor', N'Orden', N'RolResponsable', N'SubTipo') AND [object_id] = OBJECT_ID(N'[wf].[PlantillasTareas]'))
        SET IDENTITY_INSERT [wf].[PlantillasTareas] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260312001834_AddWorkflowHojaRutaPlantillas'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Activo', N'Codigo', N'CreadoPor', N'Descripcion', N'EmpresaId', N'EntityType', N'EsAutomatico', N'FechaCreacion', N'FechaModificacion', N'ModificadoPor', N'Orden', N'RolResponsable', N'SubTipo') AND [object_id] = OBJECT_ID(N'[wf].[PlantillasTareas]'))
        SET IDENTITY_INSERT [wf].[PlantillasTareas] ON;
    EXEC(N'INSERT INTO [wf].[PlantillasTareas] ([Id], [Activo], [Codigo], [CreadoPor], [Descripcion], [EmpresaId], [EntityType], [EsAutomatico], [FechaCreacion], [FechaModificacion], [ModificadoPor], [Orden], [RolResponsable], [SubTipo])
    VALUES (52, CAST(1 AS bit), N''CIERRE-HR'', NULL, N''Hoja de ruta de traspaso cerrada'', NULL, N''HojaRuta'', CAST(1 AS bit), ''2025-01-01T00:00:00.0000000Z'', NULL, NULL, 5, NULL, N''TRASPASO_INTERNO''),
    (53, CAST(1 AS bit), N''HR-CREADA'', NULL, N''Hoja de ruta de entrega creada'', NULL, N''HojaRuta'', CAST(1 AS bit), ''2025-01-01T00:00:00.0000000Z'', NULL, NULL, 1, NULL, N''ENTREGA'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Activo', N'Codigo', N'CreadoPor', N'Descripcion', N'EmpresaId', N'EntityType', N'EsAutomatico', N'FechaCreacion', N'FechaModificacion', N'ModificadoPor', N'Orden', N'RolResponsable', N'SubTipo') AND [object_id] = OBJECT_ID(N'[wf].[PlantillasTareas]'))
        SET IDENTITY_INSERT [wf].[PlantillasTareas] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260312001834_AddWorkflowHojaRutaPlantillas'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Activo', N'Codigo', N'CreadoPor', N'Descripcion', N'EmpresaId', N'EntityType', N'FechaCreacion', N'FechaModificacion', N'ModificadoPor', N'Orden', N'RolResponsable', N'SubTipo') AND [object_id] = OBJECT_ID(N'[wf].[PlantillasTareas]'))
        SET IDENTITY_INSERT [wf].[PlantillasTareas] ON;
    EXEC(N'INSERT INTO [wf].[PlantillasTareas] ([Id], [Activo], [Codigo], [CreadoPor], [Descripcion], [EmpresaId], [EntityType], [FechaCreacion], [FechaModificacion], [ModificadoPor], [Orden], [RolResponsable], [SubTipo])
    VALUES (54, CAST(1 AS bit), N''PREPARACION'', NULL, N''Pedido preparado para entrega al cliente'', NULL, N''HojaRuta'', ''2025-01-01T00:00:00.0000000Z'', NULL, NULL, 2, N''ALMACEN'', N''ENTREGA''),
    (55, CAST(1 AS bit), N''DESPACHO'', NULL, N''Vehículo de reparto despachado'', NULL, N''HojaRuta'', ''2025-01-01T00:00:00.0000000Z'', NULL, NULL, 3, N''LOGISTICA'', N''ENTREGA''),
    (56, CAST(1 AS bit), N''ENTREGADA'', NULL, N''Entrega al cliente confirmada'', NULL, N''HojaRuta'', ''2025-01-01T00:00:00.0000000Z'', NULL, NULL, 4, N''LOGISTICA'', N''ENTREGA'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Activo', N'Codigo', N'CreadoPor', N'Descripcion', N'EmpresaId', N'EntityType', N'FechaCreacion', N'FechaModificacion', N'ModificadoPor', N'Orden', N'RolResponsable', N'SubTipo') AND [object_id] = OBJECT_ID(N'[wf].[PlantillasTareas]'))
        SET IDENTITY_INSERT [wf].[PlantillasTareas] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260312001834_AddWorkflowHojaRutaPlantillas'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Activo', N'Codigo', N'CreadoPor', N'Descripcion', N'EmpresaId', N'EntityType', N'EsAutomatico', N'FechaCreacion', N'FechaModificacion', N'ModificadoPor', N'Orden', N'RolResponsable', N'SubTipo') AND [object_id] = OBJECT_ID(N'[wf].[PlantillasTareas]'))
        SET IDENTITY_INSERT [wf].[PlantillasTareas] ON;
    EXEC(N'INSERT INTO [wf].[PlantillasTareas] ([Id], [Activo], [Codigo], [CreadoPor], [Descripcion], [EmpresaId], [EntityType], [EsAutomatico], [FechaCreacion], [FechaModificacion], [ModificadoPor], [Orden], [RolResponsable], [SubTipo])
    VALUES (57, CAST(1 AS bit), N''CIERRE-HR'', NULL, N''Hoja de ruta cerrada'', NULL, N''HojaRuta'', CAST(1 AS bit), ''2025-01-01T00:00:00.0000000Z'', NULL, NULL, 5, NULL, N''ENTREGA'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Activo', N'Codigo', N'CreadoPor', N'Descripcion', N'EmpresaId', N'EntityType', N'EsAutomatico', N'FechaCreacion', N'FechaModificacion', N'ModificadoPor', N'Orden', N'RolResponsable', N'SubTipo') AND [object_id] = OBJECT_ID(N'[wf].[PlantillasTareas]'))
        SET IDENTITY_INSERT [wf].[PlantillasTareas] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260312001834_AddWorkflowHojaRutaPlantillas'
)
BEGIN
    INSERT INTO [core].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260312001834_AddWorkflowHojaRutaPlantillas', N'8.0.12');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260312014903_LOG_HojaRuta_OrdenPedidoId'
)
BEGIN
    ALTER TABLE [log].[HojasRuta] ADD [OrdenPedidoId] bigint NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260312014903_LOG_HojaRuta_OrdenPedidoId'
)
BEGIN
    CREATE INDEX [IX_HojasRuta_OrdenPedidoId] ON [log].[HojasRuta] ([EmpresaId], [OrdenPedidoId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260312014903_LOG_HojaRuta_OrdenPedidoId'
)
BEGIN
    INSERT INTO [core].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260312014903_LOG_HojaRuta_OrdenPedidoId', N'8.0.12');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260312030743_SeedOrdenPedidoPlantillasGenericas'
)
BEGIN

                    IF NOT EXISTS (SELECT 1 FROM [wf].[PlantillasTareas] WHERE [EntityType] = 'OrdenPedido' AND [SubTipo] IS NULL)
                    BEGIN
                        INSERT INTO [wf].[PlantillasTareas]
                            ([EntityType],[SubTipo],[Orden],[Codigo],[Descripcion],[EsAutomatico],[RolResponsable],[EmpresaId],[Activo],[FechaCreacion],[CreadoPor],[FechaModificacion],[ModificadoPor])
                        VALUES
                            ('OrdenPedido', NULL, 1, 'OP-CREADA',   'Pedido interno creado en el sistema',          1, NULL,       NULL, 1, '2025-01-01', NULL, NULL, NULL),
                            ('OrdenPedido', NULL, 2, 'REVISION',    'Revisión de disponibilidad de stock',          0, 'ALMACEN',  NULL, 1, '2025-01-01', NULL, NULL, NULL),
                            ('OrdenPedido', NULL, 3, 'APROBACION',  'Aprobación del pedido por el responsable',     0, 'GERENCIA', NULL, 1, '2025-01-01', NULL, NULL, NULL),
                            ('OrdenPedido', NULL, 4, 'OC-GENERADA', 'Orden de compra generada a partir del pedido', 0, 'COMPRAS',  NULL, 1, '2025-01-01', NULL, NULL, NULL),
                            ('OrdenPedido', NULL, 5, 'RECEPCION',   'Mercancía recepcionada en almacén destino',    0, 'ALMACEN',  NULL, 1, '2025-01-01', NULL, NULL, NULL),
                            ('OrdenPedido', NULL, 6, 'CIERRE',      'Pedido cerrado y stock actualizado',           1, NULL,       NULL, 1, '2025-01-01', NULL, NULL, NULL)
                    END
                
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260312030743_SeedOrdenPedidoPlantillasGenericas'
)
BEGIN
    INSERT INTO [core].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260312030743_SeedOrdenPedidoPlantillasGenericas', N'8.0.12');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260409134456_ACC_CuentaContable_ClasificacionFlujo'
)
BEGIN
    ALTER TABLE [acc].[CuentasContables] ADD [ClasificacionFlujo] int NOT NULL DEFAULT 0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260409134456_ACC_CuentaContable_ClasificacionFlujo'
)
BEGIN
    INSERT INTO [core].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260409134456_ACC_CuentaContable_ClasificacionFlujo', N'8.0.12');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410232130_AddCierreContable'
)
BEGIN
    CREATE TABLE [ACC_CierresContables] (
        [Id] uniqueidentifier NOT NULL,
        [Gestion] int NOT NULL,
        [FechaCierre] datetime2 NOT NULL,
        [Estado] nvarchar(20) NOT NULL,
        [Observaciones] nvarchar(500) NOT NULL,
        [FechaCreacion] datetime2 NOT NULL,
        [CreadoPor] nvarchar(max) NULL,
        [FechaModificacion] datetime2 NULL,
        [ModificadoPor] nvarchar(max) NULL,
        [Activo] bit NOT NULL,
        [EmpresaId] int NOT NULL,
        CONSTRAINT [PK_ACC_CierresContables] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410232130_AddCierreContable'
)
BEGIN
    CREATE INDEX [IX_ACC_CierresContables_EmpresaId] ON [ACC_CierresContables] ([EmpresaId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410232130_AddCierreContable'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ACC_CierresContables_EmpresaId_Gestion] ON [ACC_CierresContables] ([EmpresaId], [Gestion]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410232130_AddCierreContable'
)
BEGIN
    INSERT INTO [core].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260410232130_AddCierreContable', N'8.0.12');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260412051006_20260312000000_TRB_RegistrosImpuesto'
)
BEGIN
    IF SCHEMA_ID(N'trb') IS NULL EXEC(N'CREATE SCHEMA [trb];');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260412051006_20260312000000_TRB_RegistrosImpuesto'
)
BEGIN
    CREATE TABLE [trb].[RegistrosImpuesto] (
        [RegistroImpuestoId] bigint NOT NULL IDENTITY,
        [PeriodoContableId] int NOT NULL,
        [TipoImpuesto] nvarchar(20) NOT NULL,
        [BaseImponible] decimal(18,2) NOT NULL,
        [Tasa] decimal(18,2) NOT NULL,
        [MontoCalculado] decimal(18,2) NOT NULL,
        [CreditoFiscal] decimal(18,2) NOT NULL,
        [DebitoFiscal] decimal(18,2) NOT NULL,
        [SaldoAFavor] decimal(18,2) NOT NULL,
        [MontoAPagar] decimal(18,2) NOT NULL,
        [Estado] nvarchar(20) NOT NULL,
        [NumeroCertificado] nvarchar(100) NULL,
        [FechaDeclaracion] datetime2 NULL,
        [AsientoContableId] bigint NULL,
        [FechaCreacion] datetime2 NOT NULL,
        [CreadoPor] nvarchar(100) NULL,
        [FechaModificacion] datetime2 NULL,
        [ModificadoPor] nvarchar(100) NULL,
        [Activo] bit NOT NULL,
        [EmpresaId] int NOT NULL,
        CONSTRAINT [PK_RegistrosImpuesto] PRIMARY KEY ([RegistroImpuestoId]),
        CONSTRAINT [FK_RegistrosImpuesto_AsientosContables_AsientoContableId] FOREIGN KEY ([AsientoContableId]) REFERENCES [acc].[AsientosContables] ([AsientoContableId]) ON DELETE NO ACTION,
        CONSTRAINT [FK_RegistrosImpuesto_PeriodosContables_PeriodoContableId] FOREIGN KEY ([PeriodoContableId]) REFERENCES [acc].[PeriodosContables] ([PeriodoContableId]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260412051006_20260312000000_TRB_RegistrosImpuesto'
)
BEGIN
    CREATE INDEX [IX_RegistrosImpuesto_Activo] ON [trb].[RegistrosImpuesto] ([Activo]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260412051006_20260312000000_TRB_RegistrosImpuesto'
)
BEGIN
    CREATE INDEX [IX_RegistrosImpuesto_AsientoContableId] ON [trb].[RegistrosImpuesto] ([AsientoContableId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260412051006_20260312000000_TRB_RegistrosImpuesto'
)
BEGIN
    CREATE INDEX [IX_RegistrosImpuesto_EmpresaId] ON [trb].[RegistrosImpuesto] ([EmpresaId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260412051006_20260312000000_TRB_RegistrosImpuesto'
)
BEGIN
    CREATE UNIQUE INDEX [IX_RegistrosImpuesto_EmpresaId_PeriodoContableId_TipoImpuesto] ON [trb].[RegistrosImpuesto] ([EmpresaId], [PeriodoContableId], [TipoImpuesto]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260412051006_20260312000000_TRB_RegistrosImpuesto'
)
BEGIN
    CREATE INDEX [IX_RegistrosImpuesto_PeriodoContableId] ON [trb].[RegistrosImpuesto] ([PeriodoContableId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260412051006_20260312000000_TRB_RegistrosImpuesto'
)
BEGIN
    INSERT INTO [core].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260412051006_20260312000000_TRB_RegistrosImpuesto', N'8.0.12');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260412052908_20260312_ACT_ActivosFijos_DepreciacionesMensuales'
)
BEGIN
    IF SCHEMA_ID(N'act') IS NULL EXEC(N'CREATE SCHEMA [act];');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260412052908_20260312_ACT_ActivosFijos_DepreciacionesMensuales'
)
BEGIN
    CREATE TABLE [act].[ActivosFijos] (
        [ActivoFijoId] bigint NOT NULL IDENTITY,
        [Codigo] nvarchar(50) NOT NULL,
        [Descripcion] nvarchar(250) NOT NULL,
        [CategoriaActivo] nvarchar(50) NOT NULL,
        [CuentaContableId] int NOT NULL,
        [CuentaDepreciacionId] int NOT NULL,
        [CuentaGastoDepreciacionId] int NOT NULL,
        [FechaAdquisicion] datetime2 NOT NULL,
        [CostoAdquisicion] decimal(18,2) NOT NULL,
        [ValorResidual] decimal(18,2) NOT NULL,
        [TasaAnualDS24051] decimal(9,4) NOT NULL,
        [VidaUtilAnios] int NOT NULL,
        [DepreciacionAcumulada] decimal(18,2) NOT NULL,
        [DepreciacionCompleta] bit NOT NULL,
        [Estado] nvarchar(20) NOT NULL,
        [FechaCreacion] datetime2 NOT NULL,
        [CreadoPor] nvarchar(100) NULL,
        [FechaModificacion] datetime2 NULL,
        [ModificadoPor] nvarchar(100) NULL,
        [Activo] bit NOT NULL,
        [EmpresaId] int NOT NULL,
        CONSTRAINT [PK_ActivosFijos] PRIMARY KEY ([ActivoFijoId]),
        CONSTRAINT [FK_ActivosFijos_CuentasContables_CuentaContableId] FOREIGN KEY ([CuentaContableId]) REFERENCES [acc].[CuentasContables] ([CuentaContableId]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ActivosFijos_CuentasContables_CuentaDepreciacionId] FOREIGN KEY ([CuentaDepreciacionId]) REFERENCES [acc].[CuentasContables] ([CuentaContableId]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ActivosFijos_CuentasContables_CuentaGastoDepreciacionId] FOREIGN KEY ([CuentaGastoDepreciacionId]) REFERENCES [acc].[CuentasContables] ([CuentaContableId]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260412052908_20260312_ACT_ActivosFijos_DepreciacionesMensuales'
)
BEGIN
    CREATE TABLE [act].[DepreciacionesMensuales] (
        [DepreciacionMensualId] bigint NOT NULL IDENTITY,
        [ActivoFijoId] bigint NOT NULL,
        [PeriodoContableId] int NOT NULL,
        [Monto] decimal(18,2) NOT NULL,
        [Fecha] datetime2 NOT NULL,
        [AsientoContableId] bigint NULL,
        [FechaCreacion] datetime2 NOT NULL,
        [CreadoPor] nvarchar(100) NULL,
        [FechaModificacion] datetime2 NULL,
        [ModificadoPor] nvarchar(100) NULL,
        [Activo] bit NOT NULL,
        CONSTRAINT [PK_DepreciacionesMensuales] PRIMARY KEY ([DepreciacionMensualId]),
        CONSTRAINT [FK_DepreciacionesMensuales_ActivosFijos_ActivoFijoId] FOREIGN KEY ([ActivoFijoId]) REFERENCES [act].[ActivosFijos] ([ActivoFijoId]) ON DELETE CASCADE,
        CONSTRAINT [FK_DepreciacionesMensuales_AsientosContables_AsientoContableId] FOREIGN KEY ([AsientoContableId]) REFERENCES [acc].[AsientosContables] ([AsientoContableId]) ON DELETE NO ACTION,
        CONSTRAINT [FK_DepreciacionesMensuales_PeriodosContables_PeriodoContableId] FOREIGN KEY ([PeriodoContableId]) REFERENCES [acc].[PeriodosContables] ([PeriodoContableId]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260412052908_20260312_ACT_ActivosFijos_DepreciacionesMensuales'
)
BEGIN
    CREATE INDEX [IX_ActivosFijos_Activo] ON [act].[ActivosFijos] ([Activo]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260412052908_20260312_ACT_ActivosFijos_DepreciacionesMensuales'
)
BEGIN
    CREATE INDEX [IX_ActivosFijos_CuentaContableId] ON [act].[ActivosFijos] ([CuentaContableId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260412052908_20260312_ACT_ActivosFijos_DepreciacionesMensuales'
)
BEGIN
    CREATE INDEX [IX_ActivosFijos_CuentaDepreciacionId] ON [act].[ActivosFijos] ([CuentaDepreciacionId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260412052908_20260312_ACT_ActivosFijos_DepreciacionesMensuales'
)
BEGIN
    CREATE INDEX [IX_ActivosFijos_CuentaGastoDepreciacionId] ON [act].[ActivosFijos] ([CuentaGastoDepreciacionId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260412052908_20260312_ACT_ActivosFijos_DepreciacionesMensuales'
)
BEGIN
    CREATE INDEX [IX_ActivosFijos_EmpresaId] ON [act].[ActivosFijos] ([EmpresaId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260412052908_20260312_ACT_ActivosFijos_DepreciacionesMensuales'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ActivosFijos_EmpresaId_Codigo] ON [act].[ActivosFijos] ([EmpresaId], [Codigo]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260412052908_20260312_ACT_ActivosFijos_DepreciacionesMensuales'
)
BEGIN
    CREATE INDEX [IX_DepreciacionesMensuales_Activo] ON [act].[DepreciacionesMensuales] ([Activo]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260412052908_20260312_ACT_ActivosFijos_DepreciacionesMensuales'
)
BEGIN
    CREATE UNIQUE INDEX [IX_DepreciacionesMensuales_ActivoFijoId_PeriodoContableId] ON [act].[DepreciacionesMensuales] ([ActivoFijoId], [PeriodoContableId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260412052908_20260312_ACT_ActivosFijos_DepreciacionesMensuales'
)
BEGIN
    CREATE INDEX [IX_DepreciacionesMensuales_AsientoContableId] ON [act].[DepreciacionesMensuales] ([AsientoContableId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260412052908_20260312_ACT_ActivosFijos_DepreciacionesMensuales'
)
BEGIN
    CREATE INDEX [IX_DepreciacionesMensuales_PeriodoContableId] ON [act].[DepreciacionesMensuales] ([PeriodoContableId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260412052908_20260312_ACT_ActivosFijos_DepreciacionesMensuales'
)
BEGIN
    INSERT INTO [core].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260412052908_20260312_ACT_ActivosFijos_DepreciacionesMensuales', N'8.0.12');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260412054830_20260311_BNC_Extractos_ConciliacionesBancarias'
)
BEGIN
    IF SCHEMA_ID(N'bnc') IS NULL EXEC(N'CREATE SCHEMA [bnc];');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260412054830_20260311_BNC_Extractos_ConciliacionesBancarias'
)
BEGIN
    CREATE TABLE [bnc].[ConciliacionesBancarias] (
        [ConciliacionBancariaId] int NOT NULL IDENTITY,
        [CuentaContableId] int NOT NULL,
        [PeriodoContableId] int NOT NULL,
        [SaldoExtracto] decimal(18,2) NOT NULL,
        [SaldoContable] decimal(18,2) NOT NULL,
        [Estado] nvarchar(20) NOT NULL,
        [FechaCreacion] datetime2 NOT NULL,
        [CreadoPor] nvarchar(100) NULL,
        [FechaModificacion] datetime2 NULL,
        [ModificadoPor] nvarchar(100) NULL,
        [Activo] bit NOT NULL,
        [EmpresaId] int NOT NULL,
        CONSTRAINT [PK_ConciliacionesBancarias] PRIMARY KEY ([ConciliacionBancariaId]),
        CONSTRAINT [FK_ConciliacionesBancarias_CuentasContables_CuentaContableId] FOREIGN KEY ([CuentaContableId]) REFERENCES [acc].[CuentasContables] ([CuentaContableId]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ConciliacionesBancarias_PeriodosContables_PeriodoContableId] FOREIGN KEY ([PeriodoContableId]) REFERENCES [acc].[PeriodosContables] ([PeriodoContableId]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260412054830_20260311_BNC_Extractos_ConciliacionesBancarias'
)
BEGIN
    CREATE TABLE [bnc].[ExtractosBancarios] (
        [ExtractoBancarioId] bigint NOT NULL IDENTITY,
        [CuentaContableId] int NOT NULL,
        [Fecha] datetime2 NOT NULL,
        [Descripcion] nvarchar(250) NOT NULL,
        [Monto] decimal(18,2) NOT NULL,
        [NumeroReferencia] nvarchar(100) NULL,
        [Conciliado] bit NOT NULL,
        [AsientoContableLineaId] bigint NULL,
        [FechaCreacion] datetime2 NOT NULL,
        [CreadoPor] nvarchar(100) NULL,
        [FechaModificacion] datetime2 NULL,
        [ModificadoPor] nvarchar(100) NULL,
        [Activo] bit NOT NULL,
        [EmpresaId] int NOT NULL,
        CONSTRAINT [PK_ExtractosBancarios] PRIMARY KEY ([ExtractoBancarioId]),
        CONSTRAINT [FK_ExtractosBancarios_AsientoContableLineas_AsientoContableLineaId] FOREIGN KEY ([AsientoContableLineaId]) REFERENCES [acc].[AsientoContableLineas] ([AsientoContableLineaId]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ExtractosBancarios_CuentasContables_CuentaContableId] FOREIGN KEY ([CuentaContableId]) REFERENCES [acc].[CuentasContables] ([CuentaContableId]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260412054830_20260311_BNC_Extractos_ConciliacionesBancarias'
)
BEGIN
    CREATE TABLE [acc].[PresupuestosContables] (
        [PresupuestoContableId] int NOT NULL IDENTITY,
        [Gestion] int NOT NULL,
        [Nombre] nvarchar(150) NOT NULL,
        [Estado] nvarchar(20) NOT NULL,
        [Observaciones] nvarchar(500) NULL,
        [FechaCreacion] datetime2 NOT NULL,
        [CreadoPor] nvarchar(100) NULL,
        [FechaModificacion] datetime2 NULL,
        [ModificadoPor] nvarchar(100) NULL,
        [Activo] bit NOT NULL,
        [EmpresaId] int NOT NULL,
        CONSTRAINT [PK_PresupuestosContables] PRIMARY KEY ([PresupuestoContableId])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260412054830_20260311_BNC_Extractos_ConciliacionesBancarias'
)
BEGIN
    CREATE TABLE [acc].[PresupuestosContablesLineas] (
        [PresupuestoContableLineaId] int NOT NULL IDENTITY,
        [PresupuestoContableId] int NOT NULL,
        [CuentaContableId] int NOT NULL,
        [CentroCostoId] int NULL,
        [Mes] int NOT NULL,
        [MontoPresupuestado] decimal(18,2) NOT NULL,
        [FechaCreacion] datetime2 NOT NULL,
        [CreadoPor] nvarchar(100) NULL,
        [FechaModificacion] datetime2 NULL,
        [ModificadoPor] nvarchar(100) NULL,
        [Activo] bit NOT NULL,
        CONSTRAINT [PK_PresupuestosContablesLineas] PRIMARY KEY ([PresupuestoContableLineaId]),
        CONSTRAINT [FK_PresupuestosContablesLineas_CentrosCosto_CentroCostoId] FOREIGN KEY ([CentroCostoId]) REFERENCES [cst].[CentrosCosto] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_PresupuestosContablesLineas_CuentasContables_CuentaContableId] FOREIGN KEY ([CuentaContableId]) REFERENCES [acc].[CuentasContables] ([CuentaContableId]) ON DELETE NO ACTION,
        CONSTRAINT [FK_PresupuestosContablesLineas_PresupuestosContables_PresupuestoContableId] FOREIGN KEY ([PresupuestoContableId]) REFERENCES [acc].[PresupuestosContables] ([PresupuestoContableId]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260412054830_20260311_BNC_Extractos_ConciliacionesBancarias'
)
BEGIN
    CREATE INDEX [IX_ConciliacionesBancarias_Activo] ON [bnc].[ConciliacionesBancarias] ([Activo]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260412054830_20260311_BNC_Extractos_ConciliacionesBancarias'
)
BEGIN
    CREATE INDEX [IX_ConciliacionesBancarias_CuentaContableId] ON [bnc].[ConciliacionesBancarias] ([CuentaContableId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260412054830_20260311_BNC_Extractos_ConciliacionesBancarias'
)
BEGIN
    CREATE INDEX [IX_ConciliacionesBancarias_EmpresaId] ON [bnc].[ConciliacionesBancarias] ([EmpresaId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260412054830_20260311_BNC_Extractos_ConciliacionesBancarias'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ConciliacionesBancarias_EmpresaId_CuentaContableId_PeriodoContableId] ON [bnc].[ConciliacionesBancarias] ([EmpresaId], [CuentaContableId], [PeriodoContableId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260412054830_20260311_BNC_Extractos_ConciliacionesBancarias'
)
BEGIN
    CREATE INDEX [IX_ConciliacionesBancarias_PeriodoContableId] ON [bnc].[ConciliacionesBancarias] ([PeriodoContableId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260412054830_20260311_BNC_Extractos_ConciliacionesBancarias'
)
BEGIN
    CREATE INDEX [IX_ExtractosBancarios_Activo] ON [bnc].[ExtractosBancarios] ([Activo]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260412054830_20260311_BNC_Extractos_ConciliacionesBancarias'
)
BEGIN
    CREATE INDEX [IX_ExtractosBancarios_AsientoContableLineaId] ON [bnc].[ExtractosBancarios] ([AsientoContableLineaId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260412054830_20260311_BNC_Extractos_ConciliacionesBancarias'
)
BEGIN
    CREATE INDEX [IX_ExtractosBancarios_CuentaContableId] ON [bnc].[ExtractosBancarios] ([CuentaContableId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260412054830_20260311_BNC_Extractos_ConciliacionesBancarias'
)
BEGIN
    CREATE INDEX [IX_ExtractosBancarios_EmpresaId] ON [bnc].[ExtractosBancarios] ([EmpresaId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260412054830_20260311_BNC_Extractos_ConciliacionesBancarias'
)
BEGIN
    CREATE INDEX [IX_ExtractosBancarios_EmpresaId_CuentaContableId_Fecha] ON [bnc].[ExtractosBancarios] ([EmpresaId], [CuentaContableId], [Fecha]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260412054830_20260311_BNC_Extractos_ConciliacionesBancarias'
)
BEGIN
    CREATE INDEX [IX_PresupuestosContables_Activo] ON [acc].[PresupuestosContables] ([Activo]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260412054830_20260311_BNC_Extractos_ConciliacionesBancarias'
)
BEGIN
    CREATE INDEX [IX_PresupuestosContables_EmpresaId] ON [acc].[PresupuestosContables] ([EmpresaId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260412054830_20260311_BNC_Extractos_ConciliacionesBancarias'
)
BEGIN
    CREATE INDEX [IX_PresupuestosContables_EmpresaId_Gestion_Estado] ON [acc].[PresupuestosContables] ([EmpresaId], [Gestion], [Estado]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260412054830_20260311_BNC_Extractos_ConciliacionesBancarias'
)
BEGIN
    CREATE INDEX [IX_PresupuestosContablesLineas_Activo] ON [acc].[PresupuestosContablesLineas] ([Activo]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260412054830_20260311_BNC_Extractos_ConciliacionesBancarias'
)
BEGIN
    CREATE INDEX [IX_PresupuestosContablesLineas_CentroCostoId] ON [acc].[PresupuestosContablesLineas] ([CentroCostoId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260412054830_20260311_BNC_Extractos_ConciliacionesBancarias'
)
BEGIN
    CREATE INDEX [IX_PresupuestosContablesLineas_CuentaContableId] ON [acc].[PresupuestosContablesLineas] ([CuentaContableId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260412054830_20260311_BNC_Extractos_ConciliacionesBancarias'
)
BEGIN
    CREATE INDEX [IX_PresupuestosContablesLineas_PresupuestoContableId_CuentaContableId_Mes_CentroCostoId] ON [acc].[PresupuestosContablesLineas] ([PresupuestoContableId], [CuentaContableId], [Mes], [CentroCostoId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260412054830_20260311_BNC_Extractos_ConciliacionesBancarias'
)
BEGIN
    INSERT INTO [core].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260412054830_20260311_BNC_Extractos_ConciliacionesBancarias', N'8.0.12');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260414075839_RefactorNumeracionComprobantes'
)
BEGIN
    DROP INDEX [IX_AsientosContables_EmpresaId_Numero] ON [acc].[AsientosContables];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260414075839_RefactorNumeracionComprobantes'
)
BEGIN
    CREATE UNIQUE INDEX [IX_AsientosContables_EmpresaId_Gestion_Numero] ON [acc].[AsientosContables] ([EmpresaId], [Gestion], [Numero]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260414075839_RefactorNumeracionComprobantes'
)
BEGIN
    INSERT INTO [core].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260414075839_RefactorNumeracionComprobantes', N'8.0.12');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260418161211_AddSucursales'
)
BEGIN
    CREATE TABLE [core].[Sucursales] (
        [Id] int NOT NULL IDENTITY,
        [Nombre] nvarchar(200) NOT NULL,
        [Codigo] nvarchar(50) NULL,
        [Direccion] nvarchar(500) NULL,
        [Ciudad] nvarchar(100) NULL,
        [Telefono] nvarchar(50) NULL,
        [Email] nvarchar(200) NULL,
        [EsCentral] bit NOT NULL,
        [Activo] bit NOT NULL,
        [FechaCreacion] datetime2 NOT NULL,
        [CreadoPor] nvarchar(max) NULL,
        [FechaModificacion] datetime2 NULL,
        [ModificadoPor] nvarchar(max) NULL,
        [EmpresaId] int NOT NULL,
        CONSTRAINT [PK_Sucursales] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Sucursales_Empresas_EmpresaId] FOREIGN KEY ([EmpresaId]) REFERENCES [core].[Empresas] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260418161211_AddSucursales'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Sucursal_Empresa_Codigo] ON [core].[Sucursales] ([EmpresaId], [Codigo]) WHERE [Codigo] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260418161211_AddSucursales'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Sucursal_Empresa_Nombre] ON [core].[Sucursales] ([EmpresaId], [Nombre]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260418161211_AddSucursales'
)
BEGIN
    CREATE INDEX [IX_Sucursales_EmpresaId] ON [core].[Sucursales] ([EmpresaId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260418161211_AddSucursales'
)
BEGIN
    INSERT INTO [core].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260418161211_AddSucursales', N'8.0.12');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260418164648_UpdateSucursalActivo'
)
BEGIN
    INSERT INTO [core].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260418164648_UpdateSucursalActivo', N'8.0.12');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260419084725_ExpandSucursalFields'
)
BEGIN
    ALTER TABLE [core].[Sucursales] ADD [Celular] nvarchar(50) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260419084725_ExpandSucursalFields'
)
BEGIN
    ALTER TABLE [core].[Sucursales] ADD [CodigoInterno] nvarchar(50) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260419084725_ExpandSucursalFields'
)
BEGIN
    ALTER TABLE [core].[Sucursales] ADD [CodigoSucursalFiscal] nvarchar(50) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260419084725_ExpandSucursalFields'
)
BEGIN
    ALTER TABLE [core].[Sucursales] ADD [Departamento] nvarchar(100) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260419084725_ExpandSucursalFields'
)
BEGIN
    ALTER TABLE [core].[Sucursales] ADD [Descripcion] nvarchar(500) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260419084725_ExpandSucursalFields'
)
BEGIN
    ALTER TABLE [core].[Sucursales] ADD [EmailAlternativo] nvarchar(200) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260419084725_ExpandSucursalFields'
)
BEGIN
    ALTER TABLE [core].[Sucursales] ADD [Latitud] decimal(11,8) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260419084725_ExpandSucursalFields'
)
BEGIN
    ALTER TABLE [core].[Sucursales] ADD [Longitud] decimal(11,8) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260419084725_ExpandSucursalFields'
)
BEGIN
    ALTER TABLE [core].[Sucursales] ADD [ManejaAlmacen] bit NOT NULL DEFAULT CAST(1 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260419084725_ExpandSucursalFields'
)
BEGIN
    ALTER TABLE [core].[Sucursales] ADD [Observaciones] nvarchar(1000) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260419084725_ExpandSucursalFields'
)
BEGIN
    ALTER TABLE [core].[Sucursales] ADD [Pais] nvarchar(100) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260419084725_ExpandSucursalFields'
)
BEGIN
    ALTER TABLE [core].[Sucursales] ADD [PermiteCompras] bit NOT NULL DEFAULT CAST(1 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260419084725_ExpandSucursalFields'
)
BEGIN
    ALTER TABLE [core].[Sucursales] ADD [PermiteDespacho] bit NOT NULL DEFAULT CAST(1 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260419084725_ExpandSucursalFields'
)
BEGIN
    ALTER TABLE [core].[Sucursales] ADD [PermiteFacturacion] bit NOT NULL DEFAULT CAST(1 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260419084725_ExpandSucursalFields'
)
BEGIN
    ALTER TABLE [core].[Sucursales] ADD [PermiteInventario] bit NOT NULL DEFAULT CAST(1 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260419084725_ExpandSucursalFields'
)
BEGIN
    ALTER TABLE [core].[Sucursales] ADD [PermiteVentas] bit NOT NULL DEFAULT CAST(1 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260419084725_ExpandSucursalFields'
)
BEGIN
    ALTER TABLE [core].[Sucursales] ADD [PrefijoDocumental] nvarchar(20) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260419084725_ExpandSucursalFields'
)
BEGIN
    ALTER TABLE [core].[Sucursales] ADD [Provincia] nvarchar(100) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260419084725_ExpandSucursalFields'
)
BEGIN
    ALTER TABLE [core].[Sucursales] ADD [Referencia] nvarchar(500) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260419084725_ExpandSucursalFields'
)
BEGIN
    ALTER TABLE [core].[Sucursales] ADD [ResponsableCargo] nvarchar(100) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260419084725_ExpandSucursalFields'
)
BEGIN
    ALTER TABLE [core].[Sucursales] ADD [ResponsableNombre] nvarchar(200) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260419084725_ExpandSucursalFields'
)
BEGIN
    ALTER TABLE [core].[Sucursales] ADD [Sigla] nvarchar(20) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260419084725_ExpandSucursalFields'
)
BEGIN
    ALTER TABLE [core].[Sucursales] ADD [UrlMapa] nvarchar(1000) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260419084725_ExpandSucursalFields'
)
BEGIN
    ALTER TABLE [core].[Sucursales] ADD [WhatsApp] nvarchar(50) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260419084725_ExpandSucursalFields'
)
BEGIN
    ALTER TABLE [core].[Sucursales] ADD [Zona] nvarchar(100) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [core].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260419084725_ExpandSucursalFields'
)
BEGIN
    INSERT INTO [core].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260419084725_ExpandSucursalFields', N'8.0.12');
END;
GO

COMMIT;
GO

