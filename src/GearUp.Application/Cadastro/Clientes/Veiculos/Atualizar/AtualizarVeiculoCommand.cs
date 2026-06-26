namespace GearUp.Application.Cadastro.Clientes.Veiculos.Atualizar;

public sealed record AtualizarVeiculoCommand(Guid ClienteId, Guid VeiculoId, string Placa, string Marca, string Modelo, int Ano);
