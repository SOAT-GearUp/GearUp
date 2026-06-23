namespace GearUp.Application.Notificacoes.Listar
{
    public interface IListarNotificacaoUseCase
    {
        Task<IReadOnlyList<ListarNotificaoResult>> ListarNotificacoesAsync(ListarNotificaoCommand command, CancellationToken ct);
    }
}
