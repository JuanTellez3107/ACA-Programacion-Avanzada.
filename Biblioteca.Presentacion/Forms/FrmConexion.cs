using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Reflection;
using System.Runtime.InteropServices;
using Biblioteca.Negocio.Services;

namespace Biblioteca.Presentacion.Forms;

public class FrmConexion : Form
{
    private static readonly Color ColorTarjeta = Color.FromArgb(186, 244, 247, 251);
    private static readonly Color ColorTitulo = Color.FromArgb(32, 33, 36);
    private static readonly Color ColorSubtitulo = Color.FromArgb(71, 78, 90);
    private static readonly Color ColorBosque = Color.FromArgb(12, 28, 48);

    private readonly ServiciosAplicacion _servicios;
    private readonly BotonAcceso _btnCrear;
    private readonly BotonAcceso _btnEntrar;
    private Bitmap? _fondo;
    private string _estado = string.Empty;
    private Color _colorEstado = ColorSubtitulo;

    public FrmConexion(ServiciosAplicacion servicios)
    {
        _servicios = servicios;
        Text = "Sistema de Gestión de Biblioteca";
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        BackColor = ColorBosque;
        Font = UiTema.Normal;
        DoubleBuffered = true;
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);

        _btnCrear = new BotonAcceso(
            "Crear base de datos",
            Color.FromArgb(46, 74, 104),
            Color.FromArgb(58, 92, 128));
        _btnCrear.Click += (_, _) => CrearBase();

        _btnEntrar = new BotonAcceso(
            "Ingresar al sistema",
            Color.FromArgb(20, 28, 40),
            Color.FromArgb(36, 48, 66));
        _btnEntrar.Click += (_, _) => Ingresar();

        Controls.Add(_btnCrear);
        Controls.Add(_btnEntrar);
        AcceptButton = _btnEntrar;
        ClientSize = new Size(1120, 680);
        Posicionar();
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        _fondo?.Dispose();
        _fondo = null;
        Posicionar();
    }

    protected override void OnPaintBackground(PaintEventArgs e)
    {
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        if (ClientSize.Width < 2 || ClientSize.Height < 2)
        {
            return;
        }

        AsegurarFondo();
        e.Graphics.DrawImageUnscaled(_fondo!, Point.Empty);
        DibujarTarjeta(e.Graphics);
        DibujarTextos(e.Graphics);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _fondo?.Dispose();
        }

        base.Dispose(disposing);
    }

    private void Posicionar()
    {
        if (_btnCrear is null || _btnEntrar is null)
        {
            return;
        }

        var tarjeta = RectTarjeta();
        const int anchoBoton = 340;
        const int altoBoton = 50;
        var x = tarjeta.X + (tarjeta.Width - anchoBoton) / 2;
        _btnCrear.SetBounds(x, tarjeta.Y + 156, anchoBoton, altoBoton);
        _btnEntrar.SetBounds(x, tarjeta.Y + 222, anchoBoton, altoBoton);
    }

    private Rectangle RectTarjeta()
    {
        const int ancho = 440;
        const int alto = 400;
        var x = Math.Max(16, (ClientSize.Width - ancho) / 2);
        var y = Math.Max(16, (ClientSize.Height - alto) / 2 + 12);
        return new Rectangle(x, y, ancho, alto);
    }

    private void AsegurarFondo()
    {
        if (_fondo is not null && _fondo.Width == ClientSize.Width && _fondo.Height == ClientSize.Height)
        {
            return;
        }

        _fondo?.Dispose();
        _fondo = CrearFondo(ClientSize);
    }

    private void DibujarTarjeta(Graphics g)
    {
        var tarjeta = RectTarjeta();
        g.SmoothingMode = SmoothingMode.AntiAlias;

        var sombra = tarjeta;
        sombra.Offset(0, 16);
        using (var pathSombra = Redondear(sombra, 24))
        using (var brochaSombra = new SolidBrush(Color.FromArgb(78, 2, 8, 20)))
        {
            g.FillPath(brochaSombra, pathSombra);
        }

        using var path = Redondear(tarjeta, 24);
        using (var vidrio = RecortarDesenfocado(tarjeta, 10))
        {
            g.SetClip(path);
            g.DrawImageUnscaled(vidrio, tarjeta.Location);
            g.ResetClip();
        }

        using var velo = new SolidBrush(ColorTarjeta);
        using var brillo = new LinearGradientBrush(
            new Rectangle(tarjeta.X, tarjeta.Y, tarjeta.Width, 120),
            Color.FromArgb(96, 255, 255, 255),
            Color.FromArgb(0, 255, 255, 255),
            LinearGradientMode.Vertical);
        using var borde = new Pen(Color.FromArgb(210, 255, 255, 255), 1.4f);
        g.FillPath(velo, path);
        g.SetClip(path);
        g.FillRectangle(brillo, tarjeta.X, tarjeta.Y, tarjeta.Width, 120);
        g.ResetClip();
        g.DrawPath(borde, path);
    }

    private void DibujarTextos(Graphics g)
    {
        var tarjeta = RectTarjeta();
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

        using var marca = new Font("Segoe UI", 20f, FontStyle.Bold);
        using var titulo = new Font("Segoe UI", 22f, FontStyle.Bold);
        using var subtitulo = new Font("Segoe UI", 10.5f, FontStyle.Regular);
        using var estado = new Font("Segoe UI", 9.5f, FontStyle.Regular);
        using var brochaMarca = new SolidBrush(Color.White);

        using var sombraMarca = new SolidBrush(Color.FromArgb(120, 0, 0, 0));
        g.DrawString("Biblioteca", marca, sombraMarca, 37, 28);
        g.DrawString("Biblioteca", marca, brochaMarca, 36, 26);

        using var formatoCentro = new StringFormat
        {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Near,
            Trimming = StringTrimming.EllipsisWord
        };

        using var brochaTitulo = new SolidBrush(ColorTitulo);
        using var brochaSub = new SolidBrush(ColorSubtitulo);
        using var brochaEstado = new SolidBrush(_colorEstado);

        var rectTitulo = new RectangleF(tarjeta.X, tarjeta.Y + 36, tarjeta.Width, 40);
        var rectSub = new RectangleF(tarjeta.X + 28, tarjeta.Y + 84, tarjeta.Width - 56, 48);
        var rectEstado = new RectangleF(tarjeta.X + 32, tarjeta.Y + 292, tarjeta.Width - 64, 84);

        g.DrawString("Ingreso", titulo, brochaTitulo, rectTitulo, formatoCentro);
        g.DrawString("Sistema de gestión de biblioteca", subtitulo, brochaSub, rectSub, formatoCentro);
        g.DrawString(_estado, estado, brochaEstado, rectEstado, formatoCentro);
    }

    private void MostrarEstado(string texto, bool? exito)
    {
        _estado = texto;
        _colorEstado = exito switch
        {
            true => Color.FromArgb(24, 112, 72),
            false => Color.FromArgb(176, 48, 40),
            _ => ColorSubtitulo
        };
        Invalidate();
    }

    private void AlternarBotones(bool habilitados)
    {
        _btnCrear.Enabled = habilitados;
        _btnEntrar.Enabled = habilitados;
        UseWaitCursor = !habilitados;
    }

    private void Ingresar()
    {
        if (_servicios.Fabrica.Probar(out var mensaje))
        {
            DialogResult = DialogResult.OK;
            Close();
            return;
        }

        MostrarEstado("No se pudo ingresar. Cree la base de datos e intente de nuevo.", false);
        UiMensajes.Error("No se pudo ingresar al sistema.\n\n" + mensaje);
    }

    private void CrearBase()
    {
        AlternarBotones(false);
        MostrarEstado("Creando la base de datos...", null);
        Update();

        try
        {
            if (!_servicios.Fabrica.ProbarServidor(out var mensajeServidor))
            {
                MostrarEstado("El servidor SQL no está disponible.", false);
                UiMensajes.Error(mensajeServidor);
                return;
            }

            var raiz = BuscarCarpetaDatabase();
            if (raiz is null)
            {
                MostrarEstado("No se encontraron los scripts SQL.", false);
                UiMensajes.Error("No se encontraron los scripts SQL en la carpeta Database.");
                return;
            }

            _servicios.Instalador.EjecutarScripts(
                Path.Combine(raiz, "Biblioteca.sql"),
                Path.Combine(raiz, "DatosPrueba.sql"));

            MostrarEstado("Base de datos creada. Ya puede ingresar al sistema.", true);
            UiMensajes.Informar("BibliotecaDB quedó lista. Pulse Ingresar al sistema para continuar.");
        }
        catch (Exception ex)
        {
            MostrarEstado("No se pudo crear la base de datos.", false);
            UiMensajes.Error(ex.Message);
        }
        finally
        {
            AlternarBotones(true);
        }
    }

    private static string? BuscarCarpetaDatabase()
    {
        var baseDir = AppContext.BaseDirectory;
        var candidatos = new[]
        {
            Path.Combine(baseDir, "Database"),
            Path.GetFullPath(Path.Combine(baseDir, "..", "Database")),
            Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "..", "Database")),
            Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "Database"))
        };
        return candidatos.FirstOrDefault(Directory.Exists);
    }

    private Bitmap RecortarDesenfocado(Rectangle zona, int radio)
    {
        var recorte = new Bitmap(zona.Width, zona.Height, PixelFormat.Format32bppArgb);
        using (var g = Graphics.FromImage(recorte))
        {
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.DrawImage(_fondo!, new Rectangle(0, 0, zona.Width, zona.Height), zona, GraphicsUnit.Pixel);
        }

        using var primero = Desenfocar(recorte, radio);
        recorte.Dispose();
        return Desenfocar(primero, radio);
    }

    private static Bitmap CrearFondo(Size tamano)
    {
        var bmp = new Bitmap(tamano.Width, tamano.Height, PixelFormat.Format32bppArgb);
        using var g = Graphics.FromImage(bmp);
        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
        g.PixelOffsetMode = PixelOffsetMode.HighQuality;
        g.Clear(ColorBosque);

        using var origen = CargarImagenFondo();
        g.DrawImage(origen, RectanguloCubrir(origen.Size, tamano));
        return bmp;
    }

    private static Bitmap CargarImagenFondo()
    {
        var ensamblado = Assembly.GetExecutingAssembly();
        var nombre = ensamblado.GetManifestResourceNames()
            .First(n => n.EndsWith("fondo-ingreso.png", StringComparison.OrdinalIgnoreCase));
        using var stream = ensamblado.GetManifestResourceStream(nombre)
            ?? throw new InvalidOperationException("No se encontró la imagen de fondo.");
        using var imagen = Image.FromStream(stream);
        return new Bitmap(imagen);
    }

    private static Rectangle RectanguloCubrir(Size origen, Size destino)
    {
        var escala = Math.Max(destino.Width / (float)origen.Width, destino.Height / (float)origen.Height);
        var ancho = (int)Math.Ceiling(origen.Width * escala);
        var alto = (int)Math.Ceiling(origen.Height * escala);
        return new Rectangle((destino.Width - ancho) / 2, (destino.Height - alto) / 2, ancho, alto);
    }

    private static Bitmap Desenfocar(Bitmap origen, int radio)
    {
        using var horizontal = DesenfocarEje(origen, radio, true);
        return DesenfocarEje(horizontal, radio, false);
    }

    private static Bitmap DesenfocarEje(Bitmap origen, int radio, bool horizontal)
    {
        var ancho = origen.Width;
        var alto = origen.Height;
        var area = new Rectangle(0, 0, ancho, alto);
        var entrada = origen.LockBits(area, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
        var destino = new Bitmap(ancho, alto, PixelFormat.Format32bppArgb);
        var salida = destino.LockBits(area, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
        var cantidad = entrada.Stride * alto;
        var pixeles = new byte[cantidad];
        var resultado = new byte[cantidad];
        Marshal.Copy(entrada.Scan0, pixeles, 0, cantidad);
        var diametro = radio * 2 + 1;

        if (horizontal)
        {
            for (var y = 0; y < alto; y++)
            {
                var fila = y * entrada.Stride;
                Acumular(pixeles, fila, 0, radio, ancho, out var azul, out var verde, out var rojo, out var alfa);
                for (var x = 0; x < ancho; x++)
                {
                    var indice = fila + x * 4;
                    resultado[indice] = (byte)(azul / diametro);
                    resultado[indice + 1] = (byte)(verde / diametro);
                    resultado[indice + 2] = (byte)(rojo / diametro);
                    resultado[indice + 3] = (byte)(alfa / diametro);
                    AjustarVentana(pixeles, fila, x, radio, ancho, ref azul, ref verde, ref rojo, ref alfa);
                }
            }
        }
        else
        {
            for (var x = 0; x < ancho; x++)
            {
                AcumularColumna(pixeles, entrada.Stride, x, 0, radio, alto, out var azul, out var verde, out var rojo, out var alfa);
                for (var y = 0; y < alto; y++)
                {
                    var indice = y * entrada.Stride + x * 4;
                    resultado[indice] = (byte)(azul / diametro);
                    resultado[indice + 1] = (byte)(verde / diametro);
                    resultado[indice + 2] = (byte)(rojo / diametro);
                    resultado[indice + 3] = (byte)(alfa / diametro);
                    AjustarVentanaColumna(pixeles, entrada.Stride, x, y, radio, alto, ref azul, ref verde, ref rojo, ref alfa);
                }
            }
        }

        Marshal.Copy(resultado, 0, salida.Scan0, cantidad);
        origen.UnlockBits(entrada);
        destino.UnlockBits(salida);
        return destino;
    }

    private static void Acumular(byte[] pixeles, int fila, int x, int radio, int ancho, out int azul, out int verde, out int rojo, out int alfa)
    {
        azul = verde = rojo = alfa = 0;
        for (var desplazamiento = -radio; desplazamiento <= radio; desplazamiento++)
        {
            var indice = fila + Math.Clamp(x + desplazamiento, 0, ancho - 1) * 4;
            azul += pixeles[indice];
            verde += pixeles[indice + 1];
            rojo += pixeles[indice + 2];
            alfa += pixeles[indice + 3];
        }
    }

    private static void AjustarVentana(byte[] pixeles, int fila, int x, int radio, int ancho, ref int azul, ref int verde, ref int rojo, ref int alfa)
    {
        var sale = fila + Math.Clamp(x - radio, 0, ancho - 1) * 4;
        var entra = fila + Math.Clamp(x + radio + 1, 0, ancho - 1) * 4;
        azul += pixeles[entra] - pixeles[sale];
        verde += pixeles[entra + 1] - pixeles[sale + 1];
        rojo += pixeles[entra + 2] - pixeles[sale + 2];
        alfa += pixeles[entra + 3] - pixeles[sale + 3];
    }

    private static void AcumularColumna(byte[] pixeles, int stride, int x, int y, int radio, int alto, out int azul, out int verde, out int rojo, out int alfa)
    {
        azul = verde = rojo = alfa = 0;
        for (var desplazamiento = -radio; desplazamiento <= radio; desplazamiento++)
        {
            var indice = Math.Clamp(y + desplazamiento, 0, alto - 1) * stride + x * 4;
            azul += pixeles[indice];
            verde += pixeles[indice + 1];
            rojo += pixeles[indice + 2];
            alfa += pixeles[indice + 3];
        }
    }

    private static void AjustarVentanaColumna(byte[] pixeles, int stride, int x, int y, int radio, int alto, ref int azul, ref int verde, ref int rojo, ref int alfa)
    {
        var sale = Math.Clamp(y - radio, 0, alto - 1) * stride + x * 4;
        var entra = Math.Clamp(y + radio + 1, 0, alto - 1) * stride + x * 4;
        azul += pixeles[entra] - pixeles[sale];
        verde += pixeles[entra + 1] - pixeles[sale + 1];
        rojo += pixeles[entra + 2] - pixeles[sale + 2];
        alfa += pixeles[entra + 3] - pixeles[sale + 3];
    }

    private static GraphicsPath Redondear(Rectangle rect, int radio)
    {
        var diametro = radio * 2;
        var path = new GraphicsPath();
        path.AddArc(rect.X, rect.Y, diametro, diametro, 180, 90);
        path.AddArc(rect.Right - diametro, rect.Y, diametro, diametro, 270, 90);
        path.AddArc(rect.Right - diametro, rect.Bottom - diametro, diametro, diametro, 0, 90);
        path.AddArc(rect.X, rect.Bottom - diametro, diametro, diametro, 90, 90);
        path.CloseFigure();
        return path;
    }

    private sealed class BotonAcceso : Button
    {
        private readonly Color _normal;
        private readonly Color _hover;
        private bool _sobre;

        public BotonAcceso(string texto, Color normal, Color hover)
        {
            _normal = normal;
            _hover = hover;
            Text = texto;
            Font = new Font("Segoe UI", 11f, FontStyle.Bold);
            ForeColor = Color.White;
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            BackColor = normal;
            Cursor = Cursors.Hand;
            TabStop = true;
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            if (Width < 2 || Height < 2)
            {
                return;
            }

            using var path = Redondear(new Rectangle(0, 0, Width, Height), 10);
            var anterior = Region;
            Region = new Region(path);
            anterior?.Dispose();
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            _sobre = true;
            Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _sobre = false;
            Invalidate();
        }

        protected override void OnEnabledChanged(EventArgs e)
        {
            base.OnEnabledChanged(e);
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            var rect = new Rectangle(0, 0, Width - 1, Height - 1);
            using var path = Redondear(rect, 8);
            var color = !Enabled
                ? Color.FromArgb(148, 156, 164)
                : _sobre ? _hover : _normal;
            using var brocha = new SolidBrush(color);
            g.FillPath(brocha, path);
            TextRenderer.DrawText(
                g,
                Text,
                Font,
                new Rectangle(0, 0, Width, Height),
                Color.White,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        }
    }
}
