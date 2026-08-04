using GearUp.Domain.Entities;
using GearUp.Domain.ValueObjects;
using GearUp.Domain.ValueObjects.Clientes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GearUp.Infrastructure.Persistence.Configurations;

internal sealed class ClienteConfiguration : IEntityTypeConfiguration<Cliente>
{
    public void Configure(EntityTypeBuilder<Cliente> builder)
    {
        builder.ToTable("Clientes");

        builder.HasKey(cliente => cliente.Id);

        builder.Property(cliente => cliente.Id)
            .ValueGeneratedNever();

        builder.Property(cliente => cliente.Nome)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(cliente => cliente.Documento)
            .HasConversion(
                documento => documento.Numero,
                numero => Documento.Criar(numero))
            .HasMaxLength(14)
            .IsUnicode(false)
            .IsRequired();

        builder.HasIndex(cliente => cliente.Documento)
            .IsUnique()
            .HasDatabaseName("UX_Clientes_Documento");

        builder.Property(cliente => cliente.Email)
            .HasConversion(
                email => email.Endereco,
                endereco => Email.Criar(endereco))
            .HasMaxLength(254)
            .IsUnicode(false)
            .IsRequired();

        builder.Property(cliente => cliente.Telefone)
            .HasConversion(
                telefone => telefone.Numero,
                numero => Telefone.Criar(numero))
            .HasMaxLength(11)
            .IsUnicode(false)
            .IsRequired();

        builder.Property(cliente => cliente.Ativo)
            .IsRequired();

        builder.Property(cliente => cliente.CriadoEm)
            .IsRequired();

        builder.HasQueryFilter(cliente => cliente.Ativo);
    }
}
