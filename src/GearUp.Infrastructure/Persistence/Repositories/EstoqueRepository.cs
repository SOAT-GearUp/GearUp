using GearUp.Application.Estoque.Common.Interfaces;
using GearUp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GearUp.Infrastructure.Persistence.Repositories
{
    internal sealed class EstoqueRepository(GearUpDbContext db) : IEstoqueRepository
    {
        public async Task AdicionarAsync(Estoque item, CancellationToken cancellationToken)
        {
            await db.EstoqueItens.AddAsync(item, cancellationToken);
        }

        public Task<Estoque?> ObterAsync(Guid id, CancellationToken cancellationToken)
        {
            return db.EstoqueItens.Include(x => x.Movimentacoes).SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        public async Task<IReadOnlyList<Estoque>> ListarAsync(CancellationToken cancellationToken)
        {
            return await db.EstoqueItens.AsNoTracking().Include(x => x.Movimentacoes).OrderBy(x => x.Nome).ToListAsync(cancellationToken);
        }
    }
}
