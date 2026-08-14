namespace SistemaGerenciamentoDeReserva.Application.DTOs.Reserva
{
    public record CriarReservaDto(long QuartoId, DateTime DataCheckIn, DateTime DataCheckOut);
}
