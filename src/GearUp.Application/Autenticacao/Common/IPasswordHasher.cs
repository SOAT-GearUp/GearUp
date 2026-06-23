namespace GearUp.Application.Autenticacao.Common
{
    public interface IPasswordHasher
    {
        string CriarHash(string senha);
        bool Verificar(string senha, string hash);
    }
}
