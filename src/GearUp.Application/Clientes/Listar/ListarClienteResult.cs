namespace GearUp.Application.Clientes.Listar
{
    public sealed record ListarClienteResult(
        Guid Id, 
        string Nome, 
        string Email, 
        string Telefone);
}
