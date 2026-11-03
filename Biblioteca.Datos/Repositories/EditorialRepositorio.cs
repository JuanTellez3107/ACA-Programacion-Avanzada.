using Biblioteca.Datos.Connection;
using Biblioteca.Datos.Interfaces;
using Biblioteca.Entidades.Models;
using Microsoft.Data.SqlClient;

namespace Biblioteca.Datos.Repositories;

public class EditorialRepositorio : IRepositorio<Editorial>
{
    private readonly SqlExecutor _sql;

    public EditorialRepositorio(SqlExecutor sql)
    {
        _sql = sql;
    }

    public IReadOnlyList<Editorial> ObtenerTodos()
    {
        const string consulta = """
            SELECT IdEditorial, Nombre, Pais, Telefono, CorreoElectronico, FechaRegistro
            FROM dbo.Editoriales
            ORDER BY Nombre;
            """;
        return _sql.Consultar(consulta, Mapear);
    }

    public IReadOnlyList<Editorial> Buscar(string texto)
    {
        const string consulta = """
            SELECT IdEditorial, Nombre, Pais, Telefono, CorreoElectronico, FechaRegistro
            FROM dbo.Editoriales
            WHERE Nombre LIKE @texto OR Pais LIKE @texto OR CorreoElectronico LIKE @texto
            ORDER BY Nombre;
            """;
        return _sql.Consultar(consulta, Mapear, p => p.AddWithValue("@texto", $"%{texto}%"));
    }

    public Editorial? ObtenerPorId(int id)
    {
        const string consulta = """
            SELECT IdEditorial, Nombre, Pais, Telefono, CorreoElectronico, FechaRegistro
            FROM dbo.Editoriales
            WHERE IdEditorial = @id;
            """;
        return _sql.ConsultarUno(consulta, Mapear, p => p.AddWithValue("@id", id));
    }

    public int Insertar(Editorial entidad)
    {
        const string sql = """
            INSERT INTO dbo.Editoriales (Nombre, Pais, Telefono, CorreoElectronico)
            OUTPUT INSERTED.IdEditorial
            VALUES (@nombre, @pais, @telefono, @correo);
            """;
        return Convert.ToInt32(_sql.Escalar(sql, p =>
        {
            p.AddWithValue("@nombre", entidad.Nombre.Trim());
            p.AddWithValue("@pais", (object?)entidad.Pais ?? DBNull.Value);
            p.AddWithValue("@telefono", (object?)entidad.Telefono ?? DBNull.Value);
            p.AddWithValue("@correo", (object?)entidad.CorreoElectronico ?? DBNull.Value);
        }));
    }

    public void Actualizar(Editorial entidad)
    {
        const string sql = """
            UPDATE dbo.Editoriales
            SET Nombre = @nombre, Pais = @pais, Telefono = @telefono, CorreoElectronico = @correo
            WHERE IdEditorial = @id;
            """;
        _sql.Ejecutar(sql, p =>
        {
            p.AddWithValue("@id", entidad.Id);
            p.AddWithValue("@nombre", entidad.Nombre.Trim());
            p.AddWithValue("@pais", (object?)entidad.Pais ?? DBNull.Value);
            p.AddWithValue("@telefono", (object?)entidad.Telefono ?? DBNull.Value);
            p.AddWithValue("@correo", (object?)entidad.CorreoElectronico ?? DBNull.Value);
        });
    }

    public void Eliminar(int id)
    {
        _sql.Ejecutar("DELETE FROM dbo.Editoriales WHERE IdEditorial = @id;", p => p.AddWithValue("@id", id));
    }

    public bool TieneLibrosAsociados(int idEditorial)
    {
        const string sql = "SELECT COUNT(1) FROM dbo.Libros WHERE IdEditorial = @id;";
        return Convert.ToInt32(_sql.Escalar(sql, p => p.AddWithValue("@id", idEditorial))) > 0;
    }

    public bool ExisteNombre(string nombre, int? excluirId = null)
    {
        const string sql = """
            SELECT COUNT(1)
            FROM dbo.Editoriales
            WHERE Nombre = @nombre AND (@excluirId IS NULL OR IdEditorial <> @excluirId);
            """;
        return Convert.ToInt32(_sql.Escalar(sql, p =>
        {
            p.AddWithValue("@nombre", nombre.Trim());
            p.AddWithValue("@excluirId", (object?)excluirId ?? DBNull.Value);
        })) > 0;
    }

    private static Editorial Mapear(SqlDataReader lector) => new()
    {
        Id = lector.GetInt32(0),
        Nombre = lector.GetString(1),
        Pais = lector.IsDBNull(2) ? null : lector.GetString(2),
        Telefono = lector.IsDBNull(3) ? null : lector.GetString(3),
        CorreoElectronico = lector.IsDBNull(4) ? null : lector.GetString(4),
        FechaRegistro = lector.GetDateTime(5)
    };
}
