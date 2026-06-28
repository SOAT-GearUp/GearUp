using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using GearUp.Domain.Enums;
using Microsoft.IdentityModel.Tokens;

namespace GearUp.Api.UnitTests.Security;

public sealed class JwtRoleClaimTests
{
    private const string JwtKey = "gearup-test-jwt-key-with-32bytes!!";

    [Fact]
    public void TokenComPerfilMecanico_ComConfiguracaoAtualDaApi_DeveSerReconhecidoComoMecanico()
    {
        var tokenString = GerarTokenComoApi(PerfilUsuario.Mecanico);
        var principal = ValidarTokenComoApi(tokenString);

        var claims = string.Join(", ", principal.Claims.Select(c => $"{c.Type}={c.Value}"));

        Assert.True(
            principal.IsInRole("Mecanico"),
            $"IsInRole('Mecanico') falhou. Claims no principal: {claims}");
    }

    private static string GerarTokenComoApi(PerfilUsuario perfil)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, Guid.NewGuid().ToString()),
            new(ClaimTypes.Name, "mecanico"),
            new(ClaimTypes.Role, perfil.ToString())
        };

        var token = new JwtSecurityToken(
            "GearUp",
            "GearUp.Clients",
            claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtKey)),
                SecurityAlgorithms.HmacSha256));

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static ClaimsPrincipal ValidarTokenComoApi(string tokenString)
    {
        var parameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = "GearUp",
            ValidAudience = "GearUp.Clients",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtKey)),
            ClockSkew = TimeSpan.FromSeconds(30)
        };

        return new JwtSecurityTokenHandler().ValidateToken(tokenString, parameters, out _);
    }
}
