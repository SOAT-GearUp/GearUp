using GearUp.Application.Notificacoes.Common.Interfaces;

namespace GearUp.Application.Notificacoes.Listar
{
    internal sealed class ListarNotificacaoUseCase(IComunicacaoRepository comunicacaoRepository) : IListarNotificacaoUseCase
    {
        public async Task<IReadOnlyList<ListarNotificaoResult>> ListarNotificacoesAsync(ListarNotificaoCommand command, CancellationToken ct)
        {
            var comunicacoes = await comunicacaoRepository.ListarAsync(command.Destinatario, command.ClienteId, ct);

            return comunicacoes
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
