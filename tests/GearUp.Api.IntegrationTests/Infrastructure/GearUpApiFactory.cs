using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.Testing;
using GearUp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Testcontainers.PostgreSql;

namespace GearUp.Api.IntegrationTests.Infrastructure;

public sealed class GearUpApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public const string AdminUser = "admin";
    public const string AdminPassword = "GearUp@123";

    private const string JwtKey = "gearup-integration-tests-jwt-key-change-me";

    private readonly PostgreSqlContainer postgres = new PostgreSqlBuilder("postgres:16")
        .WithDatabase("GearUp")
        .WithUsername("gearup")
        .WithPassword("GearUp_Strong!Pass123")
        .Build();

    public GearUpApiFactory()
    {
        Environment.SetEnvironmentVariable("Jwt__Key", JwtKey);
        Environment.SetEnvironmentVariable("Jwt__Issuer", "GearUp");
        Environment.SetEnvironmentVariable("Jwt__Audience", "GearUp.Clients");
        Environment.SetEnvironmentVariable("Jwt__ExpirationMinutes", "60");
    }

    public async Task InitializeAsync()
    {
        await postgres.StartAsync();
    }

    public new async Task DisposeAsync()
    {
        await postgres.DisposeAsync();
        await base.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:GearUpDatabase"] = postgres.GetConnectionString(),
                ["Jwt:Key"] = JwtKey,
                ["Jwt:Issuer"] = "GearUp",
                ["Jwt:Audience"] = "GearUp.Clients",
                ["Jwt:ExpirationMinutes"] = "60",
                ["Seed:AdminUser"] = AdminUser,
                ["Seed:AdminPassword"] = AdminPassword
            });
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<GearUpDbContext>>();
            services.AddDbContext<GearUpDbContext>(options =>
                options.UseNpgsql(postgres.GetConnectionString(), npgsqlOptions => npgsqlOptions.EnableRetryOnFailure()));

            services.PostConfigureAll<JwtBearerOptions>(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
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
            });
        });
    }
}
