using SistemaGerenciamentoDeReserva.Domain.Entity;

namespace SistemaGerenciamentoDeReserva.Domain.Interfaces
{
    public interface IHotelRepository
    {
        Task<Hotel?> ObterPorIdAsync(long id);
        Task<IEnumerable<Hotel>> ObterTodosAsync();
        Task<long> AdicionarAsync(Hotel hotel);
        Task AtualizarAsync(Hotel hotel);
        Task DeletarAsync(long id);
    }
}
