using System.ComponentModel.DataAnnotations;

namespace GearUp.Api.Contracts.Clientes.Veiculos.Cadastrar;

public sealed record CadastrarVeiculoRequest(
    [Required] string Placa,
    [Required] string Marca,
    [Required] string Modelo,
    [Range(1900, 2100)][Required] int Ano);
