using GearUp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GearUp.Infrastructure.Persistence.Configurations;

internal sealed class OrdemServicoConfiguration : IEntityTypeConfiguration<OrdemServico>
{
    public void Configure(EntityTypeBuilder<OrdemServico> b)
    {
        b.ToTable("OrdensServico");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).ValueGeneratedNever();
        b.Property(x => x.SolicitacaoInicial).HasMaxLength(1000).IsRequired();
        b.Property(x => x.Diagnostico).HasMaxLength(4000);
        b.HasIndex(x => new { x.Status, x.Prioridade, x.CriadaEm });
        // Listagem de OS do cliente autenticado por CPF, mais recentes primeiro.
        // Substitui o índice simples da FK ClienteId (mesmo prefixo).
        b.HasIndex(x => new { x.ClienteId, x.CriadaEm }).IsDescending(false, true);
        b.HasIndex(x => x.MecanicoId);
        b.HasOne<Cliente>().WithMany().HasForeignKey(x => x.ClienteId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Veiculo>().WithMany().HasForeignKey(x => x.VeiculoId).OnDelete(DeleteBehavior.Restrict);
        b.HasMany(x => x.Historico).WithOne().HasForeignKey(x => x.OrdemServicoId).OnDelete(DeleteBehavior.Cascade);
    }
}
