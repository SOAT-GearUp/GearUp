namespace GearUp.Application.Atendimento.Clientes.Veiculos.Cadastrar;

public sealed record CadastrarVeiculoCommand(Guid ClienteId, string Placa, string Marca, string Modelo, int Ano);
