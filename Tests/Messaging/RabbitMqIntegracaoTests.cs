using System.Text.Json;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using SistemaGerenciamentoDeReserva.Application.DTOs.Reserva;
using SistemaGerenciamentoDeReserva.Infrastruture.Messaging;

namespace SistemaGerenciamentoDeReserva.Tests.Messaging
{
    public class RabbitMqIntegracaoTests
    {
        /// <summary>
        /// Exchange própria dos testes. Publicar na exchange real faria as mensagens
        /// caírem na fila do sistema, e o consumidor da aplicação gravaria notificações
        /// falsas no banco de desenvolvimento.
        /// </summary>
        private const string ExchangeDeTeste = "reservas.exchange.teste";

        private static RabbitMqSettings Settings() => new()
        {
            HostName = RabbitMqDisponivel.HostName,
            Port = RabbitMqDisponivel.Port,
            UserName = RabbitMqDisponivel.UserName,
            Password = RabbitMqDisponivel.Password,
            Exchange = ExchangeDeTeste
        };

        private static ReservaNotificacaoEvento Evento(string tipo, long reservaId) => new(
            ReservaId: reservaId,
            UsuarioId: 77,
            QuartoId: 88,
            TipoEvento: tipo,
            DataCheckIn: DateTime.Today.AddDays(10).AddHours(14),
            DataCheckOut: DateTime.Today.AddDays(12).AddHours(12),
            OcorridoEmUtc: DateTime.UtcNow,
            Hospede: "Hóspede de Integração",
            HospedeEmail: "integracao@teste.local",
            Hotel: "Hotel de Integração",
            QuartoNumero: 404);

        /// <summary>Cria uma fila temporária amarrada à exchange de reservas.</summary>
        private static async Task<(IConnection Conexao, IChannel Canal, string Fila)> EscutarAsync()
        {
            var fabrica = new ConnectionFactory
            {
                HostName = RabbitMqDisponivel.HostName,
                Port = RabbitMqDisponivel.Port,
                UserName = RabbitMqDisponivel.UserName,
                Password = RabbitMqDisponivel.Password
            };

            var conexao = await fabrica.CreateConnectionAsync();
            var canal = await conexao.CreateChannelAsync();

            // Precisa bater com a declaração do publisher, senão o broker recusa
            // com PRECONDITION_FAILED por argumento divergente.
            await canal.ExchangeDeclareAsync(ExchangeDeTeste, ExchangeType.Topic, durable: true);

            var fila = $"teste.integracao.{Guid.NewGuid():N}";

            await canal.QueueDeclareAsync(fila, durable: false, exclusive: true, autoDelete: true);

            await canal.QueueBindAsync(
                fila, ExchangeDeTeste, ReservaMessagingConstantes.RoutingKeyPattern);

            return (conexao, canal, fila);
        }

        private static async Task<BasicGetResult?> ReceberAsync(IChannel canal, string fila)
        {
            for (var tentativa = 0; tentativa < 30; tentativa++)
            {
                var resultado = await canal.BasicGetAsync(fila, autoAck: true);

                if (resultado is not null)
                    return resultado;

                await Task.Delay(100);
            }

            return null;
        }

        [FatoComRabbitMq]
        public async Task Publisher_DeveEntregarOEventoNaFilaAmarradaNaExchange()
        {
            var (conexao, canal, fila) = await EscutarAsync();

            await using (conexao)
            await using (canal)
            {
                var reservaId = Random.Shared.NextInt64(1_000, 9_999);

                await using var publisher = new RabbitMqReservaNotificacaoPublisher(
                    Options.Create(Settings()),
                    NullLogger<RabbitMqReservaNotificacaoPublisher>.Instance);

                await publisher.PublicarAsync(Evento("Confirmada", reservaId));

                var recebido = await ReceberAsync(canal, fila);

                Assert.NotNull(recebido);

                var evento = JsonSerializer.Deserialize<ReservaNotificacaoEvento>(recebido!.Body.Span);

                Assert.NotNull(evento);
                Assert.Equal(reservaId, evento!.ReservaId);
                Assert.Equal("Confirmada", evento.TipoEvento);
                Assert.Equal("Hóspede de Integração", evento.Hospede);
                Assert.Equal(404, evento.QuartoNumero);
            }
        }

        [FatoComRabbitMq]
        public async Task Publisher_DeveUsarARoutingKeyDoTipoDoEvento()
        {
            var (conexao, canal, fila) = await EscutarAsync();

            await using (conexao)
            await using (canal)
            {
                await using var publisher = new RabbitMqReservaNotificacaoPublisher(
                    Options.Create(Settings()),
                    NullLogger<RabbitMqReservaNotificacaoPublisher>.Instance);

                await publisher.PublicarAsync(Evento("Cancelada", 1234));

                var recebido = await ReceberAsync(canal, fila);

                Assert.NotNull(recebido);
                Assert.Equal("reserva.cancelada", recebido!.RoutingKey);
            }
        }

        [FatoComRabbitMq]
        public async Task Publisher_DevePublicarMensagemPersistenteEmJson()
        {
            var (conexao, canal, fila) = await EscutarAsync();

            await using (conexao)
            await using (canal)
            {
                await using var publisher = new RabbitMqReservaNotificacaoPublisher(
                    Options.Create(Settings()),
                    NullLogger<RabbitMqReservaNotificacaoPublisher>.Instance);

                await publisher.PublicarAsync(Evento("Lembrete", 555));

                var recebido = await ReceberAsync(canal, fila);

                Assert.NotNull(recebido);

                 Assert.True(recebido!.BasicProperties.Persistent);
                Assert.Equal("application/json", recebido.BasicProperties.ContentType);
            }
        }

        [FatoComRabbitMq]
        public async Task Publisher_DeveReaproveitarAConexaoEntrePublicacoes()
        {
            var (conexao, canal, fila) = await EscutarAsync();

            await using (conexao)
            await using (canal)
            {
                await using var publisher = new RabbitMqReservaNotificacaoPublisher(
                    Options.Create(Settings()),
                    NullLogger<RabbitMqReservaNotificacaoPublisher>.Instance);

                await publisher.PublicarAsync(Evento("Confirmada", 1));
                await publisher.PublicarAsync(Evento("Atualizada", 2));
                await publisher.PublicarAsync(Evento("Cancelada", 3));

                var recebidas = 0;

                for (var i = 0; i < 3; i++)
                {
                    if (await ReceberAsync(canal, fila) is not null)
                        recebidas++;
                }

                Assert.Equal(3, recebidas);
            }
        }
    }
}
