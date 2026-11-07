using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Biblioteca.Presentacion;

public static class UiFondo
{
    public enum Capa
    {
        Imagen,
        VidrioClaro,
        VidrioOscuro
    }

    private static Bitmap? _fondo;
    private static Bitmap? _desenfocado;

    public static void Ajustar(Size tamano)
    {
        if (tamano.Width < 2 || tamano.Height < 2)
        {
            return;
        }

        if (_fondo is not null && _fondo.Width == tamano.Width && _fondo.Height == tamano.Height)
        {
            return;
        }

        _fondo?.Dispose();
        _desenfocado?.Dispose();
        _fondo = Crear(tamano);
        using var paso = Desenfocar(_fondo, 8);
        _desenfocado = Desenfocar(paso, 8);
    }

    public static void Pintar(Graphics g, Control control, Capa capa)
    {
        var formulario = control.FindForm();
        if (formulario is not null)
        {
            Ajustar(formulario.ClientSize);
        }

        var mapa = capa == Capa.Imagen ? _fondo : _desenfocado;
        if (mapa is null || formulario is null || !control.IsHandleCreated || !formulario.IsHandleCreated)
        {
            g.Clear(capa == Capa.VidrioOscuro ? UiTema.Sidebar : UiTema.Tarjeta);
        }
        else
        {
            var esquina = control.PointToScreen(Point.Empty);
            var origen = formulario.PointToScreen(Point.Empty);
            var recorte = new Rectangle(esquina.X - origen.X, esquina.Y - origen.Y, control.Width, control.Height);
            if (recorte.Width > 0 && recorte.Height > 0)
            {
                g.DrawImage(mapa, new Rectangle(0, 0, control.Width, control.Height), recorte, GraphicsUnit.Pixel);
            }
        }

        if (capa == Capa.Imagen)
        {
            return;
        }

        var velo = capa == Capa.VidrioOscuro ? UiTema.VidrioOscuro : UiTema.VidrioClaro;
        using (var brocha = new SolidBrush(velo))
        {
            g.FillRectangle(brocha, control.ClientRectangle);
        }

        var altoBrillo = Math.Min(90, control.Height);
        if (altoBrillo > 1 && control.Width > 1)
        {
            using var brillo = new LinearGradientBrush(
                new Rectangle(0, 0, control.Width, altoBrillo),
                Color.FromArgb(capa == Capa.VidrioOscuro ? 36 : 90, 255, 255, 255),
                Color.FromArgb(0, 255, 255, 255),
                LinearGradientMode.Vertical);
            g.FillRectangle(brillo, 0, 0, control.Width, altoBrillo);
        }

        using var borde = new Pen(Color.FromArgb(capa == Capa.VidrioOscuro ? 70 : 180, 255, 255, 255));
        g.DrawRectangle(borde, 0, 0, control.Width - 1, control.Height - 1);
    }

    private static Bitmap Crear(Size tamano)
    {
        var bmp = new Bitmap(tamano.Width, tamano.Height, PixelFormat.Format32bppArgb);
        using var g = Graphics.FromImage(bmp);
        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
        g.Clear(UiTema.Fondo);
        using var origen = CargarImagen();
        g.DrawImage(origen, Cubrir(origen.Size, tamano));
        return bmp;
    }

    private static Bitmap CargarImagen()
    {
        var ensamblado = Assembly.GetExecutingAssembly();
        var nombre = ensamblado.GetManifestResourceNames()
            .First(n => n.EndsWith("fondo-ingreso.png", StringComparison.OrdinalIgnoreCase));
        using var stream = ensamblado.GetManifestResourceStream(nombre)
            ?? throw new InvalidOperationException("No se encontró la imagen de fondo.");
        using var imagen = Image.FromStream(stream);
        return new Bitmap(imagen);
    }

    private static Rectangle Cubrir(Size origen, Size destino)
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
                Sumar(pixeles, fila, 0, radio, ancho, out var azul, out var verde, out var rojo, out var alfa);
                for (var x = 0; x < ancho; x++)
                {
                    var indice = fila + x * 4;
                    resultado[indice] = (byte)(azul / diametro);
                    resultado[indice + 1] = (byte)(verde / diametro);
                    resultado[indice + 2] = (byte)(rojo / diametro);
                    resultado[indice + 3] = (byte)(alfa / diametro);
                    Mover(pixeles, fila, x, radio, ancho, ref azul, ref verde, ref rojo, ref alfa);
                }
            }
        }
        else
        {
            for (var x = 0; x < ancho; x++)
            {
                SumarColumna(pixeles, entrada.Stride, x, radio, alto, out var azul, out var verde, out var rojo, out var alfa);
                for (var y = 0; y < alto; y++)
                {
                    var indice = y * entrada.Stride + x * 4;
                    resultado[indice] = (byte)(azul / diametro);
                    resultado[indice + 1] = (byte)(verde / diametro);
                    resultado[indice + 2] = (byte)(rojo / diametro);
                    resultado[indice + 3] = (byte)(alfa / diametro);
                    MoverColumna(pixeles, entrada.Stride, x, y, radio, alto, ref azul, ref verde, ref rojo, ref alfa);
                }
            }
        }

        Marshal.Copy(resultado, 0, salida.Scan0, cantidad);
        origen.UnlockBits(entrada);
        destino.UnlockBits(salida);
        return destino;
    }

    private static void Sumar(byte[] pixeles, int fila, int x, int radio, int ancho, out int azul, out int verde, out int rojo, out int alfa)
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

    private static void Mover(byte[] pixeles, int fila, int x, int radio, int ancho, ref int azul, ref int verde, ref int rojo, ref int alfa)
    {
        var sale = fila + Math.Clamp(x - radio, 0, ancho - 1) * 4;
        var entra = fila + Math.Clamp(x + radio + 1, 0, ancho - 1) * 4;
        azul += pixeles[entra] - pixeles[sale];
        verde += pixeles[entra + 1] - pixeles[sale + 1];
        rojo += pixeles[entra + 2] - pixeles[sale + 2];
        alfa += pixeles[entra + 3] - pixeles[sale + 3];
    }

    private static void SumarColumna(byte[] pixeles, int stride, int x, int radio, int alto, out int azul, out int verde, out int rojo, out int alfa)
    {
        azul = verde = rojo = alfa = 0;
        for (var desplazamiento = -radio; desplazamiento <= radio; desplazamiento++)
        {
            var indice = Math.Clamp(desplazamiento, 0, alto - 1) * stride + x * 4;
            azul += pixeles[indice];
            verde += pixeles[indice + 1];
            rojo += pixeles[indice + 2];
            alfa += pixeles[indice + 3];
        }
    }

    private static void MoverColumna(byte[] pixeles, int stride, int x, int y, int radio, int alto, ref int azul, ref int verde, ref int rojo, ref int alfa)
    {
        var sale = Math.Clamp(y - radio, 0, alto - 1) * stride + x * 4;
        var entra = Math.Clamp(y + radio + 1, 0, alto - 1) * stride + x * 4;
        azul += pixeles[entra] - pixeles[sale];
        verde += pixeles[entra + 1] - pixeles[sale + 1];
        rojo += pixeles[entra + 2] - pixeles[sale + 2];
        alfa += pixeles[entra + 3] - pixeles[sale + 3];
    }
}

public class PanelCristal : Panel
{
    public UiFondo.Capa Capa { get; set; } = UiFondo.Capa.VidrioClaro;

    public PanelCristal()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        DoubleBuffered = true;
    }

    protected override void OnPaintBackground(PaintEventArgs e)
    {
        UiFondo.Pintar(e.Graphics, this, Capa);
    }
}

public class FlowCristal : FlowLayoutPanel
{
    public UiFondo.Capa Capa { get; set; } = UiFondo.Capa.Imagen;

    public FlowCristal()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        DoubleBuffered = true;
    }

    protected override void OnPaintBackground(PaintEventArgs e)
    {
        UiFondo.Pintar(e.Graphics, this, Capa);
    }
}
