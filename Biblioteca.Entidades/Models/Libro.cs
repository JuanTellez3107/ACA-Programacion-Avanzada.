namespace Biblioteca.Entidades.Models;

public class Libro : EntidadBase
{
    public string Isbn { get; set; } = string.Empty;
    public string Titulo { get; set; } = string.Empty;
    public int IdEditorial { get; set; }
    public string? EditorialNombre { get; set; }
    public int AnioPublicacion { get; set; } = DateTime.Today.Year;
    public string? Genero { get; set; }
    public int NumeroPaginas { get; set; } = 1;
    public int Existencias { get; set; } = 1;
    public int Disponibles { get; set; } = 1;
    public DateTime FechaRegistro { get; set; }
    public List<int> IdsAutores { get; set; } = new();
    public string AutoresNombres { get; set; } = string.Empty;

    public bool TieneDisponibilidad => Disponibles > 0;

    public string TituloInventario => $"{Titulo} ({Disponibles}/{Existencias} disp.)";
}
