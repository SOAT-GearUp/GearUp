namespace GearUp.Application.Clientes.Atualizar
{
    public sealed record AtualizarClienteCommand(
        Guid Id,
        string Nome,
        string Email,
        string Telefone
    );
}
