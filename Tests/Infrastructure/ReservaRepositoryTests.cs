using SistemaGerenciamentoDeReserva.Domain.Entity;
using SistemaGerenciamentoDeReserva.Domain.Enums;
using SistemaGerenciamentoDeReserva.Infrastruture.Repositories;

namespace SistemaGerenciamentoDeReserva.Tests.Infrastructure
{
    public class ReservaRepositoryTests : RepositoryTestBase
    {
        private static Reserva NovaReserva(
            long usuarioId,
            long quartoId,
            int entradaEm,
            int saidaEm,
            StatusReserva status = StatusReserva.Confirmada) => new()
            {
                UsuarioId = usuarioId,
                QuartoId = quartoId,
                DataCheckIn = CheckIn(entradaEm),
                DataCheckOut = CheckOut(saidaEm),
                Status = status
            };

        [Fact]
        public async Task AdicionarAsync_DeveRetornarIdGerado()
        {
            var usuarioId = await SemearUsuarioAsync();
            var quartoId = await SemearQuartoAsync();

            using var conexao = CriarConexao();
            var repositorio = new ReservaRepository(conexao);

            var id = await repositorio.AdicionarAsync(NovaReserva(usuarioId, quartoId, 10, 12));

            Assert.True(id > 0);
        }

        [Fact]
        public async Task ObterPorIdAsync_DevePreservarDatasEStatus()
        {
            var usuarioId = await SemearUsuarioAsync();
            var quartoId = await SemearQuartoAsync();

            using var conexao = CriarConexao();
            var repositorio = new ReservaRepository(conexao);

            var reserva = NovaReserva(usuarioId, quartoId, 10, 12);
            var id = await repositorio.AdicionarAsync(reserva);

            var encontrada = await repositorio.ObterPorIdAsync(id);

            Assert.NotNull(encontrada);
            Assert.Equal(usuarioId, encontrada!.UsuarioId);
            Assert.Equal(quartoId, encontrada.QuartoId);
            Assert.Equal(reserva.DataCheckIn, encontrada.DataCheckIn);
            Assert.Equal(reserva.DataCheckOut, encontrada.DataCheckOut);
            Assert.Equal(StatusReserva.Confirmada, encontrada.Status);
        }

        [Fact]
        public async Task ObterPorUsuarioAsync_NaoDeveRetornarReservaDeOutroUsuario()
        {
            var dono = await SemearUsuarioAsync();
            var outro = await SemearUsuarioAsync();
            var quartoId = await SemearQuartoAsync();

            using var conexao = CriarConexao();
            var repositorio = new ReservaRepository(conexao);

            var doDono = await repositorio.AdicionarAsync(NovaReserva(dono, quartoId, 10, 12));
            var doOutro = await repositorio.AdicionarAsync(NovaReserva(outro, quartoId, 20, 22));

            var doUsuario = await repositorio.ObterPorUsuarioAsync(dono);

            Assert.Contains(doUsuario, r => r.Id == doDono);
            Assert.DoesNotContain(doUsuario, r => r.Id == doOutro);
        }

        [Fact]
        public async Task ObterPorQuartoAsync_DeveRetornarReservasDoQuarto()
        {
            var usuarioId = await SemearUsuarioAsync();
            var quartoId = await SemearQuartoAsync();

            using var conexao = CriarConexao();
            var repositorio = new ReservaRepository(conexao);

            var id = await repositorio.AdicionarAsync(NovaReserva(usuarioId, quartoId, 30, 32));

            Assert.Contains(await repositorio.ObterPorQuartoAsync(quartoId), r => r.Id == id);
        }

        [Fact]
        public async Task ExisteConflitoAsync_DeveAcusarPeriodoSobreposto()
        {
            var usuarioId = await SemearUsuarioAsync();
            var quartoId = await SemearQuartoAsync();

            using var conexao = CriarConexao();
            var repositorio = new ReservaRepository(conexao);

            await repositorio.AdicionarAsync(NovaReserva(usuarioId, quartoId, 10, 12));

            var conflito = await repositorio.ExisteConflitoAsync(quartoId, CheckIn(11), CheckOut(13));

            Assert.True(conflito);
        }

        [Fact]
        public async Task ExisteConflitoAsync_NaoDeveAcusarQuandoUmSaiEOutroEntraNoMesmoDia()
        {
            var usuarioId = await SemearUsuarioAsync();
            var quartoId = await SemearQuartoAsync();

            using var conexao = CriarConexao();
            var repositorio = new ReservaRepository(conexao);

            // Hóspede A: entra no dia 10 às 14h, sai no dia 12 às 12h.
            await repositorio.AdicionarAsync(NovaReserva(usuarioId, quartoId, 10, 12));

            // Hóspede B: entra no dia 12 às 14h. Não conflita, porque A já saiu ao meio-dia.
            var conflito = await repositorio.ExisteConflitoAsync(quartoId, CheckIn(12), CheckOut(14));

            Assert.False(conflito);
        }

        [Fact]
        public async Task ExisteConflitoAsync_NaoDeveAcusarPeriodoTotalmenteDiferente()
        {
            var usuarioId = await SemearUsuarioAsync();
            var quartoId = await SemearQuartoAsync();

            using var conexao = CriarConexao();
            var repositorio = new ReservaRepository(conexao);

            await repositorio.AdicionarAsync(NovaReserva(usuarioId, quartoId, 10, 12));

            var conflito = await repositorio.ExisteConflitoAsync(quartoId, CheckIn(40), CheckOut(42));

            Assert.False(conflito);
        }

        [Fact]
        public async Task ExisteConflitoAsync_DeveIgnorarAPropriaReservaNaEdicao()
        {
            var usuarioId = await SemearUsuarioAsync();
            var quartoId = await SemearQuartoAsync();

            using var conexao = CriarConexao();
            var repositorio = new ReservaRepository(conexao);

            var id = await repositorio.AdicionarAsync(NovaReserva(usuarioId, quartoId, 10, 12));

            // Editar a própria reserva mantendo o mesmo período não pode acusar conflito.
            var conflito = await repositorio.ExisteConflitoAsync(
                quartoId, CheckIn(10), CheckOut(12), reservaId: id);

            Assert.False(conflito);
        }

        [Fact]
        public async Task ExisteConflitoAsync_DeveIgnorarReservaCancelada()
        {
            var usuarioId = await SemearUsuarioAsync();
            var quartoId = await SemearQuartoAsync();

            using var conexao = CriarConexao();
            var repositorio = new ReservaRepository(conexao);

            var id = await repositorio.AdicionarAsync(NovaReserva(usuarioId, quartoId, 10, 12));
            await repositorio.CancelarAsync(id);

            // Quarto cancelado libera o período.
            var conflito = await repositorio.ExisteConflitoAsync(quartoId, CheckIn(10), CheckOut(12));

            Assert.False(conflito);
        }

        [Fact]
        public async Task AtualizarAsync_DevePersistirNovasDatas()
        {
            var usuarioId = await SemearUsuarioAsync();
            var quartoId = await SemearQuartoAsync();

            using var conexao = CriarConexao();
            var repositorio = new ReservaRepository(conexao);

            var reserva = NovaReserva(usuarioId, quartoId, 10, 12);
            var id = await repositorio.AdicionarAsync(reserva);

            reserva.Id = id;
            reserva.DataCheckIn = CheckIn(50);
            reserva.DataCheckOut = CheckOut(52);

            await repositorio.AtualizarAsync(reserva);

            var atualizada = await repositorio.ObterPorIdAsync(id);

            Assert.NotNull(atualizada);
            Assert.Equal(CheckIn(50), atualizada!.DataCheckIn);
            Assert.Equal(CheckOut(52), atualizada.DataCheckOut);
        }

        [Fact]
        public async Task CancelarAsync_DeveMudarStatusParaCancelada()
        {
            var usuarioId = await SemearUsuarioAsync();
            var quartoId = await SemearQuartoAsync();

            using var conexao = CriarConexao();
            var repositorio = new ReservaRepository(conexao);

            var id = await repositorio.AdicionarAsync(NovaReserva(usuarioId, quartoId, 10, 12));

            await repositorio.CancelarAsync(id);

            var cancelada = await repositorio.ObterPorIdAsync(id);

            Assert.NotNull(cancelada);
            Assert.Equal(StatusReserva.Cancelada, cancelada!.Status);
        }

        [Fact]
        public async Task DeletarAsync_DeveRemoverDasConsultas()
        {
            var usuarioId = await SemearUsuarioAsync();
            var quartoId = await SemearQuartoAsync();

            using var conexao = CriarConexao();
            var repositorio = new ReservaRepository(conexao);

            var id = await repositorio.AdicionarAsync(NovaReserva(usuarioId, quartoId, 10, 12));

            await repositorio.DeletarAsync(id);

            Assert.Null(await repositorio.ObterPorIdAsync(id));
            Assert.DoesNotContain(await repositorio.ObterPorUsuarioAsync(usuarioId), r => r.Id == id);
        }

        [Fact]
        public async Task ObterHistoricoPorUsuarioAsync_DeveConterReservaCancelada()
        {
            var usuarioId = await SemearUsuarioAsync();
            var quartoId = await SemearQuartoAsync();

            using var conexao = CriarConexao();
            var repositorio = new ReservaRepository(conexao);

            var cancelada = await repositorio.AdicionarAsync(NovaReserva(usuarioId, quartoId, 10, 12));
            await repositorio.CancelarAsync(cancelada);

            var ativa = await repositorio.AdicionarAsync(NovaReserva(usuarioId, quartoId, 60, 62));

            var historico = await repositorio.ObterHistoricoPorUsuarioAsync(usuarioId);

            Assert.Contains(historico, r => r.Id == cancelada);
            Assert.DoesNotContain(historico, r => r.Id == ativa);
        }
    }
}
