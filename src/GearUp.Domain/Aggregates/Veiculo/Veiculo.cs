using System.Text.RegularExpressions;
using GearUp.Domain.Common;

namespace GearUp.Domain.Entities;

public sealed partial class Veiculo : AggregateRoot
{
    private Veiculo() { }

    private Veiculo(Guid clienteId, string placa, string marca, string modelo, int ano)
    {
        Id = Guid.NewGuid();
        ClienteId = clienteId;
        Atualizar(placa, marca, modelo, ano);
        Ativo = true;
    }

    public Guid Id { get; private set; }
    public Guid ClienteId { get; private set; }
    public string Placa { get; private set; } = string.Empty;
    public string Marca { get; private set; } = string.Empty;
    public string Modelo { get; private set; } = string.Empty;
    public int Ano { get; private set; }
    public bool Ativo { get; private set; }

    public static Veiculo Criar(Guid clienteId, string placa, string marca, string modelo, int ano) =>
        new(clienteId, placa, marca, modelo, ano);

    public void Atualizar(string placa, string marca, string modelo, int ano)
    {
        var placaNormalizada = (placa ?? string.Empty).Replace("-", string.Empty).Trim().ToUpperInvariant();

        if (!PlacaRegex().IsMatch(placaNormalizada))
            throw new ArgumentException("A placa informada é inválida.", nameof(placa));

        if (string.IsNullOrWhiteSpace(marca) || string.IsNullOrWhiteSpace(modelo))
            throw new ArgumentException("Marca e modelo são obrigatórios.");

        if (ano is < 1900 || ano > 2100)
            throw new ArgumentException("O ano do veículo é inválido.", nameof(ano));

        Placa = placaNormalizada;
        Marca = marca.Trim();
        Modelo = modelo.Trim();
        Ano = ano;
    }

    public void Excluir() => Ativo = false;

    [GeneratedRegex("^[A-Z]{3}[0-9][A-Z0-9][0-9]{2}$")]
    private static partial Regex PlacaRegex();
}
