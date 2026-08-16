using SistemaGerenciamentoDeReserva.Application.DTOs.Reserva;

namespace SistemaGerenciamentoDeReserva.Application.Interface
{
    public interface IReservaNotificacaoPublisher
    {
        Task PublicarAsync(ReservaNotificacaoEvento evento);
    }
}
