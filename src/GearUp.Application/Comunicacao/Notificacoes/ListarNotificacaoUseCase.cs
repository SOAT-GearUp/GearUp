using GearUp.Application.Comunicacao.Common.Interfaces;

namespace GearUp.Application.Comunicacao.Notificacoes
{
    internal sealed class ListarNotificacaoUseCase(INotificacaoRepository notificacaoRepository) : IListarNotificacaoUseCase
    {
        public async Task<IReadOnlyList<ListarNotificaoResult>> ListarNotificacoesAsync(ListarNotificaoCommand command, CancellationToken ct)
        {
            var notificacoes = await notificacaoRepository.ListarAsync(command.Destinatario, command.ClienteId, ct);

            return notificacoes
                .Select(c => new ListarNotificaoResult(
                    c.Id,
                    c.OrdemServicoId,
                    c.ClienteId,
                    c.Destinatario,
                    c.Mensagem,
                    c.CriadaEm,
                    c.LidaEm))
                .ToList();
        }
    }
}
