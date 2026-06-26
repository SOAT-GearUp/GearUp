namespace GearUp.Application.Comunicacao.Notificacoes
{
    public interface IListarNotificacaoUseCase
    {
        Task<IReadOnlyList<ListarNotificaoResult>> ListarNotificacoesAsync(ListarNotificaoCommand command, CancellationToken ct);
    }
}
