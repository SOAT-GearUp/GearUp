using GearUp.Domain.ValueObjects;

namespace GearUp.Domain.Entities;

public sealed class Cliente
{
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
