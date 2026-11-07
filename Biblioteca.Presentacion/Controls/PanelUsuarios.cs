using Biblioteca.Entidades.Models;
using Biblioteca.Negocio.Services;

namespace Biblioteca.Presentacion.Controls;

public class PanelUsuarios : PanelModulo
{
    private readonly ServiciosAplicacion _servicios;
    private readonly DataGridView _grid = new();
    private readonly TextBox _txtBuscar = UiTema.Caja();
    private readonly TextBox _txtNombre = UiTema.Caja();
    private readonly TextBox _txtApellido = UiTema.Caja();
    private readonly TextBox _txtDocumento = UiTema.Caja();
    private readonly TextBox _txtTelefono = UiTema.Caja();
    private readonly TextBox _txtCorreo = UiTema.Caja();
    private readonly TextBox _txtDireccion = UiTema.Caja();
    private readonly CheckBox _chkActivo = new() { Text = "Usuario activo", Checked = true, AutoSize = true };
    private int _idSeleccionado;

    public PanelUsuarios(ServiciosAplicacion servicios)
        : base("Usuarios", "Gestión de lectores de la biblioteca")
    {
        _servicios = servicios;
        var formulario = new PanelCristal { Dock = DockStyle.Left, Width = 340, Padding = new Padding(8), AutoScroll = true, Capa = UiFondo.Capa.VidrioClaro };
        int y = 8;
        void Campo(string etiqueta, Control control)
        {
            var lbl = UiTema.Etiqueta(etiqueta);
            lbl.Location = new Point(8, y);
            control.Location = new Point(8, y + 22);
            control.Width = 300;
            formulario.Controls.Add(lbl);
            formulario.Controls.Add(control);
            y += 56;
        }
        Campo("Nombre", _txtNombre);
        Campo("Apellido", _txtApellido);
        Campo("Documento", _txtDocumento);
        Campo("Teléfono", _txtTelefono);
        Campo("Correo electrónico", _txtCorreo);
        Campo("Dirección", _txtDireccion);
        _chkActivo.Location = new Point(8, y);
        formulario.Controls.Add(_chkActivo);

        var acciones = new FlowCristal { Location = new Point(8, y + 32), Size = new Size(320, 90), Capa = UiFondo.Capa.VidrioClaro };
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
        _txtBuscar.PlaceholderText = "Buscar por nombre, documento o correo...";
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
        _grid.DataSource = _servicios.Usuarios.Buscar(_txtBuscar.Text)
            .Select(u => new { u.Id, u.Nombre, u.Apellido, u.Documento, u.Telefono, u.CorreoElectronico, u.Activo }).ToList();
    }

    private void Seleccionar()
    {
        if (_grid.CurrentRow is null) return;
        _idSeleccionado = Convert.ToInt32(_grid.CurrentRow.Cells["Id"].Value);
        var u = _servicios.Usuarios.Obtener(_idSeleccionado);
        if (u is null) return;
        _txtNombre.Text = u.Nombre;
        _txtApellido.Text = u.Apellido;
        _txtDocumento.Text = u.Documento;
        _txtTelefono.Text = u.Telefono ?? string.Empty;
        _txtCorreo.Text = u.CorreoElectronico;
        _txtDireccion.Text = u.Direccion ?? string.Empty;
        _chkActivo.Checked = u.Activo;
    }

    private void Limpiar()
    {
        _idSeleccionado = 0;
        _txtNombre.Clear();
        _txtApellido.Clear();
        _txtDocumento.Clear();
        _txtTelefono.Clear();
        _txtCorreo.Clear();
        _txtDireccion.Clear();
        _chkActivo.Checked = true;
    }

    private void Guardar()
    {
        var usuario = new Usuario
        {
            Id = _idSeleccionado,
            Nombre = _txtNombre.Text,
            Apellido = _txtApellido.Text,
            Documento = _txtDocumento.Text,
            Telefono = _txtTelefono.Text,
            CorreoElectronico = _txtCorreo.Text,
            Direccion = _txtDireccion.Text,
            Activo = _chkActivo.Checked
        };
        var resultado = _servicios.Usuarios.Guardar(usuario);
        UiMensajes.Mostrar(resultado);
        if (resultado.EsExitoso) Recargar();
    }

    private void Eliminar()
    {
        if (_idSeleccionado == 0) { UiMensajes.Error("Seleccione un usuario."); return; }
        if (!UiMensajes.Confirmar("¿Eliminar el usuario seleccionado?")) return;
        var resultado = _servicios.Usuarios.Eliminar(_idSeleccionado);
        UiMensajes.Mostrar(resultado);
        if (resultado.EsExitoso) Recargar();
    }
}
