using GearUp.Application.Cadastro.Clientes.Common.Interfaces;
using GearUp.Application.Autenticacao.Common;
using GearUp.Application.Autenticacao.GerenciarUsuarios;
using GearUp.Application.Common.Exceptions;
using GearUp.Application.Common.Interfaces;
using GearUp.Domain.Entities;
using GearUp.Domain.Enums;
using GearUp.Domain.ValueObjects.Clientes;

namespace GearUp.Application.UnitTests.Autenticacao;

public sealed class GerenciarUsuariosUseCaseTests
{
    [Fact]
    public async Task CriarAsync_ComAdminCriandoFuncionarioNovo_DeveAdicionarESalvar()
    {
        var usuarios = new UsuarioRepositoryFake(existe: false);
        var clientes = new ClienteRepositoryFake(cliente: null);
        var hasher = new PasswordHasherFake();
        var unitOfWork = new UnitOfWorkFake();
        var useCase = new GerenciarUsuariosUseCase(usuarios, clientes, hasher, unitOfWork);
        var command = new CriarUsuarioCommand("  Atendente  ", "senha123", PerfilUsuario.Atendente, null, PerfilUsuario.Admin);

        var result = await useCase.CriarAsync(command, CancellationToken.None);

        Assert.NotNull(usuarios.UsuarioAdicionado);
        Assert.Equal(result.Id, usuarios.UsuarioAdicionado.Id);
        Assert.Equal("atendente", usuarios.UsuarioAdicionado.NomeUsuario);
        Assert.Equal("hash:senha123", usuarios.UsuarioAdicionado.SenhaHash);
        Assert.Equal("atendente", usuarios.NomeConsultado);
        Assert.Equal(1, unitOfWork.SaveChangesChamadas);
    }

    [Fact]
    public async Task CriarAsync_ComAtendenteCriandoClienteVinculadoValido_DeveAdicionarESalvar()
    {
        var cliente = Cliente.Criar("Maria da Silva", "52998224725", "maria@email.com", "11999999999");
        var usuarios = new UsuarioRepositoryFake(existe: false);
        var clientes = new ClienteRepositoryFake(cliente);
        var hasher = new PasswordHasherFake();
        var unitOfWork = new UnitOfWorkFake();
        var useCase = new GerenciarUsuariosUseCase(usuarios, clientes, hasher, unitOfWork);
        var command = new CriarUsuarioCommand("maria", "senha123", PerfilUsuario.Cliente, cliente.Id, PerfilUsuario.Atendente);

        var result = await useCase.CriarAsync(command, CancellationToken.None);

        Assert.NotNull(usuarios.UsuarioAdicionado);
        Assert.Equal(result.Id, usuarios.UsuarioAdicionado.Id);
        Assert.Equal(cliente.Id, usuarios.UsuarioAdicionado.ClienteId);
        Assert.Equal(cliente.Id, clientes.IdConsultado);
        Assert.Equal(1, unitOfWork.SaveChangesChamadas);
    }

    [Fact]
    public async Task CriarAsync_ComAtendenteCriandoFuncionario_DeveRejeitar()
    {
        var usuarios = new UsuarioRepositoryFake(existe: false);
        var clientes = new ClienteRepositoryFake(cliente: null);
        var unitOfWork = new UnitOfWorkFake();
        var useCase = new GerenciarUsuariosUseCase(usuarios, clientes, new PasswordHasherFake(), unitOfWork);
        var command = new CriarUsuarioCommand("mecanico", "senha123", PerfilUsuario.Mecanico, null, PerfilUsuario.Atendente);

        var ex = await Assert.ThrowsAsync<AcessoNegadoException>(
            () => useCase.CriarAsync(command, CancellationToken.None));

        Assert.Equal("PERFIL_NAO_PERMITIDO", ex.Codigo);
        Assert.Null(usuarios.UsuarioAdicionado);
        Assert.Equal(0, unitOfWork.SaveChangesChamadas);
    }

    [Fact]
    public async Task CriarAsync_ComNomeDuplicado_DeveRejeitar()
    {
        var usuarios = new UsuarioRepositoryFake(existe: true);
        var clientes = new ClienteRepositoryFake(cliente: null);
        var unitOfWork = new UnitOfWorkFake();
        var useCase = new GerenciarUsuariosUseCase(usuarios, clientes, new PasswordHasherFake(), unitOfWork);
        var command = new CriarUsuarioCommand("atendente", "senha123", PerfilUsuario.Atendente, null, PerfilUsuario.Admin);

        var ex = await Assert.ThrowsAsync<ConflitoException>(
            () => useCase.CriarAsync(command, CancellationToken.None));

        Assert.Equal("USUARIO_DUPLICADO", ex.Codigo);
        Assert.Null(usuarios.UsuarioAdicionado);
        Assert.Equal(0, unitOfWork.SaveChangesChamadas);
    }

    [Fact]
    public async Task CriarAsync_ComPerfilClienteSemClienteId_DeveRejeitar()
    {
        var usuarios = new UsuarioRepositoryFake(existe: false);
        var clientes = new ClienteRepositoryFake(cliente: null);
        var unitOfWork = new UnitOfWorkFake();
        var useCase = new GerenciarUsuariosUseCase(usuarios, clientes, new PasswordHasherFake(), unitOfWork);
        var command = new CriarUsuarioCommand("maria", "senha123", PerfilUsuario.Cliente, null, PerfilUsuario.Atendente);

        var ex = await Assert.ThrowsAsync<RecursoNaoEncontradoException>(
            () => useCase.CriarAsync(command, CancellationToken.None));

        Assert.Equal("CLIENTE_NAO_ENCONTRADO", ex.Codigo);
        Assert.Null(usuarios.UsuarioAdicionado);
        Assert.Equal(0, unitOfWork.SaveChangesChamadas);
    }

    [Fact]
    public async Task CriarAsync_ComPerfilClienteEClienteInexistente_DeveRejeitar()
    {
        var usuarios = new UsuarioRepositoryFake(existe: false);
        var clientes = new ClienteRepositoryFake(cliente: null);
        var unitOfWork = new UnitOfWorkFake();
        var useCase = new GerenciarUsuariosUseCase(usuarios, clientes, new PasswordHasherFake(), unitOfWork);
        var command = new CriarUsuarioCommand("maria", "senha123", PerfilUsuario.Cliente, Guid.NewGuid(), PerfilUsuario.Atendente);

        var ex = await Assert.ThrowsAsync<RecursoNaoEncontradoException>(
            () => useCase.CriarAsync(command, CancellationToken.None));

        Assert.Equal("CLIENTE_NAO_ENCONTRADO", ex.Codigo);
        Assert.Null(usuarios.UsuarioAdicionado);
        Assert.Equal(0, unitOfWork.SaveChangesChamadas);
    }

    private sealed class UsuarioRepositoryFake(bool existe) : IUsuarioRepository
    {
        public string? NomeConsultado { get; private set; }
        public Usuario? UsuarioAdicionado { get; private set; }

        public Task<bool> ExisteAsync(string nomeUsuario, CancellationToken cancellationToken)
        {
            NomeConsultado = nomeUsuario;
            return Task.FromResult(existe);
        }

        public Task AdicionarAsync(Usuario usuario, CancellationToken cancellationToken)
        {
            UsuarioAdicionado = usuario;
            return Task.CompletedTask;
        }

        public Task<Usuario?> ObterPorNomeAsync(string nomeUsuario, CancellationToken cancellationToken) =>
            Task.FromResult<Usuario?>(null);
    }

    private sealed class ClienteRepositoryFake(Cliente? cliente) : IClienteRepository
    {
        public Guid? IdConsultado { get; private set; }

        public Task<Cliente?> ObterAsync(Guid id, CancellationToken cancellationToken)
        {
            IdConsultado = id;
            return Task.FromResult(cliente);
        }

        public Task<IReadOnlyList<Cliente>> ListarAsync(CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<Cliente>>([]);

        public Task<bool> DocumentoExisteAsync(Documento documento, CancellationToken cancellationToken) =>
            Task.FromResult(false);

        public Task AdicionarAsync(Cliente cliente, CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class PasswordHasherFake : IPasswordHasher
    {
        public string CriarHash(string senha) => $"hash:{senha}";
        public bool Verificar(string senha, string hash) => true;
    }

    private sealed class UnitOfWorkFake : IUnitOfWork
    {
        public int SaveChangesChamadas { get; private set; }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SaveChangesChamadas++;
            return Task.FromResult(1);
        }
    }
}
