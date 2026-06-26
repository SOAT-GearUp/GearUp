using GearUp.Application.OrdemDeServico.Common;
using GearUp.Application.OrdemDeServico.Common.Interfaces;
using GearUp.Domain.Entities;

namespace GearUp.Application.OrdemDeServico.Ordens.Consultar;

internal sealed class ConsultarOrdemServicoUseCase(
    IOrdemServicoRepository ordemServicoRepository,
    IOrcamentoRepository orcamentoRepository) : IConsultarOrdemServicoUseCase
{
    public async Task<ConsultarOrdemServicoResult> ObterAsync(ConsultarOrdemServicoCommand command, CancellationToken ct)
    {
        var ordem = await ordemServicoRepository.ObterAsync(command.Id, ct)
            ?? throw new RecursoNaoEncontradoException("OS_NAO_ENCONTRADA", "Ordem de serviço não encontrada.");

        var orcamentos = await orcamentoRepository.ListarPorOrdemServicoAsync(ordem.Id, ct);

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
            orcamentos.Select(MapearOrcamento).ToList(),
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
