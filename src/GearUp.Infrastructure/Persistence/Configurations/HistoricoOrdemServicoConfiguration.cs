using GearUp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GearUp.Infrastructure.Persistence.Configurations;

internal sealed class HistoricoOrdemServicoConfiguration : IEntityTypeConfiguration<HistoricoOrdemServico>
{
    public void Configure(EntityTypeBuilder<HistoricoOrdemServico> b)
    {
        b.ToTable("HistoricoOrdensServico");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).ValueGeneratedNever();
        b.Property(x => x.Tipo).HasMaxLength(80);
        b.Property(x => x.Descricao).HasMaxLength(500);
        b.HasIndex(x => x.CriadoEm);
    }
}
