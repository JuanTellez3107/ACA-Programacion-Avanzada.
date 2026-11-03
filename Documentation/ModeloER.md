# Modelo entidad-relación

Base de datos: **BibliotecaDB**.

```mermaid
erDiagram
    EDITORIALES ||--o{ LIBROS : publica
    LIBROS ||--o{ LIBROAUTORES : tiene
    AUTORES ||--o{ LIBROAUTORES : escribe
    USUARIOS ||--o{ PRESTAMOS : realiza
    LIBROS ||--o{ PRESTAMOS : se_presta

    EDITORIALES {
        int IdEditorial PK
        nvarchar Nombre UK
        nvarchar Pais
        nvarchar Telefono
        nvarchar CorreoElectronico
        datetime2 FechaRegistro
    }
    AUTORES {
        int IdAutor PK
        nvarchar Nombre
        nvarchar Apellido
        nvarchar Nacionalidad
        date FechaNacimiento
        datetime2 FechaRegistro
    }
    LIBROS {
        int IdLibro PK
        nvarchar Isbn UK
        nvarchar Titulo
        int IdEditorial FK
        int AnioPublicacion
        nvarchar Genero
        int NumeroPaginas
        int Existencias
        int Disponibles
        datetime2 FechaRegistro
    }
    LIBROAUTORES {
        int IdLibro PK_FK
        int IdAutor PK_FK
    }
    USUARIOS {
        int IdUsuario PK
        nvarchar Nombre
        nvarchar Apellido
        nvarchar Documento UK
        nvarchar Telefono
        nvarchar CorreoElectronico UK
        nvarchar Direccion
        datetime2 FechaRegistro
        bit Activo
    }
    PRESTAMOS {
        int IdPrestamo PK
        int IdUsuario FK
        int IdLibro FK
        date FechaPrestamo
        date FechaDevolucionEstimada
        date FechaDevolucionReal
        nvarchar Estado
        nvarchar Observaciones
        datetime2 FechaRegistro
    }
```

## Cardinalidades

- Una editorial publica muchos libros; un libro pertenece a una editorial.
- Un libro puede tener varios autores y un autor varios libros (`LibroAutores`).
- Un usuario realiza muchos préstamos; cada préstamo corresponde a un ejemplar de un libro.
- El estado del préstamo es `Prestado` o `Devuelto`. Un préstamo activo exige `FechaDevolucionReal` nula.
