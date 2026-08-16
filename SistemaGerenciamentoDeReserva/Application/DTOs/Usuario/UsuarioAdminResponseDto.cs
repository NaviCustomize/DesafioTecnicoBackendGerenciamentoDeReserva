using SistemaGerenciamentoDeReserva.Domain.Enums;

namespace SistemaGerenciamentoDeReserva.Application.DTOs.Usuario
{
    public record UsuarioAdminResponseDto(
        long Id,
        string Nome,
        string Sobrenome,
        string Email,
        RoleUsuario Role,
        bool Ativo,
        DateTime? InativoDesde
    );
}
