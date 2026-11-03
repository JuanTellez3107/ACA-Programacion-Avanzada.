using Biblioteca.Datos.Repositories;
using Biblioteca.Entidades.Models;
using Biblioteca.Negocio.Validators;

namespace Biblioteca.Negocio.Services;

public class EditorialServicio
{
    private readonly EditorialRepositorio _repositorio;

    public EditorialServicio(EditorialRepositorio repositorio)
    {
        _repositorio = repositorio;
    }

    public IReadOnlyList<Editorial> Listar() => _repositorio.ObtenerTodos();

    public IReadOnlyList<Editorial> Buscar(string? texto) =>
        string.IsNullOrWhiteSpace(texto) ? _repositorio.ObtenerTodos() : _repositorio.Buscar(texto.Trim());

    public Editorial? Obtener(int id) => _repositorio.ObtenerPorId(id);

    public ResultadoOperacion Guardar(Editorial editorial)
    {
        try
        {
            editorial.Nombre = ValidadorCampos.Longitud(ValidadorCampos.Requerido(editorial.Nombre, "Nombre"), "Nombre", 2, 150);
            editorial.Pais = string.IsNullOrWhiteSpace(editorial.Pais) ? null : editorial.Pais.Trim();
            editorial.Telefono = ValidadorCampos.SoloDigitosOpcional(editorial.Telefono, "Teléfono");
            editorial.CorreoElectronico = ValidadorCampos.CorreoOpcional(editorial.CorreoElectronico, "Correo electrónico");

            if (_repositorio.ExisteNombre(editorial.Nombre, editorial.Id == 0 ? null : editorial.Id))
            {
                return ResultadoOperacion.Error("Ya existe una editorial con ese nombre.");
            }

            if (editorial.Id == 0)
            {
                var id = _repositorio.Insertar(editorial);
                return ResultadoOperacion.Ok("Editorial registrada correctamente.", id);
            }

            _repositorio.Actualizar(editorial);
            return ResultadoOperacion.Ok("Editorial actualizada correctamente.", editorial.Id);
        }
        catch (ValidacionException ex)
        {
            return ResultadoOperacion.Error(ex.Message);
        }
        catch (Exception ex)
        {
            return ResultadoOperacion.Error($"No fue posible guardar la editorial: {ex.Message}");
        }
    }

    public ResultadoOperacion Eliminar(int id)
    {
        try
        {
            if (_repositorio.ObtenerPorId(id) is null)
            {
                return ResultadoOperacion.Error("La editorial no existe.");
            }

            if (_repositorio.TieneLibrosAsociados(id))
            {
                return ResultadoOperacion.Error("No se puede eliminar la editorial porque tiene libros asociados.");
            }

            _repositorio.Eliminar(id);
            return ResultadoOperacion.Ok("Editorial eliminada correctamente.");
        }
        catch (Exception ex)
        {
            return ResultadoOperacion.Error($"No fue posible eliminar la editorial: {ex.Message}");
        }
    }
}
