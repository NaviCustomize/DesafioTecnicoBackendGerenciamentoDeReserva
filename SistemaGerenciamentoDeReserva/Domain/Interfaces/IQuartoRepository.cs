using SistemaGerenciamentoDeReserva.Domain.Entity;

namespace SistemaGerenciamentoDeReserva.Domain.Interfaces
{
    public interface IQuartoRepository
    {
        Task<Quarto?> ObterPorIdAsync(long id);
        Task<IEnumerable<Quarto>> ObterTodosAsync();
        Task<long> AdicionarAsync(Quarto quarto);
        Task AtualizarAsync(Quarto quarto);
        Task DeletarAsync(long id);
    }
}
