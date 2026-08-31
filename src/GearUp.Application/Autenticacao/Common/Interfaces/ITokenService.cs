using GearUp.Application.Autenticacao.Autenticar;
using GearUp.Domain.Entities;

namespace GearUp.Application.Autenticacao.Common.Interfaces
{
    public interface ITokenService
    {
        TokenResult Gerar(Usuario usuario);
    }
}
