using GearUp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GearUp.Infrastructure.Persistence.Configurations;

internal sealed class MovimentacaoEstoqueConfiguration : IEntityTypeConfiguration<MovimentacaoEstoque>
{
    public void Configure(EntityTypeBuilder<MovimentacaoEstoque> b)
    {
        b.ToTable("MovimentacoesEstoque", t =>
            t.HasCheckConstraint("CK_MovimentacoesEstoque_Quantidade", "\"Quantidade\" > 0"));
        // Rastreia as peças consumidas por uma OS (referência entre contextos,
        // sem FK: Estoque não depende do ciclo de vida da OrdemServico).
        b.HasIndex(x => x.OrdemServicoId);
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).ValueGeneratedNever();
        b.Property(x => x.Quantidade).HasPrecision(18, 3);
        b.Property(x => x.Motivo).HasMaxLength(300).IsRequired();
        b.HasIndex(x => x.CriadoEm);
    }
}
