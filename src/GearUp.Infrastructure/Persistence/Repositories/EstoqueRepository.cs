using GearUp.Application.Estoque.Common.Interfaces;
using GearUp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GearUp.Infrastructure.Persistence.Repositories
{

    internal sealed class EstoqueRepository(GearUpDbContext db) : IEstoqueRepository
    {
        public async Task AdicionarAsync(EstoqueItem item, CancellationToken ct)
        {
            await db.EstoqueItens.AddAsync(item, ct);
        }

        public Task<EstoqueItem?> ObterAsync(Guid id, CancellationToken ct)
        {
            return db.EstoqueItens.Include(x => x.Movimentacoes).SingleOrDefaultAsync(x => x.Id == id, ct);
        }

        public async Task<IReadOnlyList<EstoqueItem>> ListarAsync(CancellationToken ct)
        {
            return await db.EstoqueItens.AsNoTracking().Include(x => x.Movimentacoes).OrderBy(x => x.Nome).ToListAsync(ct);
        }
    }
}
