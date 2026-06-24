using GearUp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GearUp.Infrastructure.Persistence.Configurations;

internal sealed class VeiculoConfiguration : IEntityTypeConfiguration<Veiculo>
{
    public void Configure(EntityTypeBuilder<Veiculo> b)
    {
        b.ToTable("Veiculos");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).ValueGeneratedNever();
        b.Property(x => x.Placa).HasMaxLength(7).IsUnicode(false).IsRequired();
        b.HasIndex(x => x.Placa).IsUnique();
        b.Property(x => x.Marca).HasMaxLength(80).IsRequired();
        b.Property(x => x.Modelo).HasMaxLength(80).IsRequired();
        b.HasQueryFilter(x => x.Ativo);
    }
}
