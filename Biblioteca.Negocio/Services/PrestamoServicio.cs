using Biblioteca.Datos.Repositories;
using Biblioteca.Entidades.Models;
using Biblioteca.Negocio.Validators;

namespace Biblioteca.Negocio.Services;

public class PrestamoServicio
{
    private readonly PrestamoRepositorio _prestamos;
    private readonly UsuarioRepositorio _usuarios;
    private readonly LibroRepositorio _libros;

    public PrestamoServicio(PrestamoRepositorio prestamos, UsuarioRepositorio usuarios, LibroRepositorio libros)
    {
        _prestamos = prestamos;
        _usuarios = usuarios;
        _libros = libros;
    }

    public IReadOnlyList<Prestamo> Listar() => _prestamos.ObtenerTodos();

    public IReadOnlyList<Prestamo> ListarActivos() => _prestamos.ObtenerPorEstado(EstadosPrestamo.Prestado);

    public IReadOnlyList<Prestamo> ListarDevueltos() => _prestamos.ObtenerPorEstado(EstadosPrestamo.Devuelto);

    public IReadOnlyList<Prestamo> Buscar(string? texto) =>
        string.IsNullOrWhiteSpace(texto) ? _prestamos.ObtenerTodos() : _prestamos.Buscar(texto.Trim());

    public Prestamo? Obtener(int id) => _prestamos.ObtenerPorId(id);

    public DashboardResumen ObtenerResumen() => _prestamos.ObtenerResumen();

    public ResultadoOperacion Registrar(Prestamo prestamo)
    {
        try
        {
            ValidadorCampos.EnteroPositivo(prestamo.IdUsuario, "Usuario");
            ValidadorCampos.EnteroPositivo(prestamo.IdLibro, "Libro");

            var usuario = _usuarios.ObtenerPorId(prestamo.IdUsuario);
            if (usuario is null)
            {
                return ResultadoOperacion.Error("No se puede registrar el préstamo: el usuario no existe.");
            }

            if (!usuario.Activo)
            {
                return ResultadoOperacion.Error("El usuario no está activo y no puede realizar préstamos.");
            }

            var libro = _libros.ObtenerPorId(prestamo.IdLibro);
            if (libro is null)
            {
                return ResultadoOperacion.Error("No se puede registrar el préstamo: el libro no existe.");
            }

            if (!libro.TieneDisponibilidad)
            {
                return ResultadoOperacion.Error("El libro no tiene ejemplares disponibles.");
            }

            if (prestamo.FechaDevolucionEstimada.Date < prestamo.FechaPrestamo.Date)
            {
                throw new ValidacionException("La fecha estimada de devolución no puede ser anterior a la fecha de préstamo.");
            }

            if (prestamo.FechaPrestamo.Date > DateTime.Today)
            {
                throw new ValidacionException("La fecha de préstamo no puede ser futura.");
            }

            if (_prestamos.ExistePrestamoActivo(prestamo.IdUsuario, prestamo.IdLibro))
            {
                return ResultadoOperacion.Error("El usuario ya tiene un préstamo activo de este libro.");
            }

            var id = _prestamos.Insertar(prestamo);
            return ResultadoOperacion.Ok("Préstamo registrado correctamente.", id);
        }
        catch (ValidacionException ex)
        {
            return ResultadoOperacion.Error(ex.Message);
        }
        catch (Exception ex)
        {
            return ResultadoOperacion.Error($"No fue posible registrar el préstamo: {ex.Message}");
        }
    }

    public ResultadoOperacion ActualizarActivo(Prestamo prestamo)
    {
        try
        {
            var actual = _prestamos.ObtenerPorId(prestamo.Id);
            if (actual is null)
            {
                return ResultadoOperacion.Error("El préstamo no existe.");
            }

            if (!actual.EstaActivo)
            {
                return ResultadoOperacion.Error("Solo se pueden editar préstamos activos.");
            }

            if (prestamo.FechaDevolucionEstimada.Date < prestamo.FechaPrestamo.Date)
            {
                throw new ValidacionException("La fecha estimada de devolución no puede ser anterior a la fecha de préstamo.");
            }

            _prestamos.Actualizar(prestamo);
            return ResultadoOperacion.Ok("Préstamo actualizado correctamente.", prestamo.Id);
        }
        catch (ValidacionException ex)
        {
            return ResultadoOperacion.Error(ex.Message);
        }
        catch (Exception ex)
        {
            return ResultadoOperacion.Error($"No fue posible actualizar el préstamo: {ex.Message}");
        }
    }

    public ResultadoOperacion RegistrarDevolucion(int idPrestamo, DateTime fechaDevolucion, string? observaciones)
    {
        try
        {
            var prestamo = _prestamos.ObtenerPorId(idPrestamo);
            if (prestamo is null)
            {
                return ResultadoOperacion.Error("El préstamo no existe.");
            }

            if (!prestamo.EstaActivo)
            {
                return ResultadoOperacion.Error("El préstamo ya fue devuelto.");
            }

            if (fechaDevolucion.Date < prestamo.FechaPrestamo.Date)
            {
                throw new ValidacionException("La fecha de devolución no puede ser anterior a la fecha de préstamo.");
            }

            if (fechaDevolucion.Date > DateTime.Today)
            {
                throw new ValidacionException("La fecha de devolución no puede ser futura.");
            }

            _prestamos.RegistrarDevolucion(idPrestamo, fechaDevolucion, observaciones);
            return ResultadoOperacion.Ok("Devolución registrada. El préstamo quedó en estado Devuelto.");
        }
        catch (ValidacionException ex)
        {
            return ResultadoOperacion.Error(ex.Message);
        }
        catch (Exception ex)
        {
            return ResultadoOperacion.Error($"No fue posible registrar la devolución: {ex.Message}");
        }
    }

    public ResultadoOperacion Eliminar(int id)
    {
        try
        {
            if (_prestamos.ObtenerPorId(id) is null)
            {
                return ResultadoOperacion.Error("El préstamo no existe.");
            }

            _prestamos.Eliminar(id);
            return ResultadoOperacion.Ok("Préstamo eliminado correctamente.");
        }
        catch (Exception ex)
        {
            return ResultadoOperacion.Error($"No fue posible eliminar el préstamo: {ex.Message}");
        }
    }
}
