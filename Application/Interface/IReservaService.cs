using SistemaGerenciamentoDeReserva.Application.DTOs.Reserva;

namespace SistemaGerenciamentoDeReserva.Application.Interface
{
    public interface IReservaService
    {
        Task<ReservaResponseDto> AdicionarReserva(CriarReservaDto dto,long usuarioId);

        Task<ReservaResponseDto?> BuscarPorId(long id);

        Task<IEnumerable<ReservaResponseDto>> ListarReserva();

        Task AtualizarReserva(long id, AtualizarReservaDto dto, long usuarioId);

        Task DeletarReserva(long id, long usuarioId);
    }
}
