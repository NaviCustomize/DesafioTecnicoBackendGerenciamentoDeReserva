using SistemaGerenciamentoDeReserva.Domain.Entity;

namespace SistemaGerenciamentoDeReserva.Domain.Interfaces
{
    public interface IReservaRepository
    {
        Task<Reserva?> ObterPorIdAsync(long id);

        Task<IEnumerable<Reserva>> ObterTodosAsync();

        Task<IEnumerable<Reserva>> ObterPorUsuarioAsync(long usuarioId);

        Task<bool> ExisteConflitoAsync(
            long quartoId,
            DateTime dataCheckIn,
            DateTime dataCheckOut,
            long? reservaId = null);

        Task<IEnumerable<Reserva>> ObterHistoricoPorUsuarioAsync(long usuarioId);

        Task<long> AdicionarAsync(Reserva reserva);

        Task AtualizarAsync(Reserva reserva);

        Task DeletarAsync(long id);

        Task CancelarAsync(long id);
    }
}
