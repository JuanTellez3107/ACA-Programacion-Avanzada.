using Microsoft.Data.SqlClient;

namespace Biblioteca.Datos.Connection;

public class InstaladorBaseDatos
{
    private readonly FabricaConexion _fabrica;

    public InstaladorBaseDatos(FabricaConexion fabrica)
    {
        _fabrica = fabrica;
    }

    public void EjecutarScripts(string rutaEsquema, string rutaDatos)
    {
        EjecutarArchivo(_fabrica.Configuracion.CadenaServidor, rutaEsquema);
        EjecutarArchivo(_fabrica.Configuracion.CadenaConexion, rutaDatos);
    }

    private static void EjecutarArchivo(string cadena, string ruta)
    {
        var contenido = File.ReadAllText(ruta);
        var lotes = contenido.Split(new[] { "\r\nGO", "\nGO", "\rGO" }, StringSplitOptions.RemoveEmptyEntries);

        using var conexion = new SqlConnection(cadena);
        conexion.Open();
        foreach (var lote in lotes)
        {
            var sql = lote.Trim();
            if (sql.Length == 0 || sql.StartsWith("GO", StringComparison.OrdinalIgnoreCase) && sql.Length <= 3)
            {
                continue;
            }

            using var comando = new SqlCommand(sql, conexion) { CommandTimeout = 120 };
            comando.ExecuteNonQuery();
        }
    }
}
