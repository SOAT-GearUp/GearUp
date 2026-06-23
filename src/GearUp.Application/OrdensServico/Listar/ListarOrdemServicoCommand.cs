namespace GearUp.Application.OrdensServico.Listar
{
    public sealed record ListarOrdemServicoCommand(
        bool EmAndamento,
        Guid? ClienteId);
}
