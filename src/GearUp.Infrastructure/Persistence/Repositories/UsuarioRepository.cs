using GearUp.Application.Autenticacao.Common.Interfaces;
using GearUp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GearUp.Infrastructure.Persistence.Repositories
{
    internal sealed class UsuarioRepository(GearUpDbContext db) : IUsuarioRepository
    {
        public Task<Usuario?> ObterPorNomeAsync(string nomeUsuario, CancellationToken cancellationToken)
        {
            return db.Usuarios
                .AsNoTracking()
                .SingleOrDefaultAsync(x => x.NomeUsuario == nomeUsuario, cancellationToken);
        }

        public Task<bool> ExisteAsync(string nomeUsuario, CancellationToken cancellationToken)
        {
            return db.Usuarios.AnyAsync(x => x.NomeUsuario == nomeUsuario, cancellationToken);
        }

        public async Task AdicionarAsync(Usuario usuario, CancellationToken cancellationToken)
        {
            await db.Usuarios.AddAsync(usuario, cancellationToken);
        }
    }
}
