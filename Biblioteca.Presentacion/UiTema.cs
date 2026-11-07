namespace Biblioteca.Presentacion;

public static class UiTema
{
    public static readonly Color Fondo = Color.FromArgb(10, 24, 44);
    public static readonly Color Sidebar = Color.FromArgb(14, 32, 54);
    public static readonly Color SidebarHover = Color.FromArgb(32, 64, 96);
    public static readonly Color Acento = Color.FromArgb(61, 126, 174);
    public static readonly Color AcentoOscuro = Color.FromArgb(36, 86, 122);
    public static readonly Color TextoClaro = Color.White;
    public static readonly Color Texto = Color.FromArgb(24, 34, 48);
    public static readonly Color TextoSuave = Color.FromArgb(206, 220, 232);
    public static readonly Color Tarjeta = Color.FromArgb(236, 242, 247);
    public static readonly Color VidrioClaro = Color.FromArgb(188, 244, 247, 251);
    public static readonly Color VidrioOscuro = Color.FromArgb(168, 8, 18, 36);
    public static readonly Color Peligro = Color.FromArgb(166, 74, 64);
    public static readonly Color Exito = Color.FromArgb(36, 122, 96);
    public static readonly Color Advertencia = Color.FromArgb(184, 132, 58);
    public static readonly Color Borde = Color.FromArgb(176, 198, 214);

    public static Font Titulo => new("Segoe UI", 18, FontStyle.Bold);
    public static Font Subtitulo => new("Segoe UI", 12, FontStyle.Regular);
    public static Font Normal => new("Segoe UI", 10F, FontStyle.Regular);
    public static Font Boton => new("Segoe UI", 10F, FontStyle.Bold);

    public static Button CrearBoton(string texto, Color fondo, int ancho = 130, int alto = 36)
    {
        return new Button
        {
            Text = texto,
            BackColor = fondo,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = Boton,
            Size = new Size(ancho, alto),
            Cursor = Cursors.Hand,
            FlatAppearance = { BorderSize = 0 }
        };
    }

    public static void EstilizarGrid(DataGridView grid)
    {
        grid.BackgroundColor = Tarjeta;
        grid.BorderStyle = BorderStyle.None;
        grid.EnableHeadersVisualStyles = false;
        grid.ColumnHeadersDefaultCellStyle.BackColor = Sidebar;
        grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        grid.ColumnHeadersHeight = 36;
        grid.DefaultCellStyle.Font = Normal;
        grid.DefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
        grid.DefaultCellStyle.ForeColor = Texto;
        grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(226, 236, 244);
        grid.AlternatingRowsDefaultCellStyle.ForeColor = Texto;
        grid.GridColor = Borde;
        grid.DefaultCellStyle.SelectionBackColor = Acento;
        grid.DefaultCellStyle.SelectionForeColor = Color.White;
        grid.RowTemplate.Height = 32;
        grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        grid.AllowUserToAddRows = false;
        grid.AllowUserToDeleteRows = false;
        grid.ReadOnly = true;
        grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        grid.MultiSelect = false;
        grid.RowHeadersVisible = false;
    }

    public static Label Etiqueta(string texto) => new()
    {
        Text = texto,
        AutoSize = true,
        Font = Normal,
        ForeColor = Texto
    };

    public static TextBox Caja() => new()
    {
        Font = Normal,
        BorderStyle = BorderStyle.FixedSingle,
        BackColor = Color.White,
        ForeColor = Texto,
        Height = 28
    };
}
