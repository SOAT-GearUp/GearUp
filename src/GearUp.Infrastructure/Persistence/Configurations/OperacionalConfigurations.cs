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
        b.Property(x => x.Marca).HasMaxLength(80).IsRequired(); b.Property(x => x.Modelo).HasMaxLength(80).IsRequired();
        b.HasQueryFilter(x => x.Ativo);
    }
}

internal sealed class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> b)
    {
        b.ToTable("Usuarios"); 
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).ValueGeneratedNever();
        b.Property(x => x.NomeUsuario).HasMaxLength(100).IsUnicode(false).IsRequired(); 
        b.HasIndex(x => x.NomeUsuario).IsUnique();
        b.Property(x => x.SenhaHash).HasMaxLength(500).IsUnicode(false).IsRequired();
        b.HasOne<Cliente>().WithMany().HasForeignKey(x => x.ClienteId).OnDelete(DeleteBehavior.Restrict);
    }
}

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

internal sealed class MovimentacaoEstoqueConfiguration : IEntityTypeConfiguration<MovimentacaoEstoque>
{
    public void Configure(EntityTypeBuilder<MovimentacaoEstoque> b)
    {
        b.ToTable("MovimentacoesEstoque"); 
        b.HasKey(x => x.Id); 
        b.Property(x => x.Id).ValueGeneratedNever();
        b.Property(x => x.Quantidade).HasPrecision(18, 3);
        b.Property(x => x.Motivo).HasMaxLength(300).IsRequired(); 
        b.HasIndex(x => x.CriadoEm);
    }
}

internal sealed class OrdemServicoConfiguration : IEntityTypeConfiguration<OrdemServico>
{
    public void Configure(EntityTypeBuilder<OrdemServico> b)
    {
        b.ToTable("OrdensServico"); b.HasKey(x => x.Id); b.Property(x => x.Id).ValueGeneratedNever(); b.Property(x => x.SolicitacaoInicial).HasMaxLength(1000).IsRequired();
        b.Property(x => x.Diagnostico).HasMaxLength(4000); b.HasIndex(x => new { x.Status, x.Prioridade, x.CriadaEm });
        b.HasOne<Cliente>().WithMany().HasForeignKey(x => x.ClienteId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Veiculo>().WithMany().HasForeignKey(x => x.VeiculoId).OnDelete(DeleteBehavior.Restrict);
        b.HasMany(x => x.Orcamentos).WithOne().HasForeignKey(x => x.OrdemServicoId).OnDelete(DeleteBehavior.Cascade);
        b.HasMany(x => x.Historico).WithOne().HasForeignKey(x => x.OrdemServicoId).OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class OrcamentoConfiguration : IEntityTypeConfiguration<Orcamento>
{
    public void Configure(EntityTypeBuilder<Orcamento> b)
    {
        b.ToTable("Orcamentos"); 
        b.HasKey(x => x.Id); 
        b.Property(x => x.Id).ValueGeneratedNever();
        b.Ignore(x => x.ValorTotal); 
        b.HasIndex(x => new { x.OrdemServicoId, x.Versao }).IsUnique();
        b.HasMany(x => x.Itens).WithOne().HasForeignKey(x => x.OrcamentoId).OnDelete(DeleteBehavior.Cascade);
    }
}

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
