namespace GearUp.Application.OrdensServico.Common
{
    public sealed record HistoricoOrdemServicoResult(
        string Tipo,
        string Descricao,
        DateTimeOffset CriadoEm);
}
