using SistemaGerenciamentoDeReserva.Domain.Entity;

namespace SistemaGerenciamentoDeReserva.Domain.Interfaces
{
    public interface IHotelRepository
    {
        Task<IEnumerable<Hotel>> ObterTodosAsync();
        Task<Hotel?> ObterPorIdAsync(long id);
        Task AdicionarAsync(Hotel hotel);
        Task AtualizarAsync(Hotel hotel);
        Task DeletarAsync(long id);
    }
}
