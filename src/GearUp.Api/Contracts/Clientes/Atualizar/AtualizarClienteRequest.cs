using System.ComponentModel.DataAnnotations;

namespace GearUp.Api.Contracts.Clientes.Atualizar;

public sealed record AtualizarClienteRequest(
    [Required, StringLength(150, MinimumLength = 3)] string Nome,
    [Required, EmailAddress] string Email,
    [Required] string Telefone);
