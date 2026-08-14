using SistemaGerenciamentoDeReserva.Application.DTOs.Quarto;
using SistemaGerenciamentoDeReserva.Application.Interface;
using SistemaGerenciamentoDeReserva.Domain.Entity;
using SistemaGerenciamentoDeReserva.Domain.Enums;
using SistemaGerenciamentoDeReserva.Domain.Interfaces;

namespace SistemaGerenciamentoDeReserva.Application.Services
{
    public class QuartoService : IQuartoService
    {
        private readonly IQuartoRepository _quartoRepository;
        private readonly IHotelRepository _hotelRepository;

        public QuartoService(
            IQuartoRepository quartoRepository,
            IHotelRepository hotelRepository)
        {
            _quartoRepository = quartoRepository;
            _hotelRepository = hotelRepository;
        }

        public async Task<QuartoResponseDto> AdicionarQuarto(
            CriarQuartoDto dto)
        {
            var hotel = await _hotelRepository
                .ObterPorIdAsync(dto.HotelId);

            if (hotel is null)
            {
                throw new KeyNotFoundException(
                    "Hotel não encontrado.");
            }

            var quarto = new Quarto
            {
                HotelId = dto.HotelId,
                Numero = dto.Numero,
                Tipo = dto.Tipo,
                PrecoPorNoite = dto.PrecoPorNoite,
                Status = StatusQuarto.Disponivel
            };

            var id = await _quartoRepository
                .AdicionarAsync(quarto);

            return new QuartoResponseDto(
                id,
                quarto.HotelId,
                quarto.Numero,
                quarto.Tipo,
                quarto.PrecoPorNoite,
                quarto.Status
            );
        }

        public async Task<QuartoResponseDto?> BuscarPorId(long id)
        {
            var quarto = await _quartoRepository
                .ObterPorIdAsync(id);

            if (quarto is null)
                return null;

            return MapearParaDto(quarto);
        }

        public async Task<IEnumerable<QuartoResponseDto>> ListarQuarto()
        {
            var quartos = await _quartoRepository
                .ObterTodosAsync();

            return quartos.Select(MapearParaDto);
        }

        public async Task AtualizarQuarto(
            long id,
            AtualizarQuartoDto dto)
        {
            var quarto = await _quartoRepository
                .ObterPorIdAsync(id);

            if (quarto is null)
            {
                throw new KeyNotFoundException(
                    "Quarto não encontrado.");
            }

            quarto.Numero = dto.Numero;
            quarto.Tipo = dto.Tipo;
            quarto.PrecoPorNoite = dto.PrecoPorNoite;

            await _quartoRepository
                .AtualizarAsync(quarto);
        }

        public async Task DeletarQuarto(long id)
        {
            var quarto = await _quartoRepository
                .ObterPorIdAsync(id);

            if (quarto is null)
            {
                throw new KeyNotFoundException(
                    "Quarto não encontrado.");
            }

            await _quartoRepository
                .DeletarAsync(id);
        }

        private static QuartoResponseDto MapearParaDto(
            Quarto quarto)
        {
            return new QuartoResponseDto(
                quarto.Id,
                quarto.HotelId,
                quarto.Numero,
                quarto.Tipo,
                quarto.PrecoPorNoite,
                quarto.Status
            );
        }
    }
}
