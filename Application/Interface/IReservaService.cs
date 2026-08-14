using SistemaGerenciamentoDeReserva.Application.DTOs.Reserva;

namespace SistemaGerenciamentoDeReserva.Application.Interface
{
    public interface IReservaService
    {
        Task<ReservaResponseDto> AdicionarReserva(CriarReservaDto dto,long usuarioId);

        Task<ReservaResponseDto?> BuscarPorId(long id);

        Task<IEnumerable<ReservaResponseDto>> ListarReservasPorUsuario(long usuarioId);
        Task<IEnumerable<ReservaResponseDto>> ListarReserva();

        Task<IEnumerable<ReservaResponseDto>> ListarHistoricoPorUsuario(long usuarioId);

        Task AtualizarReserva(long id, AtualizarReservaDto dto, long usuarioId);

        Task DeletarReserva(long id, long usuarioId);

        Task CancelarReserva(long id, long usuarioid);

    }
}
