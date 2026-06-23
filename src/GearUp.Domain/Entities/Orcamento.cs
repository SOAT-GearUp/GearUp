using GearUp.Domain.Common;
using GearUp.Domain.Common.Exceptions;
using GearUp.Domain.Enums;
using GearUp.Domain.ValueObjects.Orcamentos;

namespace GearUp.Domain.Entities
{
    public sealed class Orcamento
    {
        private readonly List<ItemOrcamento> _itens = [];
        private Orcamento() { }
        private Orcamento(Guid ordemServicoId, int versao) 
        { 
            Id = Guid.NewGuid(); 
            OrdemServicoId = ordemServicoId; 
            Versao = versao; 
            Status = StatusOrcamento.Pendente; 
            CriadoEm = DateTimeOffset.UtcNow; 
        }

        public Guid Id { get; private set; }
        public Guid OrdemServicoId { get; private set; }
        public int Versao { get; private set; }
        public StatusOrcamento Status { get; private set; }
        public DateTimeOffset CriadoEm { get; private set; }
        public DateTimeOffset? DecididoEm { get; private set; }
        public IReadOnlyCollection<ItemOrcamento> Itens => _itens.AsReadOnly();
        public decimal ValorTotal => _itens.Sum(item => item.ValorTotal);

        internal static Orcamento Criar(Guid osId, int versao, IEnumerable<NovoItemOrcamento> itens)
        {
            var orcamento = new Orcamento(osId, versao);

            foreach (var item in itens) 
                orcamento._itens.Add(ItemOrcamento.Criar(orcamento.Id, item));

            if (orcamento._itens.Count == 0) 
                throw new ArgumentException("O orçamento deve possuir ao menos um item.");

            return orcamento;
        }

        internal void Decidir(bool aprovado)
        {
            if (Status != StatusOrcamento.Pendente) 
                throw new RegraNegocioException("ORCAMENTO_JA_DECIDIDO", "Orçamento já foi decidido.");

            if (_itens.Count == 0) 
                throw new RegraNegocioException("ORCAMENTO_SEM_ITENS", "Orçamento deve possuir itens.");

            Status = aprovado ? StatusOrcamento.Aprovado : StatusOrcamento.Rejeitado; 
            
            DecididoEm = DateTimeOffset.UtcNow;
        }

        public ItemOrcamento AdicionarItem(NovoItemOrcamento item)
        {
            ExigirPendente(); 
            
            var novo = ItemOrcamento.Criar(Id, item); 
            
            _itens.Add(novo); 
            
            return novo;
        }

        public void AtualizarItem(Guid itemId, NovoItemOrcamento dados)
        {
            ExigirPendente(); 
            
            var item = _itens.SingleOrDefault(x => x.Id == itemId)
                ?? throw new RegraNegocioException("ITEM_ORCAMENTO_NAO_ENCONTRADO", "Item de orçamento não encontrado.");

            item.Atualizar(dados);
        }

        public void RemoverItem(Guid itemId)
        {
            ExigirPendente(); 
            
            var item = _itens.SingleOrDefault(x => x.Id == itemId)
                ?? throw new RegraNegocioException("ITEM_ORCAMENTO_NAO_ENCONTRADO", "Item de orçamento não encontrado.");

            _itens.Remove(item);
        }

        private void ExigirPendente()
        {
            if (Status != StatusOrcamento.Pendente) 
                throw new RegraNegocioException("ORCAMENTO_JA_DECIDIDO", "Somente orçamento pendente pode ser alterado.");
        }
    }

}
