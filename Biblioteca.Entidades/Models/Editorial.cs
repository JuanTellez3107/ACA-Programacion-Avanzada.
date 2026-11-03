namespace Biblioteca.Entidades.Models;

public class Editorial : EntidadBase
{
    public string Nombre { get; set; } = string.Empty;
    public string? Pais { get; set; }
    public string? Telefono { get; set; }
    public string? CorreoElectronico { get; set; }
    public DateTime FechaRegistro { get; set; }
}
