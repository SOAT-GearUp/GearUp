using GearUp.Application.Common.Interfaces;
using GearUp.Application.OrdensServico.Common.Interfaces;

namespace GearUp.Application.OrdensServico.Diagnosticos.Iniciar
{
    internal sealed class IniciarDiagnosticoUseCase(IOrdemServicoRepository ordemServicoRepository, IUnitOfWork unitOfWork) : IIniciarDiagnosticoUseCase
    {
        public async Task IniciarAsync(IniciarDiagnosticoCommand command, CancellationToken cancellationToken)
        { 
            var os = await ordemServicoRepository.ObterAsync(command.OrdemServicoId, cancellationToken)
                ?? throw new RecursoNaoEncontradoException("ORCAMENTO_NAO_ENCONTRADO", "Orçamento não encontrado.");

            os.IniciarDiagnostico(command.MecanicoId); 

            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
