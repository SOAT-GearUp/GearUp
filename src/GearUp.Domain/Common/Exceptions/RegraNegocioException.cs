namespace GearUp.Domain.Common.Exceptions;

public sealed class RegraNegocioException(string codigo, string mensagem)
    : InvalidOperationException(mensagem)
{
    public string Codigo { get; } = codigo;
}
