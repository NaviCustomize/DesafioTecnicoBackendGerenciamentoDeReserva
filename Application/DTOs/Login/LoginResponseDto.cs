namespace SistemaGerenciamentoDeReserva.Application.DTOs.Login
{
    public record LoginResponseDto(
        string Token,
        long UsuarioId,
        string Nome
    );
}
