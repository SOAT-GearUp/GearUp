using GearUp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GearUp.Infrastructure.Persistence.Configurations;

internal sealed class OrcamentoConfiguration : IEntityTypeConfiguration<Orcamento>
{
    public void Configure(EntityTypeBuilder<Orcamento> b)
    {
        b.ToTable("Orcamentos", t =>
            t.HasCheckConstraint("CK_Orcamentos_Versao", "\"Versao\" > 0"));
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).ValueGeneratedNever();
        b.Ignore(x => x.ValorTotal);
        b.HasIndex(x => new { x.OrdemServicoId, x.Versao }).IsUnique();
        b.HasOne<OrdemServico>().WithMany().HasForeignKey(x => x.OrdemServicoId).OnDelete(DeleteBehavior.Cascade);
        b.HasMany(x => x.Itens).WithOne().HasForeignKey(x => x.OrcamentoId).OnDelete(DeleteBehavior.Cascade);
    }
}
