using System.Text.Json;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SistemaGerenciamentoDeReserva.Application.DTOs.Reserva;
using SistemaGerenciamentoDeReserva.Domain.Entity;
using SistemaGerenciamentoDeReserva.Domain.Interfaces;

namespace SistemaGerenciamentoDeReserva.Infrastruture.Messaging
{
    public class ReservaNotificacaoConsumer : BackgroundService
    {
        private readonly ILogger<ReservaNotificacaoConsumer> _logger;
        private readonly RabbitMqSettings _settings;
        private readonly IServiceScopeFactory _scopeFactory;
        private IConnection? _connection;
        private IChannel? _channel;

        public ReservaNotificacaoConsumer(
            ILogger<ReservaNotificacaoConsumer> logger,
            IOptions<RabbitMqSettings> settings,
            IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _settings = settings.Value;
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var factory = new ConnectionFactory
            {
                HostName = _settings.HostName,
                Port = _settings.Port,
                UserName = _settings.UserName,
                Password = _settings.Password
            };

            try
            {
                _connection = await factory.CreateConnectionAsync(stoppingToken);
                _channel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken);

                await DeclararTopologiaAsync(stoppingToken);
            }
            catch (RabbitMQ.Client.Exceptions.BrokerUnreachableException ex)
            {
                _logger.LogWarning(ex, "RabbitMQ indisponível — o consumidor de notificações de reserva não será iniciado.");
                return;
            }

            var consumer = new AsyncEventingBasicConsumer(_channel);

            consumer.ReceivedAsync += async (sender, args) =>
            {
                try
                {
                    var evento = JsonSerializer.Deserialize<ReservaNotificacaoEvento>(args.Body.Span)
                        ?? throw new InvalidOperationException("Mensagem vazia ou em formato inesperado.");

                    await RegistrarAsync(evento);

                    _logger.LogInformation(
                        "Notificação registrada: reserva {ReservaId} {TipoEvento} — {Hospede}",
                        evento.ReservaId, evento.TipoEvento, evento.Hospede);

                    await _channel!.BasicAckAsync(args.DeliveryTag, multiple: false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Falha ao processar notificação — mensagem enviada para a dead-letter.");
                    await _channel!.BasicNackAsync(args.DeliveryTag, multiple: false, requeue: false);
                }
            };

            await _channel.BasicConsumeAsync(
                queue: ReservaMessagingConstantes.Queue,
                autoAck: false,
                consumer: consumer,
                cancellationToken: stoppingToken);

            await Task.Delay(Timeout.Infinite, stoppingToken).ContinueWith(_ => { }, TaskScheduler.Default);
        }

        private async Task DeclararTopologiaAsync(CancellationToken stoppingToken)
        {
            await _channel!.ExchangeDeclareAsync(
                exchange: _settings.Exchange,
                type: ExchangeType.Topic,
                durable: true,
                cancellationToken: stoppingToken);

            await _channel.ExchangeDeclareAsync(
                exchange: ReservaMessagingConstantes.DeadLetterExchange,
                type: ExchangeType.Direct,
                durable: true,
                cancellationToken: stoppingToken);

            await _channel.QueueDeclareAsync(
                queue: ReservaMessagingConstantes.DeadLetterQueue,
                durable: true,
                exclusive: false,
                autoDelete: false,
                cancellationToken: stoppingToken);

            await _channel.QueueBindAsync(
                queue: ReservaMessagingConstantes.DeadLetterQueue,
                exchange: ReservaMessagingConstantes.DeadLetterExchange,
                routingKey: ReservaMessagingConstantes.DeadLetterRoutingKey,
                cancellationToken: stoppingToken);

            await _channel.QueueDeclareAsync(
                queue: ReservaMessagingConstantes.Queue,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: new Dictionary<string, object?>
                {
                    ["x-dead-letter-exchange"] = ReservaMessagingConstantes.DeadLetterExchange,
                    ["x-dead-letter-routing-key"] = ReservaMessagingConstantes.DeadLetterRoutingKey
                },
                cancellationToken: stoppingToken);

            await _channel.QueueBindAsync(
                queue: ReservaMessagingConstantes.Queue,
                exchange: _settings.Exchange,
                routingKey: ReservaMessagingConstantes.RoutingKeyPattern,
                cancellationToken: stoppingToken);
        }

        private async Task RegistrarAsync(ReservaNotificacaoEvento evento)
        {
            using var escopo = _scopeFactory.CreateScope();

            var repositorio = escopo.ServiceProvider.GetRequiredService<INotificacaoRepository>();

            await repositorio.AdicionarAsync(new Notificacao
            {
                ReservaId = evento.ReservaId,
                UsuarioId = evento.UsuarioId,
                QuartoId = evento.QuartoId,
                TipoEvento = evento.TipoEvento,
                Hospede = evento.Hospede,
                HospedeEmail = evento.HospedeEmail,
                Hotel = evento.Hotel,
                QuartoNumero = evento.QuartoNumero,
                DataCheckIn = evento.DataCheckIn,
                DataCheckOut = evento.DataCheckOut,
                OcorridoEm = evento.OcorridoEmUtc
            });
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_channel is not null)
                await _channel.CloseAsync(cancellationToken);

            if (_connection is not null)
                await _connection.CloseAsync(cancellationToken);

            await base.StopAsync(cancellationToken);
        }
    }
}
