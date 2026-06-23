using GearUp.Application.Notificacoes.Common.Interfaces;

namespace GearUp.Application.Notificacoes.Listar
{
    internal sealed class ListarNotificacaoUseCase(INotificacaoRepository notificacaoRepository) : IListarNotificacaoUseCase
    {
        public async Task<IReadOnlyList<ListarNotificaoResult>> ListarNotificacoesAsync(ListarNotificaoCommand command, CancellationToken ct)
        {
            var notificacoes = await notificacaoRepository.ListarAsync(command.Destinatario, command.ClienteId, ct);

            return notificacoes
                .Select(notificacao => new ListarNotificaoResult(
                    notificacao.Id,
                    notificacao.OrdemServicoId,
                    notificacao.ClienteId,
                    notificacao.Destinatario,
                    notificacao.Mensagem,
                    notificacao.CriadaEm,
                    notificacao.LidaEm))
                .ToList();
        }
    }
}
