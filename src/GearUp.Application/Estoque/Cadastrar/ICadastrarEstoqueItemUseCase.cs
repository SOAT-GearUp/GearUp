using GearUp.Domain.Entities;
using GearUp.Domain.ValueObjects;

namespace GearUp.Application.Estoque.Cadastrar
{
    public interface ICadastrarEstoqueItemUseCase
    {
        Task<CadastrarEstoqueItemResult> CadastrarAsync(CadastrarEstoqueItemCommand command, CancellationToken ct);
    }
}
