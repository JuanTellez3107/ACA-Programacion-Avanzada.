/* =============================================================================
   Sistema de Gestión de Biblioteca - Script de creación
   Base de datos: BibliotecaDB
   Motor: Microsoft SQL Server
   ============================================================================= */

IF DB_ID(N'BibliotecaDB') IS NULL
BEGIN
    CREATE DATABASE BibliotecaDB;
END
GO

USE BibliotecaDB;
GO

IF OBJECT_ID(N'dbo.Prestamos', N'U') IS NULL
    AND OBJECT_ID(N'dbo.LibroAutores', N'U') IS NULL
    AND OBJECT_ID(N'dbo.Libros', N'U') IS NULL
BEGIN
    PRINT N'Creando esquema inicial de BibliotecaDB...';
END
GO

IF OBJECT_ID(N'dbo.Editoriales', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Editoriales
    (
        IdEditorial        INT IDENTITY(1,1) NOT NULL,
        Nombre             NVARCHAR(150)     NOT NULL,
        Pais               NVARCHAR(80)      NULL,
        Telefono           NVARCHAR(20)      NULL,
        CorreoElectronico  NVARCHAR(150)     NULL,
        FechaRegistro      DATETIME2(0)      NOT NULL CONSTRAINT DF_Editoriales_FechaRegistro DEFAULT (SYSDATETIME()),
        CONSTRAINT PK_Editoriales PRIMARY KEY CLUSTERED (IdEditorial),
        CONSTRAINT UQ_Editoriales_Nombre UNIQUE (Nombre),
        CONSTRAINT CK_Editoriales_Nombre CHECK (LEN(LTRIM(RTRIM(Nombre))) > 0)
    );
END
GO

IF OBJECT_ID(N'dbo.Autores', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Autores
    (
        IdAutor            INT IDENTITY(1,1) NOT NULL,
        Nombre             NVARCHAR(80)      NOT NULL,
        Apellido           NVARCHAR(80)      NOT NULL,
        Nacionalidad       NVARCHAR(80)      NULL,
        FechaNacimiento    DATE              NULL,
        FechaRegistro      DATETIME2(0)      NOT NULL CONSTRAINT DF_Autores_FechaRegistro DEFAULT (SYSDATETIME()),
        CONSTRAINT PK_Autores PRIMARY KEY CLUSTERED (IdAutor),
        CONSTRAINT CK_Autores_Nombre CHECK (LEN(LTRIM(RTRIM(Nombre))) > 0),
        CONSTRAINT CK_Autores_Apellido CHECK (LEN(LTRIM(RTRIM(Apellido))) > 0),
        CONSTRAINT UQ_Autores_NombreApellido UNIQUE (Nombre, Apellido)
    );
END
GO

IF OBJECT_ID(N'dbo.Usuarios', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Usuarios
    (
        IdUsuario          INT IDENTITY(1,1) NOT NULL,
        Nombre             NVARCHAR(80)      NOT NULL,
        Apellido           NVARCHAR(80)      NOT NULL,
        Documento          NVARCHAR(20)      NOT NULL,
        Telefono           NVARCHAR(20)      NULL,
        CorreoElectronico  NVARCHAR(150)     NOT NULL,
        Direccion          NVARCHAR(200)     NULL,
        FechaRegistro      DATETIME2(0)      NOT NULL CONSTRAINT DF_Usuarios_FechaRegistro DEFAULT (SYSDATETIME()),
        Activo             BIT               NOT NULL CONSTRAINT DF_Usuarios_Activo DEFAULT (1),
        CONSTRAINT PK_Usuarios PRIMARY KEY CLUSTERED (IdUsuario),
        CONSTRAINT UQ_Usuarios_Documento UNIQUE (Documento),
        CONSTRAINT UQ_Usuarios_Correo UNIQUE (CorreoElectronico),
        CONSTRAINT CK_Usuarios_Nombre CHECK (LEN(LTRIM(RTRIM(Nombre))) > 0),
        CONSTRAINT CK_Usuarios_Apellido CHECK (LEN(LTRIM(RTRIM(Apellido))) > 0),
        CONSTRAINT CK_Usuarios_Documento CHECK (LEN(LTRIM(RTRIM(Documento))) >= 5),
        CONSTRAINT CK_Usuarios_Correo CHECK (CorreoElectronico LIKE N'%@%.%')
    );
END
GO

IF OBJECT_ID(N'dbo.Libros', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Libros
    (
        IdLibro            INT IDENTITY(1,1) NOT NULL,
        Isbn               NVARCHAR(20)      NOT NULL,
        Titulo             NVARCHAR(200)     NOT NULL,
        IdEditorial        INT               NOT NULL,
        AnioPublicacion    INT               NOT NULL,
        Genero             NVARCHAR(80)      NULL,
        NumeroPaginas      INT               NOT NULL CONSTRAINT DF_Libros_NumeroPaginas DEFAULT (1),
        Existencias        INT               NOT NULL CONSTRAINT DF_Libros_Existencias DEFAULT (1),
        Disponibles        INT               NOT NULL CONSTRAINT DF_Libros_Disponibles DEFAULT (1),
        FechaRegistro      DATETIME2(0)      NOT NULL CONSTRAINT DF_Libros_FechaRegistro DEFAULT (SYSDATETIME()),
        CONSTRAINT PK_Libros PRIMARY KEY CLUSTERED (IdLibro),
        CONSTRAINT UQ_Libros_Isbn UNIQUE (Isbn),
        CONSTRAINT FK_Libros_Editoriales FOREIGN KEY (IdEditorial)
            REFERENCES dbo.Editoriales (IdEditorial),
        CONSTRAINT CK_Libros_Titulo CHECK (LEN(LTRIM(RTRIM(Titulo))) > 0),
        CONSTRAINT CK_Libros_Isbn CHECK (LEN(LTRIM(RTRIM(Isbn))) >= 5),
        CONSTRAINT CK_Libros_Anio CHECK (AnioPublicacion BETWEEN 1450 AND YEAR(SYSDATETIME()) + 1),
        CONSTRAINT CK_Libros_Paginas CHECK (NumeroPaginas >= 1),
        CONSTRAINT CK_Libros_Existencias CHECK (Existencias >= 0),
        CONSTRAINT CK_Libros_Disponibles CHECK (Disponibles >= 0 AND Disponibles <= Existencias)
    );
END
GO

IF OBJECT_ID(N'dbo.LibroAutores', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.LibroAutores
    (
        IdLibro            INT NOT NULL,
        IdAutor            INT NOT NULL,
        CONSTRAINT PK_LibroAutores PRIMARY KEY CLUSTERED (IdLibro, IdAutor),
        CONSTRAINT FK_LibroAutores_Libros FOREIGN KEY (IdLibro)
            REFERENCES dbo.Libros (IdLibro),
        CONSTRAINT FK_LibroAutores_Autores FOREIGN KEY (IdAutor)
            REFERENCES dbo.Autores (IdAutor)
    );
END
GO

IF OBJECT_ID(N'dbo.Prestamos', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Prestamos
    (
        IdPrestamo                 INT IDENTITY(1,1) NOT NULL,
        IdUsuario                  INT               NOT NULL,
        IdLibro                    INT               NOT NULL,
        FechaPrestamo              DATE              NOT NULL CONSTRAINT DF_Prestamos_FechaPrestamo DEFAULT (CAST(SYSDATETIME() AS DATE)),
        FechaDevolucionEstimada    DATE              NOT NULL,
        FechaDevolucionReal        DATE              NULL,
        Estado                     NVARCHAR(20)      NOT NULL CONSTRAINT DF_Prestamos_Estado DEFAULT (N'Prestado'),
        Observaciones              NVARCHAR(300)     NULL,
        FechaRegistro              DATETIME2(0)      NOT NULL CONSTRAINT DF_Prestamos_FechaRegistro DEFAULT (SYSDATETIME()),
        CONSTRAINT PK_Prestamos PRIMARY KEY CLUSTERED (IdPrestamo),
        CONSTRAINT FK_Prestamos_Usuarios FOREIGN KEY (IdUsuario)
            REFERENCES dbo.Usuarios (IdUsuario),
        CONSTRAINT FK_Prestamos_Libros FOREIGN KEY (IdLibro)
            REFERENCES dbo.Libros (IdLibro),
        CONSTRAINT CK_Prestamos_Estado CHECK (Estado IN (N'Prestado', N'Devuelto')),
        CONSTRAINT CK_Prestamos_FechasEstimada CHECK (FechaDevolucionEstimada >= FechaPrestamo),
        CONSTRAINT CK_Prestamos_FechasReal CHECK (FechaDevolucionReal IS NULL OR FechaDevolucionReal >= FechaPrestamo),
        CONSTRAINT CK_Prestamos_EstadoFecha CHECK (
            (Estado = N'Prestado' AND FechaDevolucionReal IS NULL)
            OR (Estado = N'Devuelto' AND FechaDevolucionReal IS NOT NULL)
        )
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Libros_Titulo' AND object_id = OBJECT_ID(N'dbo.Libros'))
    CREATE NONCLUSTERED INDEX IX_Libros_Titulo ON dbo.Libros (Titulo);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Prestamos_Estado' AND object_id = OBJECT_ID(N'dbo.Prestamos'))
    CREATE NONCLUSTERED INDEX IX_Prestamos_Estado ON dbo.Prestamos (Estado) INCLUDE (IdUsuario, IdLibro, FechaPrestamo);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Usuarios_Apellido' AND object_id = OBJECT_ID(N'dbo.Usuarios'))
    CREATE NONCLUSTERED INDEX IX_Usuarios_Apellido ON dbo.Usuarios (Apellido, Nombre);
GO

PRINT N'Esquema de BibliotecaDB verificado.';
GO
