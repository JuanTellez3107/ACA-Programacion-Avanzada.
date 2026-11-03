namespace Biblioteca.Entidades.Models;

public class Usuario : EntidadBase
{
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string Documento { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public string CorreoElectronico { get; set; } = string.Empty;
    public string? Direccion { get; set; }
    public DateTime FechaRegistro { get; set; }
    public bool Activo { get; set; } = true;

    public string NombreCompleto => $"{Nombre} {Apellido}".Trim();
}
