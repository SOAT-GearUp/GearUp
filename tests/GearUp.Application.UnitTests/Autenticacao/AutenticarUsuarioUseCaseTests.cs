using System.Reflection;
using GearUp.Application.Autenticacao.Autenticar;
using GearUp.Application.Autenticacao.Common.Exceptions;
using GearUp.Application.Autenticacao.Common.Interfaces;
using GearUp.Domain.Entities;
using GearUp.Domain.Enums;

namespace GearUp.Application.UnitTests.Autenticacao;

public sealed class AutenticarUsuarioUseCaseTests
{
    [Fact]
    public async Task LogarAsync_ComCredenciaisValidas_DeveGerarToken()
    {
        var usuario = CriarUsuario(ativo: true);
        var repository = new UsuarioRepositoryFake(usuario);
        var hasher = new PasswordHasherFake(verificar: true);
        var tokenService = new TokenServiceFake();
        var useCase = new AutenticarUsuarioUseCase(repository, hasher, tokenService);
        var command = new LoginCommand("  Atendente  ", "senha-correta");

        var result = await useCase.LogarAsync(command, CancellationToken.None);

        Assert.Same(tokenService.TokenGerado, result);
        Assert.Same(usuario, tokenService.UsuarioRecebido);
    }

    [Fact]
    public async Task LogarAsync_DeveNormalizarNomeAntesDeConsultarRepositorio()
    {
        var usuario = CriarUsuario(ativo: true);
        var repository = new UsuarioRepositoryFake(usuario);
        var hasher = new PasswordHasherFake(verificar: true);
        var useCase = new AutenticarUsuarioUseCase(repository, hasher, new TokenServiceFake());
        var command = new LoginCommand("  Atendente  ", "senha-correta");

        await useCase.LogarAsync(command, CancellationToken.None);

        Assert.Equal("atendente", repository.NomeConsultado);
    }

    [Fact]
    public async Task LogarAsync_QuandoUsuarioNaoExiste_DeveRejeitar()
    {
        var repository = new UsuarioRepositoryFake(usuario: null);
        var hasher = new PasswordHasherFake(verificar: true);
        var tokenService = new TokenServiceFake();
        var useCase = new AutenticarUsuarioUseCase(repository, hasher, tokenService);
        var command = new LoginCommand("inexistente", "qualquer");

        await Assert.ThrowsAsync<CredenciaisInvalidasException>(
            () => useCase.LogarAsync(command, CancellationToken.None));

        Assert.Null(tokenService.UsuarioRecebido);
    }

    [Fact]
    public async Task LogarAsync_QuandoUsuarioInativo_DeveRejeitar()
    {
        var usuario = CriarUsuario(ativo: false);
        var repository = new UsuarioRepositoryFake(usuario);
        var hasher = new PasswordHasherFake(verificar: true);
        var tokenService = new TokenServiceFake();
        var useCase = new AutenticarUsuarioUseCase(repository, hasher, tokenService);
        var command = new LoginCommand("atendente", "senha-correta");

        await Assert.ThrowsAsync<CredenciaisInvalidasException>(
            () => useCase.LogarAsync(command, CancellationToken.None));

        Assert.Null(tokenService.UsuarioRecebido);
    }

    [Fact]
    public async Task LogarAsync_QuandoSenhaInvalida_DeveRejeitar()
    {
        var usuario = CriarUsuario(ativo: true);
        var repository = new UsuarioRepositoryFake(usuario);
        var hasher = new PasswordHasherFake(verificar: false);
        var tokenService = new TokenServiceFake();
        var useCase = new AutenticarUsuarioUseCase(repository, hasher, tokenService);
        var command = new LoginCommand("atendente", "senha-errada");

        await Assert.ThrowsAsync<CredenciaisInvalidasException>(
            () => useCase.LogarAsync(command, CancellationToken.None));

        Assert.Null(tokenService.UsuarioRecebido);
    }

    private static Usuario CriarUsuario(bool ativo)
    {
        var usuario = Usuario.Criar("atendente", "hash-armazenado", PerfilUsuario.Atendente);

        if (!ativo)
            typeof(Usuario)
                .GetProperty(nameof(Usuario.Ativo))!
                .GetSetMethod(nonPublic: true)!
                .Invoke(usuario, [false]);

        return usuario;
    }

    private sealed class UsuarioRepositoryFake(Usuario? usuario) : IUsuarioRepository
    {
        public string? NomeConsultado { get; private set; }

        public Task<Usuario?> ObterPorNomeAsync(string nomeUsuario, CancellationToken cancellationToken)
        {
            NomeConsultado = nomeUsuario;
            return Task.FromResult(usuario);
        }

        public Task<bool> ExisteAsync(string nomeUsuario, CancellationToken cancellationToken) => Task.FromResult(false);
        public Task AdicionarAsync(Usuario usuario, CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class PasswordHasherFake(bool verificar) : IPasswordHasher
    {
        public string CriarHash(string senha) => $"hash:{senha}";
        public bool Verificar(string senha, string hash) => verificar;
    }

    private sealed class TokenServiceFake : ITokenService
    {
        public TokenResult TokenGerado { get; } = new("access-token", DateTimeOffset.UtcNow.AddHours(1));
        public Usuario? UsuarioRecebido { get; private set; }

        public TokenResult Gerar(Usuario usuario)
        {
            UsuarioRecebido = usuario;
            return TokenGerado;
        }
    }
}
