namespace GearUp.Application.Estoque.Common.Interfaces;

public interface IEstoqueRepository
{
    Task AdicionarAsync(GearUp.Domain.Entities.Estoque item, CancellationToken cancellationToken);
    Task<GearUp.Domain.Entities.Estoque?> ObterAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<GearUp.Domain.Entities.Estoque>> ListarAsync(CancellationToken cancellationToken);
}
