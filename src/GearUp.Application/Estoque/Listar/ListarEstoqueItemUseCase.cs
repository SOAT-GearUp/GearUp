using GearUp.Application.Estoque.Common.Interfaces;

namespace GearUp.Application.Estoque.Listar
{
    internal sealed class ListarEstoqueItemUseCase(IEstoqueRepository estoqueRepository) : IListarEstoqueItemUseCase
    {
        public async Task<IReadOnlyList<ListarEstoqueItemResult>> ListarAsync(CancellationToken ct)
        {
            var itens = await estoqueRepository.ListarAsync(ct);

            return itens.Select(i => new ListarEstoqueItemResult(
                i.Id,
                i.Nome,
                i.Tipo.ToString(),
                i.QuantidadeDisponivel,
                i.PrecoUnitario
            )).ToList();
        }
    }
}
