using Biblioteca.Negocio.Services;
using Biblioteca.Presentacion.Controls;

namespace Biblioteca.Presentacion.Forms;

public class FrmPrincipal : Form
{
    private readonly ServiciosAplicacion _servicios;
    private readonly Panel _contenido;
    private readonly Dictionary<string, PanelModulo> _modulos = new();
    private readonly Dictionary<string, Button> _botones = new();

    public FrmPrincipal(ServiciosAplicacion servicios)
    {
        _servicios = servicios;
        Text = "Sistema de Gestión de Biblioteca";
        WindowState = FormWindowState.Maximized;
        MinimumSize = new Size(1100, 700);
        BackColor = UiTema.Fondo;
        Font = UiTema.Normal;
        StartPosition = FormStartPosition.CenterScreen;
        DoubleBuffered = true;
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);

        var sidebar = new PanelCristal { Dock = DockStyle.Left, Width = 230, Capa = UiFondo.Capa.VidrioOscuro };
        var marca = new Label
        {
            Text = "Biblioteca\nInstitucional",
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 14, FontStyle.Bold),
            Location = new Point(16, 20),
            AutoSize = true,
            BackColor = Color.Transparent
        };
        sidebar.Controls.Add(marca);

        int y = 100;
        foreach (var item in new[] { "Inicio", "Libros", "Autores", "Editoriales", "Usuarios", "Préstamos", "Devoluciones", "Reportes" })
        {
            var boton = new Button
            {
                Text = "   " + item,
                Tag = item,
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                BackColor = UiTema.Sidebar,
                Font = UiTema.Boton,
                Size = new Size(230, 42),
                Location = new Point(0, y),
                TextAlign = ContentAlignment.MiddleLeft,
                Cursor = Cursors.Hand
            };
            boton.FlatAppearance.BorderSize = 0;
            boton.FlatAppearance.MouseOverBackColor = UiTema.SidebarHover;
            boton.FlatAppearance.MouseDownBackColor = UiTema.Acento;
            boton.Click += (_, _) => Mostrar(item);
            sidebar.Controls.Add(boton);
            _botones[item] = boton;
            y += 46;
        }

        var btnConexion = UiTema.CrearBoton("Conexión SQL", UiTema.Acento, 198, 34);
        btnConexion.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;
        sidebar.Resize += (_, _) => btnConexion.Location = new Point(16, Math.Max(520, sidebar.Height - 54));
        btnConexion.Click += (_, _) =>
        {
            using var frm = new FrmConexion(_servicios);
            frm.ShowDialog(this);
        };
        sidebar.Controls.Add(btnConexion);

        var top = new PanelCristal { Dock = DockStyle.Top, Height = 52, Capa = UiFondo.Capa.VidrioOscuro };
        var titulo = new Label
        {
            Text = "Sistema de Gestión de Biblioteca",
            Font = new Font("Segoe UI", 13, FontStyle.Bold),
            ForeColor = UiTema.TextoClaro,
            AutoSize = true,
            Location = new Point(16, 14),
            BackColor = Color.Transparent
        };
        top.Controls.Add(titulo);

        _contenido = new PanelCristal { Dock = DockStyle.Fill, Capa = UiFondo.Capa.Imagen };
        Controls.Add(_contenido);
        Controls.Add(top);
        Controls.Add(sidebar);

        _modulos["Inicio"] = new PanelDashboard(_servicios);
        _modulos["Libros"] = new PanelLibros(_servicios);
        _modulos["Autores"] = new PanelAutores(_servicios);
        _modulos["Editoriales"] = new PanelEditoriales(_servicios);
        _modulos["Usuarios"] = new PanelUsuarios(_servicios);
        _modulos["Préstamos"] = new PanelPrestamos(_servicios);
        _modulos["Devoluciones"] = new PanelDevoluciones(_servicios);
        _modulos["Reportes"] = new PanelReportes(_servicios);

        foreach (var modulo in _modulos.Values)
        {
            modulo.Visible = false;
            _contenido.Controls.Add(modulo);
        }

        Mostrar("Inicio");
    }

    protected override void OnPaintBackground(PaintEventArgs e)
    {
        UiFondo.Pintar(e.Graphics, this, UiFondo.Capa.Imagen);
    }

    private void Mostrar(string nombre)
    {
        foreach (var par in _botones)
        {
            par.Value.BackColor = par.Key == nombre ? UiTema.Acento : UiTema.Sidebar;
        }

        foreach (var par in _modulos)
        {
            par.Value.Visible = par.Key == nombre;
            if (par.Key == nombre)
            {
                par.Value.BringToFront();
                par.Value.Recargar();
            }
        }
    }
}
