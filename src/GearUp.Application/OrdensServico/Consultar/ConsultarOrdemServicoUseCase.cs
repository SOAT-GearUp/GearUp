using GearUp.Application.Common;
using GearUp.Application.OrdensServico.Common;
using GearUp.Application.OrdensServico.Common.Interfaces;
using GearUp.Domain.Entities;

namespace GearUp.Application.OrdensServico.Consultar
{
    internal sealed class ConsultarOrdemServicoUseCase(IOrdemServicoRepository ordemServicoRepository) : IConsultarOrdemServicoUseCase
    {
        public async Task<ConsultarOrdemServicoResult> ObterAsync(ConsultarOrdemServicoCommand command, CancellationToken cancellationToken)
        {
            var ordem = await ordemServicoRepository.ObterAsync(command.Id, cancellationToken)
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
                    .Select(evento => new HistoricoOrdemServicoResult(
                        evento.Tipo,
                        evento.Descricao,
                        evento.CriadoEm))
                    .ToList());
        }

        private static OrcamentoResult MapearOrcamento(
            Orcamento orcamento)
        {
            return new OrcamentoResult(
                orcamento.Id,
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

    }
}
