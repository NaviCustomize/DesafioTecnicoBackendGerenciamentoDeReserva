using SistemaGerenciamentoDeReserva.Application.DTOs.Usuario;

namespace SistemaGerenciamentoDeReserva.Application.Interface
{
    public interface IUsuarioService
    {
        Task<UsuarioResponseDto> AdicionarUsuario(CriarUsuarioDto dto);

        Task<UsuarioResponseDto?> BuscarPorId(long id);

        Task<IEnumerable<UsuarioResponseDto>> ListarUsuarios();


        Task<IEnumerable<UsuarioAdminResponseDto>> ListarUsuariosParaAdmin();


        Task ReativarUsuario(long id);

        Task AtualizarUsuario(long id, AtualizarUsuarioDto dto);

        Task DeletarUsuario(long id);


        Task AlterarSenha(long id, AlterarSenhaDto dto);


        Task EncerrarPropriaConta(long id, ConfirmarSenhaDto dto);
    }
}
