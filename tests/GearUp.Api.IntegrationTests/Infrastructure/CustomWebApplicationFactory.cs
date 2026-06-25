using System.Security.Claims;
using System.Text;
using GearUp.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Testcontainers.PostgreSql;

namespace GearUp.Api.IntegrationTests.Infrastructure;

/// <summary>
/// Fábrica de aplicação para testes de integração. Sobe um container PostgreSQL
/// dedicado via Testcontainers e injeta a configuração necessária (JWT e seed)
/// para que o <c>DatabaseInitializer</c> real crie o schema e os usuários de
/// desenvolvimento (atendente, auxiliar, mecanico).
/// </summary>
/// <remarks>
/// Cada classe de teste recebe sua própria instância (via <c>IClassFixture</c>) e,
/// portanto, seu próprio container — garantindo isolamento total entre classes.
/// </remarks>
public sealed class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    /// <summary>
    /// Senha aplicada a todos os usuários de seed. Reutilize nos logins dos testes.
    /// </summary>
    public const string SenhaSeed = "Teste@Integracao123";

    private const string JwtKey = "chave-de-teste-de-integracao-com-mais-de-32-chars";
    private const string JwtIssuer = "GearUp.IntegrationTests";
    private const string JwtAudience = "GearUp.IntegrationTests";

    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:16-alpine")
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = JwtKey,
                ["Jwt:Issuer"] = JwtIssuer,
                ["Jwt:Audience"] = JwtAudience,

                ["Seed:DevelopmentUsers"] = "true",
                ["Seed:DevelopmentPassword"] = SenhaSeed,
            });
        });

        // Program.cs captura Jwt:Key e a connection string no registro dos serviços,
        // antes dos callbacks de ConfigureAppConfiguration. Por isso sobrescrevemos
        // DbContext e JwtBearer aqui, apontando tudo ao container e às credenciais
        // de teste — isolado por factory, sem estado global.
        builder.ConfigureTestServices(services =>
        {
            RemoverRegistrosDbContext(services);

            services.AddDbContext<GearUpDbContext>(options =>
                options.UseNpgsql(
                    _container.GetConnectionString(),
                    npgsql => npgsql.EnableRetryOnFailure()));

            services.PostConfigure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                // O TokenService emite "role" no JWT; no .NET 8+ o bearer não remapeia
                // claims de entrada por padrão, e [Authorize(Roles=...)] retorna 401.
                options.MapInboundClaims = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = JwtIssuer,
                    ValidAudience = JwtAudience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtKey)),
                    RoleClaimType = ClaimTypes.Role,
                    ClockSkew = TimeSpan.FromSeconds(30),
                };
            });
        });
    }

    private static void RemoverRegistrosDbContext(IServiceCollection services)
    {
        var descritores = services
            .Where(d =>
                d.ServiceType == typeof(DbContextOptions<GearUpDbContext>)
                || d.ServiceType == typeof(DbContextOptions)
                || d.ServiceType == typeof(GearUpDbContext)
                // IDbContextOptionsConfiguration<TContext> é registrado pelo AddDbContext
                // no EF Core 9+; comparamos por nome para não depender do tipo público.
                || d.ServiceType.Name.StartsWith("IDbContextOptionsConfiguration", StringComparison.Ordinal))
            .ToList();

        foreach (var descritor in descritores)
            services.Remove(descritor);
    }

    async Task IAsyncLifetime.InitializeAsync() => await _container.StartAsync();

    async Task IAsyncLifetime.DisposeAsync() => await _container.DisposeAsync();
}
