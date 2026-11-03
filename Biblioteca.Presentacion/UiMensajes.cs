using Biblioteca.Entidades.Models;

namespace Biblioteca.Presentacion;

public static class UiMensajes
{
    public static void Informar(string mensaje) =>
        MessageBox.Show(mensaje, "Sistema de Gestión de Biblioteca", MessageBoxButtons.OK, MessageBoxIcon.Information);

    public static void Error(string mensaje) =>
        MessageBox.Show(mensaje, "Sistema de Gestión de Biblioteca", MessageBoxButtons.OK, MessageBoxIcon.Error);

    public static bool Confirmar(string mensaje) =>
        MessageBox.Show(mensaje, "Confirmar operación", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;

    public static void Mostrar(ResultadoOperacion resultado)
    {
        if (resultado.EsExitoso)
        {
            Informar(resultado.Mensaje);
        }
        else
        {
            Error(resultado.Mensaje);
        }
    }
}
