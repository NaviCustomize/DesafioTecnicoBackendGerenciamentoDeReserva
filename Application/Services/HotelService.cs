using SistemaGerenciamentoDeReserva.Application.DTOs.Hotel;
using SistemaGerenciamentoDeReserva.Application.Interface;
using SistemaGerenciamentoDeReserva.Domain.Entity;
using SistemaGerenciamentoDeReserva.Domain.Interfaces;

namespace SistemaGerenciamentoDeReserva.Application.Services
{
    public class HotelService : IHotelService
    {
        private readonly IHotelRepository _hotelRepository;

        public HotelService(IHotelRepository hotelRepository)
        {
            _hotelRepository = hotelRepository;
        }

        public async Task<HotelResponseDto> AdicionarHotel(
            CriarHotelDto dto)
        {
            var hotel = new Hotel
            {
                Nome = dto.Nome,
                Localizacao = dto.Localizacao,
                Descricao = dto.Descricao
            };

            var id = await _hotelRepository.AdicionarAsync(hotel);

            return new HotelResponseDto(
                id,
                hotel.Nome,
                hotel.Localizacao,
                hotel.Descricao
            );
        }

        public async Task<HotelResponseDto?> BuscarPorId(long id)
        {
            var hotel = await _hotelRepository.ObterPorIdAsync(id);

            if (hotel is null)
                return null;

            return new HotelResponseDto(
                hotel.Id,
                hotel.Nome,
                hotel.Localizacao,
                hotel.Descricao
            );
        }

        public async Task<IEnumerable<HotelResponseDto>> ListarHotel()
        {
            var hoteis = await _hotelRepository.ObterTodosAsync();

            return hoteis.Select(hotel =>
                new HotelResponseDto(
                    hotel.Id,
                    hotel.Nome,
                    hotel.Localizacao,
                    hotel.Descricao
                ));
        }

        public async Task AtualizarHotel(
            long id,
            AtualizarHotelDto dto)
        {
            var hotel = await _hotelRepository.ObterPorIdAsync(id);

            if (hotel is null)
            {
                throw new KeyNotFoundException(
                    "Hotel não encontrado.");
            }

            hotel.Nome = dto.Nome;
            hotel.Localizacao = dto.Localizacao;
            hotel.Descricao = dto.Descricao;

            await _hotelRepository.AtualizarAsync(hotel);
        }

        public async Task DeletarHotel(long id)
        {
            var hotel = await _hotelRepository.ObterPorIdAsync(id);

            if (hotel is null)
            {
                throw new KeyNotFoundException(
                    "Hotel não encontrado.");
            }

            await _hotelRepository.DeletarAsync(id);
        }
    }
}
