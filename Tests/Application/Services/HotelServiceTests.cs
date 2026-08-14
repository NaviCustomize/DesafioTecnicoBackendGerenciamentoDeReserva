using Moq;
using SistemaGerenciamentoDeReserva.Application.DTOs.Hotel;
using SistemaGerenciamentoDeReserva.Application.Services;
using SistemaGerenciamentoDeReserva.Domain.Entity;
using SistemaGerenciamentoDeReserva.Domain.Interfaces;

namespace SistemaGerenciamentoDeReserva.Tests.Application.Services
{
    public class HotelServiceTests
    {
        private readonly Mock<IHotelRepository> _hotelRepositoryMock;
        private readonly HotelService _service;

        public HotelServiceTests()
        {
            _hotelRepositoryMock = new Mock<IHotelRepository>();
            _service = new HotelService(_hotelRepositoryMock.Object);
        }

        [Fact]
        public async Task AdicionarHotel_CenarioValido_RetornaDtoComIdGerado()
        {
            var dto = new CriarHotelDto("Hotel Central", "São Paulo", "Perto do centro");

            _hotelRepositoryMock
                .Setup(r => r.AdicionarAsync(It.IsAny<Hotel>()))
                .ReturnsAsync(1L);

            var resultado = await _service.AdicionarHotel(dto);

            Assert.Equal(1L, resultado.Id);
            Assert.Equal(dto.Nome, resultado.Nome);
            Assert.Equal(dto.Localizacao, resultado.Localizacao);
        }

        [Fact]
        public async Task BuscarPorId_HotelInexistente_RetornaNull()
        {
            _hotelRepositoryMock
                .Setup(r => r.ObterPorIdAsync(1))
                .ReturnsAsync((Hotel?)null);

            var resultado = await _service.BuscarPorId(1);

            Assert.Null(resultado);
        }

        [Fact]
        public async Task AtualizarHotel_HotelInexistente_LancaKeyNotFoundException()
        {
            var dto = new AtualizarHotelDto("Hotel Novo", "Rio de Janeiro", null);

            _hotelRepositoryMock
                .Setup(r => r.ObterPorIdAsync(1))
                .ReturnsAsync((Hotel?)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _service.AtualizarHotel(1, dto));
        }

        [Fact]
        public async Task AtualizarHotel_CenarioValido_AtualizaCampos()
        {
            var dto = new AtualizarHotelDto("Hotel Renovado", "Curitiba", "Nova descrição");
            var hotel = new Hotel("Hotel Antigo", "São Paulo", "Descrição antiga") { Id = 1 };

            _hotelRepositoryMock
                .Setup(r => r.ObterPorIdAsync(1))
                .ReturnsAsync(hotel);

            await _service.AtualizarHotel(1, dto);

            _hotelRepositoryMock.Verify(
                r => r.AtualizarAsync(It.Is<Hotel>(
                    h => h.Nome == dto.Nome
                        && h.Localizacao == dto.Localizacao
                        && h.Descricao == dto.Descricao)),
                Times.Once);
        }

        [Fact]
        public async Task DeletarHotel_HotelInexistente_LancaKeyNotFoundException()
        {
            _hotelRepositoryMock
                .Setup(r => r.ObterPorIdAsync(1))
                .ReturnsAsync((Hotel?)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _service.DeletarHotel(1));
        }

        [Fact]
        public async Task DeletarHotel_CenarioValido_ChamaRepositorio()
        {
            var hotel = new Hotel("Hotel", "São Paulo", "") { Id = 1 };

            _hotelRepositoryMock
                .Setup(r => r.ObterPorIdAsync(1))
                .ReturnsAsync(hotel);

            await _service.DeletarHotel(1);

            _hotelRepositoryMock.Verify(r => r.DeletarAsync(1), Times.Once);
        }
    }
}
