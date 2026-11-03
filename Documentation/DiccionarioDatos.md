# Diccionario de datos — BibliotecaDB

## Editoriales

| Columna | Tipo | Nulo | Restricciones | Descripción |
| --- | --- | --- | --- | --- |
| IdEditorial | INT IDENTITY | No | PK | Identificador |
| Nombre | NVARCHAR(150) | No | UNIQUE, CHECK | Nombre comercial |
| Pais | NVARCHAR(80) | Sí | | País |
| Telefono | NVARCHAR(20) | Sí | | Teléfono |
| CorreoElectronico | NVARCHAR(150) | Sí | | Correo |
| FechaRegistro | DATETIME2(0) | No | DEFAULT SYSDATETIME() | Alta |

## Autores

| Columna | Tipo | Nulo | Restricciones | Descripción |
| --- | --- | --- | --- | --- |
| IdAutor | INT IDENTITY | No | PK | Identificador |
| Nombre | NVARCHAR(80) | No | CHECK | Nombre |
| Apellido | NVARCHAR(80) | No | CHECK | Apellido |
| Nacionalidad | NVARCHAR(80) | Sí | | Nacionalidad |
| FechaNacimiento | DATE | Sí | | Nacimiento |
| FechaRegistro | DATETIME2(0) | No | DEFAULT | Alta |
| | | | UNIQUE (Nombre, Apellido) | Evita duplicados |

## Libros

| Columna | Tipo | Nulo | Restricciones | Descripción |
| --- | --- | --- | --- | --- |
| IdLibro | INT IDENTITY | No | PK | Identificador |
| Isbn | NVARCHAR(20) | No | UNIQUE, CHECK | Código ISBN |
| Titulo | NVARCHAR(200) | No | CHECK | Título |
| IdEditorial | INT | No | FK Editoriales | Editorial |
| AnioPublicacion | INT | No | CHECK 1450–año actual+1 | Año |
| Genero | NVARCHAR(80) | Sí | | Género |
| NumeroPaginas | INT | No | CHECK >= 1 | Páginas |
| Existencias | INT | No | CHECK >= 0 | Ejemplares |
| Disponibles | INT | No | CHECK 0–Existencias | Disponibles |
| FechaRegistro | DATETIME2(0) | No | DEFAULT | Alta |

## LibroAutores

| Columna | Tipo | Nulo | Restricciones | Descripción |
| --- | --- | --- | --- | --- |
| IdLibro | INT | No | PK, FK Libros | Libro |
| IdAutor | INT | No | PK, FK Autores | Autor |

## Usuarios

| Columna | Tipo | Nulo | Restricciones | Descripción |
| --- | --- | --- | --- | --- |
| IdUsuario | INT IDENTITY | No | PK | Identificador |
| Nombre | NVARCHAR(80) | No | CHECK | Nombre |
| Apellido | NVARCHAR(80) | No | CHECK | Apellido |
| Documento | NVARCHAR(20) | No | UNIQUE, CHECK len>=5 | Documento |
| Telefono | NVARCHAR(20) | Sí | | Teléfono |
| CorreoElectronico | NVARCHAR(150) | No | UNIQUE, CHECK formato | Correo |
| Direccion | NVARCHAR(200) | Sí | | Dirección |
| FechaRegistro | DATETIME2(0) | No | DEFAULT | Alta |
| Activo | BIT | No | DEFAULT 1 | Habilitado para préstamos |

## Prestamos

| Columna | Tipo | Nulo | Restricciones | Descripción |
| --- | --- | --- | --- | --- |
| IdPrestamo | INT IDENTITY | No | PK | Identificador |
| IdUsuario | INT | No | FK Usuarios | Usuario |
| IdLibro | INT | No | FK Libros | Libro |
| FechaPrestamo | DATE | No | DEFAULT hoy | Fecha préstamo |
| FechaDevolucionEstimada | DATE | No | >= FechaPrestamo | Fecha pactada |
| FechaDevolucionReal | DATE | Sí | >= FechaPrestamo | Fecha real |
| Estado | NVARCHAR(20) | No | CHECK Prestado/Devuelto | Estado |
| Observaciones | NVARCHAR(300) | Sí | | Notas |
| FechaRegistro | DATETIME2(0) | No | DEFAULT | Alta |

CHECK de coherencia: si el estado es `Prestado`, la fecha real debe ser nula; si es `Devuelto`, debe existir fecha real.

## Índices

- `IX_Libros_Titulo`
- `IX_Prestamos_Estado` (incluye usuario, libro y fecha)
- `IX_Usuarios_Apellido` (Apellido, Nombre)
