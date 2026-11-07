using Biblioteca.Entidades.Models;
using Biblioteca.Negocio.Services;

namespace Biblioteca.Presentacion.Controls;

public class PanelPrestamos : PanelModulo
{
    private readonly ServiciosAplicacion _servicios;
    private readonly DataGridView _grid = new();
    private readonly TextBox _txtBuscar = UiTema.Caja();
    private readonly ComboBox _cboUsuario = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ComboBox _cboLibro = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly DateTimePicker _dtPrestamo = new() { Format = DateTimePickerFormat.Short };
    private readonly DateTimePicker _dtEstimada = new() { Format = DateTimePickerFormat.Short };
    private readonly TextBox _txtObservaciones = UiTema.Caja();
    private int _idSeleccionado;

    public PanelPrestamos(ServiciosAplicacion servicios)
        : base("Préstamos", "Registro, consulta, edición y eliminación de préstamos")
    {
        _servicios = servicios;
        var formulario = new PanelCristal { Dock = DockStyle.Left, Width = 360, Padding = new Padding(8), Capa = UiFondo.Capa.VidrioClaro };
        int y = 8;
        void Campo(string etiqueta, Control control)
        {
            var lbl = UiTema.Etiqueta(etiqueta);
            lbl.Location = new Point(8, y);
            control.Location = new Point(8, y + 22);
            control.Width = 320;
            formulario.Controls.Add(lbl);
            formulario.Controls.Add(control);
            y += 58;
        }
        Campo("Usuario", _cboUsuario);
        Campo("Libro", _cboLibro);
        Campo("Fecha de préstamo", _dtPrestamo);
        Campo("Devolución estimada", _dtEstimada);
        Campo("Observaciones", _txtObservaciones);
        _dtEstimada.Value = DateTime.Today.AddDays(15);

        var acciones = new FlowCristal { Location = new Point(8, y + 8), Size = new Size(330, 90), Capa = UiFondo.Capa.VidrioClaro };
        var btnNuevo = UiTema.CrearBoton("Nuevo", UiTema.Sidebar, 95);
        var btnGuardar = UiTema.CrearBoton("Guardar", UiTema.Exito, 95);
        var btnEliminar = UiTema.CrearBoton("Eliminar", UiTema.Peligro, 95);
        btnNuevo.Click += (_, _) => Limpiar();
        btnGuardar.Click += (_, _) => Guardar();
        btnEliminar.Click += (_, _) => Eliminar();
        acciones.Controls.AddRange(new Control[] { btnNuevo, btnGuardar, btnEliminar });
        formulario.Controls.Add(acciones);

        var derecha = new PanelCristal { Dock = DockStyle.Fill, Padding = new Padding(8), Capa = UiFondo.Capa.VidrioClaro };
        _txtBuscar.Dock = DockStyle.Top;
        _txtBuscar.PlaceholderText = "Buscar préstamos...";
        _txtBuscar.TextChanged += (_, _) => Cargar();
        UiTema.EstilizarGrid(_grid);
        _grid.Dock = DockStyle.Fill;
        _grid.CellClick += (_, _) => Seleccionar();
        derecha.Controls.Add(_grid);
        derecha.Controls.Add(_txtBuscar);
        Cuerpo.Controls.Add(derecha);
        Cuerpo.Controls.Add(formulario);
    }

    public override void Recargar()
    {
        CargarListas();
        Limpiar();
        Cargar();
    }

    private void CargarListas()
    {
        _cboUsuario.DisplayMember = "NombreCompleto";
        _cboUsuario.ValueMember = "Id";
        _cboUsuario.DataSource = _servicios.Usuarios.Listar().Where(u => u.Activo).ToList();

        _cboLibro.DisplayMember = "TituloInventario";
        _cboLibro.ValueMember = "Id";
        _cboLibro.DataSource = _servicios.Libros.Listar().ToList();
    }

    private void Cargar()
    {
        _grid.DataSource = _servicios.Prestamos.Buscar(_txtBuscar.Text)
            .Select(p => new
            {
                p.Id,
                Usuario = p.UsuarioNombre,
                Documento = p.UsuarioDocumento,
                Libro = p.LibroTitulo,
                p.FechaPrestamo,
                Estimada = p.FechaDevolucionEstimada,
                Real = p.FechaDevolucionReal,
                p.Estado
            }).ToList();
    }

    private void Seleccionar()
    {
        if (_grid.CurrentRow is null) return;
        _idSeleccionado = Convert.ToInt32(_grid.CurrentRow.Cells["Id"].Value);
        var p = _servicios.Prestamos.Obtener(_idSeleccionado);
        if (p is null) return;
        _cboUsuario.SelectedValue = p.IdUsuario;
        _cboLibro.SelectedValue = p.IdLibro;
        _dtPrestamo.Value = p.FechaPrestamo;
        _dtEstimada.Value = p.FechaDevolucionEstimada;
        _txtObservaciones.Text = p.Observaciones ?? string.Empty;
    }

    private void Limpiar()
    {
        _idSeleccionado = 0;
        _dtPrestamo.Value = DateTime.Today;
        _dtEstimada.Value = DateTime.Today.AddDays(15);
        _txtObservaciones.Clear();
        if (_cboUsuario.Items.Count > 0) _cboUsuario.SelectedIndex = 0;
        if (_cboLibro.Items.Count > 0) _cboLibro.SelectedIndex = 0;
    }

    private void Guardar()
    {
        if (_cboUsuario.SelectedValue is null || _cboLibro.SelectedValue is null)
        {
            UiMensajes.Error("Seleccione usuario y libro.");
            return;
        }

        var prestamo = new Prestamo
        {
            Id = _idSeleccionado,
            IdUsuario = Convert.ToInt32(_cboUsuario.SelectedValue),
            IdLibro = Convert.ToInt32(_cboLibro.SelectedValue),
            FechaPrestamo = _dtPrestamo.Value.Date,
            FechaDevolucionEstimada = _dtEstimada.Value.Date,
            Observaciones = _txtObservaciones.Text
        };

        var resultado = _idSeleccionado == 0
            ? _servicios.Prestamos.Registrar(prestamo)
            : _servicios.Prestamos.ActualizarActivo(prestamo);
        UiMensajes.Mostrar(resultado);
        if (resultado.EsExitoso) Recargar();
    }

    private void Eliminar()
    {
        if (_idSeleccionado == 0) { UiMensajes.Error("Seleccione un préstamo."); return; }
        if (!UiMensajes.Confirmar("¿Eliminar el préstamo? Si está activo se devolverá el ejemplar al inventario.")) return;
        var resultado = _servicios.Prestamos.Eliminar(_idSeleccionado);
        UiMensajes.Mostrar(resultado);
        if (resultado.EsExitoso) Recargar();
    }
}
