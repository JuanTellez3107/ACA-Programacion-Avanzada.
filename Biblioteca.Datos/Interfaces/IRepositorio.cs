using Biblioteca.Entidades.Models;

namespace Biblioteca.Datos.Interfaces;

public interface IRepositorio<T> where T : EntidadBase
{
    IReadOnlyList<T> ObtenerTodos();
    T? ObtenerPorId(int id);
    int Insertar(T entidad);
    void Actualizar(T entidad);
    void Eliminar(int id);
}
