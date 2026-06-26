using System.ComponentModel.DataAnnotations;

namespace GearUp.Api.Contracts.Clientes.Veiculos.Atualizar;

public sealed record AtualizarVeiculoRequest(
    [Required] string Placa,
    [Required] string Marca,
    [Required] string Modelo,
    [Range(1900, 2100)][Required] int Ano);
