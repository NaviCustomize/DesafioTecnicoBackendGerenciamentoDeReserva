using SistemaGerenciamentoDeReserva.Application.DTOs.Reserva;
using SistemaGerenciamentoDeReserva.Application.Interface;
using SistemaGerenciamentoDeReserva.Domain.Enums;
using SistemaGerenciamentoDeReserva.Domain.Interfaces;

namespace SistemaGerenciamentoDeReserva.Infrastruture.Messaging
{
    public class LembreteCheckInWorker : BackgroundService
    {
        private static readonly TimeSpan Intervalo = TimeSpan.FromMinutes(30);

        private readonly ILogger<LembreteCheckInWorker> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        private readonly HashSet<long> _jaLembradas = [];

        public LembreteCheckInWorker(
            ILogger<LembreteCheckInWorker> logger,
            IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await PublicarLembretesAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Falha ao publicar lembretes de check-in.");
                }

                try
                {
                    await Task.Delay(Intervalo, stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    return;
                }
            }
        }

        private async Task PublicarLembretesAsync()
        {
            using var escopo = _scopeFactory.CreateScope();

            var reservaRepository = escopo.ServiceProvider.GetRequiredService<IReservaRepository>();
            var quartoRepository = escopo.ServiceProvider.GetRequiredService<IQuartoRepository>();
            var usuarioRepository = escopo.ServiceProvider.GetRequiredService<IUsuarioRepository>();
            var hotelRepository = escopo.ServiceProvider.GetRequiredService<IHotelRepository>();
            var publisher = escopo.ServiceProvider.GetRequiredService<IReservaNotificacaoPublisher>();

            var amanha = DateTime.Today.AddDays(1);

            var reservas = await reservaRepository.ObterTodosAsync();

            var doDiaSeguinte = reservas.Where(r =>
                r.DataCheckIn.Date == amanha &&
                (r.Status == StatusReserva.Pendente || r.Status == StatusReserva.Confirmada) &&
                !_jaLembradas.Contains(r.Id));

            foreach (var reserva in doDiaSeguinte)
            {
                var usuario = await usuarioRepository.ObterPorIdAsync(reserva.UsuarioId);
                var quarto = await quartoRepository.ObterPorIdAsync(reserva.QuartoId);

                var hotel = quarto is null
                    ? null
                    : await hotelRepository.ObterPorIdAsync(quarto.HotelId);

                await publisher.PublicarAsync(new ReservaNotificacaoEvento(
                    reserva.Id,
                    reserva.UsuarioId,
                    reserva.QuartoId,
                    "Lembrete",
                    reserva.DataCheckIn,
                    reserva.DataCheckOut,
                    DateTime.UtcNow,
                    usuario?.NomeCompleto ?? string.Empty,
                    usuario?.Email ?? string.Empty,
                    hotel?.Nome ?? string.Empty,
                    quarto?.Numero ?? 0));

                _jaLembradas.Add(reserva.Id);

                _logger.LogInformation(
                    "Lembrete publicado: reserva {ReservaId}, check-in {DataCheckIn:d}",
                    reserva.Id, reserva.DataCheckIn);
            }
        }
    }
}
