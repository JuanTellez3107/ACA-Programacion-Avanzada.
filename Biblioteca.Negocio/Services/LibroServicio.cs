using Biblioteca.Datos.Repositories;
using Biblioteca.Entidades.Models;
using Biblioteca.Negocio.Validators;

namespace Biblioteca.Negocio.Services;

public class LibroServicio
{
    private readonly LibroRepositorio _libros;
    private readonly EditorialRepositorio _editoriales;
    private readonly AutorRepositorio _autores;

    public LibroServicio(LibroRepositorio libros, EditorialRepositorio editoriales, AutorRepositorio autores)
    {
        _libros = libros;
        _editoriales = editoriales;
        _autores = autores;
    }

    public IReadOnlyList<Libro> Listar() => _libros.ObtenerTodos();

    public IReadOnlyList<Libro> Buscar(string? texto) =>
        string.IsNullOrWhiteSpace(texto) ? _libros.ObtenerTodos() : _libros.Buscar(texto.Trim());

    public Libro? Obtener(int id) => _libros.ObtenerPorId(id);

    public ResultadoOperacion Guardar(Libro libro)
    {
        try
        {
            libro.Isbn = ValidadorCampos.Longitud(ValidadorCampos.Requerido(libro.Isbn, "ISBN"), "ISBN", 5, 20);
            libro.Titulo = ValidadorCampos.Longitud(ValidadorCampos.Requerido(libro.Titulo, "Título"), "Título", 2, 200);
            libro.Genero = string.IsNullOrWhiteSpace(libro.Genero) ? null : libro.Genero.Trim();
            ValidadorCampos.EnteroPositivo(libro.IdEditorial, "Editorial");
            ValidadorCampos.EnteroPositivo(libro.NumeroPaginas, "Número de páginas");
            if (libro.Existencias < 0)
            {
                throw new ValidacionException("Las existencias no pueden ser negativas.");
            }

            var anioMaximo = DateTime.Today.Year + 1;
            if (libro.AnioPublicacion < 1450 || libro.AnioPublicacion > anioMaximo)
            {
                throw new ValidacionException($"El año de publicación debe estar entre 1450 y {anioMaximo}.");
            }

            if (_editoriales.ObtenerPorId(libro.IdEditorial) is null)
            {
                return ResultadoOperacion.Error("La editorial seleccionada no existe.");
            }

            if (libro.IdsAutores.Count == 0)
            {
                return ResultadoOperacion.Error("Debe asociar al menos un autor.");
            }

            foreach (var idAutor in libro.IdsAutores)
            {
                if (_autores.ObtenerPorId(idAutor) is null)
                {
                    return ResultadoOperacion.Error("Uno de los autores seleccionados no existe.");
                }
            }

            if (_libros.ExisteIsbn(libro.Isbn, libro.Id == 0 ? null : libro.Id))
            {
                return ResultadoOperacion.Error("El ISBN ya está registrado.");
            }

            if (libro.Id == 0)
            {
                libro.Disponibles = libro.Existencias;
                var id = _libros.Insertar(libro);
                return ResultadoOperacion.Ok("Libro registrado correctamente.", id);
            }

            var actual = _libros.ObtenerPorId(libro.Id);
            if (actual is null)
            {
                return ResultadoOperacion.Error("El libro no existe.");
            }

            var prestados = actual.Existencias - actual.Disponibles;
            if (libro.Existencias < prestados)
            {
                return ResultadoOperacion.Error($"No puede reducir las existencias por debajo de {prestados} ejemplares prestados.");
            }

            libro.Disponibles = libro.Existencias - prestados;
            _libros.Actualizar(libro);
            return ResultadoOperacion.Ok("Libro actualizado correctamente.", libro.Id);
        }
        catch (ValidacionException ex)
        {
            return ResultadoOperacion.Error(ex.Message);
        }
        catch (Exception ex)
        {
            return ResultadoOperacion.Error($"No fue posible guardar el libro: {ex.Message}");
        }
    }

    public ResultadoOperacion Eliminar(int id)
    {
        try
        {
            if (_libros.ObtenerPorId(id) is null)
            {
                return ResultadoOperacion.Error("El libro no existe.");
            }

            if (_libros.TienePrestamos(id))
            {
                return ResultadoOperacion.Error("No se puede eliminar el libro porque tiene préstamos asociados.");
            }

            _libros.Eliminar(id);
            return ResultadoOperacion.Ok("Libro eliminado correctamente.");
        }
        catch (Exception ex)
        {
            return ResultadoOperacion.Error($"No fue posible eliminar el libro: {ex.Message}");
        }
    }
}
