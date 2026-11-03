namespace Biblioteca.Presentacion;

public static class UiTema
{
    public static readonly Color Fondo = Color.FromArgb(244, 246, 247);
    public static readonly Color Sidebar = Color.FromArgb(21, 67, 96);
    public static readonly Color SidebarHover = Color.FromArgb(27, 79, 114);
    public static readonly Color Acento = Color.FromArgb(20, 143, 119);
    public static readonly Color AcentoOscuro = Color.FromArgb(17, 122, 101);
    public static readonly Color TextoClaro = Color.White;
    public static readonly Color Texto = Color.FromArgb(33, 47, 61);
    public static readonly Color Tarjeta = Color.White;
    public static readonly Color Peligro = Color.FromArgb(192, 57, 43);
    public static readonly Color Exito = Color.FromArgb(30, 132, 73);
    public static readonly Color Advertencia = Color.FromArgb(211, 84, 0);
    public static readonly Color Borde = Color.FromArgb(214, 219, 223);

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
        grid.BackgroundColor = Color.White;
        grid.BorderStyle = BorderStyle.None;
        grid.EnableHeadersVisualStyles = false;
        grid.ColumnHeadersDefaultCellStyle.BackColor = Sidebar;
        grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        grid.ColumnHeadersHeight = 36;
        grid.DefaultCellStyle.Font = Normal;
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
        Height = 28
    };
}
