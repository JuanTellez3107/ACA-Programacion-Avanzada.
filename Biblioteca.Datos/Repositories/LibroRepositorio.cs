using Biblioteca.Datos.Connection;
using Biblioteca.Datos.Interfaces;
using Biblioteca.Entidades.Models;
using Microsoft.Data.SqlClient;

namespace Biblioteca.Datos.Repositories;

public class LibroRepositorio : IRepositorio<Libro>
{
    private readonly SqlExecutor _sql;

    public LibroRepositorio(SqlExecutor sql)
    {
        _sql = sql;
    }

    private const string SelectBase = """
        SELECT l.IdLibro, l.Isbn, l.Titulo, l.IdEditorial, e.Nombre AS EditorialNombre,
               l.AnioPublicacion, l.Genero, l.NumeroPaginas, l.Existencias, l.Disponibles, l.FechaRegistro,
               STUFF((
                   SELECT ', ' + a.Nombre + ' ' + a.Apellido
                   FROM dbo.LibroAutores la
                   INNER JOIN dbo.Autores a ON a.IdAutor = la.IdAutor
                   WHERE la.IdLibro = l.IdLibro
                   FOR XML PATH(''), TYPE).value('.', 'nvarchar(max)'), 1, 2, '') AS Autores
        FROM dbo.Libros l
        INNER JOIN dbo.Editoriales e ON e.IdEditorial = l.IdEditorial
        """;

    public IReadOnlyList<Libro> ObtenerTodos()
    {
        return _sql.Consultar(SelectBase + " ORDER BY l.Titulo;", MapearConAutores);
    }

    public IReadOnlyList<Libro> Buscar(string texto)
    {
        var sql = SelectBase + """
             WHERE l.Isbn LIKE @texto OR l.Titulo LIKE @texto OR e.Nombre LIKE @texto
                OR EXISTS (
                    SELECT 1 FROM dbo.LibroAutores la
                    INNER JOIN dbo.Autores a ON a.IdAutor = la.IdAutor
                    WHERE la.IdLibro = l.IdLibro
                      AND (a.Nombre LIKE @texto OR a.Apellido LIKE @texto)
                )
            ORDER BY l.Titulo;
            """;
        return _sql.Consultar(sql, MapearConAutores, p => p.AddWithValue("@texto", $"%{texto}%"));
    }

    public Libro? ObtenerPorId(int id)
    {
        var libro = _sql.ConsultarUno(SelectBase + " WHERE l.IdLibro = @id;", MapearConAutores, p => p.AddWithValue("@id", id));
        if (libro is null)
        {
            return null;
        }

        libro.IdsAutores = ObtenerIdsAutores(id);
        return libro;
    }

    public int Insertar(Libro entidad)
    {
        using var conexion = _sql.CrearConexion();
        conexion.Open();
        using var transaccion = conexion.BeginTransaction();
        try
        {
            const string insertarLibro = """
                INSERT INTO dbo.Libros (Isbn, Titulo, IdEditorial, AnioPublicacion, Genero, NumeroPaginas, Existencias, Disponibles)
                OUTPUT INSERTED.IdLibro
                VALUES (@isbn, @titulo, @idEditorial, @anio, @genero, @paginas, @existencias, @disponibles);
                """;
            using var cmd = new SqlCommand(insertarLibro, conexion, transaccion);
            AsignarLibro(cmd.Parameters, entidad);
            var id = Convert.ToInt32(cmd.ExecuteScalar());
            GuardarAutores(conexion, transaccion, id, entidad.IdsAutores);
            transaccion.Commit();
            return id;
        }
        catch
        {
            transaccion.Rollback();
            throw;
        }
    }

    public void Actualizar(Libro entidad)
    {
        using var conexion = _sql.CrearConexion();
        conexion.Open();
        using var transaccion = conexion.BeginTransaction();
        try
        {
            const string actualizar = """
                UPDATE dbo.Libros
                SET Isbn = @isbn, Titulo = @titulo, IdEditorial = @idEditorial, AnioPublicacion = @anio,
                    Genero = @genero, NumeroPaginas = @paginas, Existencias = @existencias, Disponibles = @disponibles
                WHERE IdLibro = @id;
                """;
            using (var cmd = new SqlCommand(actualizar, conexion, transaccion))
            {
                cmd.Parameters.AddWithValue("@id", entidad.Id);
                AsignarLibro(cmd.Parameters, entidad);
                cmd.ExecuteNonQuery();
            }

            using (var borrar = new SqlCommand("DELETE FROM dbo.LibroAutores WHERE IdLibro = @id;", conexion, transaccion))
            {
                borrar.Parameters.AddWithValue("@id", entidad.Id);
                borrar.ExecuteNonQuery();
            }

            GuardarAutores(conexion, transaccion, entidad.Id, entidad.IdsAutores);
            transaccion.Commit();
        }
        catch
        {
            transaccion.Rollback();
            throw;
        }
    }

    public void Eliminar(int id)
    {
        using var conexion = _sql.CrearConexion();
        conexion.Open();
        using var transaccion = conexion.BeginTransaction();
        try
        {
            using (var borrarRel = new SqlCommand("DELETE FROM dbo.LibroAutores WHERE IdLibro = @id;", conexion, transaccion))
            {
                borrarRel.Parameters.AddWithValue("@id", id);
                borrarRel.ExecuteNonQuery();
            }

            using var borrar = new SqlCommand("DELETE FROM dbo.Libros WHERE IdLibro = @id;", conexion, transaccion);
            borrar.Parameters.AddWithValue("@id", id);
            borrar.ExecuteNonQuery();
            transaccion.Commit();
        }
        catch
        {
            transaccion.Rollback();
            throw;
        }
    }

    public bool ExisteIsbn(string isbn, int? excluirId = null)
    {
        const string sql = """
            SELECT COUNT(1) FROM dbo.Libros
            WHERE Isbn = @isbn AND (@excluirId IS NULL OR IdLibro <> @excluirId);
            """;
        return Convert.ToInt32(_sql.Escalar(sql, p =>
        {
            p.AddWithValue("@isbn", isbn.Trim());
            p.AddWithValue("@excluirId", (object?)excluirId ?? DBNull.Value);
        })) > 0;
    }

    public bool TienePrestamos(int idLibro)
    {
        const string sql = "SELECT COUNT(1) FROM dbo.Prestamos WHERE IdLibro = @id;";
        return Convert.ToInt32(_sql.Escalar(sql, p => p.AddWithValue("@id", idLibro))) > 0;
    }

    public int ContarPrestamosActivos(int idLibro)
    {
        const string sql = "SELECT COUNT(1) FROM dbo.Prestamos WHERE IdLibro = @id AND Estado = N'Prestado';";
        return Convert.ToInt32(_sql.Escalar(sql, p => p.AddWithValue("@id", idLibro)));
    }

    private List<int> ObtenerIdsAutores(int idLibro)
    {
        const string sql = "SELECT IdAutor FROM dbo.LibroAutores WHERE IdLibro = @id;";
        return _sql.Consultar(sql, r => r.GetInt32(0), p => p.AddWithValue("@id", idLibro));
    }

    private static void GuardarAutores(SqlConnection conexion, SqlTransaction transaccion, int idLibro, IEnumerable<int> ids)
    {
        foreach (var idAutor in ids.Distinct())
        {
            using var cmd = new SqlCommand(
                "INSERT INTO dbo.LibroAutores (IdLibro, IdAutor) VALUES (@idLibro, @idAutor);",
                conexion, transaccion);
            cmd.Parameters.AddWithValue("@idLibro", idLibro);
            cmd.Parameters.AddWithValue("@idAutor", idAutor);
            cmd.ExecuteNonQuery();
        }
    }

    private static void AsignarLibro(SqlParameterCollection p, Libro entidad)
    {
        p.AddWithValue("@isbn", entidad.Isbn.Trim());
        p.AddWithValue("@titulo", entidad.Titulo.Trim());
        p.AddWithValue("@idEditorial", entidad.IdEditorial);
        p.AddWithValue("@anio", entidad.AnioPublicacion);
        p.AddWithValue("@genero", (object?)entidad.Genero ?? DBNull.Value);
        p.AddWithValue("@paginas", entidad.NumeroPaginas);
        p.AddWithValue("@existencias", entidad.Existencias);
        p.AddWithValue("@disponibles", entidad.Disponibles);
    }

    private static Libro MapearConAutores(SqlDataReader lector) => new()
    {
        Id = lector.GetInt32(0),
        Isbn = lector.GetString(1),
        Titulo = lector.GetString(2),
        IdEditorial = lector.GetInt32(3),
        EditorialNombre = lector.GetString(4),
        AnioPublicacion = lector.GetInt32(5),
        Genero = lector.IsDBNull(6) ? null : lector.GetString(6),
        NumeroPaginas = lector.GetInt32(7),
        Existencias = lector.GetInt32(8),
        Disponibles = lector.GetInt32(9),
        FechaRegistro = lector.GetDateTime(10),
        AutoresNombres = lector.IsDBNull(11) ? string.Empty : lector.GetString(11)
    };
}
