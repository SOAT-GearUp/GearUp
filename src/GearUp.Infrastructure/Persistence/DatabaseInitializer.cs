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

            if (!bool.TryParse(configuration["Seed:DevelopmentUsers"], out var seedHabilitado) || !seedHabilitado) return;
            if (await db.Usuarios.AnyAsync(ct)) return;

            const string senhaDesenvolvimento = "GearUp@123";
            db.Usuarios.AddRange(
                Usuario.Criar("atendente", hasher.CriarHash(senhaDesenvolvimento), PerfilUsuario.Atendente),
                Usuario.Criar("auxiliar", hasher.CriarHash(senhaDesenvolvimento), PerfilUsuario.Auxiliar),
                Usuario.Criar("mecanico", hasher.CriarHash(senhaDesenvolvimento), PerfilUsuario.Mecanico));
            await db.SaveChangesAsync(ct);
            logger.LogWarning("Usuários de desenvolvimento criados. Altere a senha padrão antes de publicar.");
        });
    }
}
