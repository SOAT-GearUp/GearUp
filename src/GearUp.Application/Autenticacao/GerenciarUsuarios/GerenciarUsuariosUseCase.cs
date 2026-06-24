using GearUp.Application.Autenticacao.Common;
using GearUp.Application.Atendimento.Clientes.Common.Interfaces;
using GearUp.Application.Common.Interfaces;

namespace GearUp.Application.Autenticacao.GerenciarUsuarios
{
    internal sealed class GerenciarUsuariosUseCase(
        IUsuarioRepository usuarios, 
        IClienteRepository clientes, 
        IPasswordHasher hasher, 
        IUnitOfWork unitOfWork) : IGerenciarUsuariosUseCase
    {
        public async Task<CriarUsuarioResult> CriarAsync(CriarUsuarioCommand command, CancellationToken ct)
        {
            var normalizado = command.Usuario.Trim().ToLowerInvariant();

            if (await usuarios.ExisteAsync(normalizado, ct)) 
                throw new ConflitoException("USUARIO_DUPLICADO", "Nome de usuário já cadastrado.");

            if (command.Perfil == PerfilUsuario.Cliente && (!command.ClienteId.HasValue || await clientes.ObterAsync(command.ClienteId.Value, ct) is null))
                throw new RecursoNaoEncontradoException("CLIENTE_NAO_ENCONTRADO", "Cliente vinculado ao usuário não encontrado.");

            var usuario = Domain.Entities.Usuario.Criar(normalizado, hasher.CriarHash(command.Senha), command.Perfil, command.ClienteId);

            await usuarios.AdicionarAsync(usuario, ct); 

            await unitOfWork.SaveChangesAsync(ct); 

            return new CriarUsuarioResult(usuario.Id);
        }
    }
}
