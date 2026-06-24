namespace GearUp.Application.Atendimento.Comum;

public sealed record HistoricoOrdemServicoResult(
    string Tipo,
    string Descricao,
    DateTimeOffset CriadoEm);
