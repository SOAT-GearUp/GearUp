using GearUp.Application.Common.Interfaces;
using GearUp.Application.Estoque.Common.Interfaces;
using GearUp.Domain.ValueObjects;

namespace GearUp.Application.Estoque.Movimentar
{
    internal sealed class MovimentarEstoqueItemUseCase(IEstoqueRepository estoqueRepository, IUnitOfWork unitOfWork) : IMovimentarEstoqueItemUseCase
    {
        public async Task MovimentarAsync(MovimentarEstoqueItemCommand command, CancellationToken ct)
        {
            var item = await estoqueRepository.ObterAsync(command.Id, ct) 
                ?? throw new RecursoNaoEncontradoException("ITEM_ESTOQUE_NAO_ENCONTRADO", "Item de estoque não encontrado.");

            item.Movimentar(command.Tipo, command.Quantidade, command.Motivo); 
            
            await unitOfWork.SaveChangesAsync(ct);

        }
    }
}
