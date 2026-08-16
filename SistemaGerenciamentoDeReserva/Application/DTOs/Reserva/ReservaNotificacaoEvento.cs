namespace SistemaGerenciamentoDeReserva.Application.DTOs.Reserva
{
    public record ReservaNotificacaoEvento(
        long ReservaId,
        long UsuarioId,
        long QuartoId,
        string TipoEvento,
        DateTime DataCheckIn,
        DateTime DataCheckOut,
        DateTime OcorridoEmUtc,
        string Hospede = "",
        string HospedeEmail = "",
        string Hotel = "",
        int QuartoNumero = 0);
}
