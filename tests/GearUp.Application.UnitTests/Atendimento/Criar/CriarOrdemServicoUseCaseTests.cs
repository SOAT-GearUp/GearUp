using GearUp.Application.Atendimento.Clientes.Common.Interfaces;
using GearUp.Application.Atendimento.Clientes.Veiculos.Common.Interfaces;
using GearUp.Application.Atendimento.Comum.Interfaces;
using GearUp.Application.Atendimento.Criar;
using GearUp.Application.Common.Exceptions;
using GearUp.Application.Common.Interfaces;
using GearUp.Domain.Entities;
using GearUp.Domain.Enums;
using GearUp.Domain.ValueObjects;

namespace GearUp.Application.UnitTests.Atendimento.Criar;

public sealed class CriarOrdemServicoUseCaseTests
{
    [Fact]
    public async Task CriarAsync_ComClienteEVeiculoValidos_DeveAdicionarESalvar()
    {
        var cliente = Cliente.Criar("Maria da Silva", "52998224725", "maria@email.com", "11999999999");
        var veiculo = Veiculo.Criar(cliente.Id, "ABC1D23", "Fiat", "Uno", 2015);

        var clienteRepository = new ClienteRepositoryFake(cliente);
        var veiculoRepository = new VeiculoRepositoryFake(veiculo);
        var ordemRepository = new OrdemServicoRepositoryFake();
        var unitOfWork = new UnitOfWorkFake();
        var useCase = new CriarOrdemServicoUseCase(clienteRepository, veiculoRepository, ordemRepository, unitOfWork);
        var command = CriarCommand(cliente.Id, veiculo.Id);

        var result = await useCase.CriarAsync(command, CancellationToken.None);

        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.NotNull(ordemRepository.OrdemAdicionada);
        Assert.Equal(result.Id, ordemRepository.OrdemAdicionada.Id);
        Assert.Equal(cliente.Id, ordemRepository.OrdemAdicionada.ClienteId);
        Assert.Equal(veiculo.Id, ordemRepository.OrdemAdicionada.VeiculoId);
        Assert.Equal(1, unitOfWork.SaveChangesChamadas);
    }

    [Fact]
    public async Task CriarAsync_ComClienteInexistente_DeveLancarRecursoNaoEncontrado()
    {
        var clienteRepository = new ClienteRepositoryFake(cliente: null);
        var veiculoRepository = new VeiculoRepositoryFake(veiculo: null);
        var ordemRepository = new OrdemServicoRepositoryFake();
        var unitOfWork = new UnitOfWorkFake();
        var useCase = new CriarOrdemServicoUseCase(clienteRepository, veiculoRepository, ordemRepository, unitOfWork);
        var command = CriarCommand(Guid.NewGuid(), Guid.NewGuid());

        var excecao = await Assert.ThrowsAsync<RecursoNaoEncontradoException>(
            () => useCase.CriarAsync(command, CancellationToken.None));

        Assert.Equal("CLIENTE_NAO_ENCONTRADO", excecao.Codigo);
        Assert.Null(ordemRepository.OrdemAdicionada);
        Assert.Equal(0, unitOfWork.SaveChangesChamadas);
    }

    [Fact]
    public async Task CriarAsync_ComVeiculoInexistente_DeveLancarRecursoNaoEncontrado()
    {
        var cliente = Cliente.Criar("Maria da Silva", "52998224725", "maria@email.com", "11999999999");

        var clienteRepository = new ClienteRepositoryFake(cliente);
        var veiculoRepository = new VeiculoRepositoryFake(veiculo: null);
        var ordemRepository = new OrdemServicoRepositoryFake();
        var unitOfWork = new UnitOfWorkFake();
        var useCase = new CriarOrdemServicoUseCase(clienteRepository, veiculoRepository, ordemRepository, unitOfWork);
        var command = CriarCommand(cliente.Id, Guid.NewGuid());

        var excecao = await Assert.ThrowsAsync<RecursoNaoEncontradoException>(
            () => useCase.CriarAsync(command, CancellationToken.None));

        Assert.Equal("VEICULO_NAO_ENCONTRADO", excecao.Codigo);
        Assert.Null(ordemRepository.OrdemAdicionada);
        Assert.Equal(0, unitOfWork.SaveChangesChamadas);
    }

    [Fact]
    public async Task CriarAsync_ComVeiculoDeOutroCliente_DeveLancarRecursoNaoEncontrado()
    {
        var cliente = Cliente.Criar("Maria da Silva", "52998224725", "maria@email.com", "11999999999");
        var veiculo = Veiculo.Criar(Guid.NewGuid(), "ABC1D23", "Fiat", "Uno", 2015);

        var clienteRepository = new ClienteRepositoryFake(cliente);
        var veiculoRepository = new VeiculoRepositoryFake(veiculo);
        var ordemRepository = new OrdemServicoRepositoryFake();
        var unitOfWork = new UnitOfWorkFake();
        var useCase = new CriarOrdemServicoUseCase(clienteRepository, veiculoRepository, ordemRepository, unitOfWork);
        var command = CriarCommand(cliente.Id, veiculo.Id);

        var excecao = await Assert.ThrowsAsync<RecursoNaoEncontradoException>(
            () => useCase.CriarAsync(command, CancellationToken.None));

        Assert.Equal("VEICULO_NAO_ENCONTRADO", excecao.Codigo);
        Assert.Null(ordemRepository.OrdemAdicionada);
        Assert.Equal(0, unitOfWork.SaveChangesChamadas);
    }

    private static CriarOrdemServicoCommand CriarCommand(Guid clienteId, Guid veiculoId) =>
        new(
            clienteId,
            veiculoId,
            "Barulho no motor ao acelerar.",
            PrioridadeOrdemServico.Normal,
            null);

    private sealed class ClienteRepositoryFake(Cliente? cliente) : IClienteRepository
    {
        public Task<Cliente?> ObterAsync(Guid id, CancellationToken cancellationToken) => Task.FromResult(cliente);
        public Task<IReadOnlyList<Cliente>> ListarAsync(CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<Cliente>>([]);
        public Task<bool> DocumentoExisteAsync(Documento documento, CancellationToken cancellationToken) => Task.FromResult(false);
        public Task AdicionarAsync(Cliente cliente, CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class VeiculoRepositoryFake(Veiculo? veiculo) : IVeiculoRepository
    {
        public Task<Veiculo?> ObterAsync(Guid id, CancellationToken ct) => Task.FromResult(veiculo);
        public Task<IReadOnlyList<Veiculo>> ListarPorClienteAsync(Guid clienteId, CancellationToken ct) => Task.FromResult<IReadOnlyList<Veiculo>>([]);
        public Task AdicionarAsync(Veiculo veiculo, CancellationToken ct) => Task.CompletedTask;
        public Task<bool> PlacaExisteAsync(string placa, Guid? ignorarId, CancellationToken ct) => Task.FromResult(false);
    }

    private sealed class OrdemServicoRepositoryFake : IOrdemServicoRepository
    {
        public OrdemServico? OrdemAdicionada { get; private set; }

        public Task AdicionarAsync(OrdemServico ordem, CancellationToken ct)
        {
            OrdemAdicionada = ordem;
            return Task.CompletedTask;
        }

        public Task<OrdemServico?> ObterAsync(Guid id, CancellationToken ct) => Task.FromResult<OrdemServico?>(null);
        public Task<IReadOnlyList<OrdemServico>> ListarAsync(bool somenteEmAndamento, Guid? clienteId, CancellationToken ct) => Task.FromResult<IReadOnlyList<OrdemServico>>([]);
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
