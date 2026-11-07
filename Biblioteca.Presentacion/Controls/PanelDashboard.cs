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
        _tarjetas = new FlowCristal
        {
            Dock = DockStyle.Top,
            Height = 310,
            Capa = UiFondo.Capa.Imagen,
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
            ForeColor = UiTema.TextoClaro,
            BackColor = Color.Transparent
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
            AgregarTarjeta("Libros", resumen.TotalLibros.ToString(), Color.FromArgb(22, 58, 96));
            AgregarTarjeta("Autores", resumen.TotalAutores.ToString(), UiTema.Acento);
            AgregarTarjeta("Editoriales", resumen.TotalEditoriales.ToString(), Color.FromArgb(30, 78, 112));
            AgregarTarjeta("Usuarios", resumen.TotalUsuarios.ToString(), Color.FromArgb(46, 104, 146));
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
        var tarjeta = new PanelCristal
        {
            Width = 210,
            Height = 78,
            Margin = new Padding(8),
            Capa = UiFondo.Capa.VidrioClaro
        };
        tarjeta.Controls.Add(new Panel
        {
            Dock = DockStyle.Left,
            Width = 6,
            BackColor = color
        });
        tarjeta.Controls.Add(new Label
        {
            Text = titulo,
            ForeColor = UiTema.Texto,
            Location = new Point(18, 8),
            AutoSize = true,
            BackColor = Color.Transparent
        });
        tarjeta.Controls.Add(new Label
        {
            Text = valor,
            ForeColor = color,
            Font = new Font("Segoe UI", 18, FontStyle.Bold),
            Location = new Point(18, 32),
            AutoSize = true,
            BackColor = Color.Transparent
        });
        _tarjetas.Controls.Add(tarjeta);
    }
}
