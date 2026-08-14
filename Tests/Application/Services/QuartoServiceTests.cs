using Moq;
using SistemaGerenciamentoDeReserva.Application.DTOs.Quarto;
using SistemaGerenciamentoDeReserva.Application.Services;
using SistemaGerenciamentoDeReserva.Domain.Entity;
using SistemaGerenciamentoDeReserva.Domain.Enums;
using SistemaGerenciamentoDeReserva.Domain.Interfaces;

namespace SistemaGerenciamentoDeReserva.Tests.Application.Services
{
    public class QuartoServiceTests
    {
        private readonly Mock<IQuartoRepository> _quartoRepositoryMock;
        private readonly Mock<IHotelRepository> _hotelRepositoryMock;
        private readonly QuartoService _service;

        public QuartoServiceTests()
        {
            _quartoRepositoryMock = new Mock<IQuartoRepository>();
            _hotelRepositoryMock = new Mock<IHotelRepository>();

            _service = new QuartoService(
                _quartoRepositoryMock.Object,
                _hotelRepositoryMock.Object);
        }

        [Fact]
        public async Task AdicionarQuarto_HotelInexistente_LancaKeyNotFoundException()
        {
            var dto = new CriarQuartoDto(HotelId: 1, Numero: 101, Tipo: TipoQuarto.Standard, PrecoPorNoite: 200m);

            _hotelRepositoryMock
                .Setup(r => r.ObterPorIdAsync(dto.HotelId))
                .ReturnsAsync((Hotel?)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _service.AdicionarQuarto(dto));

            _quartoRepositoryMock.Verify(
                r => r.AdicionarAsync(It.IsAny<Quarto>()),
                Times.Never);
        }

        [Fact]
        public async Task AdicionarQuarto_CenarioValido_CriaComStatusDisponivel()
        {
            var dto = new CriarQuartoDto(HotelId: 1, Numero: 101, Tipo: TipoQuarto.Luxo, PrecoPorNoite: 350m);

            _hotelRepositoryMock
                .Setup(r => r.ObterPorIdAsync(dto.HotelId))
                .ReturnsAsync(new Hotel("Hotel Central", "São Paulo", "") { Id = 1 });

            _quartoRepositoryMock
                .Setup(r => r.AdicionarAsync(It.IsAny<Quarto>()))
                .ReturnsAsync(1L);

            var resultado = await _service.AdicionarQuarto(dto);

            Assert.Equal(StatusQuarto.Disponivel, resultado.Status);
            Assert.Equal(dto.Numero, resultado.Numero);
            Assert.Equal(dto.PrecoPorNoite, resultado.PrecoPorNoite);
        }

        [Fact]
        public async Task BuscarPorId_QuartoInexistente_RetornaNull()
        {
            _quartoRepositoryMock
                .Setup(r => r.ObterPorIdAsync(1))
                .ReturnsAsync((Quarto?)null);

            var resultado = await _service.BuscarPorId(1);

            Assert.Null(resultado);
        }

        [Fact]
        public async Task AtualizarQuarto_QuartoInexistente_LancaKeyNotFoundException()
        {
            var dto = new AtualizarQuartoDto(Numero: 102, Tipo: TipoQuarto.SuiteMaster, PrecoPorNoite: 500m);

            _quartoRepositoryMock
                .Setup(r => r.ObterPorIdAsync(1))
                .ReturnsAsync((Quarto?)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _service.AtualizarQuarto(1, dto));
        }

        [Fact]
        public async Task AtualizarQuarto_CenarioValido_AtualizaCampos()
        {
            var dto = new AtualizarQuartoDto(Numero: 205, Tipo: TipoQuarto.Luxo, PrecoPorNoite: 300m);
            var quarto = new Quarto(1, 101, TipoQuarto.Standard, 150m, StatusQuarto.Disponivel) { Id = 1 };

            _quartoRepositoryMock
                .Setup(r => r.ObterPorIdAsync(1))
                .ReturnsAsync(quarto);

            await _service.AtualizarQuarto(1, dto);

            _quartoRepositoryMock.Verify(
                r => r.AtualizarAsync(It.Is<Quarto>(
                    q => q.Numero == dto.Numero
                        && q.Tipo == dto.Tipo
                        && q.PrecoPorNoite == dto.PrecoPorNoite)),
                Times.Once);
        }

        [Fact]
        public async Task DeletarQuarto_QuartoInexistente_LancaKeyNotFoundException()
        {
            _quartoRepositoryMock
                .Setup(r => r.ObterPorIdAsync(1))
                .ReturnsAsync((Quarto?)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _service.DeletarQuarto(1));
        }

        [Fact]
        public async Task DeletarQuarto_CenarioValido_ChamaRepositorio()
        {
            var quarto = new Quarto(1, 101, TipoQuarto.Standard, 150m, StatusQuarto.Disponivel) { Id = 1 };

            _quartoRepositoryMock
                .Setup(r => r.ObterPorIdAsync(1))
                .ReturnsAsync(quarto);

            await _service.DeletarQuarto(1);

            _quartoRepositoryMock.Verify(r => r.DeletarAsync(1), Times.Once);
        }
    }
}
