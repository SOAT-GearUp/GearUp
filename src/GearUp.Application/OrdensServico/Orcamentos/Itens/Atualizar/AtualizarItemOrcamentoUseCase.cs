using GearUp.Application.Common.Interfaces;
using GearUp.Application.OrdensServico.Common.Interfaces;
using GearUp.Domain.ValueObjects.Orcamentos;

namespace GearUp.Application.OrdensServico.Orcamentos.Itens.Atualizar
{
    internal class AtualizarItemOrcamentoUseCase(IOrdemServicoRepository ordemServicoRepository, IUnitOfWork unitOfWork) : IAtualizarItemOrcamentoUseCase
    {
        public async Task AtualizarAsync(AtualizarItemOrcamentoCommand command, CancellationToken ct)
        {
            var os = await ordemServicoRepository.ObterAsync(command.OrdemServicoId, ct)
                ?? throw new RecursoNaoEncontradoException("OS_NAO_ENCONTRADA", "Ordem de serviço não encontrada.");

            var orcamento = os.Orcamentos.SingleOrDefault(x => x.Id == command.OrcamentoId)
                ?? throw new RecursoNaoEncontradoException("ORCAMENTO_NAO_ENCONTRADO", "Orçamento não encontrado.");

            var item = NovoItemOrcamento.Criar(
                command.Tipo,
                command.Descricao,
                command.Quantidade,
                command.ValorUnitario,
                command.EstoqueItemId);

            orcamento.AtualizarItem(command.ItemId, item);

            await unitOfWork.SaveChangesAsync(ct);
        }
    }
}
