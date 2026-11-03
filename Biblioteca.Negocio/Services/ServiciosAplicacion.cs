using Biblioteca.Datos.Connection;
using Biblioteca.Datos.Repositories;

namespace Biblioteca.Negocio.Services;

public class ServiciosAplicacion
{
    public FabricaConexion Fabrica { get; }
    public AutorServicio Autores { get; }
    public EditorialServicio Editoriales { get; }
    public UsuarioServicio Usuarios { get; }
    public LibroServicio Libros { get; }
    public PrestamoServicio Prestamos { get; }
    public InstaladorBaseDatos Instalador { get; }

    public ServiciosAplicacion(FabricaConexion fabrica)
    {
        Fabrica = fabrica;
        var sql = new SqlExecutor(() => fabrica.Crear());
        var autores = new AutorRepositorio(sql);
        var editoriales = new EditorialRepositorio(sql);
        var usuarios = new UsuarioRepositorio(sql);
        var libros = new LibroRepositorio(sql);
        var prestamos = new PrestamoRepositorio(sql);

        Autores = new AutorServicio(autores);
        Editoriales = new EditorialServicio(editoriales);
        Usuarios = new UsuarioServicio(usuarios);
        Libros = new LibroServicio(libros, editoriales, autores);
        Prestamos = new PrestamoServicio(prestamos, usuarios, libros);
        Instalador = new InstaladorBaseDatos(fabrica);
    }

    public static ServiciosAplicacion CrearPorDefecto()
    {
        var config = AlmacenConexion.Cargar();
        return new ServiciosAplicacion(new FabricaConexion(config));
    }
}
