using System.Security.Claims;

namespace GearUp.Api.Authorization
{
    public static class ClaimsPrincipalExtensions
    {
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
