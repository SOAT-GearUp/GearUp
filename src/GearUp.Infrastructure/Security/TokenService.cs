using GearUp.Application.Autenticacao.Autenticar;
using GearUp.Application.Autenticacao.Common;
using GearUp.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace GearUp.Infrastructure.Security;

internal sealed class TokenService(IConfiguration configuration) : ITokenService
{
    public TokenResult Gerar(Usuario usuario)
    {
        var chave = ObterChaveJwt();
        
        var emissor = configuration["Jwt:Issuer"] ?? "GearUp"; var audiencia = configuration["Jwt:Audience"] ?? "GearUp.Clients";
        
        var minutos = int.TryParse(configuration["Jwt:ExpirationMinutes"], out var configurado) ? configurado : 60;
        
        var expiraEm = DateTimeOffset.UtcNow.AddMinutes(minutos);

        var claims = new List<Claim> { 
            new(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()), 
            new(ClaimTypes.Name, usuario.NomeUsuario), 
            new(ClaimTypes.Role, usuario.Perfil.ToString()) 
        };
        
        if (usuario.ClienteId.HasValue) 
            claims.Add(new("cliente_id", usuario.ClienteId.Value.ToString()));

        var token = new JwtSecurityToken(
            emissor, 
            audiencia, 
            claims, 
            expires: expiraEm.UtcDateTime,
            signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(chave)), SecurityAlgorithms.HmacSha256));
        
        return new TokenResult(new JwtSecurityTokenHandler().WriteToken(token), expiraEm);
    }

    private string ObterChaveJwt()
    {
        var chave = configuration["Jwt:Key"];

        if (string.IsNullOrWhiteSpace(chave))
            throw new InvalidOperationException("Jwt:Key não configurada.");

        if (Encoding.UTF8.GetByteCount(chave) < 32)
            throw new InvalidOperationException("Jwt:Key deve possuir pelo menos 32 bytes.");

        return chave;
    }
}
