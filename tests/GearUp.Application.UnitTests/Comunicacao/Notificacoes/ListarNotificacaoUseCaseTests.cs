using GearUp.Application.Comunicacao.Common.Interfaces;
using GearUp.Application.Comunicacao.Notificacoes;
using GearUp.Domain.Enums;
using NotificacaoEntity = GearUp.Domain.Entities.Notificacao;

namespace GearUp.Application.UnitTests.Comunicacao.Notificacoes;

public sealed class ListarNotificacaoUseCaseTests
{
    [Fact]
    public async Task ListarNotificacoesAsync_ComNotificacoes_DeveMapearTodosOsCampos()
    {
        var clienteId = Guid.NewGuid();
        var osId = Guid.NewGuid();
        var notificacao = NotificacaoEntity.Criar(osId, clienteId, DestinatarioNotificacao.Cliente, "Orçamento disponível.");
        var repository = new NotificacaoRepositoryFake([notificacao]);
        var useCase = new ListarNotificacaoUseCase(repository);
        var command = new ListarNotificaoCommand(DestinatarioNotificacao.Cliente, clienteId);

        var result = await useCase.ListarNotificacoesAsync(command, CancellationToken.None);

        var item = Assert.Single(result);
        Assert.Equal(notificacao.Id, item.Id);
        Assert.Equal(osId, item.OrdemServicoId);
        Assert.Equal(clienteId, item.ClienteId);
        Assert.Equal(DestinatarioNotificacao.Cliente, item.Destinatario);
        Assert.Equal("Orçamento disponível.", item.Mensagem);
        Assert.Equal(notificacao.CriadaEm, item.CriadaEm);
        Assert.Equal(notificacao.LidaEm, item.LidaEm);
    }

    [Fact]
    public async Task ListarNotificacoesAsync_DeveRepassarFiltrosAoRepositorio()
    {
        var clienteId = Guid.NewGuid();
        var repository = new NotificacaoRepositoryFake([]);
        var useCase = new ListarNotificacaoUseCase(repository);
        var command = new ListarNotificaoCommand(DestinatarioNotificacao.Atendente, clienteId);

        await useCase.ListarNotificacoesAsync(command, CancellationToken.None);

        Assert.Equal(DestinatarioNotificacao.Atendente, repository.DestinatarioConsultado);
        Assert.Equal(clienteId, repository.ClienteIdConsultado);
    }

    [Fact]
    public async Task ListarNotificacoesAsync_SemNotificacoes_DeveRetornarListaVazia()
    {
        var repository = new NotificacaoRepositoryFake([]);
        var useCase = new ListarNotificacaoUseCase(repository);
        var command = new ListarNotificaoCommand(DestinatarioNotificacao.Cliente, null);

        var result = await useCase.ListarNotificacoesAsync(command, CancellationToken.None);

        Assert.Empty(result);
    }

    private sealed class NotificacaoRepositoryFake(IReadOnlyList<NotificacaoEntity> notificacoes) : INotificacaoRepository
    {
        public DestinatarioNotificacao? DestinatarioConsultado { get; private set; }
        public Guid? ClienteIdConsultado { get; private set; }

        public Task<IReadOnlyList<NotificacaoEntity>> ListarAsync(
            DestinatarioNotificacao destinatario,
            Guid? clienteId,
            CancellationToken ct)
        {
            DestinatarioConsultado = destinatario;
            ClienteIdConsultado = clienteId;
            return Task.FromResult(notificacoes);
        }

        public Task AdicionarAsync(NotificacaoEntity notificacao, CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
