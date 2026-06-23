using GearUp.Application.Common.Interfaces;
using GearUp.Application.Estoque.Common.Interfaces;
using GearUp.Application.OrdensServico.Common.Interfaces;
using GearUp.Domain.Enums;

namespace GearUp.Application.OrdensServico.Status.Alterar
{
    internal sealed class AlterarStatusUseCase(
        IOrdemServicoRepository ordemServicoRepository, 
        IEstoqueRepository estoqueRepository, 
        IUnitOfWork unitOfWork) : IAlterarStatusUseCase
    {
        public async Task AlterarAsync(AlterarStatusCommand command, CancellationToken ct)
        {
            var os = await ordemServicoRepository.ObterAsync(command.Id, ct)
                ?? throw new RecursoNaoEncontradoException("OS_NAO_ENCONTRADA", "Ordem de serviço não encontrada.");

            // Se a OS estiver entrando em execução, precisamos movimentar o estoque dos itens aprovados no orçamento
            if (command.Status == StatusOrdemServico.EmExecucao)
            {
                var aprovado = os.Orcamentos.Single(o => o.Status == StatusOrcamento.Aprovado);

                foreach (var item in aprovado.Itens.Where(i => i.EstoqueItemId.HasValue))
                {
                    var estoqueItem = await estoqueRepository.ObterAsync(item.EstoqueItemId!.Value, ct)
                        ?? throw new RecursoNaoEncontradoException("ITEM_ESTOQUE_NAO_ENCONTRADO", $"Item {item.Descricao} não encontrado.");

                    estoqueItem.Movimentar(TipoMovimentacaoEstoque.Saida, item.Quantidade, $"Consumo automático na OS {os.Id}", os.Id);
                }
            }

            os.AlterarStatus(command.Status); 
            
            await unitOfWork.SaveChangesAsync(ct);
        }
    }
}
