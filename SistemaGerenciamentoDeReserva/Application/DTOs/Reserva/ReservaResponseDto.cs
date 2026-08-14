using SistemaGerenciamentoDeReserva.Domain.Enums;

namespace SistemaGerenciamentoDeReserva.Application.DTOs.Reserva
{
    public record ReservaResponseDto(long Id, DateTime DataCheckIn, DateTime DataCheckOut, StatusReserva Status, long UsuarioId, long QuartoId);
}
