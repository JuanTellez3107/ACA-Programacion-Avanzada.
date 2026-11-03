using Biblioteca.Negocio.Services;
using Biblioteca.Presentacion.Forms;

namespace Biblioteca.Presentacion;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
        Application.ThreadException += (_, e) =>
            MessageBox.Show(e.Exception.Message, "Error no controlado", MessageBoxButtons.OK, MessageBoxIcon.Error);
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
            MessageBox.Show(e.ExceptionObject.ToString(), "Error no controlado", MessageBoxButtons.OK, MessageBoxIcon.Error);

        var servicios = ServiciosAplicacion.CrearPorDefecto();
        if (!servicios.Fabrica.Probar(out _))
        {
            using var conexion = new FrmConexion(servicios);
            if (conexion.ShowDialog() != DialogResult.OK)
            {
                return;
            }
        }

        Application.Run(new FrmPrincipal(servicios));
    }
}
