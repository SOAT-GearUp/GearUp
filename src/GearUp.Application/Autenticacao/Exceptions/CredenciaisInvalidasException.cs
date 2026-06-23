namespace GearUp.Application.Autenticacao.Exceptions
{
    public sealed class CredenciaisInvalidasException()
        : UnauthorizedAccessException("Usuário ou senha inválidos.");
}
