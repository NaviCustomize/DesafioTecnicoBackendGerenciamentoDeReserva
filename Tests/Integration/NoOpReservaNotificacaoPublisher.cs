using SistemaGerenciamentoDeReserva.Application.DTOs.Reserva;
using SistemaGerenciamentoDeReserva.Application.Interface;

namespace SistemaGerenciamentoDeReserva.Tests.Integration
{
    public class NoOpReservaNotificacaoPublisher : IReservaNotificacaoPublisher
    {
        public Task PublicarAsync(ReservaNotificacaoEvento evento) => Task.CompletedTask;
    }
}
