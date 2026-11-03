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
        BackColor = UiTema.Fondo;
        Encabezado = new Panel { Dock = DockStyle.Top, Height = 78, BackColor = UiTema.Fondo };
        Titulo = new Label
        {
            Text = titulo,
            Font = UiTema.Titulo,
            ForeColor = UiTema.Sidebar,
            AutoSize = true,
            Location = new Point(16, 12)
        };
        Subtitulo = new Label
        {
            Text = subtitulo,
            Font = UiTema.Subtitulo,
            ForeColor = UiTema.Texto,
            AutoSize = true,
            Location = new Point(18, 46)
        };
        Encabezado.Controls.Add(Titulo);
        Encabezado.Controls.Add(Subtitulo);
        Cuerpo = new Panel { Dock = DockStyle.Fill, Padding = new Padding(16), BackColor = UiTema.Fondo };
        Controls.Add(Cuerpo);
        Controls.Add(Encabezado);
    }

    public abstract void Recargar();
}
