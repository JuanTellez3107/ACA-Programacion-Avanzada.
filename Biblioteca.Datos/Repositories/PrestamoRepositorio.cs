using Biblioteca.Datos.Connection;
using Biblioteca.Datos.Interfaces;
using Biblioteca.Entidades.Models;
using Microsoft.Data.SqlClient;

namespace Biblioteca.Datos.Repositories;

public class PrestamoRepositorio : IRepositorio<Prestamo>
{
    private readonly SqlExecutor _sql;

    public PrestamoRepositorio(SqlExecutor sql)
    {
        _sql = sql;
    }

    private const string SelectBase = """
        SELECT p.IdPrestamo, p.IdUsuario, p.IdLibro,
               u.Nombre + ' ' + u.Apellido AS UsuarioNombre, u.Documento,
               l.Titulo, l.Isbn,
               p.FechaPrestamo, p.FechaDevolucionEstimada, p.FechaDevolucionReal,
               p.Estado, p.Observaciones, p.FechaRegistro
        FROM dbo.Prestamos p
        INNER JOIN dbo.Usuarios u ON u.IdUsuario = p.IdUsuario
        INNER JOIN dbo.Libros l ON l.IdLibro = p.IdLibro
        """;

    public IReadOnlyList<Prestamo> ObtenerTodos() =>
        _sql.Consultar(SelectBase + " ORDER BY p.FechaPrestamo DESC, p.IdPrestamo DESC;", Mapear);

    public IReadOnlyList<Prestamo> ObtenerPorEstado(string estado) =>
        _sql.Consultar(SelectBase + " WHERE p.Estado = @estado ORDER BY p.FechaPrestamo DESC;", Mapear,
            p => p.AddWithValue("@estado", estado));

    public IReadOnlyList<Prestamo> Buscar(string texto)
    {
        var sql = SelectBase + """
             WHERE u.Nombre LIKE @texto OR u.Apellido LIKE @texto OR u.Documento LIKE @texto
                OR l.Titulo LIKE @texto OR l.Isbn LIKE @texto OR p.Estado LIKE @texto
            ORDER BY p.FechaPrestamo DESC;
            """;
        return _sql.Consultar(sql, Mapear, p => p.AddWithValue("@texto", $"%{texto}%"));
    }

    public Prestamo? ObtenerPorId(int id) =>
        _sql.ConsultarUno(SelectBase + " WHERE p.IdPrestamo = @id;", Mapear, p => p.AddWithValue("@id", id));

    public int Insertar(Prestamo entidad)
    {
        using var conexion = _sql.CrearConexion();
        conexion.Open();
        using var transaccion = conexion.BeginTransaction();
        try
        {
            const string insertar = """
                INSERT INTO dbo.Prestamos (IdUsuario, IdLibro, FechaPrestamo, FechaDevolucionEstimada, Estado, Observaciones)
                OUTPUT INSERTED.IdPrestamo
                VALUES (@idUsuario, @idLibro, @fechaPrestamo, @fechaEstimada, N'Prestado', @observaciones);
                """;
            int id;
            using (var cmd = new SqlCommand(insertar, conexion, transaccion))
            {
                cmd.Parameters.AddWithValue("@idUsuario", entidad.IdUsuario);
                cmd.Parameters.AddWithValue("@idLibro", entidad.IdLibro);
                cmd.Parameters.AddWithValue("@fechaPrestamo", entidad.FechaPrestamo.Date);
                cmd.Parameters.AddWithValue("@fechaEstimada", entidad.FechaDevolucionEstimada.Date);
                cmd.Parameters.AddWithValue("@observaciones", (object?)entidad.Observaciones ?? DBNull.Value);
                id = Convert.ToInt32(cmd.ExecuteScalar());
            }

            using (var stock = new SqlCommand(
                       "UPDATE dbo.Libros SET Disponibles = Disponibles - 1 WHERE IdLibro = @idLibro;",
                       conexion, transaccion))
            {
                stock.Parameters.AddWithValue("@idLibro", entidad.IdLibro);
                stock.ExecuteNonQuery();
            }

            transaccion.Commit();
            return id;
        }
        catch
        {
            transaccion.Rollback();
            throw;
        }
    }

    public void Actualizar(Prestamo entidad)
    {
        const string sql = """
            UPDATE dbo.Prestamos
            SET FechaPrestamo = @fechaPrestamo,
                FechaDevolucionEstimada = @fechaEstimada,
                Observaciones = @observaciones
            WHERE IdPrestamo = @id AND Estado = N'Prestado';
            """;
        _sql.Ejecutar(sql, p =>
        {
            p.AddWithValue("@id", entidad.Id);
            p.AddWithValue("@fechaPrestamo", entidad.FechaPrestamo.Date);
            p.AddWithValue("@fechaEstimada", entidad.FechaDevolucionEstimada.Date);
            p.AddWithValue("@observaciones", (object?)entidad.Observaciones ?? DBNull.Value);
        });
    }

    public void RegistrarDevolucion(int idPrestamo, DateTime fechaDevolucion, string? observaciones)
    {
        using var conexion = _sql.CrearConexion();
        conexion.Open();
        using var transaccion = conexion.BeginTransaction();
        try
        {
            const string actualizar = """
                UPDATE dbo.Prestamos
                SET FechaDevolucionReal = @fecha,
                    Estado = N'Devuelto',
                    Observaciones = COALESCE(@observaciones, Observaciones)
                WHERE IdPrestamo = @id AND Estado = N'Prestado';
                """;
            int afectados;
            using (var cmd = new SqlCommand(actualizar, conexion, transaccion))
            {
                cmd.Parameters.AddWithValue("@id", idPrestamo);
                cmd.Parameters.AddWithValue("@fecha", fechaDevolucion.Date);
                cmd.Parameters.AddWithValue("@observaciones", (object?)observaciones ?? DBNull.Value);
                afectados = cmd.ExecuteNonQuery();
            }

            if (afectados == 0)
            {
                throw new InvalidOperationException("El préstamo no está activo o no existe.");
            }

            using (var stock = new SqlCommand("""
                UPDATE dbo.Libros
                SET Disponibles = Disponibles + 1
                WHERE IdLibro = (SELECT IdLibro FROM dbo.Prestamos WHERE IdPrestamo = @id);
                """, conexion, transaccion))
            {
                stock.Parameters.AddWithValue("@id", idPrestamo);
                stock.ExecuteNonQuery();
            }

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
            object? estado;
            object? idLibro;
            using (var consulta = new SqlCommand(
                       "SELECT Estado, IdLibro FROM dbo.Prestamos WHERE IdPrestamo = @id;", conexion, transaccion))
            {
                consulta.Parameters.AddWithValue("@id", id);
                using var lector = consulta.ExecuteReader();
                if (!lector.Read())
                {
                    throw new InvalidOperationException("El préstamo no existe.");
                }

                estado = lector.GetString(0);
                idLibro = lector.GetInt32(1);
            }

            using (var borrar = new SqlCommand("DELETE FROM dbo.Prestamos WHERE IdPrestamo = @id;", conexion, transaccion))
            {
                borrar.Parameters.AddWithValue("@id", id);
                borrar.ExecuteNonQuery();
            }

            if (Convert.ToString(estado) == EstadosPrestamo.Prestado)
            {
                using var stock = new SqlCommand(
                    "UPDATE dbo.Libros SET Disponibles = Disponibles + 1 WHERE IdLibro = @idLibro;",
                    conexion, transaccion);
                stock.Parameters.AddWithValue("@idLibro", idLibro);
                stock.ExecuteNonQuery();
            }

            transaccion.Commit();
        }
        catch
        {
            transaccion.Rollback();
            throw;
        }
    }

    public bool ExistePrestamoActivo(int idUsuario, int idLibro)
    {
        const string sql = """
            SELECT COUNT(1) FROM dbo.Prestamos
            WHERE IdUsuario = @idUsuario AND IdLibro = @idLibro AND Estado = N'Prestado';
            """;
        return Convert.ToInt32(_sql.Escalar(sql, p =>
        {
            p.AddWithValue("@idUsuario", idUsuario);
            p.AddWithValue("@idLibro", idLibro);
        })) > 0;
    }

    public DashboardResumen ObtenerResumen()
    {
        const string sql = """
            SELECT
                (SELECT COUNT(1) FROM dbo.Libros) AS TotalLibros,
                (SELECT COUNT(1) FROM dbo.Autores) AS TotalAutores,
                (SELECT COUNT(1) FROM dbo.Editoriales) AS TotalEditoriales,
                (SELECT COUNT(1) FROM dbo.Usuarios) AS TotalUsuarios,
                (SELECT COUNT(1) FROM dbo.Prestamos WHERE Estado = N'Prestado') AS PrestamosActivos,
                (SELECT COUNT(1) FROM dbo.Prestamos WHERE Estado = N'Devuelto') AS PrestamosDevueltos,
                (SELECT COALESCE(SUM(Disponibles), 0) FROM dbo.Libros) AS EjemplaresDisponibles,
                (SELECT COALESCE(SUM(Existencias - Disponibles), 0) FROM dbo.Libros) AS EjemplaresPrestados;
            """;
        return _sql.ConsultarUno(sql, r => new DashboardResumen
        {
            TotalLibros = r.GetInt32(0),
            TotalAutores = r.GetInt32(1),
            TotalEditoriales = r.GetInt32(2),
            TotalUsuarios = r.GetInt32(3),
            PrestamosActivos = r.GetInt32(4),
            PrestamosDevueltos = r.GetInt32(5),
            EjemplaresDisponibles = Convert.ToInt32(r.GetValue(6)),
            EjemplaresPrestados = Convert.ToInt32(r.GetValue(7))
        }) ?? new DashboardResumen();
    }

    private static Prestamo Mapear(SqlDataReader lector) => new()
    {
        Id = lector.GetInt32(0),
        IdUsuario = lector.GetInt32(1),
        IdLibro = lector.GetInt32(2),
        UsuarioNombre = lector.GetString(3),
        UsuarioDocumento = lector.GetString(4),
        LibroTitulo = lector.GetString(5),
        LibroIsbn = lector.GetString(6),
        FechaPrestamo = lector.GetDateTime(7),
        FechaDevolucionEstimada = lector.GetDateTime(8),
        FechaDevolucionReal = lector.IsDBNull(9) ? null : lector.GetDateTime(9),
        Estado = lector.GetString(10),
        Observaciones = lector.IsDBNull(11) ? null : lector.GetString(11),
        FechaRegistro = lector.GetDateTime(12)
    };
}
