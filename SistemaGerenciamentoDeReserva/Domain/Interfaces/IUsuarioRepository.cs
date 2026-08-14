using SistemaGerenciamentoDeReserva.Domain.Entity;

namespace SistemaGerenciamentoDeReserva.Domain.Interfaces
{
    public interface IUsuarioRepository
    {
        Task<Usuario?> ObterPorIdAsync(long id); //tarefa assincrona
        Task<IEnumerable<Usuario>> ObterTodosAsync();
        Task<long> AdicionarAsync(Usuario usuario);
        Task AtualizarAsync(Usuario usuario);
        Task DeletarAsync(long id);
        Task<Usuario?> ObterPorEmailAsync(string email);
    }
}
