namespace Biblioteca.Entidades.Models;

public class DashboardResumen
{
    public int TotalLibros { get; set; }
    public int TotalAutores { get; set; }
    public int TotalEditoriales { get; set; }
    public int TotalUsuarios { get; set; }
    public int PrestamosActivos { get; set; }
    public int PrestamosDevueltos { get; set; }
    public int EjemplaresDisponibles { get; set; }
    public int EjemplaresPrestados { get; set; }
}
