using Biblioteca.Datos.Connection;
using Biblioteca.Entidades.Models;
using Biblioteca.Negocio.Services;

var config = new ConfiguracionConexion
{
    Server = @"(localdb)\MSSQLLocalDB",
    Database = "BibliotecaDB",
    IntegratedSecurity = true,
    TrustServerCertificate = true
};
var servicios = new ServiciosAplicacion(new FabricaConexion(config));

if (!servicios.Fabrica.ProbarServidor(out var msgServidor))
{
    Console.WriteLine("FALLO servidor: " + msgServidor);
    return 1;
}
Console.WriteLine(msgServidor);

var raiz = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "Database"));
if (!Directory.Exists(raiz))
{
    raiz = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "Database"));
}
Console.WriteLine("Scripts: " + raiz);
servicios.Instalador.EjecutarScripts(Path.Combine(raiz, "Biblioteca.sql"), Path.Combine(raiz, "DatosPrueba.sql"));

if (!servicios.Fabrica.Probar(out var msgBd))
{
    Console.WriteLine("FALLO base: " + msgBd);
    return 1;
}
Console.WriteLine(msgBd);

var resumen = servicios.Prestamos.ObtenerResumen();
Console.WriteLine($"Libros={resumen.TotalLibros} Autores={resumen.TotalAutores} Usuarios={resumen.TotalUsuarios} Activos={resumen.PrestamosActivos}");
if (resumen.TotalLibros < 1 || resumen.TotalUsuarios < 1)
{
    Console.WriteLine("FALLO: datos de prueba incompletos.");
    return 1;
}

var alta = servicios.Autores.Guardar(new Autor { Nombre = "Prueba", Apellido = "Integracion" + DateTime.Now.Ticks });
Console.WriteLine("Alta autor: " + alta.Mensaje);
if (!alta.EsExitoso || alta.IdGenerado is null)
{
    return 1;
}

var baja = servicios.Autores.Eliminar(alta.IdGenerado.Value);
Console.WriteLine("Baja autor: " + baja.Mensaje);

var dup = servicios.Usuarios.Guardar(new Usuario
{
    Nombre = "Ana",
    Apellido = "Duplicada",
    Documento = "1002345678",
    CorreoElectronico = "otro@correo.edu"
});
Console.WriteLine("Documento duplicado: " + dup.Mensaje + " (esperado error)");
if (dup.EsExitoso)
{
    Console.WriteLine("FALLO: debió rechazar documento duplicado.");
    return 1;
}

var activosAntes = servicios.Prestamos.ListarActivos().Count;
var primerActivo = servicios.Prestamos.ListarActivos().FirstOrDefault();
if (primerActivo is not null)
{
    var dev = servicios.Prestamos.RegistrarDevolucion(primerActivo.Id, DateTime.Today, "Devolución de prueba automática");
    Console.WriteLine("Devolución: " + dev.Mensaje);
    var activosDespues = servicios.Prestamos.ListarActivos().Count;
    if (!dev.EsExitoso || activosDespues != activosAntes - 1)
    {
        Console.WriteLine("FALLO devolución.");
        return 1;
    }
}

Console.WriteLine("SMOKE TEST OK");
return 0;
