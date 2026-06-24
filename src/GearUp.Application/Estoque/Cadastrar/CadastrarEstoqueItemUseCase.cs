using GearUp.Application.Common.Interfaces;
using GearUp.Application.Estoque.Common.Interfaces;
using GearUp.Domain.Entities;

namespace GearUp.Application.Estoque.Cadastrar
{
    internal sealed class CadastrarEstoqueItemUseCase(IEstoqueRepository estoqueRepository, IUnitOfWork unitOfWork) : ICadastrarEstoqueItemUseCase
    {
        public async Task<CadastrarEstoqueItemResult> CadastrarAsync(CadastrarEstoqueItemCommand command, CancellationToken ct)
        {
            var item = GearUp.Domain.Entities.Estoque.Criar(command.Nome, command.Tipo, command.Preco, command.QuantidadeInicial);

            await estoqueRepository.AdicionarAsync(item, ct);

            await unitOfWork.SaveChangesAsync(ct);

            return new CadastrarEstoqueItemResult(item.Id);
        }
    }
}
