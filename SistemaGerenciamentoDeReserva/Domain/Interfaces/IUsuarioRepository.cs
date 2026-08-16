using SistemaGerenciamentoDeReserva.Domain.Entity;

namespace SistemaGerenciamentoDeReserva.Domain.Interfaces
{
    public interface IUsuarioRepository
    {
        Task<Usuario?> ObterPorIdAsync(long id);
        Task<IEnumerable<Usuario>> ObterTodosAsync();


        Task<IEnumerable<Usuario>> ObterTodosIncluindoInativosAsync();


        Task<Usuario?> ObterPorIdIncluindoInativoAsync(long id);

        Task ReativarAsync(long id);
        Task<long> AdicionarAsync(Usuario usuario);
        Task AtualizarAsync(Usuario usuario);
        Task DeletarAsync(long id);
        Task<Usuario?> ObterPorEmailAsync(string email);
    }
}
