using GearUp.Api.Contracts.Clientes.Veiculos.Atualizar;
using GearUp.Api.Contracts.Clientes.Veiculos.Cadastrar;
using GearUp.Application.Clientes.Veiculos.Atualizar;
using GearUp.Application.Clientes.Veiculos.Cadastrar;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GearUp.Api.Controllers
{
    [Route("api/clientes/{clienteId:guid}/veiculos")]
    [ApiController]
    public class VeiculosController(
        ICadastrarVeiculoUseCase cadastrarVeiculoUseCase,
        IAtualizarVeiculoUseCase atualizarVeiculoUseCase) : ControllerBase
    {

        [Authorize(Roles = "Atendente"), HttpPost]
        [ProducesResponseType<CadastrarVeiculoResult>(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> AdicionarVeiculo(Guid clienteId, CadastrarVeiculoRequest request, CancellationToken ct)
        {
            var command = new CadastrarVeiculoCommand(
                clienteId,
                request.Placa,
                request.Marca,
                request.Modelo,
                request.Ano);

            var veiculo = await cadastrarVeiculoUseCase.CadastrarVeiculoAsync(command, ct);

            return Created($"/api/clientes/{clienteId}/veiculos/{veiculo.VeiculoId}", veiculo);
        }

        [Authorize(Roles = "Atendente"), HttpPut("{veiculoId:guid}")]
        [ProducesResponseType<CadastrarVeiculoResult>(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> AtualizarVeiculo(Guid clienteId, Guid veiculoId, AtualizarVeiculoRequest request, CancellationToken ct)
        {
            var command = new AtualizarVeiculoCommand(
                clienteId,
                veiculoId,
                request.Placa,
                request.Marca,
                request.Modelo,
                request.Ano);

            await atualizarVeiculoUseCase.AtualizarVeiculoAsync(command, ct);

            return NoContent();
        }
    }
}
