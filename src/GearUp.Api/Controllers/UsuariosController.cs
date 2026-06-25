using GearUp.Api.Authorization;
using GearUp.Api.Contracts.Usuarios;
using GearUp.Application.Autenticacao.GerenciarUsuarios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GearUp.Api.Controllers;

[ApiController, Route("api/usuarios"), Authorize(Roles = "Admin,Atendente")]
public sealed class UsuariosController(IGerenciarUsuariosUseCase usuarios) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Criar(CriarUsuarioRequest request, CancellationToken ct)
    { 
        var result = await usuarios.CriarAsync(
            new CriarUsuarioCommand(
                request.Usuario, 
                request.Senha, 
                request.Perfil, 
                request.ClienteId,
                User.ObterPerfil()), ct); 
        
        return Created($"/api/usuarios/{result.Id}", new { id = result.Id }); 
    }
}
