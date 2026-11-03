namespace Biblioteca.Entidades.Models;

public class Prestamo : EntidadBase
{
    public int IdUsuario { get; set; }
    public int IdLibro { get; set; }
    public string UsuarioNombre { get; set; } = string.Empty;
    public string UsuarioDocumento { get; set; } = string.Empty;
    public string LibroTitulo { get; set; } = string.Empty;
    public string LibroIsbn { get; set; } = string.Empty;
    public DateTime FechaPrestamo { get; set; } = DateTime.Today;
    public DateTime FechaDevolucionEstimada { get; set; } = DateTime.Today.AddDays(15);
    public DateTime? FechaDevolucionReal { get; set; }
    public string Estado { get; set; } = EstadosPrestamo.Prestado;
    public string? Observaciones { get; set; }
    public DateTime FechaRegistro { get; set; }

    public bool EstaActivo => Estado == EstadosPrestamo.Prestado;
}
