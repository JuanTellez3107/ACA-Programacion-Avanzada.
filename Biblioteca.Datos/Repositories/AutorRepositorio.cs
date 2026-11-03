using Biblioteca.Datos.Connection;
using Biblioteca.Datos.Interfaces;
using Biblioteca.Entidades.Models;
using Microsoft.Data.SqlClient;

namespace Biblioteca.Datos.Repositories;

public class AutorRepositorio : IRepositorio<Autor>
{
    private readonly SqlExecutor _sql;

    public AutorRepositorio(SqlExecutor sql)
    {
        _sql = sql;
    }

    public IReadOnlyList<Autor> ObtenerTodos()
    {
        const string consulta = """
            SELECT IdAutor, Nombre, Apellido, Nacionalidad, FechaNacimiento, FechaRegistro
            FROM dbo.Autores
            ORDER BY Apellido, Nombre;
            """;
        return _sql.Consultar(consulta, Mapear);
    }

    public IReadOnlyList<Autor> Buscar(string texto)
    {
        const string consulta = """
            SELECT IdAutor, Nombre, Apellido, Nacionalidad, FechaNacimiento, FechaRegistro
            FROM dbo.Autores
            WHERE Nombre LIKE @texto OR Apellido LIKE @texto OR Nacionalidad LIKE @texto
            ORDER BY Apellido, Nombre;
            """;
        return _sql.Consultar(consulta, Mapear, p => p.AddWithValue("@texto", $"%{texto}%"));
    }

    public Autor? ObtenerPorId(int id)
    {
        const string consulta = """
            SELECT IdAutor, Nombre, Apellido, Nacionalidad, FechaNacimiento, FechaRegistro
            FROM dbo.Autores
            WHERE IdAutor = @id;
            """;
        return _sql.ConsultarUno(consulta, Mapear, p => p.AddWithValue("@id", id));
    }

    public int Insertar(Autor entidad)
    {
        const string sql = """
            INSERT INTO dbo.Autores (Nombre, Apellido, Nacionalidad, FechaNacimiento)
            OUTPUT INSERTED.IdAutor
            VALUES (@nombre, @apellido, @nacionalidad, @fechaNacimiento);
            """;
        return Convert.ToInt32(_sql.Escalar(sql, p =>
        {
            p.AddWithValue("@nombre", entidad.Nombre.Trim());
            p.AddWithValue("@apellido", entidad.Apellido.Trim());
            p.AddWithValue("@nacionalidad", (object?)entidad.Nacionalidad ?? DBNull.Value);
            p.AddWithValue("@fechaNacimiento", (object?)entidad.FechaNacimiento ?? DBNull.Value);
        }));
    }

    public void Actualizar(Autor entidad)
    {
        const string sql = """
            UPDATE dbo.Autores
            SET Nombre = @nombre,
                Apellido = @apellido,
                Nacionalidad = @nacionalidad,
                FechaNacimiento = @fechaNacimiento
            WHERE IdAutor = @id;
            """;
        _sql.Ejecutar(sql, p =>
        {
            p.AddWithValue("@id", entidad.Id);
            p.AddWithValue("@nombre", entidad.Nombre.Trim());
            p.AddWithValue("@apellido", entidad.Apellido.Trim());
            p.AddWithValue("@nacionalidad", (object?)entidad.Nacionalidad ?? DBNull.Value);
            p.AddWithValue("@fechaNacimiento", (object?)entidad.FechaNacimiento ?? DBNull.Value);
        });
    }

    public void Eliminar(int id)
    {
        _sql.Ejecutar("DELETE FROM dbo.Autores WHERE IdAutor = @id;", p => p.AddWithValue("@id", id));
    }

    public bool TieneLibrosAsociados(int idAutor)
    {
        const string sql = "SELECT COUNT(1) FROM dbo.LibroAutores WHERE IdAutor = @id;";
        return Convert.ToInt32(_sql.Escalar(sql, p => p.AddWithValue("@id", idAutor))) > 0;
    }

    public bool ExisteNombreApellido(string nombre, string apellido, int? excluirId = null)
    {
        const string sql = """
            SELECT COUNT(1)
            FROM dbo.Autores
            WHERE Nombre = @nombre AND Apellido = @apellido
              AND (@excluirId IS NULL OR IdAutor <> @excluirId);
            """;
        return Convert.ToInt32(_sql.Escalar(sql, p =>
        {
            p.AddWithValue("@nombre", nombre.Trim());
            p.AddWithValue("@apellido", apellido.Trim());
            p.AddWithValue("@excluirId", (object?)excluirId ?? DBNull.Value);
        })) > 0;
    }

    private static Autor Mapear(SqlDataReader lector) => new()
    {
        Id = lector.GetInt32(0),
        Nombre = lector.GetString(1),
        Apellido = lector.GetString(2),
        Nacionalidad = lector.IsDBNull(3) ? null : lector.GetString(3),
        FechaNacimiento = lector.IsDBNull(4) ? null : lector.GetDateTime(4),
        FechaRegistro = lector.GetDateTime(5)
    };
}
