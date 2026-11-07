using Biblioteca.Negocio.Services;

namespace Biblioteca.Presentacion.Controls;

public class PanelReportes : PanelModulo
{
    private readonly ServiciosAplicacion _servicios;
    private readonly ComboBox _cboReporte = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 280 };
    private readonly DataGridView _grid = new();

    public PanelReportes(ServiciosAplicacion servicios)
        : base("Reportes", "Consultas consolidadas de la biblioteca")
    {
        _servicios = servicios;
        var barra = new PanelCristal { Dock = DockStyle.Top, Height = 48, Capa = UiFondo.Capa.VidrioClaro };
        var lbl = UiTema.Etiqueta("Seleccione un reporte");
        lbl.Location = new Point(8, 14);
        _cboReporte.Location = new Point(180, 10);
        _cboReporte.Items.AddRange(new object[]
        {
            "Listado de libros",
            "Listado de autores",
            "Listado de editoriales",
            "Listado de usuarios",
            "Préstamos activos",
            "Préstamos devueltos",
            "Historial de préstamos"
        });
        _cboReporte.SelectedIndex = 0;
        _cboReporte.SelectedIndexChanged += (_, _) => Cargar();
        var btn = UiTema.CrearBoton("Actualizar", UiTema.Acento, 120);
        btn.Location = new Point(480, 8);
        btn.Click += (_, _) => Cargar();
        barra.Controls.AddRange(new Control[] { lbl, _cboReporte, btn });
        UiTema.EstilizarGrid(_grid);
        _grid.Dock = DockStyle.Fill;
        Cuerpo.Controls.Add(_grid);
        Cuerpo.Controls.Add(barra);
    }

    public override void Recargar() => Cargar();

    private void Cargar()
    {
        object datos = _cboReporte.SelectedItem?.ToString() switch
        {
            "Listado de libros" => _servicios.Libros.Listar().Select(l => new
            {
                l.Id, l.Isbn, l.Titulo, Editorial = l.EditorialNombre, Autores = l.AutoresNombres, l.Existencias, l.Disponibles
            }).ToList(),
            "Listado de autores" => _servicios.Autores.Listar().Select(a => new
            {
                a.Id, a.Nombre, a.Apellido, a.Nacionalidad, a.FechaNacimiento
            }).ToList(),
            "Listado de editoriales" => _servicios.Editoriales.Listar().Select(e => new
            {
                e.Id, e.Nombre, e.Pais, e.Telefono, e.CorreoElectronico
            }).ToList(),
            "Listado de usuarios" => _servicios.Usuarios.Listar().Select(u => new
            {
                u.Id, u.Nombre, u.Apellido, u.Documento, u.Telefono, u.CorreoElectronico, u.Activo
            }).ToList(),
            "Préstamos activos" => _servicios.Prestamos.ListarActivos().Select(p => new
            {
                p.Id, Usuario = p.UsuarioNombre, Libro = p.LibroTitulo, p.FechaPrestamo, p.FechaDevolucionEstimada, p.Estado
            }).ToList(),
            "Préstamos devueltos" => _servicios.Prestamos.ListarDevueltos().Select(p => new
            {
                p.Id, Usuario = p.UsuarioNombre, Libro = p.LibroTitulo, p.FechaPrestamo, p.FechaDevolucionReal, p.Estado
            }).ToList(),
            _ => _servicios.Prestamos.Listar().Select(p => new
            {
                p.Id, Usuario = p.UsuarioNombre, Documento = p.UsuarioDocumento, Libro = p.LibroTitulo,
                p.FechaPrestamo, Estimada = p.FechaDevolucionEstimada, Real = p.FechaDevolucionReal, p.Estado
            }).ToList()
        };
        _grid.DataSource = datos;
    }
}
