using GearUp.Domain.Entities;

namespace GearUp.Application.Estoque.Common.Interfaces;

public interface IEstoqueRepository
{
    Task AdicionarAsync(Estoque item, CancellationToken cancellationToken);
    Task<Estoque?> ObterAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<Estoque>> ListarAsync(CancellationToken cancellationToken);
}
