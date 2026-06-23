using GearUp.Api.Contracts.Autenticacao;
using GearUp.Application.Autenticacao;
using GearUp.Application.Autenticacao.Autenticar;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GearUp.Api.Controllers;

[ApiController]
[Route("api/autenticacao")]
public sealed class AutenticacaoController(IAutenticarUsuarioUseCase autenticar) : ControllerBase
{
    [AllowAnonymous, HttpPost("login")]
    //[ValidateAntiForgeryToken]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Login(LoginRequest request, CancellationToken ct)
    {
        var token = await autenticar.LogarAsync(
            new LoginCommand(
                request.Usuario, 
                request.Senha), ct);

        return Ok(token);
    }

    [Authorize, HttpPost("logout")]
    public IActionResult Logout() => NoContent();
}
