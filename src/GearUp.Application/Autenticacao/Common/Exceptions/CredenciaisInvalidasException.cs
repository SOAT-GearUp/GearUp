namespace GearUp.Application.Autenticacao.Common.Exceptions
{
    public sealed class CredenciaisInvalidasException()
        : UnauthorizedAccessException("Usuário ou senha inválidos.");
}
