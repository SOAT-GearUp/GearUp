using GearUp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GearUp.Infrastructure.Persistence.Configurations;

internal sealed class EstoqueConfiguration : IEntityTypeConfiguration<Estoque>
{
    public void Configure(EntityTypeBuilder<Estoque> b)
    {
        b.ToTable("EstoqueItens", t =>
        {
            // Saldo nunca negativo: última barreira contra baixa concorrente
            // que passe pela validação do agregado.
            t.HasCheckConstraint("CK_EstoqueItens_QuantidadeDisponivel", "\"QuantidadeDisponivel\" >= 0");
            t.HasCheckConstraint("CK_EstoqueItens_PrecoUnitario", "\"PrecoUnitario\" >= 0");
        });
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).ValueGeneratedNever();
        b.Property(x => x.Nome).HasMaxLength(150).IsRequired();
        b.Property(x => x.PrecoUnitario).HasPrecision(18, 2);
        b.Property(x => x.QuantidadeDisponivel).HasPrecision(18, 3);
        b.HasMany(x => x.Movimentacoes).WithOne().HasForeignKey(x => x.EstoqueItemId).OnDelete(DeleteBehavior.Cascade);
    }
}
