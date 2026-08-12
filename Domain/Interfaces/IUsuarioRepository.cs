using SistemaGerenciamentoDeReserva.Domain.Entity;

namespace SistemaGerenciamentoDeReserva.Domain.Interfaces
{
    public interface IUsuarioRepository
    {
        Task<Usuario?> BuscarPorId(long id); //task significa que o metodo e assincrono
        Task<List<Usuario>> ListarUsuarios();
        Task Adicionar(Usuario usuario);
        Task Atualizar(Usuario usuario);
        Task Deletar(long id);
        Task<Usuario?> BuscarPorEmail(string email);
    }
}
