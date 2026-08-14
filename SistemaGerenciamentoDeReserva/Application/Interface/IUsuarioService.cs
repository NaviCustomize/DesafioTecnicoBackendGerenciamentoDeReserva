using SistemaGerenciamentoDeReserva.Application.DTOs.Usuario;

namespace SistemaGerenciamentoDeReserva.Application.Interface
{
    public interface IUsuarioService
    {
        Task<UsuarioResponseDto> AdicionarUsuario(CriarUsuarioDto dto);

        Task<UsuarioResponseDto?> BuscarPorId(long id);

        Task<IEnumerable<UsuarioResponseDto>> ListarUsuarios();

        Task AtualizarUsuario(long id, AtualizarUsuarioDto dto);

        Task DeletarUsuario(long id);
    }
}
