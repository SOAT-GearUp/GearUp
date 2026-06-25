using GearUp.Application.Autenticacao.Common;
using GearUp.Domain.Entities;
using GearUp.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GearUp.Infrastructure.Persistence;

public sealed class DatabaseInitializer(GearUpDbContext db, IPasswordHasher hasher, IConfiguration configuration, ILogger<DatabaseInitializer> logger)
{
    public async Task InitializeAsync(CancellationToken ct = default)
    {
        // para que migrações e seed sejam reexecutados em falhas transitórias.
        var strategy = db.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(async () =>
        {
            await db.Database.MigrateAsync(ct);

            if (await db.Usuarios.AnyAsync(ct)) return;

            var adminUsuario = configuration["Seed:AdminUser"];
            var adminSenha = configuration["Seed:AdminPassword"];
            if (string.IsNullOrWhiteSpace(adminUsuario) || string.IsNullOrWhiteSpace(adminSenha))
            {
                logger.LogWarning(
                    "Seed do usuário admin ignorado: configure Seed:AdminUser e Seed:AdminPassword (ex.: SEED_ADMIN_USER e SEED_ADMIN_PASSWORD no .env).");
                return;
            }

            db.Usuarios.Add(Usuario.Criar(
                adminUsuario.Trim(),
                hasher.CriarHash(adminSenha),
                PerfilUsuario.Admin));
            await db.SaveChangesAsync(ct);
            logger.LogInformation("Usuário admin inicial criado ({Usuario}).", adminUsuario.Trim().ToLowerInvariant());
        });
    }
}
