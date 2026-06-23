using GearUp.Domain.Entities;

using GearUp.Domain.Enums;

namespace GearUp.Api.Contracts.Usuarios
{
    public sealed record CriarUsuarioRequest(
        string Usuario, 
        string Senha, 
        PerfilUsuario Perfil, 
        Guid? ClienteId);
}
