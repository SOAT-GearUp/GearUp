using GearUp.Domain.Common;
using GearUp.Domain.Common.Exceptions;
using GearUp.Domain.DomainEvents.Estoque;
using GearUp.Domain.Enums;

namespace GearUp.Domain.Entities;

public sealed class Estoque : AggregateRoot
{
    private readonly List<MovimentacaoEstoque> _movimentacoes = [];
    private Estoque() { }

    private Estoque(string nome, TipoItemEstoque tipo, decimal precoUnitario, decimal quantidadeInicial)
    {
        Id = Guid.NewGuid();
        Nome = ValidarNome(nome);
        Tipo = tipo;
        DefinirPreco(precoUnitario);
        Ativo = true;

        if (quantidadeInicial > 0)
            Movimentar(TipoMovimentacaoEstoque.Entrada, quantidadeInicial, "Saldo inicial");
    }

    public Guid Id { get; private set; }
    public string Nome { get; private set; } = string.Empty;
    public TipoItemEstoque Tipo { get; private set; }
    public decimal PrecoUnitario { get; private set; }
    public decimal QuantidadeDisponivel { get; private set; }
    public bool Ativo { get; private set; }
    public IReadOnlyCollection<MovimentacaoEstoque> Movimentacoes => _movimentacoes.AsReadOnly();

    public static Estoque Criar(string nome, TipoItemEstoque tipo, decimal precoUnitario, decimal quantidadeInicial = 0)
    {
        if (quantidadeInicial < 0)
            throw new ArgumentException("A quantidade inicial não pode ser negativa.", nameof(quantidadeInicial));

        return new(nome, tipo, precoUnitario, quantidadeInicial);
    }

    public void Atualizar(string nome, decimal precoUnitario)
    {
        Nome = ValidarNome(nome);
        DefinirPreco(precoUnitario);
    }

    public void Movimentar(TipoMovimentacaoEstoque tipo, decimal quantidade, string motivo, Guid? ordemServicoId = null)
    {
        if (quantidade <= 0)
            throw new ArgumentException("A quantidade deve ser maior que zero.", nameof(quantidade));

        if (string.IsNullOrWhiteSpace(motivo))
            throw new ArgumentException("O motivo é obrigatório.", nameof(motivo));

        if (tipo == TipoMovimentacaoEstoque.Saida && QuantidadeDisponivel < quantidade)
            throw new RegraNegocioException("ESTOQUE_INSUFICIENTE", $"Estoque insuficiente para {Nome}.");

        QuantidadeDisponivel += tipo == TipoMovimentacaoEstoque.Entrada ? quantidade : -quantidade;

        _movimentacoes.Add(MovimentacaoEstoque.Criar(Id, tipo, quantidade, motivo.Trim(), ordemServicoId));
        AdicionarDomainEvent(new EstoqueItemMovimentadoDomainEvent(Id, Nome, tipo, quantidade, QuantidadeDisponivel, ordemServicoId, DateTimeOffset.UtcNow));
    }

    private static string ValidarNome(string nome)
    {
        return string.IsNullOrWhiteSpace(nome)
            ? throw new ArgumentException("O nome do item é obrigatório.", nameof(nome))
            : nome.Trim();
    }

    private void DefinirPreco(decimal preco)
    {
        PrecoUnitario = preco < 0
            ? throw new ArgumentException("O preço não pode ser negativo.", nameof(preco))
            : decimal.Round(preco, 2);
    }
}
