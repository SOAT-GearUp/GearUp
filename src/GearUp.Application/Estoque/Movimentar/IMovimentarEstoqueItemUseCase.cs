using GearUp.Domain.ValueObjects;

namespace GearUp.Application.Estoque.Movimentar
{
    public interface IMovimentarEstoqueItemUseCase
    {
        Task MovimentarAsync(MovimentarEstoqueItemCommand command, CancellationToken ct);
    }
}
