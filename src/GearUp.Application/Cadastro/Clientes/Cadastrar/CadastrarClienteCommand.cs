namespace GearUp.Application.Cadastro.Clientes.Cadastrar;

public sealed record CadastrarClienteCommand(string Nome, string Documento, string Email, string Telefone);
