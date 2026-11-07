using Biblioteca.Entidades.Models;
using Biblioteca.Negocio.Services;

namespace Biblioteca.Presentacion.Controls;

public class PanelAutores : PanelModulo
{
    private readonly ServiciosAplicacion _servicios;
    private readonly DataGridView _grid = new();
    private readonly TextBox _txtBuscar = UiTema.Caja();
    private readonly TextBox _txtNombre = UiTema.Caja();
    private readonly TextBox _txtApellido = UiTema.Caja();
    private readonly TextBox _txtNacionalidad = UiTema.Caja();
    private readonly DateTimePicker _dtNacimiento = new() { Format = DateTimePickerFormat.Short, ShowCheckBox = true, Checked = false };
    private int _idSeleccionado;

    public PanelAutores(ServiciosAplicacion servicios)
        : base("Autores", "Registrar, consultar, actualizar y eliminar autores")
    {
        _servicios = servicios;
        Construir();
    }

    private void Construir()
    {
        var formulario = new PanelCristal { Dock = DockStyle.Left, Width = 340, Padding = new Padding(8), Capa = UiFondo.Capa.VidrioClaro };
        int y = 8;
        void Campo(string etiqueta, Control control)
        {
            var lbl = UiTema.Etiqueta(etiqueta);
            lbl.Location = new Point(8, y);
            control.Location = new Point(8, y + 22);
            control.Width = 300;
            formulario.Controls.Add(lbl);
            formulario.Controls.Add(control);
            y += 58;
        }

        Campo("Nombre", _txtNombre);
        Campo("Apellido", _txtApellido);
        Campo("Nacionalidad", _txtNacionalidad);
        Campo("Fecha de nacimiento", _dtNacimiento);

        var acciones = new FlowCristal { Location = new Point(8, y + 8), Size = new Size(320, 90), Capa = UiFondo.Capa.VidrioClaro };
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
        _txtBuscar.PlaceholderText = "Buscar autor...";
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
        Limpiar();
        Cargar();
    }

    private void Cargar()
    {
        _grid.DataSource = _servicios.Autores.Buscar(_txtBuscar.Text)
            .Select(a => new { a.Id, a.Nombre, a.Apellido, a.Nacionalidad, a.FechaNacimiento }).ToList();
    }

    private void Seleccionar()
    {
        if (_grid.CurrentRow?.DataBoundItem is null) return;
        _idSeleccionado = Convert.ToInt32(_grid.CurrentRow.Cells["Id"].Value);
        var autor = _servicios.Autores.Obtener(_idSeleccionado);
        if (autor is null) return;
        _txtNombre.Text = autor.Nombre;
        _txtApellido.Text = autor.Apellido;
        _txtNacionalidad.Text = autor.Nacionalidad ?? string.Empty;
        _dtNacimiento.Checked = autor.FechaNacimiento.HasValue;
        if (autor.FechaNacimiento.HasValue) _dtNacimiento.Value = autor.FechaNacimiento.Value;
    }

    private void Limpiar()
    {
        _idSeleccionado = 0;
        _txtNombre.Clear();
        _txtApellido.Clear();
        _txtNacionalidad.Clear();
        _dtNacimiento.Checked = false;
        _grid.ClearSelection();
    }

    private void Guardar()
    {
        var autor = new Autor
        {
            Id = _idSeleccionado,
            Nombre = _txtNombre.Text,
            Apellido = _txtApellido.Text,
            Nacionalidad = _txtNacionalidad.Text,
            FechaNacimiento = _dtNacimiento.Checked ? _dtNacimiento.Value.Date : null
        };
        var resultado = _servicios.Autores.Guardar(autor);
        UiMensajes.Mostrar(resultado);
        if (resultado.EsExitoso) Recargar();
    }

    private void Eliminar()
    {
        if (_idSeleccionado == 0) { UiMensajes.Error("Seleccione un autor."); return; }
        if (!UiMensajes.Confirmar("¿Eliminar el autor seleccionado?")) return;
        var resultado = _servicios.Autores.Eliminar(_idSeleccionado);
        UiMensajes.Mostrar(resultado);
        if (resultado.EsExitoso) Recargar();
    }
}
