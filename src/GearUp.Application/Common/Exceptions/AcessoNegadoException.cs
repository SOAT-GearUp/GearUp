namespace GearUp.Application.Common.Exceptions;

public sealed class AcessoNegadoException(string codigo, string mensagem) : Exception(mensagem)
{
    public string Codigo { get; } = codigo;
}
