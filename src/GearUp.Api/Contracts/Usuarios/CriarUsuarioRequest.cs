using GearUp.Domain.Entities;

using GearUp.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace GearUp.Api.Contracts.Usuarios
{
    public sealed record CriarUsuarioRequest(
        [Required] string Usuario,
        [Required] string Senha, 
        [Required] PerfilUsuario Perfil, 
        Guid? ClienteId);
}
