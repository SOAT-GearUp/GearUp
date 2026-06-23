namespace GearUp.Application.Autenticacao.Autenticar
{
    public interface IAutenticarUsuarioUseCase
    {
        Task<TokenResult> LogarAsync(LoginCommand command, CancellationToken cancellationToken);
    }
}
