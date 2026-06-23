using GearUp.Application.Notificacoes.Listar;
using GearUp.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GearUp.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificacoesController(
        IListarNotificacaoUseCase listarNotificacaoUseCase) : ControllerBase
    {
        [HttpGet("notificacoes")]
        public async Task<IActionResult> Notificacoes(CancellationToken ct)
        {
            var destinatario = User.IsInRole("Cliente")
                ? DestinatarioNotificacao.Cliente
                : DestinatarioNotificacao.Atendente;

            Guid? clienteId = User.IsInRole("Cliente")
                ? ObterClienteId()
                : null;

            return Ok(await listarNotificacaoUseCase.ListarNotificacoesAsync(
                new ListarNotificaoCommand(destinatario, clienteId), ct));
        }


        private Guid ObterClienteId()
        {
            return Guid.TryParse(User.FindFirstValue("cliente_id"), out var id)
                ? id
                : throw new UnauthorizedAccessException("Usuário não vinculado a um cliente.");
        }
    }
}
