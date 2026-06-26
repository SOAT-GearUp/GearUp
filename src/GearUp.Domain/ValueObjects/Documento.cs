using GearUp.Domain.Enums;

namespace GearUp.Domain.ValueObjects;

public sealed record Documento
{
    private Documento(string numero, TipoDocumento tipo)
    {
        Numero = numero;
        Tipo = tipo;
    }

    public string Numero { get; }

    public TipoDocumento Tipo { get; }

    public static Documento Criar(string valor)
    {
        var numero = ApenasDigitos(valor);

        return numero.Length switch
        {
            11 when CpfValido(numero) => new Documento(numero, TipoDocumento.Cpf),
            14 when CnpjValido(numero) => new Documento(numero, TipoDocumento.Cnpj),
            11 => throw new ArgumentException("O CPF informado é inválido.", nameof(valor)),
            14 => throw new ArgumentException("O CNPJ informado é inválido.", nameof(valor)),
            _ => throw new ArgumentException(
                "O documento deve ser um CPF ou CNPJ válido.",
                nameof(valor))
        };
    }

    private static string ApenasDigitos(string valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            throw new ArgumentException("O documento é obrigatório.", nameof(valor));
        }

        return new string(valor.Where(char.IsDigit).ToArray());
    }

    private static bool CpfValido(string cpf)
    {
        if (TodosDigitosIguais(cpf))
        {
            return false;
        }

        var primeiroDigito = CalcularDigito(cpf.AsSpan(0, 9), 10);
        var segundoDigito = CalcularDigito(cpf.AsSpan(0, 10), 11);

        return cpf[9] - '0' == primeiroDigito
            && cpf[10] - '0' == segundoDigito;
    }

    private static bool CnpjValido(string cnpj)
    {
        if (TodosDigitosIguais(cnpj))
        {
            return false;
        }

        int[] pesosPrimeiro = [5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];
        int[] pesosSegundo = [6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];

        var primeiroDigito = CalcularDigito(cnpj.AsSpan(0, 12), pesosPrimeiro);
        var segundoDigito = CalcularDigito(cnpj.AsSpan(0, 13), pesosSegundo);

        return cnpj[12] - '0' == primeiroDigito
            && cnpj[13] - '0' == segundoDigito;
    }

    private static int CalcularDigito(ReadOnlySpan<char> numeros, int pesoInicial)
    {
        var soma = 0;

        for (var indice = 0; indice < numeros.Length; indice++)
        {
            soma += (numeros[indice] - '0') * (pesoInicial - indice);
        }

        var resto = soma % 11;
        return resto < 2 ? 0 : 11 - resto;
    }

    private static int CalcularDigito(
        ReadOnlySpan<char> numeros,
        int[] pesos)
    {
        var soma = 0;

        for (var indice = 0; indice < numeros.Length; indice++)
        {
            soma += (numeros[indice] - '0') * pesos[indice];
        }

        var resto = soma % 11;
        return resto < 2 ? 0 : 11 - resto;
    }

    private static bool TodosDigitosIguais(string numero) =>
        numero.All(digito => digito == numero[0]);
}
