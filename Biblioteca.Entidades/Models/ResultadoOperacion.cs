namespace Biblioteca.Entidades.Models;

public class ResultadoOperacion
{
    public bool EsExitoso { get; init; }
    public string Mensaje { get; init; } = string.Empty;
    public int? IdGenerado { get; init; }

    public static ResultadoOperacion Ok(string mensaje, int? id = null) =>
        new() { EsExitoso = true, Mensaje = mensaje, IdGenerado = id };

    public static ResultadoOperacion Error(string mensaje) =>
        new() { EsExitoso = false, Mensaje = mensaje };
}
