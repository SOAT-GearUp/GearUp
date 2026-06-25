using GearUp.Application.Notificacoes.Common.Interfaces;
using GearUp.Application.Notificacoes.Listar;
using GearUp.Domain.Entities;
using GearUp.Domain.Enums;

namespace GearUp.Application.UnitTests.Notificacoes;

public sealed class ListarNotificacaoUseCaseTests
{
    [Fact]
    public async Task ListarNotificacoesAsync_ComComunicacoes_DeveMapearTodosOsCampos()
    {
        var clienteId = Guid.NewGuid();
        var osId = Guid.NewGuid();
        var comunicacao = Comunicacao.Criar(osId, clienteId, DestinatarioNotificacao.Cliente, "Orçamento disponível.");
        var repository = new ComunicacaoRepositoryFake([comunicacao]);
        var useCase = new ListarNotificacaoUseCase(repository);
        var command = new ListarNotificaoCommand(DestinatarioNotificacao.Cliente, clienteId);

        var result = await useCase.ListarNotificacoesAsync(command, CancellationToken.None);

        var item = Assert.Single(result);
        Assert.Equal(comunicacao.Id, item.Id);
        Assert.Equal(osId, item.OrdemServicoId);
        Assert.Equal(clienteId, item.ClienteId);
        Assert.Equal(DestinatarioNotificacao.Cliente, item.Destinatario);
        Assert.Equal("Orçamento disponível.", item.Mensagem);
        Assert.Equal(comunicacao.CriadaEm, item.CriadaEm);
        Assert.Equal(comunicacao.LidaEm, item.LidaEm);
    }

    [Fact]
    public async Task ListarNotificacoesAsync_DeveRepassarFiltrosAoRepositorio()
    {
        var clienteId = Guid.NewGuid();
        var repository = new ComunicacaoRepositoryFake([]);
        var useCase = new ListarNotificacaoUseCase(repository);
        var command = new ListarNotificaoCommand(DestinatarioNotificacao.Atendente, clienteId);

        await useCase.ListarNotificacoesAsync(command, CancellationToken.None);

        Assert.Equal(DestinatarioNotificacao.Atendente, repository.DestinatarioConsultado);
        Assert.Equal(clienteId, repository.ClienteIdConsultado);
    }

    [Fact]
    public async Task ListarNotificacoesAsync_SemComunicacoes_DeveRetornarListaVazia()
    {
        var repository = new ComunicacaoRepositoryFake([]);
        var useCase = new ListarNotificacaoUseCase(repository);
        var command = new ListarNotificaoCommand(DestinatarioNotificacao.Cliente, null);

        var result = await useCase.ListarNotificacoesAsync(command, CancellationToken.None);

        Assert.Empty(result);
    }

    private sealed class ComunicacaoRepositoryFake(IReadOnlyList<Comunicacao> comunicacoes) : IComunicacaoRepository
    {
        public DestinatarioNotificacao? DestinatarioConsultado { get; private set; }
        public Guid? ClienteIdConsultado { get; private set; }

        public Task<IReadOnlyList<Comunicacao>> ListarAsync(
            DestinatarioNotificacao destinatario,
            Guid? clienteId,
            CancellationToken ct)
        {
            DestinatarioConsultado = destinatario;
            ClienteIdConsultado = clienteId;
            return Task.FromResult(comunicacoes);
        }

        public Task AdicionarAsync(Comunicacao comunicacao, CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
