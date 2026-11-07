namespace Biblioteca.Presentacion.Controls;

public abstract class PanelModulo : UserControl
{
    protected readonly Panel Encabezado;
    protected readonly Panel Cuerpo;
    protected readonly Label Titulo;
    protected readonly Label Subtitulo;

    protected PanelModulo(string titulo, string subtitulo)
    {
        Dock = DockStyle.Fill;
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
        BackColor = Color.Transparent;
        Encabezado = new PanelCristal { Dock = DockStyle.Top, Height = 78, Capa = UiFondo.Capa.VidrioClaro };
        Titulo = new Label
        {
            Text = titulo,
            Font = UiTema.Titulo,
            ForeColor = UiTema.Texto,
            AutoSize = true,
            Location = new Point(16, 12),
            BackColor = Color.Transparent
        };
        Subtitulo = new Label
        {
            Text = subtitulo,
            Font = UiTema.Subtitulo,
            ForeColor = Color.FromArgb(70, 86, 104),
            AutoSize = true,
            Location = new Point(18, 46),
            BackColor = Color.Transparent
        };
        Encabezado.Controls.Add(Titulo);
        Encabezado.Controls.Add(Subtitulo);
        Cuerpo = new PanelCristal { Dock = DockStyle.Fill, Padding = new Padding(16), Capa = UiFondo.Capa.Imagen };
        Controls.Add(Cuerpo);
        Controls.Add(Encabezado);
    }

    protected override void OnPaintBackground(PaintEventArgs e)
    {
        UiFondo.Pintar(e.Graphics, this, UiFondo.Capa.Imagen);
    }

    public abstract void Recargar();
}
