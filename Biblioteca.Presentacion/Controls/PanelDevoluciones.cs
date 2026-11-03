using Biblioteca.Negocio.Services;

namespace Biblioteca.Presentacion.Controls;

public class PanelDevoluciones : PanelModulo
{
    private readonly ServiciosAplicacion _servicios;
    private readonly DataGridView _grid = new();
    private readonly DateTimePicker _dtDevolucion = new() { Format = DateTimePickerFormat.Short };
    private readonly TextBox _txtObservaciones = UiTema.Caja();

    public PanelDevoluciones(ServiciosAplicacion servicios)
        : base("Devoluciones", "Registrar devolución, actualizar estado y disponibilidad")
    {
        _servicios = servicios;
        var formulario = new Panel { Dock = DockStyle.Left, Width = 340, Padding = new Padding(8) };
        var lbl = UiTema.Etiqueta("Fecha de devolución");
        lbl.Location = new Point(8, 8);
        _dtDevolucion.Location = new Point(8, 30);
        _dtDevolucion.Width = 300;
        var lblObs = UiTema.Etiqueta("Observaciones");
        lblObs.Location = new Point(8, 70);
        _txtObservaciones.Location = new Point(8, 92);
        _txtObservaciones.Width = 300;
        var btn = UiTema.CrearBoton("Registrar devolución", UiTema.Exito, 220);
        btn.Location = new Point(8, 140);
        btn.Click += (_, _) => Devolver();
        formulario.Controls.AddRange(new Control[] { lbl, _dtDevolucion, lblObs, _txtObservaciones, btn });

        var derecha = new Panel { Dock = DockStyle.Fill, Padding = new Padding(8) };
        var info = new Label
        {
            Text = "Seleccione un préstamo activo.",
            Dock = DockStyle.Top,
            Height = 28,
            ForeColor = UiTema.Sidebar
        };
        UiTema.EstilizarGrid(_grid);
        _grid.Dock = DockStyle.Fill;
        derecha.Controls.Add(_grid);
        derecha.Controls.Add(info);
        Cuerpo.Controls.Add(derecha);
        Cuerpo.Controls.Add(formulario);
    }

    public override void Recargar()
    {
        _dtDevolucion.Value = DateTime.Today;
        _txtObservaciones.Clear();
        _grid.DataSource = _servicios.Prestamos.ListarActivos()
            .Select(p => new
            {
                p.Id,
                Usuario = p.UsuarioNombre,
                Libro = p.LibroTitulo,
                p.FechaPrestamo,
                Estimada = p.FechaDevolucionEstimada,
                p.Estado
            }).ToList();
    }

    private void Devolver()
    {
        if (_grid.CurrentRow is null)
        {
            UiMensajes.Error("Seleccione un préstamo activo.");
            return;
        }

        var id = Convert.ToInt32(_grid.CurrentRow.Cells["Id"].Value);
        if (!UiMensajes.Confirmar("¿Registrar la devolución del préstamo seleccionado?")) return;
        var resultado = _servicios.Prestamos.RegistrarDevolucion(id, _dtDevolucion.Value.Date, _txtObservaciones.Text);
        UiMensajes.Mostrar(resultado);
        if (resultado.EsExitoso) Recargar();
    }
}
