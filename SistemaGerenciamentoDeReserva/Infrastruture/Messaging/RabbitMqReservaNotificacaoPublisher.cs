using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using SistemaGerenciamentoDeReserva.Application.DTOs.Reserva;
using SistemaGerenciamentoDeReserva.Application.Interface;

namespace SistemaGerenciamentoDeReserva.Infrastruture.Messaging
{
    public class RabbitMqReservaNotificacaoPublisher : IReservaNotificacaoPublisher, IAsyncDisposable
    {
        private readonly RabbitMqSettings _settings;
        private readonly ILogger<RabbitMqReservaNotificacaoPublisher> _logger;
        private IConnection? _connection;

        public RabbitMqReservaNotificacaoPublisher(
            IOptions<RabbitMqSettings> settings,
            ILogger<RabbitMqReservaNotificacaoPublisher> logger)
        {
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task PublicarAsync(ReservaNotificacaoEvento evento)
        {
            try
            {
                var connection = await ObterConexaoAsync();

                await using var channel = await connection.CreateChannelAsync();

                await channel.ExchangeDeclareAsync(
                    exchange: ReservaMessagingConstantes.Exchange,
                    type: ExchangeType.Topic,
                    durable: true);

                var body = JsonSerializer.SerializeToUtf8Bytes(evento);
                var routingKey = ReservaMessagingConstantes.RoutingKeyPara(evento.TipoEvento);

                var properties = new BasicProperties
                {
                    ContentType = "application/json",
                    Persistent = true
                };

                await channel.BasicPublishAsync(
                    exchange: ReservaMessagingConstantes.Exchange,
                    routingKey: routingKey,
                    mandatory: false,
                    basicProperties: properties,
                    body: body);
            }
            catch (BrokerUnreachableException ex)
            {
                _logger.LogWarning(ex, "RabbitMQ indisponível — notificação da reserva {ReservaId} não foi publicada.", evento.ReservaId);
            }
            catch (AlreadyClosedException ex)
            {
                _logger.LogWarning(ex, "Conexão com RabbitMQ fechada — notificação da reserva {ReservaId} não foi publicada.", evento.ReservaId);
            }
        }

        private async Task<IConnection> ObterConexaoAsync()
        {
            if (_connection is { IsOpen: true })
                return _connection;

            var factory = new ConnectionFactory
            {
                HostName = _settings.HostName,
                Port = _settings.Port,
                UserName = _settings.UserName,
                Password = _settings.Password
            };

            _connection = await factory.CreateConnectionAsync();
            return _connection;
        }

        public async ValueTask DisposeAsync()
        {
            if (_connection is not null)
                await _connection.DisposeAsync();
        }
    }
}
