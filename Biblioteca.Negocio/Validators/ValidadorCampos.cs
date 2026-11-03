using System.Net.Mail;
using System.Text.RegularExpressions;

namespace Biblioteca.Negocio.Validators;

public static class ValidadorCampos
{
    public static string Requerido(string? valor, string nombreCampo)
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            throw new ValidacionException($"El campo {nombreCampo} es obligatorio.");
        }

        return valor.Trim();
    }

    public static string Longitud(string valor, string nombreCampo, int minimo, int maximo)
    {
        if (valor.Length < minimo || valor.Length > maximo)
        {
            throw new ValidacionException($"El campo {nombreCampo} debe tener entre {minimo} y {maximo} caracteres.");
        }

        return valor;
    }

    public static string Correo(string valor, string nombreCampo)
    {
        try
        {
            var correo = new MailAddress(valor);
            if (!valor.Contains('.') || correo.Address != valor)
            {
                throw new ValidacionException($"El campo {nombreCampo} no tiene un formato válido.");
            }
        }
        catch
        {
            throw new ValidacionException($"El campo {nombreCampo} no tiene un formato válido.");
        }

        return valor;
    }

    public static string? CorreoOpcional(string? valor, string nombreCampo)
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            return null;
        }

        return Correo(valor.Trim(), nombreCampo);
    }

    public static int EnteroPositivo(int valor, string nombreCampo, int minimo = 1)
    {
        if (valor < minimo)
        {
            throw new ValidacionException($"El campo {nombreCampo} debe ser mayor o igual a {minimo}.");
        }

        return valor;
    }

    public static string? SoloDigitosOpcional(string? valor, string nombreCampo)
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            return null;
        }

        var limpio = valor.Trim();
        if (!Regex.IsMatch(limpio, @"^[0-9+\-\s]+$"))
        {
            throw new ValidacionException($"El campo {nombreCampo} solo admite números y símbolos telefónicos.");
        }

        return limpio;
    }
}
