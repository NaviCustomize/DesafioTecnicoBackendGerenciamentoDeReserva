using System.Text.Json;
using SistemaGerenciamentoDeReserva.Application.DTOs.Reserva;

namespace SistemaGerenciamentoDeReserva.Tests.Messaging
{
    public class ReservaNotificacaoEventoTests
    {
        private static ReservaNotificacaoEvento EventoCompleto() => new(
            ReservaId: 42,
            UsuarioId: 7,
            QuartoId: 13,
            TipoEvento: "Confirmada",
            DataCheckIn: new DateTime(2026, 9, 10, 14, 0, 0, DateTimeKind.Unspecified),
            DataCheckOut: new DateTime(2026, 9, 12, 12, 0, 0, DateTimeKind.Unspecified),
            OcorridoEmUtc: new DateTime(2026, 8, 17, 10, 30, 0, DateTimeKind.Utc),
            Hospede: "Larissa Andrade",
            HospedeEmail: "larissa.andrade@hospede.com",
            Hotel: "Hotel Quitandinha",
            QuartoNumero: 305);

        [Fact]
        public void Evento_DeveSobreviverAoCicloDeSerializacao()
        {
            var original = EventoCompleto();

            var bytes = JsonSerializer.SerializeToUtf8Bytes(original);
            var recuperado = JsonSerializer.Deserialize<ReservaNotificacaoEvento>(bytes);

            Assert.NotNull(recuperado);
            Assert.Equal(original, recuperado);
        }

        [Fact]
        public void Evento_DeveCarregarOsDadosLegiveisDoHospede()
        {
            var bytes = JsonSerializer.SerializeToUtf8Bytes(EventoCompleto());
            var recuperado = JsonSerializer.Deserialize<ReservaNotificacaoEvento>(bytes)!;

            Assert.Equal("Larissa Andrade", recuperado.Hospede);
            Assert.Equal("larissa.andrade@hospede.com", recuperado.HospedeEmail);
            Assert.Equal("Hotel Quitandinha", recuperado.Hotel);
            Assert.Equal(305, recuperado.QuartoNumero);
        }

        [Fact]
        public void Evento_DevePreservarOsHorariosDeCheckInECheckOut()
        {
            var bytes = JsonSerializer.SerializeToUtf8Bytes(EventoCompleto());
            var recuperado = JsonSerializer.Deserialize<ReservaNotificacaoEvento>(bytes)!;

            Assert.Equal(14, recuperado.DataCheckIn.Hour);
            Assert.Equal(12, recuperado.DataCheckOut.Hour);
        }

        [Fact]
        public void Evento_DeveAceitarCamposLegiveisVazios()
        {
            var semNomes = new ReservaNotificacaoEvento(
                ReservaId: 1,
                UsuarioId: 2,
                QuartoId: 3,
                TipoEvento: "Cancelada",
                DataCheckIn: DateTime.Today,
                DataCheckOut: DateTime.Today.AddDays(1),
                OcorridoEmUtc: DateTime.UtcNow);

            var bytes = JsonSerializer.SerializeToUtf8Bytes(semNomes);
            var recuperado = JsonSerializer.Deserialize<ReservaNotificacaoEvento>(bytes)!;

            Assert.Equal(string.Empty, recuperado.Hospede);
            Assert.Equal(0, recuperado.QuartoNumero);
            Assert.Equal("Cancelada", recuperado.TipoEvento);
        }
    }
}
