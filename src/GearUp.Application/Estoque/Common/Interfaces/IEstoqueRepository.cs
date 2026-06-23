using GearUp.Domain.Entities;

namespace GearUp.Application.Estoque.Common.Interfaces;

public interface IEstoqueRepository
{
    Task AdicionarAsync(EstoqueItem item, CancellationToken cancellationToken);
    Task<EstoqueItem?> ObterAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<EstoqueItem>> ListarAsync(CancellationToken cancellationToken);
}

