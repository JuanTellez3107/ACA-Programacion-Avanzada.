using Biblioteca.Datos.Connection;
using Biblioteca.Datos.Interfaces;
using Biblioteca.Entidades.Models;
using Microsoft.Data.SqlClient;

namespace Biblioteca.Datos.Repositories;

public class UsuarioRepositorio : IRepositorio<Usuario>
{
    private readonly SqlExecutor _sql;

    public UsuarioRepositorio(SqlExecutor sql)
    {
        _sql = sql;
    }

    public IReadOnlyList<Usuario> ObtenerTodos()
    {
        const string consulta = """
            SELECT IdUsuario, Nombre, Apellido, Documento, Telefono, CorreoElectronico, Direccion, FechaRegistro, Activo
            FROM dbo.Usuarios
            ORDER BY Apellido, Nombre;
            """;
        return _sql.Consultar(consulta, Mapear);
    }

    public IReadOnlyList<Usuario> Buscar(string texto)
    {
        const string consulta = """
            SELECT IdUsuario, Nombre, Apellido, Documento, Telefono, CorreoElectronico, Direccion, FechaRegistro, Activo
            FROM dbo.Usuarios
            WHERE Nombre LIKE @texto OR Apellido LIKE @texto OR Documento LIKE @texto OR CorreoElectronico LIKE @texto
            ORDER BY Apellido, Nombre;
            """;
        return _sql.Consultar(consulta, Mapear, p => p.AddWithValue("@texto", $"%{texto}%"));
    }

    public Usuario? ObtenerPorId(int id)
    {
        const string consulta = """
            SELECT IdUsuario, Nombre, Apellido, Documento, Telefono, CorreoElectronico, Direccion, FechaRegistro, Activo
            FROM dbo.Usuarios
            WHERE IdUsuario = @id;
            """;
        return _sql.ConsultarUno(consulta, Mapear, p => p.AddWithValue("@id", id));
    }

    public int Insertar(Usuario entidad)
    {
        const string sql = """
            INSERT INTO dbo.Usuarios (Nombre, Apellido, Documento, Telefono, CorreoElectronico, Direccion, Activo)
            OUTPUT INSERTED.IdUsuario
            VALUES (@nombre, @apellido, @documento, @telefono, @correo, @direccion, @activo);
            """;
        return Convert.ToInt32(_sql.Escalar(sql, p => Asignar(p, entidad)));
    }

    public void Actualizar(Usuario entidad)
    {
        const string sql = """
            UPDATE dbo.Usuarios
            SET Nombre = @nombre, Apellido = @apellido, Documento = @documento,
                Telefono = @telefono, CorreoElectronico = @correo, Direccion = @direccion, Activo = @activo
            WHERE IdUsuario = @id;
            """;
        _sql.Ejecutar(sql, p =>
        {
            p.AddWithValue("@id", entidad.Id);
            Asignar(p, entidad);
        });
    }

    public void Eliminar(int id)
    {
        _sql.Ejecutar("DELETE FROM dbo.Usuarios WHERE IdUsuario = @id;", p => p.AddWithValue("@id", id));
    }

    public bool TienePrestamos(int idUsuario)
    {
        const string sql = "SELECT COUNT(1) FROM dbo.Prestamos WHERE IdUsuario = @id;";
        return Convert.ToInt32(_sql.Escalar(sql, p => p.AddWithValue("@id", idUsuario))) > 0;
    }

    public bool ExisteDocumento(string documento, int? excluirId = null)
    {
        const string sql = """
            SELECT COUNT(1) FROM dbo.Usuarios
            WHERE Documento = @documento AND (@excluirId IS NULL OR IdUsuario <> @excluirId);
            """;
        return Convert.ToInt32(_sql.Escalar(sql, p =>
        {
            p.AddWithValue("@documento", documento.Trim());
            p.AddWithValue("@excluirId", (object?)excluirId ?? DBNull.Value);
        })) > 0;
    }

    public bool ExisteCorreo(string correo, int? excluirId = null)
    {
        const string sql = """
            SELECT COUNT(1) FROM dbo.Usuarios
            WHERE CorreoElectronico = @correo AND (@excluirId IS NULL OR IdUsuario <> @excluirId);
            """;
        return Convert.ToInt32(_sql.Escalar(sql, p =>
        {
            p.AddWithValue("@correo", correo.Trim());
            p.AddWithValue("@excluirId", (object?)excluirId ?? DBNull.Value);
        })) > 0;
    }

    private static void Asignar(SqlParameterCollection p, Usuario entidad)
    {
        p.AddWithValue("@nombre", entidad.Nombre.Trim());
        p.AddWithValue("@apellido", entidad.Apellido.Trim());
        p.AddWithValue("@documento", entidad.Documento.Trim());
        p.AddWithValue("@telefono", (object?)entidad.Telefono ?? DBNull.Value);
        p.AddWithValue("@correo", entidad.CorreoElectronico.Trim());
        p.AddWithValue("@direccion", (object?)entidad.Direccion ?? DBNull.Value);
        p.AddWithValue("@activo", entidad.Activo);
    }

    private static Usuario Mapear(SqlDataReader lector) => new()
    {
        Id = lector.GetInt32(0),
        Nombre = lector.GetString(1),
        Apellido = lector.GetString(2),
        Documento = lector.GetString(3),
        Telefono = lector.IsDBNull(4) ? null : lector.GetString(4),
        CorreoElectronico = lector.GetString(5),
        Direccion = lector.IsDBNull(6) ? null : lector.GetString(6),
        FechaRegistro = lector.GetDateTime(7),
        Activo = lector.GetBoolean(8)
    };
}
