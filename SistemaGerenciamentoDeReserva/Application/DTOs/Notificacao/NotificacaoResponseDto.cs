namespace SistemaGerenciamentoDeReserva.Application.DTOs.Notificacao
{
    public record NotificacaoResponseDto(
        long Id,
        long ReservaId,
        string TipoEvento,
        string Hospede,
        string HospedeEmail,
        string Hotel,
        int QuartoNumero,
        DateTime DataCheckIn,
        DateTime DataCheckOut,
        DateTime ProcessadoEm
    );
}
