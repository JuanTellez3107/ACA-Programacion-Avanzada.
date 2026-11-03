using Microsoft.Data.SqlClient;

namespace Biblioteca.Datos.Connection;

public class FabricaConexion
{
    private ConfiguracionConexion _configuracion;

    public FabricaConexion(ConfiguracionConexion configuracion)
    {
        _configuracion = configuracion;
    }

    public ConfiguracionConexion Configuracion => _configuracion;

    public void Actualizar(ConfiguracionConexion configuracion)
    {
        _configuracion = configuracion;
        AlmacenConexion.Guardar(configuracion);
    }

    public SqlConnection Crear() => new(_configuracion.CadenaConexion);

    public SqlConnection CrearMaster() => new(_configuracion.CadenaServidor);

    public bool Probar(out string mensaje)
    {
        try
        {
            using var conexion = Crear();
            conexion.Open();
            mensaje = $"Conexión establecida con {conexion.DataSource} / {conexion.Database}.";
            return true;
        }
        catch (Exception ex)
        {
            mensaje = ex.Message;
            return false;
        }
    }

    public bool ProbarServidor(out string mensaje)
    {
        try
        {
            using var conexion = CrearMaster();
            conexion.Open();
            mensaje = $"Servidor disponible: {conexion.DataSource}.";
            return true;
        }
        catch (Exception ex)
        {
            mensaje = ex.Message;
            return false;
        }
    }
}
