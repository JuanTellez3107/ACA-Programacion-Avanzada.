using Microsoft.Data.SqlClient;

namespace Biblioteca.Datos.Connection;

public class SqlExecutor
{
    private readonly Func<SqlConnection> _crearConexion;

    public SqlExecutor(Func<SqlConnection> crearConexion)
    {
        _crearConexion = crearConexion;
    }

    public SqlConnection CrearConexion() => _crearConexion();

    public int Ejecutar(string sql, Action<SqlParameterCollection>? parametros = null, SqlTransaction? transaccion = null)
    {
        if (transaccion is not null)
        {
            using var comando = new SqlCommand(sql, transaccion.Connection, transaccion) { CommandTimeout = 30 };
            parametros?.Invoke(comando.Parameters);
            return comando.ExecuteNonQuery();
        }

        using var conexion = _crearConexion();
        conexion.Open();
        using var cmd = new SqlCommand(sql, conexion) { CommandTimeout = 30 };
        parametros?.Invoke(cmd.Parameters);
        return cmd.ExecuteNonQuery();
    }

    public object? Escalar(string sql, Action<SqlParameterCollection>? parametros = null, SqlTransaction? transaccion = null)
    {
        if (transaccion is not null)
        {
            using var comando = new SqlCommand(sql, transaccion.Connection, transaccion) { CommandTimeout = 30 };
            parametros?.Invoke(comando.Parameters);
            return comando.ExecuteScalar();
        }

        using var conexion = _crearConexion();
        conexion.Open();
        using var cmd = new SqlCommand(sql, conexion) { CommandTimeout = 30 };
        parametros?.Invoke(cmd.Parameters);
        return cmd.ExecuteScalar();
    }

    public List<T> Consultar<T>(string sql, Func<SqlDataReader, T> mapear, Action<SqlParameterCollection>? parametros = null)
    {
        var lista = new List<T>();
        using var conexion = _crearConexion();
        conexion.Open();
        using var cmd = new SqlCommand(sql, conexion) { CommandTimeout = 30 };
        parametros?.Invoke(cmd.Parameters);
        using var lector = cmd.ExecuteReader();
        while (lector.Read())
        {
            lista.Add(mapear(lector));
        }

        return lista;
    }

    public T? ConsultarUno<T>(string sql, Func<SqlDataReader, T> mapear, Action<SqlParameterCollection>? parametros = null)
        where T : class
    {
        using var conexion = _crearConexion();
        conexion.Open();
        using var cmd = new SqlCommand(sql, conexion) { CommandTimeout = 30 };
        parametros?.Invoke(cmd.Parameters);
        using var lector = cmd.ExecuteReader();
        return lector.Read() ? mapear(lector) : null;
    }
}
