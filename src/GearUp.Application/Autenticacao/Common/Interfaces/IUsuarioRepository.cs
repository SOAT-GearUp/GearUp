using GearUp.Domain.Entities;

namespace GearUp.Application.Autenticacao.Common.Interfaces
{
    public interface IUsuarioRepository
    {
        Task<Usuario?> ObterPorNomeAsync(string nomeUsuario, CancellationToken cancellationToken);
        Task<bool> ExisteAsync(string nomeUsuario, CancellationToken cancellationToken);
        Task AdicionarAsync(Usuario usuario, CancellationToken cancellationToken);
    }
}
