namespace GearUp.Application.OrdemDeServico.Common;

public sealed record HistoricoOrdemServicoResult(
    string Tipo,
    string Descricao,
    DateTimeOffset CriadoEm);
