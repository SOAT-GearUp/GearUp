using System.Security.Claims;
using GearUp.Domain.Enums;

namespace GearUp.Api.Authorization
{
    public static class ClaimsPrincipalExtensions
    {
        public static PerfilUsuario ObterPerfil(this ClaimsPrincipal user)
        {
            var role = user.FindFirstValue(ClaimTypes.Role);
            if (role is null || !Enum.TryParse(role, ignoreCase: true, out PerfilUsuario perfil) || !Enum.IsDefined(perfil))
                throw new UnauthorizedAccessException("Perfil do usuário autenticado inválido.");

            return perfil;
        }

        public static Guid? ObterClienteId(this ClaimsPrincipal user)
        {
            return Guid.TryParse(user.FindFirstValue("cliente_id"), out var clienteId)
                ? clienteId
                : null;
        }

        public static bool PodeAcessarOrdemServico(this ClaimsPrincipal user, Guid clienteIdDaOrdem)
        {
            if (!user.IsInRole("Cliente"))
            {
                return true;
            }

            var clienteId = user.ObterClienteId();

            return clienteId.HasValue
                && clienteId.Value == clienteIdDaOrdem;
        }
    }
}
