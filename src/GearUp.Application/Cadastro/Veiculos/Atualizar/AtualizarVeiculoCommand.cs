namespace GearUp.Application.Cadastro.Veiculos.Atualizar;

public sealed record AtualizarVeiculoCommand(Guid ClienteId, Guid VeiculoId, string Placa, string Marca, string Modelo, int Ano);
