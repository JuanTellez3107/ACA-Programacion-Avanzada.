using Biblioteca.Datos.Repositories;
using Biblioteca.Entidades.Models;
using Biblioteca.Negocio.Validators;

namespace Biblioteca.Negocio.Services;

public class UsuarioServicio
{
    private readonly UsuarioRepositorio _repositorio;

    public UsuarioServicio(UsuarioRepositorio repositorio)
    {
        _repositorio = repositorio;
    }

    public IReadOnlyList<Usuario> Listar() => _repositorio.ObtenerTodos();

    public IReadOnlyList<Usuario> Buscar(string? texto) =>
        string.IsNullOrWhiteSpace(texto) ? _repositorio.ObtenerTodos() : _repositorio.Buscar(texto.Trim());

    public Usuario? Obtener(int id) => _repositorio.ObtenerPorId(id);

    public ResultadoOperacion Guardar(Usuario usuario)
    {
        try
        {
            usuario.Nombre = ValidadorCampos.Longitud(ValidadorCampos.Requerido(usuario.Nombre, "Nombre"), "Nombre", 2, 80);
            usuario.Apellido = ValidadorCampos.Longitud(ValidadorCampos.Requerido(usuario.Apellido, "Apellido"), "Apellido", 2, 80);
            usuario.Documento = ValidadorCampos.Longitud(ValidadorCampos.Requerido(usuario.Documento, "Documento"), "Documento", 5, 20);
            usuario.CorreoElectronico = ValidadorCampos.Correo(
                ValidadorCampos.Longitud(ValidadorCampos.Requerido(usuario.CorreoElectronico, "Correo electrónico"), "Correo electrónico", 6, 150),
                "Correo electrónico");
            usuario.Telefono = ValidadorCampos.SoloDigitosOpcional(usuario.Telefono, "Teléfono");
            usuario.Direccion = string.IsNullOrWhiteSpace(usuario.Direccion) ? null : usuario.Direccion.Trim();

            if (_repositorio.ExisteDocumento(usuario.Documento, usuario.Id == 0 ? null : usuario.Id))
            {
                return ResultadoOperacion.Error("El documento ya está registrado.");
            }

            if (_repositorio.ExisteCorreo(usuario.CorreoElectronico, usuario.Id == 0 ? null : usuario.Id))
            {
                return ResultadoOperacion.Error("El correo electrónico ya está registrado.");
            }

            if (usuario.Id == 0)
            {
                var id = _repositorio.Insertar(usuario);
                return ResultadoOperacion.Ok("Usuario registrado correctamente.", id);
            }

            _repositorio.Actualizar(usuario);
            return ResultadoOperacion.Ok("Usuario actualizado correctamente.", usuario.Id);
        }
        catch (ValidacionException ex)
        {
            return ResultadoOperacion.Error(ex.Message);
        }
        catch (Exception ex)
        {
            return ResultadoOperacion.Error($"No fue posible guardar el usuario: {ex.Message}");
        }
    }

    public ResultadoOperacion Eliminar(int id)
    {
        try
        {
            if (_repositorio.ObtenerPorId(id) is null)
            {
                return ResultadoOperacion.Error("El usuario no existe.");
            }

            if (_repositorio.TienePrestamos(id))
            {
                return ResultadoOperacion.Error("No se puede eliminar el usuario porque tiene préstamos asociados.");
            }

            _repositorio.Eliminar(id);
            return ResultadoOperacion.Ok("Usuario eliminado correctamente.");
        }
        catch (Exception ex)
        {
            return ResultadoOperacion.Error($"No fue posible eliminar el usuario: {ex.Message}");
        }
    }
}
