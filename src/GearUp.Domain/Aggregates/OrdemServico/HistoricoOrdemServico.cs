namespace GearUp.Domain.Entities
{
    public sealed class HistoricoOrdemServico
    {
        private HistoricoOrdemServico() { }
        private HistoricoOrdemServico(Guid osId, string tipo, string descricao) { Id = Guid.NewGuid(); OrdemServicoId = osId; Tipo = tipo; Descricao = descricao; CriadoEm = DateTimeOffset.UtcNow; }
        public Guid Id { get; private set; }
        public Guid OrdemServicoId { get; private set; }
        public string Tipo { get; private set; } = string.Empty;
        public string Descricao { get; private set; } = string.Empty;
        public DateTimeOffset CriadoEm { get; private set; }
        internal static HistoricoOrdemServico Criar(Guid osId, string tipo, string descricao) => new(osId, tipo, descricao);
    }
}
