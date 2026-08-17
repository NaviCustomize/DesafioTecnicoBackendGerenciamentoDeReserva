using SistemaGerenciamentoDeReserva.Domain.Entity;
using SistemaGerenciamentoDeReserva.Infrastruture.Repositories;

namespace SistemaGerenciamentoDeReserva.Tests.Infrastructure
{
    public class NotificacaoRepositoryTests : RepositoryTestBase
    {
        private static Notificacao NovaNotificacao(
            long reservaId, long usuarioId, long quartoId, string tipoEvento = "Confirmada") => new()
            {
                ReservaId = reservaId,
                UsuarioId = usuarioId,
                QuartoId = quartoId,
                TipoEvento = tipoEvento,
                Hospede = TextoUnico("Hospede"),
                HospedeEmail = EmailUnico("hospede"),
                Hotel = TextoUnico("Hotel"),
                QuartoNumero = NumeroUnico(),
                DataCheckIn = CheckIn(10),
                DataCheckOut = CheckOut(12),
                OcorridoEm = DateTime.UtcNow
            };

        private static async Task<(long Usuario, long Quarto, long Reserva)> SemearReservaAsync()
        {
            var usuarioId = await SemearUsuarioAsync();
            var quartoId = await SemearQuartoAsync();

            using var conexao = CriarConexao();

            var reservaId = await new ReservaRepository(conexao).AdicionarAsync(new Reserva
            {
                UsuarioId = usuarioId,
                QuartoId = quartoId,
                DataCheckIn = CheckIn(10),
                DataCheckOut = CheckOut(12),
                Status = Domain.Enums.StatusReserva.Confirmada
            });

            return (usuarioId, quartoId, reservaId);
        }

        [Fact]
        public async Task AdicionarAsync_DeveRetornarIdGerado()
        {
            var (usuarioId, quartoId, reservaId) = await SemearReservaAsync();

            using var conexao = CriarConexao();
            var repositorio = new NotificacaoRepository(conexao);

            var id = await repositorio.AdicionarAsync(
                NovaNotificacao(reservaId, usuarioId, quartoId));

            Assert.True(id > 0);
        }

        [Fact]
        public async Task ObterRecentesAsync_DeveTrazerOsDadosLegiveisDaNotificacao()
        {
            var (usuarioId, quartoId, reservaId) = await SemearReservaAsync();

            using var conexao = CriarConexao();
            var repositorio = new NotificacaoRepository(conexao);

            var notificacao = NovaNotificacao(reservaId, usuarioId, quartoId, "Cancelada");
            await repositorio.AdicionarAsync(notificacao);

            var recentes = await repositorio.ObterRecentesAsync(50);

            var gravada = Assert.Single(recentes, n => n.ReservaId == reservaId);

            Assert.Equal("Cancelada", gravada.TipoEvento);
            Assert.Equal(notificacao.Hospede, gravada.Hospede);
            Assert.Equal(notificacao.HospedeEmail, gravada.HospedeEmail);
            Assert.Equal(notificacao.Hotel, gravada.Hotel);
            Assert.Equal(notificacao.QuartoNumero, gravada.QuartoNumero);
        }

        [Fact]
        public async Task ObterRecentesAsync_DeveRespeitarOLimite()
        {
            var (usuarioId, quartoId, reservaId) = await SemearReservaAsync();

            using var conexao = CriarConexao();
            var repositorio = new NotificacaoRepository(conexao);

            for (var i = 0; i < 3; i++)
            {
                await repositorio.AdicionarAsync(NovaNotificacao(reservaId, usuarioId, quartoId));
            }

            var recentes = await repositorio.ObterRecentesAsync(2);

            Assert.Equal(2, recentes.Count());
        }
    }
}
