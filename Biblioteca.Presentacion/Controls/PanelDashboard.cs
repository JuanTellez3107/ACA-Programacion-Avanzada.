using Biblioteca.Negocio.Services;

namespace Biblioteca.Presentacion.Controls;

public class PanelDashboard : PanelModulo
{
    private readonly ServiciosAplicacion _servicios;
    private readonly FlowLayoutPanel _tarjetas;
    private readonly DataGridView _grid;

    public PanelDashboard(ServiciosAplicacion servicios)
        : base("Inicio", "Resumen operativo de la biblioteca")
    {
        _servicios = servicios;
        _tarjetas = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 170,
            BackColor = UiTema.Fondo,
            WrapContents = true
        };
        _grid = new DataGridView { Dock = DockStyle.Fill };
        UiTema.EstilizarGrid(_grid);
        var lbl = new Label
        {
            Text = "Préstamos activos",
            Font = UiTema.Subtitulo,
            Dock = DockStyle.Top,
            Height = 32,
            ForeColor = UiTema.Sidebar
        };
        Cuerpo.Controls.Add(_grid);
        Cuerpo.Controls.Add(lbl);
        Cuerpo.Controls.Add(_tarjetas);
    }

    public override void Recargar()
    {
        try
        {
            var resumen = _servicios.Prestamos.ObtenerResumen();
            _tarjetas.Controls.Clear();
            AgregarTarjeta("Libros", resumen.TotalLibros.ToString(), UiTema.Sidebar);
            AgregarTarjeta("Autores", resumen.TotalAutores.ToString(), UiTema.Acento);
            AgregarTarjeta("Editoriales", resumen.TotalEditoriales.ToString(), Color.FromArgb(108, 52, 131));
            AgregarTarjeta("Usuarios", resumen.TotalUsuarios.ToString(), Color.FromArgb(40, 116, 166));
            AgregarTarjeta("Préstamos activos", resumen.PrestamosActivos.ToString(), UiTema.Advertencia);
            AgregarTarjeta("Devueltos", resumen.PrestamosDevueltos.ToString(), UiTema.Exito);
            AgregarTarjeta("Disponibles", resumen.EjemplaresDisponibles.ToString(), UiTema.AcentoOscuro);
            AgregarTarjeta("En préstamo", resumen.EjemplaresPrestados.ToString(), UiTema.Peligro);

            _grid.DataSource = _servicios.Prestamos.ListarActivos()
                .Select(p => new
                {
                    p.Id,
                    Usuario = p.UsuarioNombre,
                    Libro = p.LibroTitulo,
                    p.FechaPrestamo,
                    Devolución = p.FechaDevolucionEstimada,
                    p.Estado
                }).ToList();
        }
        catch (Exception ex)
        {
            UiMensajes.Error("No se pudo cargar el dashboard: " + ex.Message);
        }
    }

    private void AgregarTarjeta(string titulo, string valor, Color color)
    {
        var tarjeta = new Panel
        {
            Width = 180,
            Height = 72,
            BackColor = color,
            Margin = new Padding(8)
        };
        tarjeta.Controls.Add(new Label
        {
            Text = titulo,
            ForeColor = Color.White,
            Location = new Point(12, 8),
            AutoSize = true
        });
        tarjeta.Controls.Add(new Label
        {
            Text = valor,
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 18, FontStyle.Bold),
            Location = new Point(12, 30),
            AutoSize = true
        });
        _tarjetas.Controls.Add(tarjeta);
    }
}
