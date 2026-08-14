using Moq;
using SistemaGerenciamentoDeReserva.Application.DTOs.Reserva;
using SistemaGerenciamentoDeReserva.Application.Services;
using SistemaGerenciamentoDeReserva.Domain.Entity;
using SistemaGerenciamentoDeReserva.Domain.Enums;
using SistemaGerenciamentoDeReserva.Domain.Interfaces;

namespace SistemaGerenciamentoDeReserva.Tests.Application.Services
{
    public class ReservaServiceTests
    {
        private readonly Mock<IReservaRepository> _reservaRepositoryMock;
        private readonly Mock<IQuartoRepository> _quartoRepositoryMock;
        private readonly ReservaService _service;

        public ReservaServiceTests()
        {
            _reservaRepositoryMock = new Mock<IReservaRepository>();
            _quartoRepositoryMock = new Mock<IQuartoRepository>();

            _service = new ReservaService(
                _reservaRepositoryMock.Object,
                _quartoRepositoryMock.Object);
        }

        

        [Fact]
        public async Task AdicionarReserva_CheckInMaiorOuIgualCheckOut_LancaArgumentException()
        {
            var dto = new CriarReservaDto(
                QuartoId: 1,
                DataCheckIn: new DateTime(2026, 1, 10),
                DataCheckOut: new DateTime(2026, 1, 9));

            await Assert.ThrowsAsync<ArgumentException>(
                () => _service.AdicionarReserva(dto, usuarioId: 1));

            _quartoRepositoryMock.Verify(
                r => r.ObterPorIdAsync(It.IsAny<long>()),
                Times.Never);
        }

        [Fact]
        public async Task AdicionarReserva_QuartoInexistente_LancaKeyNotFoundException()
        {
            var dto = new CriarReservaDto(
                QuartoId: 1,
                DataCheckIn: new DateTime(2026, 1, 10),
                DataCheckOut: new DateTime(2026, 1, 12));

            _quartoRepositoryMock
                .Setup(r => r.ObterPorIdAsync(dto.QuartoId))
                .ReturnsAsync((Quarto?)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _service.AdicionarReserva(dto, usuarioId: 1));
        }

        [Fact]
        public async Task AdicionarReserva_QuartoNaoDisponivel_LancaInvalidOperationException()
        {
            var dto = new CriarReservaDto(
                QuartoId: 1,
                DataCheckIn: new DateTime(2026, 1, 10),
                DataCheckOut: new DateTime(2026, 1, 12));

            var quarto = new Quarto(
                hotelId: 1,
                numero: 101,
                tipo: TipoQuarto.Standard,
                precoPorNoite: 200m,
                status: StatusQuarto.Manutencao);

            _quartoRepositoryMock
                .Setup(r => r.ObterPorIdAsync(dto.QuartoId))
                .ReturnsAsync(quarto);

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.AdicionarReserva(dto, usuarioId: 1));
        }

        [Fact]
        public async Task AdicionarReserva_ConflitoDePeriodo_LancaInvalidOperationException()
        {
            var dto = new CriarReservaDto(
                QuartoId: 1,
                DataCheckIn: new DateTime(2026, 1, 10),
                DataCheckOut: new DateTime(2026, 1, 12));

            var quarto = new Quarto(
                hotelId: 1,
                numero: 101,
                tipo: TipoQuarto.Standard,
                precoPorNoite: 200m,
                status: StatusQuarto.Disponivel);

            _quartoRepositoryMock
                .Setup(r => r.ObterPorIdAsync(dto.QuartoId))
                .ReturnsAsync(quarto);

            _reservaRepositoryMock
                .Setup(r => r.ExisteConflitoAsync(
                    dto.QuartoId, dto.DataCheckIn, dto.DataCheckOut, null))
                .ReturnsAsync(true);

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.AdicionarReserva(dto, usuarioId: 1));
        }

        [Fact]
        public async Task AdicionarReserva_CenarioValido_CriaReservaConfirmadaERetornaDto()
        {
            var dto = new CriarReservaDto(
                QuartoId: 1,
                DataCheckIn: new DateTime(2026, 1, 10),
                DataCheckOut: new DateTime(2026, 1, 12));

            var quarto = new Quarto(
                hotelId: 1,
                numero: 101,
                tipo: TipoQuarto.Standard,
                precoPorNoite: 200m,
                status: StatusQuarto.Disponivel);

            _quartoRepositoryMock
                .Setup(r => r.ObterPorIdAsync(dto.QuartoId))
                .ReturnsAsync(quarto);

            _reservaRepositoryMock
                .Setup(r => r.ExisteConflitoAsync(
                    dto.QuartoId, dto.DataCheckIn, dto.DataCheckOut, null))
                .ReturnsAsync(false);

            _reservaRepositoryMock
                .Setup(r => r.AdicionarAsync(It.IsAny<Reserva>()))
                .ReturnsAsync(42L);

            var resultado = await _service.AdicionarReserva(dto, usuarioId: 7);

            Assert.Equal(42L, resultado.Id);
            Assert.Equal(7L, resultado.UsuarioId);
            Assert.Equal(dto.QuartoId, resultado.QuartoId);
            Assert.Equal(StatusReserva.Confirmada, resultado.Status);

            _reservaRepositoryMock.Verify(
                r => r.AdicionarAsync(It.Is<Reserva>(
                    res => res.UsuarioId == 7
                        && res.QuartoId == dto.QuartoId
                        && res.Status == StatusReserva.Confirmada)),
                Times.Once);
        }

        

        [Fact]
        public async Task AtualizarReserva_CheckInMaiorOuIgualCheckOut_LancaArgumentException()
        {
            var dto = new AtualizarReservaDto(
                DataCheckIn: new DateTime(2026, 1, 10),
                DataCheckOut: new DateTime(2026, 1, 10));

            await Assert.ThrowsAsync<ArgumentException>(
                () => _service.AtualizarReserva(id: 1, dto, usuarioId: 1));

            _reservaRepositoryMock.Verify(
                r => r.ObterPorIdAsync(It.IsAny<long>()),
                Times.Never);
        }

        [Fact]
        public async Task AtualizarReserva_ReservaInexistente_LancaKeyNotFoundException()
        {
            var dto = new AtualizarReservaDto(
                DataCheckIn: new DateTime(2026, 1, 10),
                DataCheckOut: new DateTime(2026, 1, 12));

            _reservaRepositoryMock
                .Setup(r => r.ObterPorIdAsync(1))
                .ReturnsAsync((Reserva?)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _service.AtualizarReserva(id: 1, dto, usuarioId: 1));
        }

        [Fact]
        public async Task AtualizarReserva_UsuarioNaoEDono_LancaUnauthorizedAccessException()
        {
            var dto = new AtualizarReservaDto(
                DataCheckIn: new DateTime(2026, 1, 10),
                DataCheckOut: new DateTime(2026, 1, 12));

            var reserva = new Reserva(
                dataCheckIn: new DateTime(2026, 1, 1),
                dataCheckOut: new DateTime(2026, 1, 5),
                status: StatusReserva.Confirmada,
                usuarioId: 99,
                quartoId: 1)
            { Id = 1 };

            _reservaRepositoryMock
                .Setup(r => r.ObterPorIdAsync(1))
                .ReturnsAsync(reserva);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => _service.AtualizarReserva(id: 1, dto, usuarioId: 1));
        }

        [Fact]
        public async Task AtualizarReserva_ReservaCancelada_LancaInvalidOperationException()
        {
            var dto = new AtualizarReservaDto(
                DataCheckIn: new DateTime(2026, 1, 10),
                DataCheckOut: new DateTime(2026, 1, 12));

            var reserva = new Reserva(
                dataCheckIn: new DateTime(2026, 1, 1),
                dataCheckOut: new DateTime(2026, 1, 5),
                status: StatusReserva.Cancelada,
                usuarioId: 1,
                quartoId: 1)
            { Id = 1 };

            _reservaRepositoryMock
                .Setup(r => r.ObterPorIdAsync(1))
                .ReturnsAsync(reserva);

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.AtualizarReserva(id: 1, dto, usuarioId: 1));
        }

        [Fact]
        public async Task AtualizarReserva_ConflitoDePeriodo_LancaInvalidOperationException()
        {
            var dto = new AtualizarReservaDto(
                DataCheckIn: new DateTime(2026, 1, 10),
                DataCheckOut: new DateTime(2026, 1, 12));

            var reserva = new Reserva(
                dataCheckIn: new DateTime(2026, 1, 1),
                dataCheckOut: new DateTime(2026, 1, 5),
                status: StatusReserva.Confirmada,
                usuarioId: 1,
                quartoId: 3)
            { Id = 1 };

            _reservaRepositoryMock
                .Setup(r => r.ObterPorIdAsync(1))
                .ReturnsAsync(reserva);

            _reservaRepositoryMock
                .Setup(r => r.ExisteConflitoAsync(
                    reserva.QuartoId, dto.DataCheckIn, dto.DataCheckOut, reserva.Id))
                .ReturnsAsync(true);

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.AtualizarReserva(id: 1, dto, usuarioId: 1));
        }

        [Fact]
        public async Task AtualizarReserva_CenarioValido_AtualizaDatasEChamaRepositorio()
        {
            var dto = new AtualizarReservaDto(
                DataCheckIn: new DateTime(2026, 1, 10),
                DataCheckOut: new DateTime(2026, 1, 12));

            var reserva = new Reserva(
                dataCheckIn: new DateTime(2026, 1, 1),
                dataCheckOut: new DateTime(2026, 1, 5),
                status: StatusReserva.Confirmada,
                usuarioId: 1,
                quartoId: 3)
            { Id = 1 };

            _reservaRepositoryMock
                .Setup(r => r.ObterPorIdAsync(1))
                .ReturnsAsync(reserva);

            _reservaRepositoryMock
                .Setup(r => r.ExisteConflitoAsync(
                    reserva.QuartoId, dto.DataCheckIn, dto.DataCheckOut, reserva.Id))
                .ReturnsAsync(false);

            await _service.AtualizarReserva(id: 1, dto, usuarioId: 1);

            _reservaRepositoryMock.Verify(
                r => r.AtualizarAsync(It.Is<Reserva>(
                    res => res.DataCheckIn == dto.DataCheckIn
                        && res.DataCheckOut == dto.DataCheckOut)),
                Times.Once);
        }

        

        [Fact]
        public async Task DeletarReserva_ReservaInexistente_LancaKeyNotFoundException()
        {
            _reservaRepositoryMock
                .Setup(r => r.ObterPorIdAsync(1))
                .ReturnsAsync((Reserva?)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _service.DeletarReserva(id: 1, usuarioId: 1));
        }

        [Fact]
        public async Task DeletarReserva_UsuarioNaoEDono_LancaUnauthorizedAccessException()
        {
            var reserva = new Reserva(
                dataCheckIn: new DateTime(2026, 1, 1),
                dataCheckOut: new DateTime(2026, 1, 5),
                status: StatusReserva.Confirmada,
                usuarioId: 99,
                quartoId: 1)
            { Id = 1 };

            _reservaRepositoryMock
                .Setup(r => r.ObterPorIdAsync(1))
                .ReturnsAsync(reserva);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => _service.DeletarReserva(id: 1, usuarioId: 1));

            _reservaRepositoryMock.Verify(
                r => r.DeletarAsync(It.IsAny<long>()),
                Times.Never);
        }

        [Fact]
        public async Task DeletarReserva_CenarioValido_ChamaRepositorio()
        {
            var reserva = new Reserva(
                dataCheckIn: new DateTime(2026, 1, 1),
                dataCheckOut: new DateTime(2026, 1, 5),
                status: StatusReserva.Confirmada,
                usuarioId: 1,
                quartoId: 1)
            { Id = 1 };

            _reservaRepositoryMock
                .Setup(r => r.ObterPorIdAsync(1))
                .ReturnsAsync(reserva);

            await _service.DeletarReserva(id: 1, usuarioId: 1);

            _reservaRepositoryMock.Verify(
                r => r.DeletarAsync(1),
                Times.Once);
        }

        

        [Fact]
        public async Task CancelarReserva_ReservaInexistente_LancaKeyNotFoundException()
        {
            _reservaRepositoryMock
                .Setup(r => r.ObterPorIdAsync(1))
                .ReturnsAsync((Reserva?)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _service.CancelarReserva(id: 1, usuarioId: 1));
        }

        [Fact]
        public async Task CancelarReserva_UsuarioNaoEDono_LancaUnauthorizedAccessException()
        {
            var reserva = new Reserva(
                dataCheckIn: new DateTime(2026, 1, 1),
                dataCheckOut: new DateTime(2026, 1, 5),
                status: StatusReserva.Confirmada,
                usuarioId: 99,
                quartoId: 1)
            { Id = 1 };

            _reservaRepositoryMock
                .Setup(r => r.ObterPorIdAsync(1))
                .ReturnsAsync(reserva);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => _service.CancelarReserva(id: 1, usuarioId: 1));
        }

        [Fact]
        public async Task CancelarReserva_JaCancelada_LancaInvalidOperationException()
        {
            var reserva = new Reserva(
                dataCheckIn: new DateTime(2026, 1, 1),
                dataCheckOut: new DateTime(2026, 1, 5),
                status: StatusReserva.Cancelada,
                usuarioId: 1,
                quartoId: 1)
            { Id = 1 };

            _reservaRepositoryMock
                .Setup(r => r.ObterPorIdAsync(1))
                .ReturnsAsync(reserva);

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.CancelarReserva(id: 1, usuarioId: 1));
        }

        [Fact]
        public async Task CancelarReserva_Finalizada_LancaInvalidOperationException()
        {
            var reserva = new Reserva(
                dataCheckIn: new DateTime(2026, 1, 1),
                dataCheckOut: new DateTime(2026, 1, 5),
                status: StatusReserva.Finalizada,
                usuarioId: 1,
                quartoId: 1)
            { Id = 1 };

            _reservaRepositoryMock
                .Setup(r => r.ObterPorIdAsync(1))
                .ReturnsAsync(reserva);

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.CancelarReserva(id: 1, usuarioId: 1));
        }

        [Fact]
        public async Task CancelarReserva_CenarioValido_ChamaRepositorio()
        {
            var reserva = new Reserva(
                dataCheckIn: new DateTime(2026, 1, 1),
                dataCheckOut: new DateTime(2026, 1, 5),
                status: StatusReserva.Confirmada,
                usuarioId: 1,
                quartoId: 1)
            { Id = 1 };

            _reservaRepositoryMock
                .Setup(r => r.ObterPorIdAsync(1))
                .ReturnsAsync(reserva);

            await _service.CancelarReserva(id: 1, usuarioId: 1);

            _reservaRepositoryMock.Verify(
                r => r.CancelarAsync(1),
                Times.Once);
        }

        

        [Fact]
        public async Task BuscarPorId_ReservaInexistente_RetornaNull()
        {
            _reservaRepositoryMock
                .Setup(r => r.ObterPorIdAsync(1))
                .ReturnsAsync((Reserva?)null);

            var resultado = await _service.BuscarPorId(1);

            Assert.Null(resultado);
        }

        [Fact]
        public async Task BuscarPorId_ReservaExistente_RetornaDtoCorrespondente()
        {
            var reserva = new Reserva(
                dataCheckIn: new DateTime(2026, 1, 1),
                dataCheckOut: new DateTime(2026, 1, 5),
                status: StatusReserva.Confirmada,
                usuarioId: 1,
                quartoId: 2)
            { Id = 10 };

            _reservaRepositoryMock
                .Setup(r => r.ObterPorIdAsync(10))
                .ReturnsAsync(reserva);

            var resultado = await _service.BuscarPorId(10);

            Assert.NotNull(resultado);
            Assert.Equal(reserva.Id, resultado!.Id);
            Assert.Equal(reserva.QuartoId, resultado.QuartoId);
            Assert.Equal(reserva.UsuarioId, resultado.UsuarioId);
            Assert.Equal(reserva.Status, resultado.Status);
        }

        [Fact]
        public async Task ListarReservasPorUsuario_RetornaTodasMapeadas()
        {
            var reservas = new List<Reserva>
            {
                new Reserva(new DateTime(2026, 1, 1), new DateTime(2026, 1, 5), StatusReserva.Confirmada, 1, 1) { Id = 1 },
                new Reserva(new DateTime(2026, 2, 1), new DateTime(2026, 2, 5), StatusReserva.Pendente, 1, 2) { Id = 2 }
            };

            _reservaRepositoryMock
                .Setup(r => r.ObterPorUsuarioAsync(1))
                .ReturnsAsync(reservas);

            var resultado = await _service.ListarReservasPorUsuario(1);

            Assert.Equal(2, resultado.Count());
            Assert.All(resultado, r => Assert.Equal(1, r.UsuarioId));
        }

        [Fact]
        public async Task ListarHistoricoPorUsuario_RetornaTodasMapeadas()
        {
            var reservas = new List<Reserva>
            {
                new Reserva(new DateTime(2025, 1, 1), new DateTime(2025, 1, 5), StatusReserva.Finalizada, 1, 1) { Id = 1 },
                new Reserva(new DateTime(2025, 2, 1), new DateTime(2025, 2, 5), StatusReserva.Cancelada, 1, 2) { Id = 2 }
            };

            _reservaRepositoryMock
                .Setup(r => r.ObterHistoricoPorUsuarioAsync(1))
                .ReturnsAsync(reservas);

            var resultado = await _service.ListarHistoricoPorUsuario(1);

            Assert.Equal(2, resultado.Count());
        }

        [Fact]
        public async Task ListarReserva_RetornaTodasMapeadas()
        {
            var reservas = new List<Reserva>
            {
                new Reserva(new DateTime(2026, 1, 1), new DateTime(2026, 1, 5), StatusReserva.Confirmada, 1, 1) { Id = 1 },
                new Reserva(new DateTime(2026, 2, 1), new DateTime(2026, 2, 5), StatusReserva.Pendente, 2, 2) { Id = 2 }
            };

            _reservaRepositoryMock
                .Setup(r => r.ObterTodosAsync())
                .ReturnsAsync(reservas);

            var resultado = await _service.ListarReserva();

            Assert.Equal(2, resultado.Count());
        }
    }
}
