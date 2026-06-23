using GearUp.Application.Autenticacao.Common;
using GearUp.Application.Autenticacao.Exceptions;

namespace GearUp.Application.Autenticacao.Autenticar;

internal sealed class AutenticarUsuarioUseCase(
    IUsuarioRepository usuarios,
    IPasswordHasher passwordHasher,
    ITokenService tokenService) : IAutenticarUsuarioUseCase
{
    public async Task<TokenResult> LogarAsync(LoginCommand command, CancellationToken cancellationToken)
    {
        var usuario = await usuarios.ObterPorNomeAsync(command.Usuario.Trim().ToLowerInvariant(), cancellationToken);

        if (usuario is null || !usuario.Ativo || !passwordHasher.Verificar(command.Senha, usuario.SenhaHash))
            throw new CredenciaisInvalidasException();
        
        return tokenService.Gerar(usuario);
    }
}
