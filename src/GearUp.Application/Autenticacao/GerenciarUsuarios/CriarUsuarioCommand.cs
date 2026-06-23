using GearUp.Domain.Entities;
using GearUp.Domain.Enums;

namespace GearUp.Application.Autenticacao.GerenciarUsuarios
{
    public sealed record CriarUsuarioCommand(
        string Usuario,
        string Senha,
        PerfilUsuario Perfil,
        Guid? ClienteId
    );
}
