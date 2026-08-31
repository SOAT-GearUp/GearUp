namespace GearUp.Application.Autenticacao.Common.Interfaces
{
    public interface IPasswordHasher
    {
        string CriarHash(string senha);
        bool Verificar(string senha, string hash);
    }
}
