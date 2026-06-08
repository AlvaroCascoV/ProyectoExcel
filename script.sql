/*==============================================================================
  Migration: Tajamar Charlas (schema + seed)
  Tables: ROLESCHARLASTAJAMAR, CURSOSTAJAMAR, USUARIOSTAJAMAR, CURSOSUSUARIOSTAJAMAR
  Data:   3 roles, 5 cursos, 115 usuarios, 110 inscripciones
==============================================================================*/

SET NOCOUNT ON;

-- Batch 1: drop existing objects (reverse FK order)
DECLARE @DropFkSql NVARCHAR(MAX) = N'';

;WITH target_tables AS (
    SELECT OBJECT_ID(N'dbo.CALENDARIOCURSO') AS id UNION ALL
    SELECT OBJECT_ID(N'dbo.ASISTENCIATAJAMAR') UNION ALL
    SELECT OBJECT_ID(N'dbo.CHECKINSTAJAMAR') UNION ALL
    SELECT OBJECT_ID(N'dbo.POSICIONUSUARIOSTAJAMAR') UNION ALL
    SELECT OBJECT_ID(N'dbo.DISPOSITIVOPOSICIONTAJAMAR') UNION ALL
    SELECT OBJECT_ID(N'dbo.POSICIONESTAJAMAR') UNION ALL
    SELECT OBJECT_ID(N'dbo.DISPOSITIVOSTAJAMAR') UNION ALL
    SELECT OBJECT_ID(N'dbo.CURSOSUSUARIOSTAJAMAR') UNION ALL
    SELECT OBJECT_ID(N'dbo.USUARIOSTAJAMAR') UNION ALL
    SELECT OBJECT_ID(N'dbo.CURSOSTAJAMAR') UNION ALL
    SELECT OBJECT_ID(N'dbo.ROLESCHARLASTAJAMAR')
),
fk_to_drop AS (
    SELECT fk.[name] AS fk_name,
           OBJECT_SCHEMA_NAME(fk.parent_object_id) AS parent_schema,
           OBJECT_NAME(fk.parent_object_id) AS parent_table
    FROM sys.foreign_keys fk
    WHERE fk.parent_object_id IN (SELECT id FROM target_tables WHERE id IS NOT NULL)
       OR fk.referenced_object_id IN (SELECT id FROM target_tables WHERE id IS NOT NULL)
)
SELECT @DropFkSql = @DropFkSql +
    N'IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N''' + REPLACE(fk_name, '''', '''''') + N''') ' +
    N'ALTER TABLE [' + parent_schema + N'].[' + parent_table + N'] DROP CONSTRAINT [' + fk_name + N'];' + CHAR(13) + CHAR(10)
FROM fk_to_drop;

EXEC sp_executesql @DropFkSql;

IF OBJECT_ID(N'dbo.CALENDARIOCURSO', N'U') IS NOT NULL DROP TABLE dbo.CALENDARIOCURSO;
IF OBJECT_ID(N'dbo.ASISTENCIATAJAMAR', N'U') IS NOT NULL DROP TABLE dbo.ASISTENCIATAJAMAR;
IF OBJECT_ID(N'dbo.CHECKINSTAJAMAR', N'U') IS NOT NULL DROP TABLE dbo.CHECKINSTAJAMAR;
IF OBJECT_ID(N'dbo.POSICIONUSUARIOSTAJAMAR', N'U') IS NOT NULL DROP TABLE dbo.POSICIONUSUARIOSTAJAMAR;
IF OBJECT_ID(N'dbo.DISPOSITIVOPOSICIONTAJAMAR', N'U') IS NOT NULL DROP TABLE dbo.DISPOSITIVOPOSICIONTAJAMAR;
IF OBJECT_ID(N'dbo.POSICIONESTAJAMAR', N'U') IS NOT NULL DROP TABLE dbo.POSICIONESTAJAMAR;
IF OBJECT_ID(N'dbo.DISPOSITIVOSTAJAMAR', N'U') IS NOT NULL DROP TABLE dbo.DISPOSITIVOSTAJAMAR;
IF OBJECT_ID(N'dbo.CURSOSUSUARIOSTAJAMAR', N'U') IS NOT NULL DROP TABLE dbo.CURSOSUSUARIOSTAJAMAR;
IF OBJECT_ID(N'dbo.USUARIOSTAJAMAR', N'U') IS NOT NULL DROP TABLE dbo.USUARIOSTAJAMAR;
IF OBJECT_ID(N'dbo.CURSOSTAJAMAR', N'U') IS NOT NULL DROP TABLE dbo.CURSOSTAJAMAR;
IF OBJECT_ID(N'dbo.ROLESCHARLASTAJAMAR', N'U') IS NOT NULL DROP TABLE dbo.ROLESCHARLASTAJAMAR;
GO

-- Batch 2: create schema
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;

CREATE TABLE [dbo].[ROLESCHARLASTAJAMAR](
    [IDROLE] [int] NOT NULL,
    [ROLE] [nvarchar](100) NULL,
    PRIMARY KEY CLUSTERED ([IDROLE] ASC)
) ON [PRIMARY];

CREATE TABLE [dbo].[CURSOSTAJAMAR](
    [IDCURSO] [int] NOT NULL,
    [NOMBRE] [nvarchar](150) NULL,
    [FECHAINICIO] [datetime] NULL,
    [FECHAFIN] [datetime] NULL,
    [ACTIVO] [bit] NULL,
    PRIMARY KEY CLUSTERED ([IDCURSO] ASC)
) ON [PRIMARY];

CREATE TABLE [dbo].[USUARIOSTAJAMAR](
    [IDUSUARIO] [int] NOT NULL,
    [NOMBRE] [nvarchar](70) NULL,
    [APELLIDOS] [nvarchar](70) NULL,
    [EMAIL] [nvarchar](70) NULL,
    [ESTADO] [bit] NULL,
    [IMAGEN] [nvarchar](600) NULL,
    [PASSWORD] [nvarchar](100) NULL,
    [IDROLE] [int] NULL,
    PRIMARY KEY CLUSTERED ([IDUSUARIO] ASC),
    CONSTRAINT [u_mailuser] UNIQUE NONCLUSTERED ([EMAIL] ASC)
) ON [PRIMARY];

ALTER TABLE [dbo].[USUARIOSTAJAMAR] WITH CHECK ADD CONSTRAINT [FK_USUARIOSTAJAMAR_ROLESCHARLASTAJAMAR]
    FOREIGN KEY([IDROLE]) REFERENCES [dbo].[ROLESCHARLASTAJAMAR] ([IDROLE]);
ALTER TABLE [dbo].[USUARIOSTAJAMAR] CHECK CONSTRAINT [FK_USUARIOSTAJAMAR_ROLESCHARLASTAJAMAR];

CREATE TABLE [dbo].[CURSOSUSUARIOSTAJAMAR](
    [IDCURSOSUSUARIOS] [int] NOT NULL,
    [IDCURSO] [int] NULL,
    [IDUSUARIO] [int] NULL,
    PRIMARY KEY CLUSTERED ([IDCURSOSUSUARIOS] ASC),
    CONSTRAINT [u_idcurso_idusuario] UNIQUE NONCLUSTERED ([IDCURSO] ASC, [IDUSUARIO] ASC)
) ON [PRIMARY];

ALTER TABLE [dbo].[CURSOSUSUARIOSTAJAMAR] WITH CHECK ADD CONSTRAINT [FK_CURSOSUSUARIOSTAJAMAR_CURSOSTAJAMAR]
    FOREIGN KEY([IDCURSO]) REFERENCES [dbo].[CURSOSTAJAMAR] ([IDCURSO]);
ALTER TABLE [dbo].[CURSOSUSUARIOSTAJAMAR] CHECK CONSTRAINT [FK_CURSOSUSUARIOSTAJAMAR_CURSOSTAJAMAR];

ALTER TABLE [dbo].[CURSOSUSUARIOSTAJAMAR] WITH CHECK ADD CONSTRAINT [FK_CURSOSUSUARIOSTAJAMAR_USUARIOSTAJAMAR]
    FOREIGN KEY([IDUSUARIO]) REFERENCES [dbo].[USUARIOSTAJAMAR] ([IDUSUARIO]);
ALTER TABLE [dbo].[CURSOSUSUARIOSTAJAMAR] CHECK CONSTRAINT [FK_CURSOSUSUARIOSTAJAMAR_USUARIOSTAJAMAR];

CREATE TABLE [dbo].[ASISTENCIATAJAMAR](
    [IDASISTENCIA] [int] IDENTITY(1,1) NOT NULL,
    [IDUSUARIO] [int] NOT NULL,
    [IDCURSO] [int] NOT NULL,
    [FECHA] [date] NOT NULL,
    [ESTADO] [tinyint] NOT NULL,
    [COMENTARIO] [nvarchar](500) NULL,
    [IDPROFESOR] [int] NOT NULL,
    [FECHAREGISTRO] [datetime2](7) NOT NULL CONSTRAINT [DF_ASISTENCIATAJAMAR_FECHAREGISTRO] DEFAULT (sysutcdatetime()),
    PRIMARY KEY CLUSTERED ([IDASISTENCIA] ASC),
    CONSTRAINT [UQ_ASISTENCIA_USUARIO_CURSO_FECHA] UNIQUE NONCLUSTERED ([IDUSUARIO] ASC, [IDCURSO] ASC, [FECHA] ASC)
) ON [PRIMARY];

CREATE NONCLUSTERED INDEX [IX_ASISTENCIATAJAMAR_CURSO_FECHA]
    ON [dbo].[ASISTENCIATAJAMAR] ([IDCURSO] ASC, [FECHA] ASC);

ALTER TABLE [dbo].[ASISTENCIATAJAMAR] WITH CHECK ADD CONSTRAINT [FK_ASISTENCIATAJAMAR_USUARIO]
    FOREIGN KEY([IDUSUARIO]) REFERENCES [dbo].[USUARIOSTAJAMAR] ([IDUSUARIO]);
ALTER TABLE [dbo].[ASISTENCIATAJAMAR] CHECK CONSTRAINT [FK_ASISTENCIATAJAMAR_USUARIO];

ALTER TABLE [dbo].[ASISTENCIATAJAMAR] WITH CHECK ADD CONSTRAINT [FK_ASISTENCIATAJAMAR_CURSO]
    FOREIGN KEY([IDCURSO]) REFERENCES [dbo].[CURSOSTAJAMAR] ([IDCURSO]);
ALTER TABLE [dbo].[ASISTENCIATAJAMAR] CHECK CONSTRAINT [FK_ASISTENCIATAJAMAR_CURSO];

ALTER TABLE [dbo].[ASISTENCIATAJAMAR] WITH CHECK ADD CONSTRAINT [FK_ASISTENCIATAJAMAR_PROFESOR]
    FOREIGN KEY([IDPROFESOR]) REFERENCES [dbo].[USUARIOSTAJAMAR] ([IDUSUARIO]);
ALTER TABLE [dbo].[ASISTENCIATAJAMAR] CHECK CONSTRAINT [FK_ASISTENCIATAJAMAR_PROFESOR];

CREATE TABLE [dbo].[DISPOSITIVOSTAJAMAR](
    [IDDISPOSITIVO] [int] IDENTITY(1,1) NOT NULL,
    [DEVICEIDENTIFIER] [nvarchar](64) NOT NULL,
    [FIRSTSEENATUTC] [datetime2](7) NOT NULL CONSTRAINT [DF_DISPOSITIVOSTAJAMAR_FIRSTSEENATUTC] DEFAULT (sysutcdatetime()),
    [LASTSEENATUTC] [datetime2](7) NOT NULL CONSTRAINT [DF_DISPOSITIVOSTAJAMAR_LASTSEENATUTC] DEFAULT (sysutcdatetime()),
    [LASTSEENIP] [nvarchar](64) NULL,
    [LASTSEENUSERAGENT] [nvarchar](512) NULL,
    [FRIENDLYNAME] [nvarchar](200) NULL,
    [ISACTIVE] [bit] NOT NULL CONSTRAINT [DF_DISPOSITIVOSTAJAMAR_ISACTIVE] DEFAULT ((1)),
    PRIMARY KEY CLUSTERED ([IDDISPOSITIVO] ASC),
    CONSTRAINT [UQ_DISPOSITIVOSTAJAMAR_DEVICEIDENTIFIER] UNIQUE NONCLUSTERED ([DEVICEIDENTIFIER] ASC)
) ON [PRIMARY];

CREATE TABLE [dbo].[POSICIONESTAJAMAR](
    [IDPOSICION] [int] IDENTITY(1,1) NOT NULL,
    [CLASSCODE] [nvarchar](16) NOT NULL,
    [DEVICECODE] [nvarchar](16) NOT NULL,
    [ISACTIVE] [bit] NOT NULL CONSTRAINT [DF_POSICIONESTAJAMAR_ISACTIVE] DEFAULT ((1)),
    PRIMARY KEY CLUSTERED ([IDPOSICION] ASC),
    CONSTRAINT [UQ_POSICIONESTAJAMAR_CLASS_DEVICE] UNIQUE NONCLUSTERED ([CLASSCODE] ASC, [DEVICECODE] ASC)
) ON [PRIMARY];

CREATE TABLE [dbo].[DISPOSITIVOPOSICIONTAJAMAR](
    [IDDISPOSITIVOPOSICION] [int] IDENTITY(1,1) NOT NULL,
    [IDDISPOSITIVO] [int] NOT NULL,
    [IDPOSICION] [int] NOT NULL,
    [ASSIGNEDATUTC] [datetime2](7) NOT NULL CONSTRAINT [DF_DISPOSITIVOPOSICIONTAJAMAR_ASSIGNEDATUTC] DEFAULT (sysutcdatetime()),
    [UNASSIGNEDATUTC] [datetime2](7) NULL,
    [ISCURRENT] [bit] NOT NULL CONSTRAINT [DF_DISPOSITIVOPOSICIONTAJAMAR_ISCURRENT] DEFAULT ((1)),
    PRIMARY KEY CLUSTERED ([IDDISPOSITIVOPOSICION] ASC)
) ON [PRIMARY];

ALTER TABLE [dbo].[DISPOSITIVOPOSICIONTAJAMAR] WITH CHECK ADD CONSTRAINT [FK_DISPOSITIVOPOSICIONTAJAMAR_DISPOSITIVO]
    FOREIGN KEY([IDDISPOSITIVO]) REFERENCES [dbo].[DISPOSITIVOSTAJAMAR] ([IDDISPOSITIVO]);
ALTER TABLE [dbo].[DISPOSITIVOPOSICIONTAJAMAR] CHECK CONSTRAINT [FK_DISPOSITIVOPOSICIONTAJAMAR_DISPOSITIVO];

ALTER TABLE [dbo].[DISPOSITIVOPOSICIONTAJAMAR] WITH CHECK ADD CONSTRAINT [FK_DISPOSITIVOPOSICIONTAJAMAR_POSICION]
    FOREIGN KEY([IDPOSICION]) REFERENCES [dbo].[POSICIONESTAJAMAR] ([IDPOSICION]);
ALTER TABLE [dbo].[DISPOSITIVOPOSICIONTAJAMAR] CHECK CONSTRAINT [FK_DISPOSITIVOPOSICIONTAJAMAR_POSICION];

CREATE UNIQUE NONCLUSTERED INDEX [UX_DISPOSITIVOPOSICIONTAJAMAR_DEVICE_CURRENT]
    ON [dbo].[DISPOSITIVOPOSICIONTAJAMAR] ([IDDISPOSITIVO] ASC)
    WHERE ([ISCURRENT] = 1);

CREATE NONCLUSTERED INDEX [IX_DISPOSITIVOPOSICIONTAJAMAR_POSICION]
    ON [dbo].[DISPOSITIVOPOSICIONTAJAMAR] ([IDPOSICION] ASC);

CREATE TABLE [dbo].[POSICIONUSUARIOSTAJAMAR](
    [IDPOSICIONUSUARIO] [int] IDENTITY(1,1) NOT NULL,
    [IDPOSICION] [int] NOT NULL,
    [IDUSUARIO] [int] NOT NULL,
    [ASSIGNEDATUTC] [datetime2](7) NOT NULL CONSTRAINT [DF_POSICIONUSUARIOSTAJAMAR_ASSIGNEDATUTC] DEFAULT (sysutcdatetime()),
    [UNASSIGNEDATUTC] [datetime2](7) NULL,
    [ISCURRENT] [bit] NOT NULL CONSTRAINT [DF_POSICIONUSUARIOSTAJAMAR_ISCURRENT] DEFAULT ((1)),
    PRIMARY KEY CLUSTERED ([IDPOSICIONUSUARIO] ASC)
) ON [PRIMARY];

ALTER TABLE [dbo].[POSICIONUSUARIOSTAJAMAR] WITH CHECK ADD CONSTRAINT [FK_POSICIONUSUARIOSTAJAMAR_POSICION]
    FOREIGN KEY([IDPOSICION]) REFERENCES [dbo].[POSICIONESTAJAMAR] ([IDPOSICION]);
ALTER TABLE [dbo].[POSICIONUSUARIOSTAJAMAR] CHECK CONSTRAINT [FK_POSICIONUSUARIOSTAJAMAR_POSICION];

ALTER TABLE [dbo].[POSICIONUSUARIOSTAJAMAR] WITH CHECK ADD CONSTRAINT [FK_POSICIONUSUARIOSTAJAMAR_USUARIO]
    FOREIGN KEY([IDUSUARIO]) REFERENCES [dbo].[USUARIOSTAJAMAR] ([IDUSUARIO]);
ALTER TABLE [dbo].[POSICIONUSUARIOSTAJAMAR] CHECK CONSTRAINT [FK_POSICIONUSUARIOSTAJAMAR_USUARIO];

CREATE UNIQUE NONCLUSTERED INDEX [UX_POSICIONUSUARIOSTAJAMAR_POSICION_CURRENT]
    ON [dbo].[POSICIONUSUARIOSTAJAMAR] ([IDPOSICION] ASC)
    WHERE ([ISCURRENT] = 1);

CREATE NONCLUSTERED INDEX [IX_POSICIONUSUARIOSTAJAMAR_USUARIO_CURRENT]
    ON [dbo].[POSICIONUSUARIOSTAJAMAR] ([IDUSUARIO] ASC)
    WHERE ([ISCURRENT] = 1);

CREATE TABLE [dbo].[CHECKINSTAJAMAR](
    [IDCHECKIN] [int] IDENTITY(1,1) NOT NULL,
    [IDUSUARIO] [int] NOT NULL,
    [IDDISPOSITIVO] [int] NOT NULL,
    [IDPOSICION] [int] NOT NULL,
    [FECHACHECKINUTC] [datetime2](7) NOT NULL CONSTRAINT [DF_CHECKINSTAJAMAR_FECHACHECKINUTC] DEFAULT (sysutcdatetime()),
    [OBSERVEDIP] [nvarchar](64) NULL,
    PRIMARY KEY CLUSTERED ([IDCHECKIN] ASC)
) ON [PRIMARY];

ALTER TABLE [dbo].[CHECKINSTAJAMAR] WITH CHECK ADD CONSTRAINT [FK_CHECKINSTAJAMAR_USUARIO]
    FOREIGN KEY([IDUSUARIO]) REFERENCES [dbo].[USUARIOSTAJAMAR] ([IDUSUARIO]);
ALTER TABLE [dbo].[CHECKINSTAJAMAR] CHECK CONSTRAINT [FK_CHECKINSTAJAMAR_USUARIO];

ALTER TABLE [dbo].[CHECKINSTAJAMAR] WITH CHECK ADD CONSTRAINT [FK_CHECKINSTAJAMAR_DISPOSITIVO]
    FOREIGN KEY([IDDISPOSITIVO]) REFERENCES [dbo].[DISPOSITIVOSTAJAMAR] ([IDDISPOSITIVO]);
ALTER TABLE [dbo].[CHECKINSTAJAMAR] CHECK CONSTRAINT [FK_CHECKINSTAJAMAR_DISPOSITIVO];

ALTER TABLE [dbo].[CHECKINSTAJAMAR] WITH CHECK ADD CONSTRAINT [FK_CHECKINSTAJAMAR_POSICION]
    FOREIGN KEY([IDPOSICION]) REFERENCES [dbo].[POSICIONESTAJAMAR] ([IDPOSICION]);
ALTER TABLE [dbo].[CHECKINSTAJAMAR] CHECK CONSTRAINT [FK_CHECKINSTAJAMAR_POSICION];

CREATE NONCLUSTERED INDEX [IX_CHECKINSTAJAMAR_USUARIO_FECHA]
    ON [dbo].[CHECKINSTAJAMAR] ([IDUSUARIO] ASC, [FECHACHECKINUTC] ASC);

CREATE NONCLUSTERED INDEX [IX_CHECKINSTAJAMAR_DISPOSITIVO_FECHA]
    ON [dbo].[CHECKINSTAJAMAR] ([IDDISPOSITIVO] ASC, [FECHACHECKINUTC] ASC);

CREATE NONCLUSTERED INDEX [IX_CHECKINSTAJAMAR_POSICION_FECHA]
    ON [dbo].[CHECKINSTAJAMAR] ([IDPOSICION] ASC, [FECHACHECKINUTC] ASC);

CREATE TABLE [dbo].[CALENDARIOCURSO](
    [Id] [int] IDENTITY(1,1) NOT NULL,
    [CourseId] [int] NOT NULL,
    [Date] [date] NOT NULL,
    [IsLective] [bit] NOT NULL,
    [DayType] [nvarchar](50) NULL,
    [Module] [nvarchar](500) NULL,
    [Teacher] [nvarchar](200) NULL,
    [Room] [nvarchar](50) NULL,
    [UploadedAt] [datetime2](7) NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [UQ_CALENDARIOCURSO_CourseId_Date] UNIQUE NONCLUSTERED ([CourseId] ASC, [Date] ASC)
) ON [PRIMARY];

ALTER TABLE [dbo].[CALENDARIOCURSO] WITH CHECK ADD CONSTRAINT [FK_CALENDARIOCURSO_CURSOSTAJAMAR]
    FOREIGN KEY([CourseId]) REFERENCES [dbo].[CURSOSTAJAMAR] ([IDCURSO]) ON DELETE CASCADE;
ALTER TABLE [dbo].[CALENDARIOCURSO] CHECK CONSTRAINT [FK_CALENDARIOCURSO_CURSOSTAJAMAR];

GO

-- Batch 3: seed data (FK order)
SET XACT_ABORT ON;
BEGIN TRANSACTION;

INSERT INTO [dbo].[ROLESCHARLASTAJAMAR] ([IDROLE], [ROLE]) VALUES (1, N'PROFESOR');
INSERT INTO [dbo].[ROLESCHARLASTAJAMAR] ([IDROLE], [ROLE]) VALUES (2, N'ALUMNO');
INSERT INTO [dbo].[ROLESCHARLASTAJAMAR] ([IDROLE], [ROLE]) VALUES (3, N'ADMINISTRADOR');

INSERT INTO [dbo].[CURSOSTAJAMAR] ([IDCURSO], [NOMBRE], [FECHAINICIO], [FECHAFIN], [ACTIVO]) VALUES (3213, N'Master Desarrollo de aplicaciones Cloud', '2024-01-10T00:00:00', '2025-03-06T00:00:00', 0);
INSERT INTO [dbo].[CURSOSTAJAMAR] ([IDCURSO], [NOMBRE], [FECHAINICIO], [FECHAFIN], [ACTIVO]) VALUES (3430, N'Master Desarrollo Apps Cloud 2025-2026', '2025-10-01T00:00:00', '2026-06-08T00:00:00', 1);
INSERT INTO [dbo].[CURSOSTAJAMAR] ([IDCURSO], [NOMBRE], [FECHAINICIO], [FECHAFIN], [ACTIVO]) VALUES (3431, N'Big Data', '2025-12-12T00:00:00', '2026-01-10T00:00:00', 1);
INSERT INTO [dbo].[CURSOSTAJAMAR] ([IDCURSO], [NOMBRE], [FECHAINICIO], [FECHAFIN], [ACTIVO]) VALUES (3434, N'Master Desarrollo Pago 2025-2026', '2025-10-01T00:00:00', '2026-06-16T00:00:00', 1);
INSERT INTO [dbo].[CURSOSTAJAMAR] ([IDCURSO], [NOMBRE], [FECHAINICIO], [FECHAFIN], [ACTIVO]) VALUES (304158642, N'IA y Big Data', '2024-09-20T00:00:00', '2025-06-30T00:00:00', 1);

INSERT INTO [dbo].[USUARIOSTAJAMAR] ([IDUSUARIO], [NOMBRE], [APELLIDOS], [EMAIL], [ESTADO], [IMAGEN], [PASSWORD], [IDROLE]) VALUES (31, N'Paco', N'Garcia Serrano', N'paco.garcia.serrano@tajamar365.com', 1, N'https://apicharlasalumnotajamar.azurewebsites.net/images/users/31_user.jpg', N'OFi9W3uf5kGsaxyTqF1ZFQ==', 1);
INSERT INTO [dbo].[USUARIOSTAJAMAR] ([IDUSUARIO], [NOMBRE], [APELLIDOS], [EMAIL], [ESTADO], [IMAGEN], [PASSWORD], [IDROLE]) VALUES (32, N'Admin', N'Admin', N'admin@tajamar365.com', 1, N'nouser.png', N'OFi9W3uf5kGsaxyTqF1ZFQ==', 3);
INSERT INTO [dbo].[USUARIOSTAJAMAR] ([IDUSUARIO], [NOMBRE], [APELLIDOS], [EMAIL], [ESTADO], [IMAGEN], [PASSWORD], [IDROLE]) VALUES (33, N'Sofia', N'Martinez', N'sofia.martinez@tajamar365.com', 1, N'https://apicharlasalumnotajamar.azurewebsites.net/images/users/33_user.jfif', N'OFi9W3uf5kGsaxyTqF1ZFQ==', 2);
INSERT INTO [dbo].[USUARIOSTAJAMAR] ([IDUSUARIO], [NOMBRE], [APELLIDOS], [EMAIL], [ESTADO], [IMAGEN], [PASSWORD], [IDROLE]) VALUES (34, N'Maki Spariva', N'Mirón Olona', N'makispariva.miron@tajamar365.com', 1, N'https://apicharlasalumnotajamar.azurewebsites.net/images/users/34_user.jfif', N'OFi9W3uf5kGsaxyTqF1ZFQ==', 2);
INSERT INTO [dbo].[USUARIOSTAJAMAR] ([IDUSUARIO], [NOMBRE], [APELLIDOS], [EMAIL], [ESTADO], [IMAGEN], [PASSWORD], [IDROLE]) VALUES (35, N'Manuel', N'Perez', N'manuel.perezbenavent@tajamar365.com', 1, N'https://apicharlasalumnotajamar.azurewebsites.net/images/users/35_user.jpg', N'OFi9W3uf5kGsaxyTqF1ZFQ==', 2);
INSERT INTO [dbo].[USUARIOSTAJAMAR] ([IDUSUARIO], [NOMBRE], [APELLIDOS], [EMAIL], [ESTADO], [IMAGEN], [PASSWORD], [IDROLE]) VALUES (36, N'Sofía', N'Villarejo Rodríguez', N'sofia.villarejo@tajamar365.com', 1, N'https://apicharlasalumnotajamar.azurewebsites.net/images/users/36_user.jpeg', N'OFi9W3uf5kGsaxyTqF1ZFQ==', 2);
INSERT INTO [dbo].[USUARIOSTAJAMAR] ([IDUSUARIO], [NOMBRE], [APELLIDOS], [EMAIL], [ESTADO], [IMAGEN], [PASSWORD], [IDROLE]) VALUES (37, N'Andrei', N'Popa', N'andreidaniel.popa@tajamar365.com', 1, N'https://cdn.pixabay.com/photo/2017/11/10/05/48/user-2935527_640.png', N'OFi9W3uf5kGsaxyTqF1ZFQ==', 2);
INSERT INTO [dbo].[USUARIOSTAJAMAR] ([IDUSUARIO], [NOMBRE], [APELLIDOS], [EMAIL], [ESTADO], [IMAGEN], [PASSWORD], [IDROLE]) VALUES (38, N'Jaime Jesús', N'Laguna Moreno', N'jaimejesus.laguna@tajamar365.com', 1, N'https://apicharlasalumnotajamar.azurewebsites.net/images/users/38_user.jpeg', N'OFi9W3uf5kGsaxyTqF1ZFQ==', 2);
INSERT INTO [dbo].[USUARIOSTAJAMAR] ([IDUSUARIO], [NOMBRE], [APELLIDOS], [EMAIL], [ESTADO], [IMAGEN], [PASSWORD], [IDROLE]) VALUES (39, N'Doryan Alexandro', N'Mamani Ticona', N'doryanalexandro.mamani@tajamar365.com', 1, N'https://apicharlasalumnotajamar.azurewebsites.net/images/users/39_user.jpg', N'OFi9W3uf5kGsaxyTqF1ZFQ==', 2);
INSERT INTO [dbo].[USUARIOSTAJAMAR] ([IDUSUARIO], [NOMBRE], [APELLIDOS], [EMAIL], [ESTADO], [IMAGEN], [PASSWORD], [IDROLE]) VALUES (41, N'Ariadna', N'López Abalo', N'ariadna.lopez@tajamar365.com', 0, N'https://apicharlasalumnotajamar.azurewebsites.net/images/users/41_user.png', N'OFi9W3uf5kGsaxyTqF1ZFQ==', 2);
INSERT INTO [dbo].[USUARIOSTAJAMAR] ([IDUSUARIO], [NOMBRE], [APELLIDOS], [EMAIL], [ESTADO], [IMAGEN], [PASSWORD], [IDROLE]) VALUES (42, N'Gabriel', N'Fonseca', N'gabrielalejandro.fonseca@tajamar365.com', 1, N'https://cdn.pixabay.com/photo/2017/11/10/05/48/user-2935527_640.png', N'OFi9W3uf5kGsaxyTqF1ZFQ==', 2);
INSERT INTO [dbo].[USUARIOSTAJAMAR] ([IDUSUARIO], [NOMBRE], [APELLIDOS], [EMAIL], [ESTADO], [IMAGEN], [PASSWORD], [IDROLE]) VALUES (43, N'Amanda', N'Crespo Luis', N'amanda.crespo@tajamar365.com', 1, N'https://cdn.pixabay.com/photo/2017/11/10/05/48/user-2935527_640.png', N'OFi9W3uf5kGsaxyTqF1ZFQ==', 2);
INSERT INTO [dbo].[USUARIOSTAJAMAR] ([IDUSUARIO], [NOMBRE], [APELLIDOS], [EMAIL], [ESTADO], [IMAGEN], [PASSWORD], [IDROLE]) VALUES (44, N'Daniel', N'Rodríguez Lancha', N'daniel.rodriguezlancha@tajamar365.com', 1, N'https://apicharlasalumnotajamar.azurewebsites.net/images/users/44_user.jpg', N'OFi9W3uf5kGsaxyTqF1ZFQ==', 2);
INSERT INTO [dbo].[USUARIOSTAJAMAR] ([IDUSUARIO], [NOMBRE], [APELLIDOS], [EMAIL], [ESTADO], [IMAGEN], [PASSWORD], [IDROLE]) VALUES (45, N'Rodrigo', N'González Orovio', N'rodrigo.gonzalezorovio@tajamar365.com', 1, N'https://apicharlasalumnotajamar.azurewebsites.net/images/users/45_user.jpg', N'OFi9W3uf5kGsaxyTqF1ZFQ==', 2);
INSERT INTO [dbo].[USUARIOSTAJAMAR] ([IDUSUARIO], [NOMBRE], [APELLIDOS], [EMAIL], [ESTADO], [IMAGEN], [PASSWORD], [IDROLE]) VALUES (46, N'Jorge', N'Ruiz Parra', N'jorge.ruiz@tajamar365.com', 1, N'https://apicharlasalumnotajamar.azurewebsites.net/images/users/46_user.jpg', N'OFi9W3uf5kGsaxyTqF1ZFQ==', 2);
INSERT INTO [dbo].[USUARIOSTAJAMAR] ([IDUSUARIO], [NOMBRE], [APELLIDOS], [EMAIL], [ESTADO], [IMAGEN], [PASSWORD], [IDROLE]) VALUES (47, N'Mario', N'Jiménez Marset', N'mario.jimenezmarset@tajamar365.com', 1, N'https://apicharlasalumnotajamar.azurewebsites.net/images/users/47_user.jpg', N'OFi9W3uf5kGsaxyTqF1ZFQ==', 2);
INSERT INTO [dbo].[USUARIOSTAJAMAR] ([IDUSUARIO], [NOMBRE], [APELLIDOS], [EMAIL], [ESTADO], [IMAGEN], [PASSWORD], [IDROLE]) VALUES (48, N'Barbara', N'Jimenez', N'barbara.jimenez@tajamar365.com', 1, N'https://apicharlasalumnotajamar.azurewebsites.net/images/users/48_user.jpg', N'OFi9W3uf5kGsaxyTqF1ZFQ==', 2);
INSERT INTO [dbo].[USUARIOSTAJAMAR] ([IDUSUARIO], [NOMBRE], [APELLIDOS], [EMAIL], [ESTADO], [IMAGEN], [PASSWORD], [IDROLE]) VALUES (49, N'Tomas', N'Santamaria', N'tomas.santamaria@tajamar365.com', 1, N'https://apicharlasalumnotajamar.azurewebsites.net/images/users/49_user.jpeg', N'OFi9W3uf5kGsaxyTqF1ZFQ==', 2);
INSERT INTO [dbo].[USUARIOSTAJAMAR] ([IDUSUARIO], [NOMBRE], [APELLIDOS], [EMAIL], [ESTADO], [IMAGEN], [PASSWORD], [IDROLE]) VALUES (50, N'Aarón', N'García Anguita', N'aaron.garcia@tajamar365.com', 1, N'https://cdn.pixabay.com/photo/2017/11/10/05/48/user-2935527_640.png', N'OFi9W3uf5kGsaxyTqF1ZFQ==', 2);
INSERT INTO [dbo].[USUARIOSTAJAMAR] ([IDUSUARIO], [NOMBRE], [APELLIDOS], [EMAIL], [ESTADO], [IMAGEN], [PASSWORD], [IDROLE]) VALUES (51, N'Óscar', N'Gómez Martin', N'oscar.gomez@tajamar365.com', 1, N'http://[::]:8080/images/users/51_user.jpg', N'OFi9W3uf5kGsaxyTqF1ZFQ==', 2);
INSERT INTO [dbo].[USUARIOSTAJAMAR] ([IDUSUARIO], [NOMBRE], [APELLIDOS], [EMAIL], [ESTADO], [IMAGEN], [PASSWORD], [IDROLE]) VALUES (52, N'Carlos', N'García Torregrosa', N'carlos.garciatorregrosa@tajamar365.com', 1, N'https://apicharlasalumnotajamar.azurewebsites.net/images/users/52_user.jpg', N'OFi9W3uf5kGsaxyTqF1ZFQ==', 2);
INSERT INTO [dbo].[USUARIOSTAJAMAR] ([IDUSUARIO], [NOMBRE], [APELLIDOS], [EMAIL], [ESTADO], [IMAGEN], [PASSWORD], [IDROLE]) VALUES (54, N'Monica', N'Delgado Capellan', N'monica.delgado@tajamar365.com', 1, N'https://cdn.pixabay.com/photo/2017/11/10/05/48/user-2935527_640.png', N'OFi9W3uf5kGsaxyTqF1ZFQ==', 2);
INSERT INTO [dbo].[USUARIOSTAJAMAR] ([IDUSUARIO], [NOMBRE], [APELLIDOS], [EMAIL], [ESTADO], [IMAGEN], [PASSWORD], [IDROLE]) VALUES (55, N'Alejandro', N'Robles Ruiz', N'alejandro.robles@tajamar365.com', 1, N'https://apicharlasalumnotajamar.azurewebsites.net/images/users/55_user.jpg', N'OFi9W3uf5kGsaxyTqF1ZFQ==', 2);
INSERT INTO [dbo].[USUARIOSTAJAMAR] ([IDUSUARIO], [NOMBRE], [APELLIDOS], [EMAIL], [ESTADO], [IMAGEN], [PASSWORD], [IDROLE]) VALUES (56, N'Carolina', N'Penalba Corpa', N'carolina.penalba@tajamar365.com', 1, N'https://apicharlasalumnotajamar.azurewebsites.net/images/users/56_user.jpg', N'OFi9W3uf5kGsaxyTqF1ZFQ==', 2);
INSERT INTO [dbo].[USUARIOSTAJAMAR] ([IDUSUARIO], [NOMBRE], [APELLIDOS], [EMAIL], [ESTADO], [IMAGEN], [PASSWORD], [IDROLE]) VALUES (57, N'Víctor', N'Castrillo Redondo', N'victor.castrillo@tajamar365.com', 1, N'https://apicharlasalumnotajamar.azurewebsites.net/images/users/57_user.jpg', N'OFi9W3uf5kGsaxyTqF1ZFQ==', 2);
INSERT INTO [dbo].[USUARIOSTAJAMAR] ([IDUSUARIO], [NOMBRE], [APELLIDOS], [EMAIL], [ESTADO], [IMAGEN], [PASSWORD], [IDROLE]) VALUES (58, N'Valentin', N'Preutesei', N'valentindanut.preutesei@tajamar365.com', 1, N'https://apicharlasalumnotajamar.azurewebsites.net/images/users/58_user.jpeg', N'OFi9W3uf5kGsaxyTqF1ZFQ==', 2);
INSERT INTO [dbo].[USUARIOSTAJAMAR] ([IDUSUARIO], [NOMBRE], [APELLIDOS], [EMAIL], [ESTADO], [IMAGEN], [PASSWORD], [IDROLE]) VALUES (59, N'Abdessamad', N'Ammi', N'abdessamad.ammi@tajamar365.com', 1, N'https://cdn.pixabay.com/photo/2017/11/10/05/48/user-2935527_640.png', N'OFi9W3uf5kGsaxyTqF1ZFQ==', 1);
INSERT INTO [dbo].[USUARIOSTAJAMAR] ([IDUSUARIO], [NOMBRE], [APELLIDOS], [EMAIL], [ESTADO], [IMAGEN], [PASSWORD], [IDROLE]) VALUES (60, N'carlos', N'Serra Martínez', N'carlos.serra.martinez@tajamar365.com', 1, N'https://cdn.pixabay.com/photo/2017/11/10/05/48/user-2935527_640.png', N'OFi9W3uf5kGsaxyTqF1ZFQ==', 1);
INSERT INTO [dbo].[USUARIOSTAJAMAR] ([IDUSUARIO], [NOMBRE], [APELLIDOS], [EMAIL], [ESTADO], [IMAGEN], [PASSWORD], [IDROLE]) VALUES (61, N'Miguel', N'Marañón Quero', N'miguel.maranon@tajamar365.com', 1, N'https://cdn.pixabay.com/photo/2017/11/10/05/48/user-2935527_640.png', N'OFi9W3uf5kGsaxyTqF1ZFQ==', 2);
INSERT INTO [dbo].[USUARIOSTAJAMAR] ([IDUSUARIO], [NOMBRE], [APELLIDOS], [EMAIL], [ESTADO], [IMAGEN], [PASSWORD], [IDROLE]) VALUES (62, N'Javier', N'Pérez Álvarez', N'javier.perezalvarez@tajamar365.com', 1, N'https://cdn.pixabay.com/photo/2017/11/10/05/48/user-2935527_640.png', N'OFi9W3uf5kGsaxyTqF1ZFQ==', 2);
INSERT INTO [dbo].[USUARIOSTAJAMAR] ([IDUSUARIO], [NOMBRE], [APELLIDOS], [EMAIL], [ESTADO], [IMAGEN], [PASSWORD], [IDROLE]) VALUES (63, N'Leonardo', N'Narvaez', N'leonardo.narvaez@tajamar365.com', 1, N'https://cdn.pixabay.com/photo/2017/11/10/05/48/user-2935527_640.png', N'OFi9W3uf5kGsaxyTqF1ZFQ==', 2);
INSERT INTO [dbo].[USUARIOSTAJAMAR] ([IDUSUARIO], [NOMBRE], [APELLIDOS], [EMAIL], [ESTADO], [IMAGEN], [PASSWORD], [IDROLE]) VALUES (64, N'Sergio', N'Pulido Salvador', N'sergio.pulido@tajamar365.com', 1, N'https://apicharlasalumnotajamar.azurewebsites.net/images/users/64_user.jpg', N'OFi9W3uf5kGsaxyTqF1ZFQ==', 2);
INSERT INTO [dbo].[USUARIOSTAJAMAR] ([IDUSUARIO], [NOMBRE], [APELLIDOS], [EMAIL], [ESTADO], [IMAGEN], [PASSWORD], [IDROLE]) VALUES (65, N'Hugo', N'Moreno Fernández', N'hugo.moreno@tajamar365.com', 1, N'https://apicharlasalumnotajamar.azurewebsites.net/images/users/65_user.jpg', N'OFi9W3uf5kGsaxyTqF1ZFQ==', 2);
INSERT INTO [dbo].[USUARIOSTAJAMAR] ([IDUSUARIO], [NOMBRE], [APELLIDOS], [EMAIL], [ESTADO], [IMAGEN], [PASSWORD], [IDROLE]) VALUES (66, N'Daniel', N'Serrano Real', N'daniel.serrano@tajamar365.com', 1, N'https://cdn.pixabay.com/photo/2017/11/10/05/48/user-2935527_640.png', N'OFi9W3uf5kGsaxyTqF1ZFQ==', 2);
INSERT INTO [dbo].[USUARIOSTAJAMAR] ([IDUSUARIO], [NOMBRE], [APELLIDOS], [EMAIL], [ESTADO], [IMAGEN], [PASSWORD], [IDROLE]) VALUES (67, N'David', N'Rubio Chavida', N'david.rubiochavida@tajamar365.com', 1, N'https://cdn.pixabay.com/photo/2017/11/10/05/48/user-2935527_640.png', N'OFi9W3uf5kGsaxyTqF1ZFQ==', 2);
INSERT INTO [dbo].[USUARIOSTAJAMAR] ([IDUSUARIO], [NOMBRE], [APELLIDOS], [EMAIL], [ESTADO], [IMAGEN], [PASSWORD], [IDROLE]) VALUES (68, N'Pablo Adrian', N'Herrera Gomez', N'pabloadrian.herrera@tajamar365.com', 1, N'https://cdn.pixabay.com/photo/2017/11/10/05/48/user-2935527_640.png', N'OFi9W3uf5kGsaxyTqF1ZFQ==', 2);
INSERT INTO [dbo].[USUARIOSTAJAMAR] ([IDUSUARIO], [NOMBRE], [APELLIDOS], [EMAIL], [ESTADO], [IMAGEN], [PASSWORD], [IDROLE]) VALUES (69, N'ana cristina', N'borja parra ', N'anacristina.borja@tajamar365.com', 1, N'https://cdn.pixabay.com/photo/2017/11/10/05/48/user-2935527_640.png', N'OFi9W3uf5kGsaxyTqF1ZFQ==', 2);
INSERT INTO [dbo].[USUARIOSTAJAMAR] ([IDUSUARIO], [NOMBRE], [APELLIDOS], [EMAIL], [ESTADO], [IMAGEN], [PASSWORD], [IDROLE]) VALUES (70, N'Daniel', N'García Valencia', N'daniel.garciavalencia@tajamar365.com', 1, N'https://cdn.pixabay.com/photo/2017/11/10/05/48/user-2935527_640.png', N'OFi9W3uf5kGsaxyTqF1ZFQ==', 2);
INSERT INTO [dbo].[USUARIOSTAJAMAR] ([IDUSUARIO], [NOMBRE], [APELLIDOS], [EMAIL], [ESTADO], [IMAGEN], [PASSWORD], [IDROLE]) VALUES (71, N'Marta', N'Fraile Jara', N'marta.fraile@tajamar365.com', 1, N'https://cdn.pixabay.com/photo/2017/11/10/05/48/user-2935527_640.png', N'OFi9W3uf5kGsaxyTqF1ZFQ==', 2);
INSERT INTO [dbo].[USUARIOSTAJAMAR] ([IDUSUARIO], [NOMBRE], [APELLIDOS], [EMAIL], [ESTADO], [IMAGEN], [PASSWORD], [IDROLE]) VALUES (72, N'Tsuen Kit', N'Lui Lin', N'tsuenkit.lui@tajamar365.com', 1, N'https://cdn.pixabay.com/photo/2017/11/10/05/48/user-2935527_640.png', N'OFi9W3uf5kGsaxyTqF1ZFQ==', 2);
INSERT INTO [dbo].[USUARIOSTAJAMAR] ([IDUSUARIO], [NOMBRE], [APELLIDOS], [EMAIL], [ESTADO], [IMAGEN], [PASSWORD], [IDROLE]) VALUES (73, N'Hugo', N'De Argila Rivera', N'hugo.deargila@tajamar365.com', 1, N'https://cdn.pixabay.com/photo/2017/11/10/05/48/user-2935527_640.png', N'OFi9W3uf5kGsaxyTqF1ZFQ==', 2);
INSERT INTO [dbo].[USUARIOSTAJAMAR] ([IDUSUARIO], [NOMBRE], [APELLIDOS], [EMAIL], [ESTADO], [IMAGEN], [PASSWORD], [IDROLE]) VALUES (74, N'Daniel', N'Mellado Vega', N'daniel.mellado@tajamar365.com', 1, N'https://cdn.pixabay.com/photo/2017/11/10/05/48/user-2935527_640.png', N'OFi9W3uf5kGsaxyTqF1ZFQ==', 2);
INSERT INTO [dbo].[USUARIOSTAJAMAR] ([IDUSUARIO], [NOMBRE], [APELLIDOS], [EMAIL], [ESTADO], [IMAGEN], [PASSWORD], [IDROLE]) VALUES (75, N'Sergio', N'Simón Fernández', N'sergio.simon@tajamar365.com', 1, N'https://cdn.pixabay.com/photo/2017/11/10/05/48/user-2935527_640.png', N'OFi9W3uf5kGsaxyTqF1ZFQ==', 2);
INSERT INTO [dbo].[USUARIOSTAJAMAR] ([IDUSUARIO], [NOMBRE], [APELLIDOS], [EMAIL], [ESTADO], [IMAGEN], [PASSWORD], [IDROLE]) VALUES (76, N'Eduardo', N'Corpa del Álamo', N'eduardo.corpa@tajamar365.com', 1, N'https://cdn.pixabay.com/photo/2017/11/10/05/48/user-2935527_640.png', N'OFi9W3uf5kGsaxyTqF1ZFQ==', 2);
INSERT INTO [dbo].[USUARIOSTAJAMAR] ([IDUSUARIO], [NOMBRE], [APELLIDOS], [EMAIL], [ESTADO], [IMAGEN], [PASSWORD], [IDROLE]) VALUES (77, N'Alejandro', N'Rodríguez Diego', N'alejandro.rodriguezdiego@tajamar365.com', 1, N'https://cdn.pixabay.com/photo/2017/11/10/05/48/user-2935527_640.png', N'OFi9W3uf5kGsaxyTqF1ZFQ==', 2);
INSERT INTO [dbo].[USUARIOSTAJAMAR] ([IDUSUARIO], [NOMBRE], [APELLIDOS], [EMAIL], [ESTADO], [IMAGEN], [PASSWORD], [IDROLE]) VALUES (78, N'Alvaro', N'Diez Morales', N'alvaro.diez@tajamar365.com', 1, N'https://cdn.pixabay.com/photo/2017/11/10/05/48/user-2935527_640.png', N'OFi9W3uf5kGsaxyTqF1ZFQ==', 2);
INSERT INTO [dbo].[USUARIOSTAJAMAR] ([IDUSUARIO], [NOMBRE], [APELLIDOS], [EMAIL], [ESTADO], [IMAGEN], [PASSWORD], [IDROLE]) VALUES (79, N'Carlos', N'Alonso', N'carlos.alonso@tajamar365.com', 1, N'https://cdn.pixabay.com/photo/2017/11/10/05/48/user-2935527_640.png', N'OFi9W3uf5kGsaxyTqF1ZFQ==', 2);
INSERT INTO [dbo].[USUARIOSTAJAMAR] ([IDUSUARIO], [NOMBRE], [APELLIDOS], [EMAIL], [ESTADO], [IMAGEN], [PASSWORD], [IDROLE]) VALUES (80, N'Mario', N'Santana Antunes', N'mario.santana@tajamar365.com', 1, N'https://cdn.pixabay.com/photo/2017/11/10/05/48/user-2935527_640.png', N'OFi9W3uf5kGsaxyTqF1ZFQ==', 2);
INSERT INTO [dbo].[USUARIOSTAJAMAR] ([IDUSUARIO], [NOMBRE], [APELLIDOS], [EMAIL], [ESTADO], [IMAGEN], [PASSWORD], [IDROLE]) VALUES (81, N'Samuel', N'Barahona Barrabino', N'samuel.barahona@tajamar365.com', 1, N'https://cdn.pixabay.com/photo/2017/11/10/05/48/user-2935527_640.png', N'OFi9W3uf5kGsaxyTqF1ZFQ==', 2);
INSERT INTO [dbo].[USUARIOSTAJAMAR] ([IDUSUARIO], [NOMBRE], [APELLIDOS], [EMAIL], [ESTADO], [IMAGEN], [PASSWORD], [IDROLE]) VALUES (82, N'María', N'Miguel Tolosana', N'maria.miguel@tajamar365.com', 1, N'https://cdn.pixabay.com/photo/2017/11/10/05/48/user-2935527_640.png', N'OFi9W3uf5kGsaxyTqF1ZFQ==', 2);
INSERT INTO [dbo].[USUARIOSTAJAMAR] ([IDUSUARIO], [NOMBRE], [APELLIDOS], [EMAIL], [ESTADO], [IMAGEN], [PASSWORD], [IDROLE]) VALUES (83, N'Alejandro', N'Ruiz García', N'alejandro.ruizgarcia@tajamar365.com', 1, N'https://cdn.pixabay.com/photo/2017/11/10/05/48/user-2935527_640.png', N'OFi9W3uf5kGsaxyTqF1ZFQ==', 2);
INSERT INTO [dbo].[USUARIOSTAJAMAR] ([IDUSUARIO], [NOMBRE], [APELLIDOS], [EMAIL], [ESTADO], [IMAGEN], [PASSWORD], [IDROLE]) VALUES (84, N'Alejandro', N'Cobo Marcos', N'alejandro.cobo@tajamar365.com', 1, N'https://cdn.pixabay.com/photo/2017/11/10/05/48/user-2935527_640.png', N'OFi9W3uf5kGsaxyTqF1ZFQ==', 2);
INSERT INTO [dbo].[USUARIOSTAJAMAR] ([IDUSUARIO], [NOMBRE], [APELLIDOS], [EMAIL], [ESTADO], [IMAGEN], [PASSWORD], [IDROLE]) VALUES (85, N'Luis Miguel', N'Cañizares Diaz', N'Luismiguel.canizares@tajamar365.com', 1, N'https://cdn.pixabay.com/photo/2017/11/10/05/48/user-2935527_640.png', N'OFi9W3uf5kGsaxyTqF1ZFQ==', 2);
INSERT INTO [dbo].[USUARIOSTAJAMAR] ([IDUSUARIO], [NOMBRE], [APELLIDOS], [EMAIL], [ESTADO], [IMAGEN], [PASSWORD], [IDROLE]) VALUES (86, N'Alberto', N'Barbacid', N'alberto.barbacid@tajamar365.com', 1, N'https://cdn.pixabay.com/photo/2017/11/10/05/48/user-2935527_640.png', N'OFi9W3uf5kGsaxyTqF1ZFQ==', 2);
INSERT INTO [dbo].[USUARIOSTAJAMAR] ([IDUSUARIO], [NOMBRE], [APELLIDOS], [EMAIL], [ESTADO], [IMAGEN], [PASSWORD], [IDROLE]) VALUES (87, N'Alonso', N'García Martín', N'alonso.garcia@tajamar365.com', 1, N'https://cdn.pixabay.com/photo/2017/11/10/05/48/user-2935527_640.png', N'OFi9W3uf5kGsaxyTqF1ZFQ==', 2);
INSERT INTO [dbo].[USUARIOSTAJAMAR] ([IDUSUARIO], [NOMBRE], [APELLIDOS], [EMAIL], [ESTADO], [IMAGEN], [PASSWORD], [IDROLE]) VALUES (88, N'Diego', N'Cardona Hernandez', N'diego.cardona@tajamar365.com', 1, N'https://cdn.pixabay.com/photo/2017/11/10/05/48/user-2935527_640.png', N'OFi9W3uf5kGsaxyTqF1ZFQ==', 2);
INSERT INTO [dbo].[USUARIOSTAJAMAR] ([IDUSUARIO], [NOMBRE], [APELLIDOS], [EMAIL], [ESTADO], [IMAGEN], [PASSWORD], [IDROLE]) VALUES (89, N'Adrian', N'Jacek', N'adrian.jacek@tajamar365.com', 1, N'https://apicharlasalumnotajamar.azurewebsites.net/images/users/89_user.png', N'OFi9W3uf5kGsaxyTqF1ZFQ==', 2);
INSERT INTO [dbo].[USUARIOSTAJAMAR] ([IDUSUARIO], [NOMBRE], [APELLIDOS], [EMAIL], [ESTADO], [IMAGEN], [PASSWORD], [IDROLE]) VALUES (90, N'Héctor', N'Gil Fuertes', N'hector.gil@tajamar365.com', 1, N'https://cdn.pixabay.com/photo/2017/11/10/05/48/user-2935527_640.png', N'OFi9W3uf5kGsaxyTqF1ZFQ==', 2);
INSERT INTO [dbo].[USUARIOSTAJAMAR] ([IDUSUARIO], [NOMBRE], [APELLIDOS], [EMAIL], [ESTADO], [IMAGEN], [PASSWORD], [IDROLE]) VALUES (91, N'Julio Alejandro', N'Ordoñez Rimacuna', N'julioalejandro.ordonez@tajamar365.com', 1, N'https://cdn.pixabay.com/photo/2017/11/10/05/48/user-2935527_640.png', N'OFi9W3uf5kGsaxyTqF1ZFQ==', 2);
INSERT INTO [dbo].[USUARIOSTAJAMAR] ([IDUSUARIO], [NOMBRE], [APELLIDOS], [EMAIL], [ESTADO], [IMAGEN], [PASSWORD], [IDROLE]) VALUES (92, N'Álvaro', N'Casco Valero', N'alvaro.casco@tajamar365.com', 1, N'https://cdn.pixabay.com/photo/2017/11/10/05/48/user-2935527_640.png', N'OFi9W3uf5kGsaxyTqF1ZFQ==', 2);
INSERT INTO [dbo].[USUARIOSTAJAMAR] ([IDUSUARIO], [NOMBRE], [APELLIDOS], [EMAIL], [ESTADO], [IMAGEN], [PASSWORD], [IDROLE]) VALUES (93, N'Angel', N'Pinto Diaz', N'angel.pinto@tajamar365.com', 1, N'https://cdn.pixabay.com/photo/2017/11/10/05/48/user-2935527_640.png', N'OFi9W3uf5kGsaxyTqF1ZFQ==', 2);
INSERT INTO [dbo].[USUARIOSTAJAMAR] ([IDUSUARIO], [NOMBRE], [APELLIDOS], [EMAIL], [ESTADO], [IMAGEN], [PASSWORD], [IDROLE]) VALUES (94, N'Raúl', N'García Muñoz', N'raul.garciamunoz@tajamar365.com', 1, N'https://cdn.pixabay.com/photo/2017/11/10/05/48/user-2935527_640.png', N'OFi9W3uf5kGsaxyTqF1ZFQ==', 2);
INSERT INTO [dbo].[USUARIOSTAJAMAR] ([IDUSUARIO], [NOMBRE], [APELLIDOS], [EMAIL], [ESTADO], [IMAGEN], [PASSWORD], [IDROLE]) VALUES (95, N'Diego', N'Pérez Gregorio', N'diego.perez@tajamar365.com', 1, N'https://cdn.pixabay.com/photo/2017/11/10/05/48/user-2935527_640.png', N'OFi9W3uf5kGsaxyTqF1ZFQ==', 2);
INSERT INTO [dbo].[USUARIOSTAJAMAR] ([IDUSUARIO], [NOMBRE], [APELLIDOS], [EMAIL], [ESTADO], [IMAGEN], [PASSWORD], [IDROLE]) VALUES (96, N'Jorge', N'Rodríguez Alonso', N'jorge.rodriguezalonso@tajamar365.com', 1, N'https://cdn.pixabay.com/photo/2017/11/10/05/48/user-2935527_640.png', N'OFi9W3uf5kGsaxyTqF1ZFQ==', 2);
INSERT INTO [dbo].[USUARIOSTAJAMAR] ([IDUSUARIO], [NOMBRE], [APELLIDOS], [EMAIL], [ESTADO], [IMAGEN], [PASSWORD], [IDROLE]) VALUES (97, N'Alejandro', N'Navarro', N'alejandro.navarro@tajamar365.com', 1, N'https://apicharlasalumnotajamar.azurewebsites.net/images/users/97_user.jfif', N'OFi9W3uf5kGsaxyTqF1ZFQ==', 2);
INSERT INTO [dbo].[USUARIOSTAJAMAR] ([IDUSUARIO], [NOMBRE], [APELLIDOS], [EMAIL], [ESTADO], [IMAGEN], [PASSWORD], [IDROLE]) VALUES (98, N'Jose Antonio', N'López Pachón', N'joseantonio.lopez@tajamar365.com', 1, N'https://cdn.pixabay.com/photo/2017/11/10/05/48/user-2935527_640.png', N'OFi9W3uf5kGsaxyTqF1ZFQ==', 2);
INSERT INTO [dbo].[USUARIOSTAJAMAR] ([IDUSUARIO], [NOMBRE], [APELLIDOS], [EMAIL], [ESTADO], [IMAGEN], [PASSWORD], [IDROLE]) VALUES (99, N'Marta', N'Quirós Martín-Portugués', N'marta.quiros@tajamar365.com', 1, N'https://cdn.pixabay.com/photo/2017/11/10/05/48/user-2935527_640.png', N'OFi9W3uf5kGsaxyTqF1ZFQ==', 2);
INSERT INTO [dbo].[USUARIOSTAJAMAR] ([IDUSUARIO], [NOMBRE], [APELLIDOS], [EMAIL], [ESTADO], [IMAGEN], [PASSWORD], [IDROLE]) VALUES (100, N'Pablo', N'Gonzalo Lucas', N'pablo.gonzalo@tajamar365.com', 1, N'https://cdn.pixabay.com/photo/2017/11/10/05/48/user-2935527_640.png', N'OFi9W3uf5kGsaxyTqF1ZFQ==', 2);
INSERT INTO [dbo].[USUARIOSTAJAMAR] ([IDUSUARIO], [NOMBRE], [APELLIDOS], [EMAIL], [ESTADO], [IMAGEN], [PASSWORD], [IDROLE]) VALUES (101, N'Juan', N'Solís Torrijos', N'juan.solis@tajamar365.com', 1, N'https://cdn.pixabay.com/photo/2017/11/10/05/48/user-2935527_640.png', N'OFi9W3uf5kGsaxyTqF1ZFQ==', 2);
INSERT INTO [dbo].[USUARIOSTAJAMAR] ([IDUSUARIO], [NOMBRE], [APELLIDOS], [EMAIL], [ESTADO], [IMAGEN], [PASSWORD], [IDROLE]) VALUES (102, N'Marcos', N'Pedroche Pérez', N'marcos.pedroche@tajamar365.com', 1, N'https://cdn.pixabay.com/photo/2017/11/10/05/48/user-2935527_640.png', N'OFi9W3uf5kGsaxyTqF1ZFQ==', 2);
INSERT INTO [dbo].[USUARIOSTAJAMAR] ([IDUSUARIO], [NOMBRE], [APELLIDOS], [EMAIL], [ESTADO], [IMAGEN], [PASSWORD], [IDROLE]) VALUES (103, N'Alberto', N'Rodriguez-Rey', N'alberto.rodriguez-rey@tajamar365.com', 1, N'https://cdn.pixabay.com/photo/2017/11/10/05/48/user-2935527_640.png', N'OFi9W3uf5kGsaxyTqF1ZFQ==', 2);
INSERT INTO [dbo].[USUARIOSTAJAMAR] ([IDUSUARIO], [NOMBRE], [APELLIDOS], [EMAIL], [ESTADO], [IMAGEN], [PASSWORD], [IDROLE]) VALUES (104, N'Kevin Sebastián', N'Bayas Sarzosa', N'kevin.bayas@tajamar365.com', 1, N'https://cdn.pixabay.com/photo/2017/11/10/05/48/user-2935527_640.png', N'OFi9W3uf5kGsaxyTqF1ZFQ==', 2);
INSERT INTO [dbo].[USUARIOSTAJAMAR] ([IDUSUARIO], [NOMBRE], [APELLIDOS], [EMAIL], [ESTADO], [IMAGEN], [PASSWORD], [IDROLE]) VALUES (106, N'Alejandro', N'Cánovas López', N'alejandro.canovas@tajamar365.com', 1, N'https://apicharlasalumnotajamar.azurewebsites.net/images/users/106_user.png', N'OFi9W3uf5kGsaxyTqF1ZFQ==', 2);
INSERT INTO [dbo].[USUARIOSTAJAMAR] ([IDUSUARIO], [NOMBRE], [APELLIDOS], [EMAIL], [ESTADO], [IMAGEN], [PASSWORD], [IDROLE]) VALUES (107, N'Javier', N'Alonso Mansilla', N'javier.alonsomansilla@tajamar365.com', 1, N'https://cdn.pixabay.com/photo/2017/11/10/05/48/user-2935527_640.png', N'OFi9W3uf5kGsaxyTqF1ZFQ==', 2);
INSERT INTO [dbo].[USUARIOSTAJAMAR] ([IDUSUARIO], [NOMBRE], [APELLIDOS], [EMAIL], [ESTADO], [IMAGEN], [PASSWORD], [IDROLE]) VALUES (108, N'Asil', N'Galan', N'asil.galan@tajamar365.com', 1, N'https://cdn.pixabay.com/photo/2017/11/10/05/48/user-2935527_640.png', N'OFi9W3uf5kGsaxyTqF1ZFQ==', 2);
INSERT INTO [dbo].[USUARIOSTAJAMAR] ([IDUSUARIO], [NOMBRE], [APELLIDOS], [EMAIL], [ESTADO], [IMAGEN], [PASSWORD], [IDROLE]) VALUES (109, N'Alejandro', N'Amores Fraile', N'alejandro.amores@tajamar365.com', 1, N'https://cdn.pixabay.com/photo/2017/11/10/05/48/user-2935527_640.png', N'OFi9W3uf5kGsaxyTqF1ZFQ==', 2);
INSERT INTO [dbo].[USUARIOSTAJAMAR] ([IDUSUARIO], [NOMBRE], [APELLIDOS], [EMAIL], [ESTADO], [IMAGEN], [PASSWORD], [IDROLE]) VALUES (110, N'Ivan', N'Vazquez', N'ivan.vazquez@tajamar365.com', 1, N'https://cdn.pixabay.com/photo/2017/11/10/05/48/user-2935527_640.png', N'OFi9W3uf5kGsaxyTqF1ZFQ==', 2);
INSERT INTO [dbo].[USUARIOSTAJAMAR] ([IDUSUARIO], [NOMBRE], [APELLIDOS], [EMAIL], [ESTADO], [IMAGEN], [PASSWORD], [IDROLE]) VALUES (111, N'Alumno', N'Test', N'alumnotest@tajamar365.com', 1, N'https://cdn.pixabay.com/photo/2017/11/10/05/48/user-2935527_640.png', N'OFi9W3uf5kGsaxyTqF1ZFQ==', 2);
INSERT INTO [dbo].[USUARIOSTAJAMAR] ([IDUSUARIO], [NOMBRE], [APELLIDOS], [EMAIL], [ESTADO], [IMAGEN], [PASSWORD], [IDROLE]) VALUES (112, N'Marisol ', N'Bao ', N'marisol.bao@tajamar365.com', 1, N'https://cdn.pixabay.com/photo/2017/11/10/05/48/user-2935527_640.png', N'OFi9W3uf5kGsaxyTqF1ZFQ==', 1);
INSERT INTO [dbo].[USUARIOSTAJAMAR] ([IDUSUARIO], [NOMBRE], [APELLIDOS], [EMAIL], [ESTADO], [IMAGEN], [PASSWORD], [IDROLE]) VALUES (113, N'Manuel', N'Diaz Bernal', N'Manuel.Diaz@tajamar365.com', 1, N'https://cdn.pixabay.com/photo/2017/11/10/05/48/user-2935527_640.png', N'OFi9W3uf5kGsaxyTqF1ZFQ==', 1);
INSERT INTO [dbo].[USUARIOSTAJAMAR] ([IDUSUARIO], [NOMBRE], [APELLIDOS], [EMAIL], [ESTADO], [IMAGEN], [PASSWORD], [IDROLE]) VALUES (114, N'Marco ', N'Carrasco Talan', N'marco.carrasco@tajamar365.com', 1, N'https://cdn.pixabay.com/photo/2017/11/10/05/48/user-2935527_640.png', N'OFi9W3uf5kGsaxyTqF1ZFQ==', 1);
INSERT INTO [dbo].[USUARIOSTAJAMAR] ([IDUSUARIO], [NOMBRE], [APELLIDOS], [EMAIL], [ESTADO], [IMAGEN], [PASSWORD], [IDROLE]) VALUES (115, N'Miguel', N'González Rubio', N'mangel.gonzalez@tajamar365.com', 1, N'https://cdn.pixabay.com/photo/2017/11/10/05/48/user-2935527_640.png', N'OFi9W3uf5kGsaxyTqF1ZFQ==', 1);
INSERT INTO [dbo].[USUARIOSTAJAMAR] ([IDUSUARIO], [NOMBRE], [APELLIDOS], [EMAIL], [ESTADO], [IMAGEN], [PASSWORD], [IDROLE]) VALUES (116, N'JAVIER', N'VAZQUEZ', N'Javier.Vazquez@tajamar365.com', 1, N'https://apicharlasalumnotajamar.azurewebsites.net/images/users/116_user.jpg', N'OFi9W3uf5kGsaxyTqF1ZFQ==', 1);
INSERT INTO [dbo].[USUARIOSTAJAMAR] ([IDUSUARIO], [NOMBRE], [APELLIDOS], [EMAIL], [ESTADO], [IMAGEN], [PASSWORD], [IDROLE]) VALUES (117, N'Alfredo', N'Blanco Garcia', N'alfredo.blancogarcia@tajamar365.com', 1, N'https://apicharlasalumnotajamar.azurewebsites.net/images/users/117_user.jfif', N'OFi9W3uf5kGsaxyTqF1ZFQ==', 2);
INSERT INTO [dbo].[USUARIOSTAJAMAR] ([IDUSUARIO], [NOMBRE], [APELLIDOS], [EMAIL], [ESTADO], [IMAGEN], [PASSWORD], [IDROLE]) VALUES (118, N'Daniel', N'Pérez Diaz', N'daniel.perezdiaz@tajamar365.com', 1, N'https://cdn.pixabay.com/photo/2017/11/10/05/48/user-2935527_640.png', N'OFi9W3uf5kGsaxyTqF1ZFQ==', 2);
INSERT INTO [dbo].[USUARIOSTAJAMAR] ([IDUSUARIO], [NOMBRE], [APELLIDOS], [EMAIL], [ESTADO], [IMAGEN], [PASSWORD], [IDROLE]) VALUES (119, N'Oscar', N'Lopez', N'oscar.lopezalcala@tajamar365.com', 1, N'https://apicharlasalumnotajamar.azurewebsites.net/images/users/119_user.jpg', N'OFi9W3uf5kGsaxyTqF1ZFQ==', 2);
INSERT INTO [dbo].[USUARIOSTAJAMAR] ([IDUSUARIO], [NOMBRE], [APELLIDOS], [EMAIL], [ESTADO], [IMAGEN], [PASSWORD], [IDROLE]) VALUES (120, N'Rodrigo', N'Ramírez López', N'rodrigo.ramirez@tajamar365.com', 1, N'https://apicharlasalumnotajamar.azurewebsites.net/images/users/120_user.png', N'OFi9W3uf5kGsaxyTqF1ZFQ==', 2);
INSERT INTO [dbo].[USUARIOSTAJAMAR] ([IDUSUARIO], [NOMBRE], [APELLIDOS], [EMAIL], [ESTADO], [IMAGEN], [PASSWORD], [IDROLE]) VALUES (121, N'Luis', N'Infantes Lacal', N'Luis.infantes@tajamar365.com', 1, N'https://apicharlasalumnotajamar.azurewebsites.net/images/users/121_user.jpeg', N'OFi9W3uf5kGsaxyTqF1ZFQ==', 2);
INSERT INTO [dbo].[USUARIOSTAJAMAR] ([IDUSUARIO], [NOMBRE], [APELLIDOS], [EMAIL], [ESTADO], [IMAGEN], [PASSWORD], [IDROLE]) VALUES (122, N'Cristian', N'Jimenez', N'cristianjavier.jimenez@tajamar365.com', 1, N'https://cdn.pixabay.com/photo/2017/11/10/05/48/user-2935527_640.png', N'OFi9W3uf5kGsaxyTqF1ZFQ==', 2);
INSERT INTO [dbo].[USUARIOSTAJAMAR] ([IDUSUARIO], [NOMBRE], [APELLIDOS], [EMAIL], [ESTADO], [IMAGEN], [PASSWORD], [IDROLE]) VALUES (123, N'Pedro', N'Álvarez', N'pedro.ruiz@tajamar365.com', 1, N'https://apicharlasalumnotajamar.azurewebsites.net/images/users/123_user.jpg', N'OFi9W3uf5kGsaxyTqF1ZFQ==', 2);
INSERT INTO [dbo].[USUARIOSTAJAMAR] ([IDUSUARIO], [NOMBRE], [APELLIDOS], [EMAIL], [ESTADO], [IMAGEN], [PASSWORD], [IDROLE]) VALUES (124, N'Elizabeth', N'Sáenz Camacho', N'elizabeth.saenz@tajamar365.com', 1, N'https://apicharlasalumnotajamar.azurewebsites.net/images/users/124_user.jpeg', N'OFi9W3uf5kGsaxyTqF1ZFQ==', 2);
INSERT INTO [dbo].[USUARIOSTAJAMAR] ([IDUSUARIO], [NOMBRE], [APELLIDOS], [EMAIL], [ESTADO], [IMAGEN], [PASSWORD], [IDROLE]) VALUES (125, N'Julian', N'Calvo', N'julian.calvo@tajamar365.com', 1, N'https://apicharlasalumnotajamar.azurewebsites.net/images/users/125_user.jfif', N'OFi9W3uf5kGsaxyTqF1ZFQ==', 2);
INSERT INTO [dbo].[USUARIOSTAJAMAR] ([IDUSUARIO], [NOMBRE], [APELLIDOS], [EMAIL], [ESTADO], [IMAGEN], [PASSWORD], [IDROLE]) VALUES (126, N'Álvaro', N'Buiza', N'alvaro.buiza@tajamar365.com', 1, N'https://cdn.pixabay.com/photo/2017/11/10/05/48/user-2935527_640.png', N'OFi9W3uf5kGsaxyTqF1ZFQ==', 2);
INSERT INTO [dbo].[USUARIOSTAJAMAR] ([IDUSUARIO], [NOMBRE], [APELLIDOS], [EMAIL], [ESTADO], [IMAGEN], [PASSWORD], [IDROLE]) VALUES (127, N'sergio', N'navas', N'sergio.navas@tajamar365.com', 1, N'https://cdn.pixabay.com/photo/2017/11/10/05/48/user-2935527_640.png', N'OFi9W3uf5kGsaxyTqF1ZFQ==', 2);
INSERT INTO [dbo].[USUARIOSTAJAMAR] ([IDUSUARIO], [NOMBRE], [APELLIDOS], [EMAIL], [ESTADO], [IMAGEN], [PASSWORD], [IDROLE]) VALUES (128, N'Salima', N'Agmir', N'salima.agmir@tajamar365.com', 1, N'https://apicharlasalumnotajamar.azurewebsites.net/images/users/128_user.jpeg', N'OFi9W3uf5kGsaxyTqF1ZFQ==', 2);
INSERT INTO [dbo].[USUARIOSTAJAMAR] ([IDUSUARIO], [NOMBRE], [APELLIDOS], [EMAIL], [ESTADO], [IMAGEN], [PASSWORD], [IDROLE]) VALUES (129, N'Jorge', N'Mori Felipe', N'jorge.mori@tajamar365.com', 1, N'https://apicharlasalumnotajamar.azurewebsites.net/images/users/129_user.jpg', N'OFi9W3uf5kGsaxyTqF1ZFQ==', 2);
INSERT INTO [dbo].[USUARIOSTAJAMAR] ([IDUSUARIO], [NOMBRE], [APELLIDOS], [EMAIL], [ESTADO], [IMAGEN], [PASSWORD], [IDROLE]) VALUES (130, N'Arturo', N'Yañez Gomez', N'arturo.yanez@tajamar365.com', 1, N'https://cdn.pixabay.com/photo/2017/11/10/05/48/user-2935527_640.png', N'OFi9W3uf5kGsaxyTqF1ZFQ==', 2);
INSERT INTO [dbo].[USUARIOSTAJAMAR] ([IDUSUARIO], [NOMBRE], [APELLIDOS], [EMAIL], [ESTADO], [IMAGEN], [PASSWORD], [IDROLE]) VALUES (131, N'Giovanny ', N'Panesso Rodriguez', N'giovanny.panesso@tajamar365.com', 1, N'https://cdn.pixabay.com/photo/2017/11/10/05/48/user-2935527_640.png', N'OFi9W3uf5kGsaxyTqF1ZFQ==', 2);
INSERT INTO [dbo].[USUARIOSTAJAMAR] ([IDUSUARIO], [NOMBRE], [APELLIDOS], [EMAIL], [ESTADO], [IMAGEN], [PASSWORD], [IDROLE]) VALUES (132, N'Daniel', N'Talavera Clemente', N'daniel.talavera@tajamar365.com', 1, N'https://cdn.pixabay.com/photo/2017/11/10/05/48/user-2935527_640.png', N'OFi9W3uf5kGsaxyTqF1ZFQ==', 2);
INSERT INTO [dbo].[USUARIOSTAJAMAR] ([IDUSUARIO], [NOMBRE], [APELLIDOS], [EMAIL], [ESTADO], [IMAGEN], [PASSWORD], [IDROLE]) VALUES (133, N'Ruben', N'Gomez-Lobo Fuentes', N'ruben.gomez-lobo@tajamar365.com', 1, N'https://apicharlasalumnotajamar.azurewebsites.net/images/users/133_user.png', N'OFi9W3uf5kGsaxyTqF1ZFQ==', 2);
INSERT INTO [dbo].[USUARIOSTAJAMAR] ([IDUSUARIO], [NOMBRE], [APELLIDOS], [EMAIL], [ESTADO], [IMAGEN], [PASSWORD], [IDROLE]) VALUES (134, N'Jorge', N'Salas Rodriguez', N'jorge.salas@tajamar365.com', 1, N'https://apicharlasalumnotajamar.azurewebsites.net/images/users/134_user.jpg', N'OFi9W3uf5kGsaxyTqF1ZFQ==', 2);
INSERT INTO [dbo].[USUARIOSTAJAMAR] ([IDUSUARIO], [NOMBRE], [APELLIDOS], [EMAIL], [ESTADO], [IMAGEN], [PASSWORD], [IDROLE]) VALUES (135, N'Luis Andres', N'Martinez Berraquerro', N'luisandres.martinez@tajamar365.com', 1, N'https://apicharlasalumnotajamar.azurewebsites.net/images/users/135_user.jpg', N'OFi9W3uf5kGsaxyTqF1ZFQ==', 2);
INSERT INTO [dbo].[USUARIOSTAJAMAR] ([IDUSUARIO], [NOMBRE], [APELLIDOS], [EMAIL], [ESTADO], [IMAGEN], [PASSWORD], [IDROLE]) VALUES (136, N'Heily Madelay', N'Ajila Tandazo', N'heilymadelay.ajila@tajamar365.com', 1, N'https://apicharlasalumnotajamar.azurewebsites.net/images/users/136_user.png', N'OFi9W3uf5kGsaxyTqF1ZFQ==', 2);
INSERT INTO [dbo].[USUARIOSTAJAMAR] ([IDUSUARIO], [NOMBRE], [APELLIDOS], [EMAIL], [ESTADO], [IMAGEN], [PASSWORD], [IDROLE]) VALUES (137, N'David', N'Martínez Estébanez', N'david.martinezestebanez@tajamar365.com', 1, N'https://apicharlasalumnotajamar.azurewebsites.net/images/users/137_user.png', N'OFi9W3uf5kGsaxyTqF1ZFQ==', 2);
INSERT INTO [dbo].[USUARIOSTAJAMAR] ([IDUSUARIO], [NOMBRE], [APELLIDOS], [EMAIL], [ESTADO], [IMAGEN], [PASSWORD], [IDROLE]) VALUES (138, N'Sofia', N'Rojas', N'sofia.rojas@tajamar365.com', 1, N'https://apicharlasalumnotajamar.azurewebsites.net/images/users/138_user.jpg', N'OFi9W3uf5kGsaxyTqF1ZFQ==', 2);
INSERT INTO [dbo].[USUARIOSTAJAMAR] ([IDUSUARIO], [NOMBRE], [APELLIDOS], [EMAIL], [ESTADO], [IMAGEN], [PASSWORD], [IDROLE]) VALUES (139, N'Gerson', N'Luque Huanca', N'gerson.luque@tajamar365.com', 1, N'https://cdn.pixabay.com/photo/2017/11/10/05/48/user-2935527_640.png', N'OFi9W3uf5kGsaxyTqF1ZFQ==', 2);
INSERT INTO [dbo].[USUARIOSTAJAMAR] ([IDUSUARIO], [NOMBRE], [APELLIDOS], [EMAIL], [ESTADO], [IMAGEN], [PASSWORD], [IDROLE]) VALUES (140, N'Juan Jose', N'Rodríguez Baza', N'juanjose.rodriguez@tajamar365.com', 1, N'https://cdn.pixabay.com/photo/2017/11/10/05/48/user-2935527_640.png', N'OFi9W3uf5kGsaxyTqF1ZFQ==', 1);
INSERT INTO [dbo].[USUARIOSTAJAMAR] ([IDUSUARIO], [NOMBRE], [APELLIDOS], [EMAIL], [ESTADO], [IMAGEN], [PASSWORD], [IDROLE]) VALUES (141, N'Mario', N'García Romero', N'mario.garciaromero@tajamar365.com', 1, N'https://cdn.pixabay.com/photo/2017/11/10/05/48/user-2935527_640.png', N'OFi9W3uf5kGsaxyTqF1ZFQ==', 2);
INSERT INTO [dbo].[USUARIOSTAJAMAR] ([IDUSUARIO], [NOMBRE], [APELLIDOS], [EMAIL], [ESTADO], [IMAGEN], [PASSWORD], [IDROLE]) VALUES (142, N'Lucian', N'Ciusa', N'lucianmarian.ciusa@tajamar365.com', 1, N'https://cdn.pixabay.com/photo/2017/11/10/05/48/user-2935527_640.png', N'OFi9W3uf5kGsaxyTqF1ZFQ==', 2);
INSERT INTO [dbo].[USUARIOSTAJAMAR] ([IDUSUARIO], [NOMBRE], [APELLIDOS], [EMAIL], [ESTADO], [IMAGEN], [PASSWORD], [IDROLE]) VALUES (143, N'Andrews', N'Dos Ramos', N'andrews.dosramos@tajamar365.com', 1, N'https://apicharlasalumnotajamar.azurewebsites.net/images/users/143_user.png', N'OFi9W3uf5kGsaxyTqF1ZFQ==', 2);
INSERT INTO [dbo].[USUARIOSTAJAMAR] ([IDUSUARIO], [NOMBRE], [APELLIDOS], [EMAIL], [ESTADO], [IMAGEN], [PASSWORD], [IDROLE]) VALUES (144, N'Sergio', N'Jimenez', N'sergio.jimenezmartin@tajamar365.com', 1, N'https://cdn.pixabay.com/photo/2017/11/10/05/48/user-2935527_640.png', N'OFi9W3uf5kGsaxyTqF1ZFQ==', 2);
INSERT INTO [dbo].[USUARIOSTAJAMAR] ([IDUSUARIO], [NOMBRE], [APELLIDOS], [EMAIL], [ESTADO], [IMAGEN], [PASSWORD], [IDROLE]) VALUES (145, N'Alvaro', N'Lopez Redondo', N'alvaro.lopezredondo@tajamar365.com', 1, N'https://cdn.pixabay.com/photo/2017/11/10/05/48/user-2935527_640.png', N'OFi9W3uf5kGsaxyTqF1ZFQ==', 2);
INSERT INTO [dbo].[USUARIOSTAJAMAR] ([IDUSUARIO], [NOMBRE], [APELLIDOS], [EMAIL], [ESTADO], [IMAGEN], [PASSWORD], [IDROLE]) VALUES (146, N'Ángel', N'Toledo Rodelgo', N'angel.toledo@tajamar365.com', 1, N'https://cdn.pixabay.com/photo/2017/11/10/05/48/user-2935527_640.png', N'OFi9W3uf5kGsaxyTqF1ZFQ==', 2);
INSERT INTO [dbo].[USUARIOSTAJAMAR] ([IDUSUARIO], [NOMBRE], [APELLIDOS], [EMAIL], [ESTADO], [IMAGEN], [PASSWORD], [IDROLE]) VALUES (147, N'Iñigo Samuel', N'Jiménez Montoro', N'inigosamuel.jimenez@tajamar365.com', 1, N'https://cdn.pixabay.com/photo/2017/11/10/05/48/user-2935527_640.png', N'OFi9W3uf5kGsaxyTqF1ZFQ==', 2);
INSERT INTO [dbo].[USUARIOSTAJAMAR] ([IDUSUARIO], [NOMBRE], [APELLIDOS], [EMAIL], [ESTADO], [IMAGEN], [PASSWORD], [IDROLE]) VALUES (148, N'Sergio', N'Rincón De La Cruz', N'sergio.rincon@tajamar365.com', 1, N'https://cdn.pixabay.com/photo/2017/11/10/05/48/user-2935527_640.png', N'OFi9W3uf5kGsaxyTqF1ZFQ==', 2);

INSERT INTO [dbo].[CURSOSUSUARIOSTAJAMAR] ([IDCURSOSUSUARIOS], [IDCURSO], [IDUSUARIO]) VALUES (1, 3213, 31);
INSERT INTO [dbo].[CURSOSUSUARIOSTAJAMAR] ([IDCURSOSUSUARIOS], [IDCURSO], [IDUSUARIO]) VALUES (2, 3213, 33);
INSERT INTO [dbo].[CURSOSUSUARIOSTAJAMAR] ([IDCURSOSUSUARIOS], [IDCURSO], [IDUSUARIO]) VALUES (3, 3213, 34);
INSERT INTO [dbo].[CURSOSUSUARIOSTAJAMAR] ([IDCURSOSUSUARIOS], [IDCURSO], [IDUSUARIO]) VALUES (4, 3213, 35);
INSERT INTO [dbo].[CURSOSUSUARIOSTAJAMAR] ([IDCURSOSUSUARIOS], [IDCURSO], [IDUSUARIO]) VALUES (5, 3213, 36);
INSERT INTO [dbo].[CURSOSUSUARIOSTAJAMAR] ([IDCURSOSUSUARIOS], [IDCURSO], [IDUSUARIO]) VALUES (6, 3213, 37);
INSERT INTO [dbo].[CURSOSUSUARIOSTAJAMAR] ([IDCURSOSUSUARIOS], [IDCURSO], [IDUSUARIO]) VALUES (7, 3213, 38);
INSERT INTO [dbo].[CURSOSUSUARIOSTAJAMAR] ([IDCURSOSUSUARIOS], [IDCURSO], [IDUSUARIO]) VALUES (8, 3213, 39);
INSERT INTO [dbo].[CURSOSUSUARIOSTAJAMAR] ([IDCURSOSUSUARIOS], [IDCURSO], [IDUSUARIO]) VALUES (10, 3213, 41);
INSERT INTO [dbo].[CURSOSUSUARIOSTAJAMAR] ([IDCURSOSUSUARIOS], [IDCURSO], [IDUSUARIO]) VALUES (11, 3213, 42);
INSERT INTO [dbo].[CURSOSUSUARIOSTAJAMAR] ([IDCURSOSUSUARIOS], [IDCURSO], [IDUSUARIO]) VALUES (12, 3213, 43);
INSERT INTO [dbo].[CURSOSUSUARIOSTAJAMAR] ([IDCURSOSUSUARIOS], [IDCURSO], [IDUSUARIO]) VALUES (13, 3213, 44);
INSERT INTO [dbo].[CURSOSUSUARIOSTAJAMAR] ([IDCURSOSUSUARIOS], [IDCURSO], [IDUSUARIO]) VALUES (14, 3213, 45);
INSERT INTO [dbo].[CURSOSUSUARIOSTAJAMAR] ([IDCURSOSUSUARIOS], [IDCURSO], [IDUSUARIO]) VALUES (15, 3213, 46);
INSERT INTO [dbo].[CURSOSUSUARIOSTAJAMAR] ([IDCURSOSUSUARIOS], [IDCURSO], [IDUSUARIO]) VALUES (16, 3213, 47);
INSERT INTO [dbo].[CURSOSUSUARIOSTAJAMAR] ([IDCURSOSUSUARIOS], [IDCURSO], [IDUSUARIO]) VALUES (17, 3213, 48);
INSERT INTO [dbo].[CURSOSUSUARIOSTAJAMAR] ([IDCURSOSUSUARIOS], [IDCURSO], [IDUSUARIO]) VALUES (18, 3213, 49);
INSERT INTO [dbo].[CURSOSUSUARIOSTAJAMAR] ([IDCURSOSUSUARIOS], [IDCURSO], [IDUSUARIO]) VALUES (19, 3213, 50);
INSERT INTO [dbo].[CURSOSUSUARIOSTAJAMAR] ([IDCURSOSUSUARIOS], [IDCURSO], [IDUSUARIO]) VALUES (20, 3213, 51);
INSERT INTO [dbo].[CURSOSUSUARIOSTAJAMAR] ([IDCURSOSUSUARIOS], [IDCURSO], [IDUSUARIO]) VALUES (21, 3213, 52);
INSERT INTO [dbo].[CURSOSUSUARIOSTAJAMAR] ([IDCURSOSUSUARIOS], [IDCURSO], [IDUSUARIO]) VALUES (23, 3213, 54);
INSERT INTO [dbo].[CURSOSUSUARIOSTAJAMAR] ([IDCURSOSUSUARIOS], [IDCURSO], [IDUSUARIO]) VALUES (24, 3213, 55);
INSERT INTO [dbo].[CURSOSUSUARIOSTAJAMAR] ([IDCURSOSUSUARIOS], [IDCURSO], [IDUSUARIO]) VALUES (25, 3213, 56);
INSERT INTO [dbo].[CURSOSUSUARIOSTAJAMAR] ([IDCURSOSUSUARIOS], [IDCURSO], [IDUSUARIO]) VALUES (26, 3213, 57);
INSERT INTO [dbo].[CURSOSUSUARIOSTAJAMAR] ([IDCURSOSUSUARIOS], [IDCURSO], [IDUSUARIO]) VALUES (27, 3213, 58);
INSERT INTO [dbo].[CURSOSUSUARIOSTAJAMAR] ([IDCURSOSUSUARIOS], [IDCURSO], [IDUSUARIO]) VALUES (50, 3430, 31);
INSERT INTO [dbo].[CURSOSUSUARIOSTAJAMAR] ([IDCURSOSUSUARIOS], [IDCURSO], [IDUSUARIO]) VALUES (51, 3430, 82);
INSERT INTO [dbo].[CURSOSUSUARIOSTAJAMAR] ([IDCURSOSUSUARIOS], [IDCURSO], [IDUSUARIO]) VALUES (52, 3430, 83);
INSERT INTO [dbo].[CURSOSUSUARIOSTAJAMAR] ([IDCURSOSUSUARIOS], [IDCURSO], [IDUSUARIO]) VALUES (53, 3430, 84);
INSERT INTO [dbo].[CURSOSUSUARIOSTAJAMAR] ([IDCURSOSUSUARIOS], [IDCURSO], [IDUSUARIO]) VALUES (54, 3430, 85);
INSERT INTO [dbo].[CURSOSUSUARIOSTAJAMAR] ([IDCURSOSUSUARIOS], [IDCURSO], [IDUSUARIO]) VALUES (55, 3430, 86);
INSERT INTO [dbo].[CURSOSUSUARIOSTAJAMAR] ([IDCURSOSUSUARIOS], [IDCURSO], [IDUSUARIO]) VALUES (56, 3430, 87);
INSERT INTO [dbo].[CURSOSUSUARIOSTAJAMAR] ([IDCURSOSUSUARIOS], [IDCURSO], [IDUSUARIO]) VALUES (57, 3430, 88);
INSERT INTO [dbo].[CURSOSUSUARIOSTAJAMAR] ([IDCURSOSUSUARIOS], [IDCURSO], [IDUSUARIO]) VALUES (58, 3430, 89);
INSERT INTO [dbo].[CURSOSUSUARIOSTAJAMAR] ([IDCURSOSUSUARIOS], [IDCURSO], [IDUSUARIO]) VALUES (59, 3430, 90);
INSERT INTO [dbo].[CURSOSUSUARIOSTAJAMAR] ([IDCURSOSUSUARIOS], [IDCURSO], [IDUSUARIO]) VALUES (60, 3430, 91);
INSERT INTO [dbo].[CURSOSUSUARIOSTAJAMAR] ([IDCURSOSUSUARIOS], [IDCURSO], [IDUSUARIO]) VALUES (61, 3430, 92);
INSERT INTO [dbo].[CURSOSUSUARIOSTAJAMAR] ([IDCURSOSUSUARIOS], [IDCURSO], [IDUSUARIO]) VALUES (62, 3430, 93);
INSERT INTO [dbo].[CURSOSUSUARIOSTAJAMAR] ([IDCURSOSUSUARIOS], [IDCURSO], [IDUSUARIO]) VALUES (63, 3430, 94);
INSERT INTO [dbo].[CURSOSUSUARIOSTAJAMAR] ([IDCURSOSUSUARIOS], [IDCURSO], [IDUSUARIO]) VALUES (64, 3430, 95);
INSERT INTO [dbo].[CURSOSUSUARIOSTAJAMAR] ([IDCURSOSUSUARIOS], [IDCURSO], [IDUSUARIO]) VALUES (65, 3430, 96);
INSERT INTO [dbo].[CURSOSUSUARIOSTAJAMAR] ([IDCURSOSUSUARIOS], [IDCURSO], [IDUSUARIO]) VALUES (66, 3430, 97);
INSERT INTO [dbo].[CURSOSUSUARIOSTAJAMAR] ([IDCURSOSUSUARIOS], [IDCURSO], [IDUSUARIO]) VALUES (67, 3430, 98);
INSERT INTO [dbo].[CURSOSUSUARIOSTAJAMAR] ([IDCURSOSUSUARIOS], [IDCURSO], [IDUSUARIO]) VALUES (68, 3430, 99);
INSERT INTO [dbo].[CURSOSUSUARIOSTAJAMAR] ([IDCURSOSUSUARIOS], [IDCURSO], [IDUSUARIO]) VALUES (69, 3430, 100);
INSERT INTO [dbo].[CURSOSUSUARIOSTAJAMAR] ([IDCURSOSUSUARIOS], [IDCURSO], [IDUSUARIO]) VALUES (70, 3430, 101);
INSERT INTO [dbo].[CURSOSUSUARIOSTAJAMAR] ([IDCURSOSUSUARIOS], [IDCURSO], [IDUSUARIO]) VALUES (71, 3430, 102);
INSERT INTO [dbo].[CURSOSUSUARIOSTAJAMAR] ([IDCURSOSUSUARIOS], [IDCURSO], [IDUSUARIO]) VALUES (72, 3430, 103);
INSERT INTO [dbo].[CURSOSUSUARIOSTAJAMAR] ([IDCURSOSUSUARIOS], [IDCURSO], [IDUSUARIO]) VALUES (73, 3430, 104);
INSERT INTO [dbo].[CURSOSUSUARIOSTAJAMAR] ([IDCURSOSUSUARIOS], [IDCURSO], [IDUSUARIO]) VALUES (75, 3430, 106);
INSERT INTO [dbo].[CURSOSUSUARIOSTAJAMAR] ([IDCURSOSUSUARIOS], [IDCURSO], [IDUSUARIO]) VALUES (76, 3430, 107);
INSERT INTO [dbo].[CURSOSUSUARIOSTAJAMAR] ([IDCURSOSUSUARIOS], [IDCURSO], [IDUSUARIO]) VALUES (77, 3430, 108);
INSERT INTO [dbo].[CURSOSUSUARIOSTAJAMAR] ([IDCURSOSUSUARIOS], [IDCURSO], [IDUSUARIO]) VALUES (78, 3430, 109);
INSERT INTO [dbo].[CURSOSUSUARIOSTAJAMAR] ([IDCURSOSUSUARIOS], [IDCURSO], [IDUSUARIO]) VALUES (79, 3430, 110);
INSERT INTO [dbo].[CURSOSUSUARIOSTAJAMAR] ([IDCURSOSUSUARIOS], [IDCURSO], [IDUSUARIO]) VALUES (80, 3430, 111);
INSERT INTO [dbo].[CURSOSUSUARIOSTAJAMAR] ([IDCURSOSUSUARIOS], [IDCURSO], [IDUSUARIO]) VALUES (105, 3431, 140);
INSERT INTO [dbo].[CURSOSUSUARIOSTAJAMAR] ([IDCURSOSUSUARIOS], [IDCURSO], [IDUSUARIO]) VALUES (106, 3431, 141);
INSERT INTO [dbo].[CURSOSUSUARIOSTAJAMAR] ([IDCURSOSUSUARIOS], [IDCURSO], [IDUSUARIO]) VALUES (107, 3431, 142);
INSERT INTO [dbo].[CURSOSUSUARIOSTAJAMAR] ([IDCURSOSUSUARIOS], [IDCURSO], [IDUSUARIO]) VALUES (108, 3431, 143);
INSERT INTO [dbo].[CURSOSUSUARIOSTAJAMAR] ([IDCURSOSUSUARIOS], [IDCURSO], [IDUSUARIO]) VALUES (109, 3431, 144);
INSERT INTO [dbo].[CURSOSUSUARIOSTAJAMAR] ([IDCURSOSUSUARIOS], [IDCURSO], [IDUSUARIO]) VALUES (110, 3431, 145);
INSERT INTO [dbo].[CURSOSUSUARIOSTAJAMAR] ([IDCURSOSUSUARIOS], [IDCURSO], [IDUSUARIO]) VALUES (111, 3431, 146);
INSERT INTO [dbo].[CURSOSUSUARIOSTAJAMAR] ([IDCURSOSUSUARIOS], [IDCURSO], [IDUSUARIO]) VALUES (112, 3431, 147);
INSERT INTO [dbo].[CURSOSUSUARIOSTAJAMAR] ([IDCURSOSUSUARIOS], [IDCURSO], [IDUSUARIO]) VALUES (113, 3431, 148);
INSERT INTO [dbo].[CURSOSUSUARIOSTAJAMAR] ([IDCURSOSUSUARIOS], [IDCURSO], [IDUSUARIO]) VALUES (81, 3434, 116);
INSERT INTO [dbo].[CURSOSUSUARIOSTAJAMAR] ([IDCURSOSUSUARIOS], [IDCURSO], [IDUSUARIO]) VALUES (82, 3434, 117);
INSERT INTO [dbo].[CURSOSUSUARIOSTAJAMAR] ([IDCURSOSUSUARIOS], [IDCURSO], [IDUSUARIO]) VALUES (83, 3434, 118);
INSERT INTO [dbo].[CURSOSUSUARIOSTAJAMAR] ([IDCURSOSUSUARIOS], [IDCURSO], [IDUSUARIO]) VALUES (84, 3434, 119);
INSERT INTO [dbo].[CURSOSUSUARIOSTAJAMAR] ([IDCURSOSUSUARIOS], [IDCURSO], [IDUSUARIO]) VALUES (85, 3434, 120);
INSERT INTO [dbo].[CURSOSUSUARIOSTAJAMAR] ([IDCURSOSUSUARIOS], [IDCURSO], [IDUSUARIO]) VALUES (86, 3434, 121);
INSERT INTO [dbo].[CURSOSUSUARIOSTAJAMAR] ([IDCURSOSUSUARIOS], [IDCURSO], [IDUSUARIO]) VALUES (87, 3434, 122);
INSERT INTO [dbo].[CURSOSUSUARIOSTAJAMAR] ([IDCURSOSUSUARIOS], [IDCURSO], [IDUSUARIO]) VALUES (88, 3434, 123);
INSERT INTO [dbo].[CURSOSUSUARIOSTAJAMAR] ([IDCURSOSUSUARIOS], [IDCURSO], [IDUSUARIO]) VALUES (89, 3434, 124);
INSERT INTO [dbo].[CURSOSUSUARIOSTAJAMAR] ([IDCURSOSUSUARIOS], [IDCURSO], [IDUSUARIO]) VALUES (90, 3434, 125);
INSERT INTO [dbo].[CURSOSUSUARIOSTAJAMAR] ([IDCURSOSUSUARIOS], [IDCURSO], [IDUSUARIO]) VALUES (91, 3434, 126);
INSERT INTO [dbo].[CURSOSUSUARIOSTAJAMAR] ([IDCURSOSUSUARIOS], [IDCURSO], [IDUSUARIO]) VALUES (92, 3434, 127);
INSERT INTO [dbo].[CURSOSUSUARIOSTAJAMAR] ([IDCURSOSUSUARIOS], [IDCURSO], [IDUSUARIO]) VALUES (93, 3434, 128);
INSERT INTO [dbo].[CURSOSUSUARIOSTAJAMAR] ([IDCURSOSUSUARIOS], [IDCURSO], [IDUSUARIO]) VALUES (94, 3434, 129);
INSERT INTO [dbo].[CURSOSUSUARIOSTAJAMAR] ([IDCURSOSUSUARIOS], [IDCURSO], [IDUSUARIO]) VALUES (95, 3434, 130);
INSERT INTO [dbo].[CURSOSUSUARIOSTAJAMAR] ([IDCURSOSUSUARIOS], [IDCURSO], [IDUSUARIO]) VALUES (96, 3434, 131);
INSERT INTO [dbo].[CURSOSUSUARIOSTAJAMAR] ([IDCURSOSUSUARIOS], [IDCURSO], [IDUSUARIO]) VALUES (97, 3434, 132);
INSERT INTO [dbo].[CURSOSUSUARIOSTAJAMAR] ([IDCURSOSUSUARIOS], [IDCURSO], [IDUSUARIO]) VALUES (98, 3434, 133);
INSERT INTO [dbo].[CURSOSUSUARIOSTAJAMAR] ([IDCURSOSUSUARIOS], [IDCURSO], [IDUSUARIO]) VALUES (99, 3434, 134);
INSERT INTO [dbo].[CURSOSUSUARIOSTAJAMAR] ([IDCURSOSUSUARIOS], [IDCURSO], [IDUSUARIO]) VALUES (100, 3434, 135);
INSERT INTO [dbo].[CURSOSUSUARIOSTAJAMAR] ([IDCURSOSUSUARIOS], [IDCURSO], [IDUSUARIO]) VALUES (101, 3434, 136);
INSERT INTO [dbo].[CURSOSUSUARIOSTAJAMAR] ([IDCURSOSUSUARIOS], [IDCURSO], [IDUSUARIO]) VALUES (102, 3434, 137);
INSERT INTO [dbo].[CURSOSUSUARIOSTAJAMAR] ([IDCURSOSUSUARIOS], [IDCURSO], [IDUSUARIO]) VALUES (103, 3434, 138);
INSERT INTO [dbo].[CURSOSUSUARIOSTAJAMAR] ([IDCURSOSUSUARIOS], [IDCURSO], [IDUSUARIO]) VALUES (104, 3434, 139);
INSERT INTO [dbo].[CURSOSUSUARIOSTAJAMAR] ([IDCURSOSUSUARIOS], [IDCURSO], [IDUSUARIO]) VALUES (28, 304158642, 59);
INSERT INTO [dbo].[CURSOSUSUARIOSTAJAMAR] ([IDCURSOSUSUARIOS], [IDCURSO], [IDUSUARIO]) VALUES (29, 304158642, 61);
INSERT INTO [dbo].[CURSOSUSUARIOSTAJAMAR] ([IDCURSOSUSUARIOS], [IDCURSO], [IDUSUARIO]) VALUES (30, 304158642, 62);
INSERT INTO [dbo].[CURSOSUSUARIOSTAJAMAR] ([IDCURSOSUSUARIOS], [IDCURSO], [IDUSUARIO]) VALUES (31, 304158642, 63);
INSERT INTO [dbo].[CURSOSUSUARIOSTAJAMAR] ([IDCURSOSUSUARIOS], [IDCURSO], [IDUSUARIO]) VALUES (32, 304158642, 64);
INSERT INTO [dbo].[CURSOSUSUARIOSTAJAMAR] ([IDCURSOSUSUARIOS], [IDCURSO], [IDUSUARIO]) VALUES (33, 304158642, 65);
INSERT INTO [dbo].[CURSOSUSUARIOSTAJAMAR] ([IDCURSOSUSUARIOS], [IDCURSO], [IDUSUARIO]) VALUES (34, 304158642, 66);
INSERT INTO [dbo].[CURSOSUSUARIOSTAJAMAR] ([IDCURSOSUSUARIOS], [IDCURSO], [IDUSUARIO]) VALUES (35, 304158642, 67);
INSERT INTO [dbo].[CURSOSUSUARIOSTAJAMAR] ([IDCURSOSUSUARIOS], [IDCURSO], [IDUSUARIO]) VALUES (36, 304158642, 68);
INSERT INTO [dbo].[CURSOSUSUARIOSTAJAMAR] ([IDCURSOSUSUARIOS], [IDCURSO], [IDUSUARIO]) VALUES (37, 304158642, 69);
INSERT INTO [dbo].[CURSOSUSUARIOSTAJAMAR] ([IDCURSOSUSUARIOS], [IDCURSO], [IDUSUARIO]) VALUES (38, 304158642, 70);
INSERT INTO [dbo].[CURSOSUSUARIOSTAJAMAR] ([IDCURSOSUSUARIOS], [IDCURSO], [IDUSUARIO]) VALUES (39, 304158642, 71);
INSERT INTO [dbo].[CURSOSUSUARIOSTAJAMAR] ([IDCURSOSUSUARIOS], [IDCURSO], [IDUSUARIO]) VALUES (40, 304158642, 72);
INSERT INTO [dbo].[CURSOSUSUARIOSTAJAMAR] ([IDCURSOSUSUARIOS], [IDCURSO], [IDUSUARIO]) VALUES (41, 304158642, 73);
INSERT INTO [dbo].[CURSOSUSUARIOSTAJAMAR] ([IDCURSOSUSUARIOS], [IDCURSO], [IDUSUARIO]) VALUES (42, 304158642, 74);
INSERT INTO [dbo].[CURSOSUSUARIOSTAJAMAR] ([IDCURSOSUSUARIOS], [IDCURSO], [IDUSUARIO]) VALUES (43, 304158642, 75);
INSERT INTO [dbo].[CURSOSUSUARIOSTAJAMAR] ([IDCURSOSUSUARIOS], [IDCURSO], [IDUSUARIO]) VALUES (44, 304158642, 76);
INSERT INTO [dbo].[CURSOSUSUARIOSTAJAMAR] ([IDCURSOSUSUARIOS], [IDCURSO], [IDUSUARIO]) VALUES (45, 304158642, 77);
INSERT INTO [dbo].[CURSOSUSUARIOSTAJAMAR] ([IDCURSOSUSUARIOS], [IDCURSO], [IDUSUARIO]) VALUES (46, 304158642, 78);
INSERT INTO [dbo].[CURSOSUSUARIOSTAJAMAR] ([IDCURSOSUSUARIOS], [IDCURSO], [IDUSUARIO]) VALUES (47, 304158642, 79);
INSERT INTO [dbo].[CURSOSUSUARIOSTAJAMAR] ([IDCURSOSUSUARIOS], [IDCURSO], [IDUSUARIO]) VALUES (48, 304158642, 80);
INSERT INTO [dbo].[CURSOSUSUARIOSTAJAMAR] ([IDCURSOSUSUARIOS], [IDCURSO], [IDUSUARIO]) VALUES (49, 304158642, 81);

COMMIT TRANSACTION;
GO
PRINT 'Migration completed: ROLESCHARLASTAJAMAR, CURSOSTAJAMAR, USUARIOSTAJAMAR, CURSOSUSUARIOSTAJAMAR, ASISTENCIATAJAMAR, CALENDARIOCURSO, DISPOSITIVOSTAJAMAR, POSICIONESTAJAMAR, DISPOSITIVOPOSICIONTAJAMAR, POSICIONUSUARIOSTAJAMAR, CHECKINSTAJAMAR.';

select * from USUARIOSTAJAMAR
select * from CURSOSTAJAMAR
select * from ROLESCHARLASTAJAMAR
select * from CURSOSUSUARIOSTAJAMAR
select * from ASISTENCIATAJAMAR
select * from CALENDARIOCURSO
select * from DISPOSITIVOSTAJAMAR
select * from POSICIONESTAJAMAR
select * from DISPOSITIVOPOSICIONTAJAMAR
select * from POSICIONUSUARIOSTAJAMAR
select * from CHECKINSTAJAMAR
