namespace GearUp.Domain.ValueObjects;

public sealed record Telefone
{
    private Telefone(string numero)
    {
        Numero = numero;
    }

    public string Numero { get; }

    public static Telefone Criar(string valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            throw new ArgumentException("O telefone é obrigatório.", nameof(valor));
        }

        var numero = new string(valor.Where(char.IsDigit).ToArray());

        if (numero.Length is not (10 or 11))
        {
            throw new ArgumentException(
                "O telefone deve conter DDD e possuir 10 ou 11 dígitos.",
                nameof(valor));
        }

        return new Telefone(numero);
    }
}
