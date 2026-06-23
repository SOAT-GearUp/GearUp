namespace GearUp.Application.Autenticacao.Autenticar
{
    public sealed record TokenResult(string AccessToken, DateTimeOffset ExpiraEm);
}
