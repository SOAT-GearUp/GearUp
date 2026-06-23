namespace GearUp.Application.Common.Exceptions;

public sealed class ConflitoException(string codigo, string mensagem)
    : InvalidOperationException(mensagem)
{
    public string Codigo { get; } = codigo;
}
