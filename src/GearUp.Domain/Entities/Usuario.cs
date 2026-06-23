using GearUp.Domain.Enums;

namespace GearUp.Domain.Entities;

public sealed class Usuario
{
    private Usuario() { }

    private Usuario(Guid id, string nomeUsuario, string senhaHash, PerfilUsuario perfil, Guid? clienteId)
    {
        Id = id;
        NomeUsuario = nomeUsuario;
        SenhaHash = senhaHash;
        Perfil = perfil;
        ClienteId = clienteId;
        Ativo = true;
    }

    public Guid Id { get; private set; }
    public string NomeUsuario { get; private set; } = string.Empty;
    public string SenhaHash { get; private set; } = string.Empty;
    public PerfilUsuario Perfil { get; private set; }
    public Guid? ClienteId { get; private set; }
    public bool Ativo { get; private set; }

    public static Usuario Criar(string nomeUsuario, string senhaHash, PerfilUsuario perfil, Guid? clienteId = null)
    {
        if (string.IsNullOrWhiteSpace(nomeUsuario) || string.IsNullOrWhiteSpace(senhaHash))
            throw new ArgumentException("Usuário e senha são obrigatórios.");

        if (perfil == PerfilUsuario.Cliente && clienteId is null)
            throw new ArgumentException("Usuário cliente deve estar vinculado a um cliente.");

        return new Usuario(Guid.NewGuid(), nomeUsuario.Trim().ToLowerInvariant(), senhaHash, perfil, clienteId);
    }
}
