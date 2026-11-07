using Biblioteca.Entidades.Models;
using Biblioteca.Negocio.Services;

namespace Biblioteca.Presentacion.Controls;

public class PanelEditoriales : PanelModulo
{
    private readonly ServiciosAplicacion _servicios;
    private readonly DataGridView _grid = new();
    private readonly TextBox _txtBuscar = UiTema.Caja();
    private readonly TextBox _txtNombre = UiTema.Caja();
    private readonly TextBox _txtPais = UiTema.Caja();
    private readonly TextBox _txtTelefono = UiTema.Caja();
    private readonly TextBox _txtCorreo = UiTema.Caja();
    private int _idSeleccionado;

    public PanelEditoriales(ServiciosAplicacion servicios)
        : base("Editoriales", "Administración de editoriales y relaciones con libros")
    {
        _servicios = servicios;
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
        Campo("País", _txtPais);
        Campo("Teléfono", _txtTelefono);
        Campo("Correo electrónico", _txtCorreo);

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
        _txtBuscar.PlaceholderText = "Buscar editorial...";
        _txtBuscar.TextChanged += (_, _) => Cargar();
        UiTema.EstilizarGrid(_grid);
        _grid.Dock = DockStyle.Fill;
        _grid.CellClick += (_, _) => Seleccionar();
        derecha.Controls.Add(_grid);
        derecha.Controls.Add(_txtBuscar);
        Cuerpo.Controls.Add(derecha);
        Cuerpo.Controls.Add(formulario);
    }

    public override void Recargar() { Limpiar(); Cargar(); }

    private void Cargar()
    {
        _grid.DataSource = _servicios.Editoriales.Buscar(_txtBuscar.Text)
            .Select(e => new { e.Id, e.Nombre, e.Pais, e.Telefono, e.CorreoElectronico }).ToList();
    }

    private void Seleccionar()
    {
        if (_grid.CurrentRow is null) return;
        _idSeleccionado = Convert.ToInt32(_grid.CurrentRow.Cells["Id"].Value);
        var item = _servicios.Editoriales.Obtener(_idSeleccionado);
        if (item is null) return;
        _txtNombre.Text = item.Nombre;
        _txtPais.Text = item.Pais ?? string.Empty;
        _txtTelefono.Text = item.Telefono ?? string.Empty;
        _txtCorreo.Text = item.CorreoElectronico ?? string.Empty;
    }

    private void Limpiar()
    {
        _idSeleccionado = 0;
        _txtNombre.Clear();
        _txtPais.Clear();
        _txtTelefono.Clear();
        _txtCorreo.Clear();
    }

    private void Guardar()
    {
        var editorial = new Editorial
        {
            Id = _idSeleccionado,
            Nombre = _txtNombre.Text,
            Pais = _txtPais.Text,
            Telefono = _txtTelefono.Text,
            CorreoElectronico = _txtCorreo.Text
        };
        var resultado = _servicios.Editoriales.Guardar(editorial);
        UiMensajes.Mostrar(resultado);
        if (resultado.EsExitoso) Recargar();
    }

    private void Eliminar()
    {
        if (_idSeleccionado == 0) { UiMensajes.Error("Seleccione una editorial."); return; }
        if (!UiMensajes.Confirmar("¿Eliminar la editorial seleccionada?")) return;
        var resultado = _servicios.Editoriales.Eliminar(_idSeleccionado);
        UiMensajes.Mostrar(resultado);
        if (resultado.EsExitoso) Recargar();
    }
}
