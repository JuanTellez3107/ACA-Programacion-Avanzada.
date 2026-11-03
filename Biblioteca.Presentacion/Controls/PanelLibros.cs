using Biblioteca.Entidades.Models;
using Biblioteca.Negocio.Services;

namespace Biblioteca.Presentacion.Controls;

public class PanelLibros : PanelModulo
{
    private readonly ServiciosAplicacion _servicios;
    private readonly DataGridView _grid = new();
    private readonly TextBox _txtBuscar = UiTema.Caja();
    private readonly TextBox _txtIsbn = UiTema.Caja();
    private readonly TextBox _txtTitulo = UiTema.Caja();
    private readonly ComboBox _cboEditorial = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly NumericUpDown _numAnio = new() { Minimum = 1450, Maximum = DateTime.Today.Year + 1, Value = DateTime.Today.Year };
    private readonly TextBox _txtGenero = UiTema.Caja();
    private readonly NumericUpDown _numPaginas = new() { Minimum = 1, Maximum = 20000, Value = 100 };
    private readonly NumericUpDown _numExistencias = new() { Minimum = 0, Maximum = 10000, Value = 1 };
    private readonly CheckedListBox _lstAutores = new() { CheckOnClick = true, Height = 120 };
    private int _idSeleccionado;

    public PanelLibros(ServiciosAplicacion servicios)
        : base("Libros", "CRUD bibliográfico, autores, editoriales y existencias")
    {
        _servicios = servicios;
        var formulario = new Panel { Dock = DockStyle.Left, Width = 360, Padding = new Padding(8), AutoScroll = true };
        int y = 8;
        void Campo(string etiqueta, Control control, int alto = 28)
        {
            var lbl = UiTema.Etiqueta(etiqueta);
            lbl.Location = new Point(8, y);
            control.Location = new Point(8, y + 20);
            control.Width = 320;
            control.Height = alto;
            formulario.Controls.Add(lbl);
            formulario.Controls.Add(control);
            y += alto + 32;
        }
        Campo("ISBN", _txtIsbn);
        Campo("Título", _txtTitulo);
        Campo("Editorial", _cboEditorial);
        Campo("Año de publicación", _numAnio);
        Campo("Género", _txtGenero);
        Campo("Número de páginas", _numPaginas);
        Campo("Existencias", _numExistencias);
        Campo("Autores", _lstAutores, 120);

        var acciones = new FlowLayoutPanel { Location = new Point(8, y + 8), Size = new Size(330, 90) };
        var btnNuevo = UiTema.CrearBoton("Nuevo", UiTema.Sidebar, 95);
        var btnGuardar = UiTema.CrearBoton("Guardar", UiTema.Exito, 95);
        var btnEliminar = UiTema.CrearBoton("Eliminar", UiTema.Peligro, 95);
        btnNuevo.Click += (_, _) => Limpiar();
        btnGuardar.Click += (_, _) => Guardar();
        btnEliminar.Click += (_, _) => Eliminar();
        acciones.Controls.AddRange(new Control[] { btnNuevo, btnGuardar, btnEliminar });
        formulario.Controls.Add(acciones);

        var derecha = new Panel { Dock = DockStyle.Fill, Padding = new Padding(8) };
        _txtBuscar.Dock = DockStyle.Top;
        _txtBuscar.PlaceholderText = "Buscar por ISBN, título o autor...";
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
        _cboEditorial.DisplayMember = "Nombre";
        _cboEditorial.ValueMember = "Id";
        _cboEditorial.DataSource = _servicios.Editoriales.Listar().ToList();
        _lstAutores.DisplayMember = "NombreCompleto";
        _lstAutores.Items.Clear();
        foreach (var autor in _servicios.Autores.Listar())
        {
            _lstAutores.Items.Add(autor, false);
        }
    }

    private void Cargar()
    {
        _grid.DataSource = _servicios.Libros.Buscar(_txtBuscar.Text)
            .Select(l => new
            {
                l.Id,
                ISBN = l.Isbn,
                l.Titulo,
                Editorial = l.EditorialNombre,
                Autores = l.AutoresNombres,
                l.AnioPublicacion,
                l.Existencias,
                l.Disponibles
            }).ToList();
    }

    private void Seleccionar()
    {
        if (_grid.CurrentRow is null) return;
        _idSeleccionado = Convert.ToInt32(_grid.CurrentRow.Cells["Id"].Value);
        var libro = _servicios.Libros.Obtener(_idSeleccionado);
        if (libro is null) return;
        _txtIsbn.Text = libro.Isbn;
        _txtTitulo.Text = libro.Titulo;
        _cboEditorial.SelectedValue = libro.IdEditorial;
        _numAnio.Value = libro.AnioPublicacion;
        _txtGenero.Text = libro.Genero ?? string.Empty;
        _numPaginas.Value = libro.NumeroPaginas;
        _numExistencias.Value = libro.Existencias;
        for (var i = 0; i < _lstAutores.Items.Count; i++)
        {
            var autor = (Autor)_lstAutores.Items[i];
            _lstAutores.SetItemChecked(i, libro.IdsAutores.Contains(autor.Id));
        }
    }

    private void Limpiar()
    {
        _idSeleccionado = 0;
        _txtIsbn.Clear();
        _txtTitulo.Clear();
        _txtGenero.Clear();
        _numAnio.Value = DateTime.Today.Year;
        _numPaginas.Value = 100;
        _numExistencias.Value = 1;
        for (var i = 0; i < _lstAutores.Items.Count; i++) _lstAutores.SetItemChecked(i, false);
        if (_cboEditorial.Items.Count > 0) _cboEditorial.SelectedIndex = 0;
    }

    private void Guardar()
    {
        if (_cboEditorial.SelectedValue is null)
        {
            UiMensajes.Error("Debe seleccionar una editorial.");
            return;
        }

        var autores = _lstAutores.CheckedItems.Cast<Autor>().Select(a => a.Id).ToList();
        var libro = new Libro
        {
            Id = _idSeleccionado,
            Isbn = _txtIsbn.Text,
            Titulo = _txtTitulo.Text,
            IdEditorial = Convert.ToInt32(_cboEditorial.SelectedValue),
            AnioPublicacion = (int)_numAnio.Value,
            Genero = _txtGenero.Text,
            NumeroPaginas = (int)_numPaginas.Value,
            Existencias = (int)_numExistencias.Value,
            IdsAutores = autores
        };
        var resultado = _servicios.Libros.Guardar(libro);
        UiMensajes.Mostrar(resultado);
        if (resultado.EsExitoso) Recargar();
    }

    private void Eliminar()
    {
        if (_idSeleccionado == 0) { UiMensajes.Error("Seleccione un libro."); return; }
        if (!UiMensajes.Confirmar("¿Eliminar el libro seleccionado?")) return;
        var resultado = _servicios.Libros.Eliminar(_idSeleccionado);
        UiMensajes.Mostrar(resultado);
        if (resultado.EsExitoso) Recargar();
    }
}
