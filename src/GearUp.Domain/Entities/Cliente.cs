using GearUp.Domain.ValueObjects;

namespace GearUp.Domain.Entities;

public sealed class Cliente
{
    private readonly List<Veiculo> _veiculos = [];

    private Cliente()
    {
    }

    private Cliente(
        Guid id,
        string nome,
        Documento documento,
        Email email,
        Telefone telefone)
    {
        Id = id;
        Nome = nome;
        Documento = documento;
        Email = email;
        Telefone = telefone;
        Ativo = true;
        CriadoEm = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }

    public string Nome { get; private set; } = string.Empty;

    public Documento Documento { get; private set; } = null!;

    public Email Email { get; private set; } = null!;

    public Telefone Telefone { get; private set; } = null!;

    public bool Ativo { get; private set; }

    public DateTimeOffset CriadoEm { get; private set; }

    public DateTimeOffset? ExcluidoEm { get; private set; }

    public IReadOnlyCollection<Veiculo> Veiculos => _veiculos.AsReadOnly();

    public static Cliente Criar(
        string nome,
        string documento,
        string email,
        string telefone)
    {
        var nomeNormalizado = NormalizarNome(nome);

        return new Cliente(
            Guid.NewGuid(),
            nomeNormalizado,
            Documento.Criar(documento),
            Email.Criar(email),
            Telefone.Criar(telefone));
    }

    public void Excluir()
    {
        if (!Ativo)
        {
            return;
        }

        Ativo = false;
        ExcluidoEm = DateTimeOffset.UtcNow;
    }

    public void Atualizar(string nome, string email, string telefone)
    {
        Nome = NormalizarNome(nome);
        Email = Email.Criar(email);
        Telefone = Telefone.Criar(telefone);
    }

    public Veiculo AdicionarVeiculo(string placa, string marca, string modelo, int ano)
    {
        if (_veiculos.Any(veiculo => veiculo.Placa == placa.Replace("-", string.Empty).ToUpperInvariant()))
            throw new ArgumentException("Já existe um veículo com essa placa para o cliente.", nameof(placa));

        var veiculo = Veiculo.Criar(Id, placa, marca, modelo, ano);
        _veiculos.Add(veiculo);
        return veiculo;
    }

    private static string NormalizarNome(string nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
        {
            throw new ArgumentException("O nome do cliente é obrigatório.", nameof(nome));
        }

        var nomeNormalizado = nome.Trim();

        if (nomeNormalizado.Length is < 3 or > 150)
        {
            throw new ArgumentException(
                "O nome do cliente deve possuir entre 3 e 150 caracteres.",
                nameof(nome));
        }

        return nomeNormalizado;
    }
}
