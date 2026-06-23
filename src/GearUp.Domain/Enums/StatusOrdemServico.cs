namespace GearUp.Domain.Enums
{
    public enum StatusOrdemServico
    {
        Recebida = 1, 
        EmDiagnostico = 2, 
        AguardandoOrcamento = 3, 
        AguardandoAprovacao = 4,
        Cancelada = 5, 
        AguardandoPecasInsumos = 6, 
        AguardandoExecucao = 7,
        EmExecucao = 8, 
        Finalizada = 9, 
        Entregue = 10
    }
}
