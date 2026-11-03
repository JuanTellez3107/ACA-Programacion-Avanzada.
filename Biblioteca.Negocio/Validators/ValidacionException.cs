namespace Biblioteca.Negocio.Validators;

public class ValidacionException : Exception
{
    public ValidacionException(string mensaje) : base(mensaje)
    {
    }
}
