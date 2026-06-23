using GearUp.Application.Autenticacao.Common;
using GearUp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GearUp.Infrastructure.Persistence.Repositories
{
    internal sealed class UsuarioRepository(GearUpDbContext db) : IUsuarioRepository
    {
        public Task<Usuario?> ObterPorNomeAsync(string nomeUsuario, CancellationToken ct)
        {
            return db.Usuarios
                .AsNoTracking()
                .SingleOrDefaultAsync(x => x.NomeUsuario == nomeUsuario, ct);
        }

        public Task<bool> ExisteAsync(string nomeUsuario, CancellationToken ct)
        {
            return db.Usuarios.AnyAsync(x => x.NomeUsuario == nomeUsuario, ct);
        }

        public async Task AdicionarAsync(Usuario usuario, CancellationToken ct)
        {
            await db.Usuarios.AddAsync(usuario, ct);
        }
    }
}
