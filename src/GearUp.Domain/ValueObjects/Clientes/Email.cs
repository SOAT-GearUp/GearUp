using System.Net.Mail;

namespace GearUp.Domain.ValueObjects.Clientes;

public sealed record Email
{
    private Email(string endereco)
    {
        Endereco = endereco;
    }

    public string Endereco { get; }

    public static Email Criar(string valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            throw new ArgumentException("O e-mail é obrigatório.", nameof(valor));
        }

        var endereco = valor.Trim().ToLowerInvariant();

        if (endereco.Length > 254
            || !MailAddress.TryCreate(endereco, out var email)
            || !string.Equals(email.Address, endereco, StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("O e-mail informado é inválido.", nameof(valor));
        }

        return new Email(endereco);
    }
}
