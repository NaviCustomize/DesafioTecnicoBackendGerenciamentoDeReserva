using SistemaGerenciamentoDeReserva.Application.DTOs.Hotel;

namespace SistemaGerenciamentoDeReserva.Application.Interface
{
    public interface IHotelService
    {
        Task<HotelResponseDto> AdicionarHotel(CriarHotelDto dto);

        Task<HotelResponseDto?> BuscarPorId(long id);

        Task<IEnumerable<HotelResponseDto>> ListarHotel();

        Task AtualizarHotel(long id, AtualizarHotelDto dto);

        Task DeletarHotel(long id);
    }
}
