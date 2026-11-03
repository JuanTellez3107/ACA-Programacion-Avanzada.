USE BibliotecaDB;
GO

SET NOCOUNT ON;

IF NOT EXISTS (SELECT 1 FROM dbo.Editoriales)
BEGIN
    INSERT INTO dbo.Editoriales (Nombre, Pais, Telefono, CorreoElectronico)
    VALUES
        (N'Planeta', N'España', N'+34912345678', N'contacto@planeta.es'),
        (N'Alfaguara', N'España', N'+34918765432', N'info@alfaguara.es'),
        (N'Norma Editorial', N'Colombia', N'+5716001234', N'editorial@norma.com'),
        (N'Sudamericana', N'Argentina', N'+541143210987', N'contacto@sudamericana.ar'),
        (N'Siglo del Hombre', N'Colombia', N'+5713419588', N'info@sigloelhombre.com');
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Autores)
BEGIN
    INSERT INTO dbo.Autores (Nombre, Apellido, Nacionalidad, FechaNacimiento)
    VALUES
        (N'Gabriel', N'García Márquez', N'Colombiana', '1927-03-06'),
        (N'Isabel', N'Allende', N'Chilena', '1942-08-02'),
        (N'Jorge', N'Luis Borges', N'Argentina', '1899-08-24'),
        (N'Mario', N'Vargas Llosa', N'Peruana', '1936-03-28'),
        (N'Laura', N'Restrepo', N'Colombiana', '1950-01-01'),
        (N'Julio', N'Cortázar', N'Argentina', '1914-08-26'),
        (N'Rosa', N'Montero', N'Española', '1951-01-03'),
        (N'Héctor', N'Abad Faciolince', N'Colombiana', '1958-10-01');
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Usuarios)
BEGIN
    INSERT INTO dbo.Usuarios (Nombre, Apellido, Documento, Telefono, CorreoElectronico, Direccion)
    VALUES
        (N'Ana', N'Martínez', N'1002345678', N'3001112233', N'ana.martinez@correo.edu', N'Calle 45 #12-30, Bogotá'),
        (N'Carlos', N'Pérez', N'1013456789', N'3102223344', N'carlos.perez@correo.edu', N'Cra 7 #80-15, Bogotá'),
        (N'Luisa', N'Gómez', N'1024567890', N'3203334455', N'luisa.gomez@correo.edu', N'Av. Suba #90-20, Bogotá'),
        (N'Andrés', N'Rodríguez', N'1035678901', N'3014445566', N'andres.rodriguez@correo.edu', N'Calle 26 #50-10, Bogotá'),
        (N'María', N'Fernández', N'1046789012', N'3155556677', N'maria.fernandez@correo.edu', N'Cra 15 #100-08, Bogotá'),
        (N'Juan', N'Herrera', N'1057890123', N'3006667788', N'juan.herrera@correo.edu', N'Calle 72 #11-40, Bogotá');
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Libros)
BEGIN
    DECLARE @idPlaneta INT = (SELECT IdEditorial FROM dbo.Editoriales WHERE Nombre = N'Planeta');
    DECLARE @idAlfaguara INT = (SELECT IdEditorial FROM dbo.Editoriales WHERE Nombre = N'Alfaguara');
    DECLARE @idNorma INT = (SELECT IdEditorial FROM dbo.Editoriales WHERE Nombre = N'Norma Editorial');
    DECLARE @idSudamericana INT = (SELECT IdEditorial FROM dbo.Editoriales WHERE Nombre = N'Sudamericana');
    DECLARE @idSiglo INT = (SELECT IdEditorial FROM dbo.Editoriales WHERE Nombre = N'Siglo del Hombre');

    INSERT INTO dbo.Libros (Isbn, Titulo, IdEditorial, AnioPublicacion, Genero, NumeroPaginas, Existencias, Disponibles)
    VALUES
        (N'9780307474728', N'Cien años de soledad', @idSudamericana, 1967, N'Realismo mágico', 471, 5, 4),
        (N'9780061122415', N'La casa de los espíritus', @idPlaneta, 1982, N'Novela', 481, 3, 2),
        (N'9788420633138', N'Ficciones', @idAlfaguara, 1944, N'Cuento', 203, 4, 4),
        (N'9788466331906', N'La ciudad y los perros', @idAlfaguara, 1963, N'Novela', 448, 2, 2),
        (N'9789584268471', N'Delirio', @idAlfaguara, 2004, N'Novela', 341, 3, 3),
        (N'9788432216428', N'Rayuela', @idAlfaguara, 1963, N'Novela', 736, 2, 1),
        (N'9788420471839', N'La loca de la casa', @idPlaneta, 2003, N'Ensayo', 272, 2, 2),
        (N'9789584272348', N'El olvido que seremos', @idPlaneta, 2006, N'Memoria', 272, 4, 3),
        (N'9789580489801', N'Crónica de una muerte anunciada', @idNorma, 1981, N'Novela', 120, 6, 6),
        (N'9789586651806', N'El amor en los tiempos del cólera', @idSiglo, 1985, N'Novela', 464, 3, 3);

    DECLARE @idGabo INT = (SELECT IdAutor FROM dbo.Autores WHERE Apellido = N'García Márquez');
    DECLARE @idAllende INT = (SELECT IdAutor FROM dbo.Autores WHERE Apellido = N'Allende');
    DECLARE @idBorges INT = (SELECT IdAutor FROM dbo.Autores WHERE Apellido = N'Luis Borges');
    DECLARE @idVargas INT = (SELECT IdAutor FROM dbo.Autores WHERE Apellido = N'Vargas Llosa');
    DECLARE @idRestrepo INT = (SELECT IdAutor FROM dbo.Autores WHERE Apellido = N'Restrepo');
    DECLARE @idCortazar INT = (SELECT IdAutor FROM dbo.Autores WHERE Apellido = N'Cortázar');
    DECLARE @idMontero INT = (SELECT IdAutor FROM dbo.Autores WHERE Apellido = N'Montero');
    DECLARE @idAbad INT = (SELECT IdAutor FROM dbo.Autores WHERE Apellido = N'Abad Faciolince');

    INSERT INTO dbo.LibroAutores (IdLibro, IdAutor)
    SELECT l.IdLibro, a.IdAutor
    FROM dbo.Libros l
    CROSS APPLY (
        SELECT CASE l.Isbn
            WHEN N'9780307474728' THEN @idGabo
            WHEN N'9780061122415' THEN @idAllende
            WHEN N'9788420633138' THEN @idBorges
            WHEN N'9788466331906' THEN @idVargas
            WHEN N'9789584268471' THEN @idRestrepo
            WHEN N'9788432216428' THEN @idCortazar
            WHEN N'9788420471839' THEN @idMontero
            WHEN N'9789584272348' THEN @idAbad
            WHEN N'9789580489801' THEN @idGabo
            WHEN N'9789586651806' THEN @idGabo
        END AS IdAutor
    ) a
    WHERE a.IdAutor IS NOT NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Prestamos)
BEGIN
    DECLARE @uAna INT = (SELECT IdUsuario FROM dbo.Usuarios WHERE Documento = N'1002345678');
    DECLARE @uCarlos INT = (SELECT IdUsuario FROM dbo.Usuarios WHERE Documento = N'1013456789');
    DECLARE @uLuisa INT = (SELECT IdUsuario FROM dbo.Usuarios WHERE Documento = N'1024567890');
    DECLARE @uAndres INT = (SELECT IdUsuario FROM dbo.Usuarios WHERE Documento = N'1035678901');

    DECLARE @lSoledad INT = (SELECT IdLibro FROM dbo.Libros WHERE Isbn = N'9780307474728');
    DECLARE @lEspiritus INT = (SELECT IdLibro FROM dbo.Libros WHERE Isbn = N'9780061122415');
    DECLARE @lRayuela INT = (SELECT IdLibro FROM dbo.Libros WHERE Isbn = N'9788432216428');
    DECLARE @lOlvido INT = (SELECT IdLibro FROM dbo.Libros WHERE Isbn = N'9789584272348');

    INSERT INTO dbo.Prestamos (IdUsuario, IdLibro, FechaPrestamo, FechaDevolucionEstimada, FechaDevolucionReal, Estado, Observaciones)
    VALUES
        (@uAna, @lSoledad, DATEADD(DAY, -20, CAST(SYSDATETIME() AS DATE)), DATEADD(DAY, -5, CAST(SYSDATETIME() AS DATE)), DATEADD(DAY, -6, CAST(SYSDATETIME() AS DATE)), N'Devuelto', N'Devolución a tiempo.'),
        (@uCarlos, @lEspiritus, DATEADD(DAY, -8, CAST(SYSDATETIME() AS DATE)), DATEADD(DAY, 7, CAST(SYSDATETIME() AS DATE)), NULL, N'Prestado', N'Préstamo vigente.'),
        (@uLuisa, @lRayuela, DATEADD(DAY, -3, CAST(SYSDATETIME() AS DATE)), DATEADD(DAY, 12, CAST(SYSDATETIME() AS DATE)), NULL, N'Prestado', N'Préstamo vigente.'),
        (@uAndres, @lOlvido, DATEADD(DAY, -30, CAST(SYSDATETIME() AS DATE)), DATEADD(DAY, -15, CAST(SYSDATETIME() AS DATE)), DATEADD(DAY, -14, CAST(SYSDATETIME() AS DATE)), N'Devuelto', N'Devolución registrada.'),
        (@uAna, @lOlvido, DATEADD(DAY, -2, CAST(SYSDATETIME() AS DATE)), DATEADD(DAY, 13, CAST(SYSDATETIME() AS DATE)), NULL, N'Prestado', N'Segundo préstamo de la usuaria.');
END
GO

PRINT N'Datos de prueba verificados.';
GO
