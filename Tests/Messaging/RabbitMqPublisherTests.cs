using System.Diagnostics;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using SistemaGerenciamentoDeReserva.Application.DTOs.Reserva;
using SistemaGerenciamentoDeReserva.Infrastruture.Messaging;

namespace SistemaGerenciamentoDeReserva.Tests.Messaging
{
    public class RabbitMqPublisherTests
    {
        private const int PortaSemBroker = 5699;

        private static RabbitMqReservaNotificacaoPublisher PublisherSemBroker() =>
            new(Options.Create(new RabbitMqSettings
            {
                HostName = "localhost",
                Port = PortaSemBroker,
                UserName = "guest",
                Password = "guest"
            }),
            NullLogger<RabbitMqReservaNotificacaoPublisher>.Instance);

        private static ReservaNotificacaoEvento Evento(string tipo = "Confirmada") => new(
            ReservaId: 1,
            UsuarioId: 2,
            QuartoId: 3,
            TipoEvento: tipo,
            DataCheckIn: DateTime.Today.AddDays(10).AddHours(14),
            DataCheckOut: DateTime.Today.AddDays(12).AddHours(12),
            OcorridoEmUtc: DateTime.UtcNow,
            Hospede: "Hóspede de Teste",
            HospedeEmail: "hospede@teste.local",
            Hotel: "Hotel de Teste",
            QuartoNumero: 101);

        [Fact]
        public async Task PublicarAsync_NaoDeveLancarQuandoOBrokerEstaForaDoAr()
        {
            await using var publisher = PublisherSemBroker();

            var excecao = await Record.ExceptionAsync(() => publisher.PublicarAsync(Evento()));

            Assert.Null(excecao);
        }

        [Fact]
        public async Task PublicarAsync_DeveFalharRapidoSemBroker()
        {
            await using var publisher = PublisherSemBroker();

            var cronometro = Stopwatch.StartNew();
            await publisher.PublicarAsync(Evento());
            cronometro.Stop();

            Assert.True(
                cronometro.Elapsed < TimeSpan.FromSeconds(15),
                $"A publicação sem broker levou {cronometro.Elapsed.TotalSeconds:0.0}s.");
        }

        [Theory]
        [InlineData("Confirmada")]
        [InlineData("Atualizada")]
        [InlineData("Cancelada")]
        [InlineData("Lembrete")]
        public async Task PublicarAsync_DeveEngolirAFalhaEmTodosOsTiposDeEvento(string tipo)
        {
            await using var publisher = PublisherSemBroker();

            var excecao = await Record.ExceptionAsync(() => publisher.PublicarAsync(Evento(tipo)));

            Assert.Null(excecao);
        }

        [Fact]
        public async Task DisposeAsync_DeveSerSeguroSemConexaoAberta()
        {
            var publisher = PublisherSemBroker();

            var excecao = await Record.ExceptionAsync(async () => await publisher.DisposeAsync());

            Assert.Null(excecao);
        }
    }
}
