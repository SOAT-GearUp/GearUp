using GearUp.Domain.Enums;

namespace GearUp.Domain.Entities
{
    public sealed class Notificacao
    {
        private Notificacao() { }
        private Notificacao(Guid osId, Guid clienteId, DestinatarioNotificacao destinatario, string mensagem)
        {
            Id = Guid.NewGuid();
            OrdemServicoId = osId;
            ClienteId = clienteId;
            Destinatario = destinatario;
            Mensagem = mensagem;
            CriadaEm = DateTimeOffset.UtcNow;
        }

        public Guid Id { get; private set; }
        public Guid OrdemServicoId { get; private set; }
        public Guid ClienteId { get; private set; }
        public DestinatarioNotificacao Destinatario { get; private set; }
        public string Mensagem { get; private set; } = string.Empty;
        public DateTimeOffset CriadaEm { get; private set; }
        public DateTimeOffset? LidaEm { get; private set; }

        public void MarcarComoLida() => LidaEm ??= DateTimeOffset.UtcNow;

        public static Notificacao Criar(Guid osId, Guid clienteId, DestinatarioNotificacao destinatario, string mensagem)
            => new(osId, clienteId, destinatario, mensagem);
    }
}
