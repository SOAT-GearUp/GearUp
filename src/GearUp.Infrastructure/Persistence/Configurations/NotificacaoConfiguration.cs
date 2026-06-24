using GearUp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GearUp.Infrastructure.Persistence.Configurations;

internal sealed class NotificacaoConfiguration : IEntityTypeConfiguration<Notificacao>
{
    public void Configure(EntityTypeBuilder<Notificacao> b)
    {
        b.ToTable("Notificacoes");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).ValueGeneratedNever();
        b.Property(x => x.Mensagem).HasMaxLength(500);
        b.HasIndex(x => new { x.Destinatario, x.LidaEm });
        b.HasOne<OrdemServico>().WithMany().HasForeignKey(x => x.OrdemServicoId).OnDelete(DeleteBehavior.Cascade);
    }
}
