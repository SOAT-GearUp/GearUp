using GearUp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GearUp.Infrastructure.Persistence.Configurations;

internal sealed class ItemOrcamentoConfiguration : IEntityTypeConfiguration<ItemOrcamento>
{
    public void Configure(EntityTypeBuilder<ItemOrcamento> b)
    {
        b.ToTable("ItensOrcamento");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).ValueGeneratedNever();
        b.Property(x => x.Descricao).HasMaxLength(300).IsRequired();
        b.Property(x => x.Quantidade).HasPrecision(18, 3);
        b.Property(x => x.ValorUnitario).HasPrecision(18, 2);
        b.Ignore(x => x.ValorTotal);
    }
}
