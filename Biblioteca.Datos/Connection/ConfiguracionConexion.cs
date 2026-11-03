using System.Text.Json;

namespace Biblioteca.Datos.Connection;

public class ConfiguracionConexion
{
    public string Server { get; set; } = "(localdb)\\MSSQLLocalDB";
    public string Database { get; set; } = "BibliotecaDB";
    public bool IntegratedSecurity { get; set; } = true;
    public bool TrustServerCertificate { get; set; } = true;
    public string? UserId { get; set; }
    public string? Password { get; set; }

    public string CadenaConexion => IntegratedSecurity
        ? $"Server={Server};Database={Database};Integrated Security=True;TrustServerCertificate={TrustServerCertificate};"
        : $"Server={Server};Database={Database};User Id={UserId};Password={Password};TrustServerCertificate={TrustServerCertificate};";

    public string CadenaServidor => IntegratedSecurity
        ? $"Server={Server};Database=master;Integrated Security=True;TrustServerCertificate={TrustServerCertificate};"
        : $"Server={Server};Database=master;User Id={UserId};Password={Password};TrustServerCertificate={TrustServerCertificate};";
}

public static class AlmacenConexion
{
    public static string RutaArchivo =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SistemaBiblioteca", "connection.json");

    public static ConfiguracionConexion Cargar()
    {
        try
        {
            if (File.Exists(RutaArchivo))
            {
                var json = File.ReadAllText(RutaArchivo);
                return JsonSerializer.Deserialize<ConfiguracionConexion>(json) ?? new ConfiguracionConexion();
            }
        }
        catch
        {
            // Se usa la configuración por defecto si el archivo local no es válido.
        }

        return new ConfiguracionConexion();
    }

    public static void Guardar(ConfiguracionConexion configuracion)
    {
        var directorio = Path.GetDirectoryName(RutaArchivo);
        if (!string.IsNullOrWhiteSpace(directorio))
        {
            Directory.CreateDirectory(directorio);
        }

        var opciones = new JsonSerializerOptions { WriteIndented = true };
        File.WriteAllText(RutaArchivo, JsonSerializer.Serialize(configuracion, opciones));
    }
}
