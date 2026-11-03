namespace Biblioteca.Entidades.Models;

public class Autor : EntidadBase
{
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string? Nacionalidad { get; set; }
    public DateTime? FechaNacimiento { get; set; }
    public DateTime FechaRegistro { get; set; }

    public string NombreCompleto => $"{Nombre} {Apellido}".Trim();
}
