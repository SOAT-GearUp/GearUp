using GearUp.Application.Common.Interfaces;
using GearUp.Application.OrdensServico.Common.Interfaces;
using GearUp.Domain.Common.Exceptions;
using GearUp.Domain.Entities;
using GearUp.Domain.ValueObjects.Orcamentos;

namespace GearUp.Application.OrdensServico.Orcamentos.Criar
{
    internal sealed class CriarOrcamentoUseCase(IOrdemServicoRepository ordemServicoRepository, IUnitOfWork unitOfWork) : ICriarOrcamentoUseCase
    {
        public async Task<CriarOrcamentoResult> CriarAsync(CriarOrcamentoCommand command, CancellationToken cancellationToken)
        {
            var ordemServico = await ordemServicoRepository.ObterAsync(command.OrdemServicoId, cancellationToken)
                ?? throw new RecursoNaoEncontradoException("OS_NAO_ENCONTRADA", "Ordem de serviço não encontrada.");

            var itens = command.Itens
                .Select(item => NovoItemOrcamento.Criar(
                    item.Tipo,
                    item.Descricao,
                    item.Quantidade,
                    item.ValorUnitario,
                    item.EstoqueItemId))
                .ToList();

            var orcamento = ordemServico.CriarOrcamento(itens);

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return new CriarOrcamentoResult(
                orcamento.Id,
                orcamento.Versao,
                orcamento.ValorTotal);
        }
    }
}
