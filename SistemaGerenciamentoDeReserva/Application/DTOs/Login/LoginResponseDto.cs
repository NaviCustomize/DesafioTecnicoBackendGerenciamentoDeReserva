using SistemaGerenciamentoDeReserva.Domain.Enums;

namespace SistemaGerenciamentoDeReserva.Application.DTOs.Login
{
    public record LoginResponseDto(
        string Token,
        long UsuarioId,
        string Nome,
        string Sobrenome,
        RoleUsuario Role
    );
}
