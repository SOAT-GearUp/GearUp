using System.Security.Cryptography;
using GearUp.Application.Autenticacao.Common.Interfaces;

namespace GearUp.Infrastructure.Security;

internal sealed class PasswordHasher : IPasswordHasher
{
    private const int Iteracoes = 210_000;

    public string CriarHash(string senha)
    {
        if (string.IsNullOrWhiteSpace(senha) || senha.Length < 8) 
            throw new ArgumentException("A senha deve possuir ao menos 8 caracteres.", nameof(senha));

        var salt = RandomNumberGenerator.GetBytes(16);

        var hash = Rfc2898DeriveBytes.Pbkdf2(senha, salt, Iteracoes, HashAlgorithmName.SHA256, 32);

        return $"PBKDF2-SHA256${Iteracoes}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
    }

    public bool Verificar(string senha, string hash)
    {
        var partes = hash.Split('$');

        if (partes.Length != 4 || !int.TryParse(partes[1], out var iteracoes)) return false;

        try
        {
            var salt = Convert.FromBase64String(partes[2]); 
            
            var esperado = Convert.FromBase64String(partes[3]);

            var calculado = Rfc2898DeriveBytes.Pbkdf2(senha, salt, iteracoes, HashAlgorithmName.SHA256, esperado.Length);

            return CryptographicOperations.FixedTimeEquals(calculado, esperado);
        }
        catch (FormatException) { return false; }
    }
}
