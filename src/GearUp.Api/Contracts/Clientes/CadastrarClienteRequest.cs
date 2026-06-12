using System.ComponentModel.DataAnnotations;

namespace GearUp.Api.Contracts.Clientes;

public sealed record CadastrarClienteRequest(
    [Required, StringLength(150, MinimumLength = 3)] string Nome,
    [Required] string Documento,
    [Required, EmailAddress] string Email,
    [Required] string Telefone);
