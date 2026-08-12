namespace SistemaGerenciamentoDeReserva.Application.DTOs
{
    public record LoginResponseDto(
        string Token,
        long UsuarioId,
        string Nome
    );
}
