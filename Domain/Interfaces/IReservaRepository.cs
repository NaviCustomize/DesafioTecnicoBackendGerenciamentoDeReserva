using SistemaGerenciamentoDeReserva.Domain.Entity;

namespace SistemaGerenciamentoDeReserva.Domain.Interfaces
{
    public interface IReservaRepository
    {
        Task<IEnumerable<Reserva>> ObterTodasAsync();//metodo que vai retornar uma lista de Oj do tipo reserva
        Task<Reserva?> ObterPorIdAsync(long id);
        Task AdicionarAsync(Reserva reserva);
        Task AtualizarAsync(Reserva reserva);
        Task DeletarAsync(long id);
    }
}
