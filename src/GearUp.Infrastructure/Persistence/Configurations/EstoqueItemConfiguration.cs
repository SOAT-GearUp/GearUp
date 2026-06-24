using GearUp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GearUp.Infrastructure.Persistence.Configurations;

internal sealed class EstoqueItemConfiguration : IEntityTypeConfiguration<EstoqueItem>
{
    public void Configure(EntityTypeBuilder<EstoqueItem> b)
    {
        b.ToTable("EstoqueItens");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).ValueGeneratedNever();
        b.Property(x => x.Nome).HasMaxLength(150).IsRequired();
        b.Property(x => x.PrecoUnitario).HasPrecision(18, 2);
        b.Property(x => x.QuantidadeDisponivel).HasPrecision(18, 3);
        b.HasMany(x => x.Movimentacoes).WithOne().HasForeignKey(x => x.EstoqueItemId).OnDelete(DeleteBehavior.Cascade);
    }
}
