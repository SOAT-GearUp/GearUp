namespace GearUp.Application.Common.Exceptions;

public sealed class RecursoNaoEncontradoException(string codigo, string mensagem)
    : KeyNotFoundException(mensagem)
{
    public string Codigo { get; } = codigo;
}
