using GearUp.Domain.Entities;

namespace GearUp.Application.Autenticacao.GerenciarUsuarios
{
    public interface IGerenciarUsuariosUseCase
    {
        Task<CriarUsuarioResult> CriarAsync(CriarUsuarioCommand command, CancellationToken ct);
    }
}
