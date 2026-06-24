using GearUp.Application.Atendimento.Comum;
using GearUp.Application.Atendimento.Comum.Interfaces;
using GearUp.Domain.Entities;

namespace GearUp.Application.Atendimento.Consultar;

internal sealed class ConsultarOrdemServicoUseCase(IOrdemServicoRepository ordemServicoRepository) : IConsultarOrdemServicoUseCase
{
    public async Task<ConsultarOrdemServicoResult> ObterAsync(ConsultarOrdemServicoCommand command, CancellationToken ct)
    {
        var ordem = await ordemServicoRepository.ObterAsync(command.Id, ct)
            ?? throw new RecursoNaoEncontradoException("OS_NAO_ENCONTRADA", "Ordem de serviço não encontrada.");

        return new ConsultarOrdemServicoResult(
            ordem.Id,
            ordem.ClienteId,
            ordem.VeiculoId,
            ordem.SolicitacaoInicial,
            ordem.Diagnostico,
            ordem.Status,
            ordem.Prioridade,
            ordem.Prazo,
            ordem.CriadaEm,
            ordem.IniciadaEm,
            ordem.FinalizadaEm,
            ordem.Orcamentos.Select(MapearOrcamento).ToList(),
            ordem.Historico
                .OrderBy(evento => evento.CriadoEm)
                .Select(evento => new HistoricoOrdemServicoResult(evento.Tipo, evento.Descricao, evento.CriadoEm))
                .ToList());
    }

    private static OrcamentoResult MapearOrcamento(Orcamento orcamento) =>
        new(orcamento.Id,
            orcamento.Versao,
            orcamento.Status,
            orcamento.ValorTotal,
            orcamento.CriadoEm,
            orcamento.DecididoEm,
            orcamento.Itens.Select(item =>
                new ItemOrcamentoResult(
                    item.Id,
                    item.Tipo,
                    item.Descricao,
                    item.Quantidade,
                    item.ValorUnitario,
                    item.ValorTotal,
                    item.EstoqueItemId))
                .ToList());
}
