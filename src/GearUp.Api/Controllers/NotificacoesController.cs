using GearUp.Application.Comunicacao.Notificacoes;
using GearUp.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GearUp.Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class NotificacoesController(
        IListarNotificacaoUseCase listarNotificacaoUseCase) : ControllerBase
    {
        [HttpGet]
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
                : throw new UnauthorizedAccessException("Usuario nao vinculado a um cliente.");
        }
    }
}
