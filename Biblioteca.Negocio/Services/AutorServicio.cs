using Biblioteca.Datos.Repositories;
using Biblioteca.Entidades.Models;
using Biblioteca.Negocio.Validators;

namespace Biblioteca.Negocio.Services;

public class AutorServicio
{
    private readonly AutorRepositorio _repositorio;

    public AutorServicio(AutorRepositorio repositorio)
    {
        _repositorio = repositorio;
    }

    public IReadOnlyList<Autor> Listar() => _repositorio.ObtenerTodos();

    public IReadOnlyList<Autor> Buscar(string? texto) =>
        string.IsNullOrWhiteSpace(texto) ? _repositorio.ObtenerTodos() : _repositorio.Buscar(texto.Trim());

    public Autor? Obtener(int id) => _repositorio.ObtenerPorId(id);

    public ResultadoOperacion Guardar(Autor autor)
    {
        try
        {
            Validar(autor);
            if (_repositorio.ExisteNombreApellido(autor.Nombre, autor.Apellido, autor.Id == 0 ? null : autor.Id))
            {
                return ResultadoOperacion.Error("Ya existe un autor con el mismo nombre y apellido.");
            }

            if (autor.Id == 0)
            {
                var id = _repositorio.Insertar(autor);
                return ResultadoOperacion.Ok("Autor registrado correctamente.", id);
            }

            _repositorio.Actualizar(autor);
            return ResultadoOperacion.Ok("Autor actualizado correctamente.", autor.Id);
        }
        catch (ValidacionException ex)
        {
            return ResultadoOperacion.Error(ex.Message);
        }
        catch (Exception ex)
        {
            return ResultadoOperacion.Error($"No fue posible guardar el autor: {ex.Message}");
        }
    }

    public ResultadoOperacion Eliminar(int id)
    {
        try
        {
            if (_repositorio.ObtenerPorId(id) is null)
            {
                return ResultadoOperacion.Error("El autor no existe.");
            }

            if (_repositorio.TieneLibrosAsociados(id))
            {
                return ResultadoOperacion.Error("No se puede eliminar el autor porque tiene libros asociados.");
            }

            _repositorio.Eliminar(id);
            return ResultadoOperacion.Ok("Autor eliminado correctamente.");
        }
        catch (Exception ex)
        {
            return ResultadoOperacion.Error($"No fue posible eliminar el autor: {ex.Message}");
        }
    }

    private static void Validar(Autor autor)
    {
        autor.Nombre = ValidadorCampos.Longitud(ValidadorCampos.Requerido(autor.Nombre, "Nombre"), "Nombre", 2, 80);
        autor.Apellido = ValidadorCampos.Longitud(ValidadorCampos.Requerido(autor.Apellido, "Apellido"), "Apellido", 2, 80);
        autor.Nacionalidad = string.IsNullOrWhiteSpace(autor.Nacionalidad) ? null : autor.Nacionalidad.Trim();
        if (autor.FechaNacimiento.HasValue && autor.FechaNacimiento.Value.Date > DateTime.Today)
        {
            throw new ValidacionException("La fecha de nacimiento no puede ser futura.");
        }
    }
}
