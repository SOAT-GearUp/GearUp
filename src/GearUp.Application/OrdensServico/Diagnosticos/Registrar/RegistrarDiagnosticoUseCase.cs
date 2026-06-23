using GearUp.Application.Common.Interfaces;
using GearUp.Application.OrdensServico.Common.Interfaces;

namespace GearUp.Application.OrdensServico.Diagnosticos.Registrar
{
    internal sealed class RegistrarDiagnosticoUseCase(IOrdemServicoRepository ordemServicoRepository, IUnitOfWork unitOfWork) : IRegistrarDiagnosticoUseCase
    {
        public async Task RegistrarAsync(RegistrarDiagnosticoCommand command, CancellationToken ct) 
        { 
            var os = await ordemServicoRepository.ObterAsync(command.OrdemServicoId, ct)
                ?? throw new RecursoNaoEncontradoException("ORCAMENTO_NAO_ENCONTRADO", "Orçamento não encontrado.");

            os.RegistrarDiagnostico(command.Diagnostico); 

            await unitOfWork.SaveChangesAsync(ct); 
        }

    }
}
