using SistemaGerenciamentoDeReserva.Domain.Enums;

namespace SistemaGerenciamentoDeReserva.Application.DTOs.Usuario
{
    public record UsuarioResponseDto(long Id, string Nome, string Email, RoleUsuario Role);
}
