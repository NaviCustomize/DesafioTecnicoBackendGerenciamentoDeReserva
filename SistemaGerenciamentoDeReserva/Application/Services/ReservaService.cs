using SistemaGerenciamentoDeReserva.Application.DTOs.Reserva;
using SistemaGerenciamentoDeReserva.Application.Interface;
using SistemaGerenciamentoDeReserva.Domain.Entity;
using SistemaGerenciamentoDeReserva.Domain.Enums;
using SistemaGerenciamentoDeReserva.Domain.Interfaces;

namespace SistemaGerenciamentoDeReserva.Application.Services
{
    public class ReservaService : IReservaService
    {

        private static readonly TimeSpan HoraDoCheckIn = new(14, 0, 0);
        private static readonly TimeSpan HoraDoCheckOut = new(12, 0, 0);

        private readonly IReservaRepository _reservaRepository;
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IHotelRepository _hotelRepository;
        private readonly IQuartoRepository _quartoRepository;
        private readonly IReservaNotificacaoPublisher _notificacaoPublisher;

        public ReservaService(
            IReservaRepository reservaRepository,
            IQuartoRepository quartoRepository,
            IUsuarioRepository usuarioRepository,
            IHotelRepository hotelRepository,
            IReservaNotificacaoPublisher notificacaoPublisher)
        {
            _reservaRepository = reservaRepository;
            _quartoRepository = quartoRepository;
            _usuarioRepository = usuarioRepository;
            _hotelRepository = hotelRepository;
            _notificacaoPublisher = notificacaoPublisher;
        }


        private async Task PublicarEventoAsync(Reserva reserva, string tipoEvento)
        {
            var hospede = string.Empty;
            var hospedeEmail = string.Empty;
            var hotel = string.Empty;
            var quartoNumero = 0;

            try
            {
                var usuario = await _usuarioRepository.ObterPorIdAsync(reserva.UsuarioId);

                if (usuario is not null)
                {
                    hospede = usuario.NomeCompleto;
                    hospedeEmail = usuario.Email;
                }

                var quarto = await _quartoRepository.ObterPorIdAsync(reserva.QuartoId);

                if (quarto is not null)
                {
                    quartoNumero = quarto.Numero;

                    var hotelDoQuarto = await _hotelRepository.ObterPorIdAsync(quarto.HotelId);
                    hotel = hotelDoQuarto?.Nome ?? string.Empty;
                }
            }
            catch (Exception)
            {
            }

            await _notificacaoPublisher.PublicarAsync(new ReservaNotificacaoEvento(
                reserva.Id,
                reserva.UsuarioId,
                reserva.QuartoId,
                tipoEvento,
                reserva.DataCheckIn,
                reserva.DataCheckOut,
                DateTime.UtcNow,
                hospede,
                hospedeEmail,
                hotel,
                quartoNumero));
        }


        private static DateTime ComHorarioPadrao(DateTime data, TimeSpan hora) =>
            data.Date + hora;

        public async Task<ReservaResponseDto> AdicionarReserva(
            CriarReservaDto dto,
            long usuarioId)
        {
            if (dto.DataCheckIn.Date >= dto.DataCheckOut.Date)
            {
                throw new ArgumentException(
                    "A data de check-in deve ser anterior à data de check-out.");
            }

            var checkIn = ComHorarioPadrao(dto.DataCheckIn, HoraDoCheckIn);
            var checkOut = ComHorarioPadrao(dto.DataCheckOut, HoraDoCheckOut);

            var quarto = await _quartoRepository.ObterPorIdAsync(dto.QuartoId);

            if (quarto is null)
            {
                throw new KeyNotFoundException(
                    "Quarto não encontrado.");
            }


            var conflito = await _reservaRepository.ExisteConflitoAsync(dto.QuartoId, checkIn, checkOut);

            if (conflito)
            {
                throw new InvalidOperationException(
                    "Já existe uma reserva para este quarto neste período.");
            }

            var reserva = new Reserva
            {
                UsuarioId = usuarioId,
                QuartoId = dto.QuartoId,
                DataCheckIn = checkIn,
                DataCheckOut = checkOut,
                Status = StatusReserva.Confirmada
            };

            var id = await _reservaRepository.AdicionarAsync(reserva);

            reserva.Id = id;

            await SincronizarStatusDoQuarto(dto.QuartoId);

            await PublicarEventoAsync(reserva, "Confirmada");

            return MapearParaDto(reserva);
        }

        public async Task<ReservaResponseDto?> BuscarPorId(long id)
        {
            var reserva = await _reservaRepository.ObterPorIdAsync(id);

            if (reserva is null)
                return null;

            return MapearParaDto(reserva);
        }

        public async Task<IEnumerable<ReservaResponseDto>> ListarReservasPorUsuario(
        long usuarioId)
        {
            var reservas = await _reservaRepository
                .ObterPorUsuarioAsync(usuarioId);

            return reservas.Select(MapearParaDto);
        }

        public async Task<IEnumerable<ReservaResponseDto>> ListarReserva()
        {
            var reservas = await _reservaRepository.ObterTodosAsync();

            return reservas.Select(reserva => MapearParaDto(reserva));
        }

        public async Task<IEnumerable<ReservaResponseDto>>ListarHistoricoPorUsuario(long usuarioId)
        {
            var reservas = await _reservaRepository.ObterHistoricoPorUsuarioAsync(usuarioId);

            return reservas.Select(MapearParaDto);
        }

        public async Task AtualizarReserva(
            long id,
            AtualizarReservaDto dto,
            long usuarioId)
        {
            if (dto.DataCheckIn.Date >= dto.DataCheckOut.Date)
            {
                throw new ArgumentException(
                    "A data de check-in deve ser anterior à data de check-out.");
            }

            var checkIn = ComHorarioPadrao(dto.DataCheckIn, HoraDoCheckIn);
            var checkOut = ComHorarioPadrao(dto.DataCheckOut, HoraDoCheckOut);

            var reserva = await _reservaRepository.ObterPorIdAsync(id);

            if (reserva is null)
            {
                throw new KeyNotFoundException(
                    "Reserva não encontrada.");
            }

            if (reserva.UsuarioId != usuarioId)
            {
                throw new UnauthorizedAccessException(
                    "Você não pode alterar esta reserva.");
            }

            if (reserva.Status == StatusReserva.Cancelada)
            {
                throw new InvalidOperationException(
                    "Uma reserva cancelada não pode ser alterada.");
            }

            var conflito = await _reservaRepository.ExisteConflitoAsync(
                    reserva.QuartoId,
                    checkIn,
                    checkOut,
                    reserva.Id);

            if (conflito)
            {
                throw new InvalidOperationException(
                    "Já existe uma reserva para este quarto neste período.");
            }

            reserva.DataCheckIn = checkIn;
            reserva.DataCheckOut = checkOut;

            await _reservaRepository.AtualizarAsync(reserva);

            await PublicarEventoAsync(reserva, "Atualizada");
        }

        public async Task DeletarReserva(
            long id,
            long usuarioId)
        {
            var reserva = await _reservaRepository.ObterPorIdAsync(id);

            if (reserva is null)
            {
                throw new KeyNotFoundException(
                    "Reserva não encontrada.");
            }

            if (reserva.UsuarioId != usuarioId)
            {
                throw new UnauthorizedAccessException(
                    "Você não pode excluir esta reserva.");
            }

            await _reservaRepository.DeletarAsync(id);
        }

        private static ReservaResponseDto MapearParaDto(
        Reserva reserva)
        {
            return new ReservaResponseDto(
                reserva.Id,
                reserva.DataCheckIn,
                reserva.DataCheckOut,
                reserva.Status,
                reserva.UsuarioId,
                reserva.QuartoId
            );
        }

        public async Task CancelarReserva(long id, long usuarioId)
        {
            var reserva = await _reservaRepository.ObterPorIdAsync(id);

            if (reserva == null)
                throw new KeyNotFoundException("Reserva não encontrada.");

            if (reserva.UsuarioId != usuarioId)
                throw new UnauthorizedAccessException(
                    "Você não pode cancelar esta reserva.");

            if (reserva.Status == StatusReserva.Cancelada)
                throw new InvalidOperationException(
                    "A reserva já está cancelada.");

            if (reserva.Status == StatusReserva.Finalizada)
                throw new InvalidOperationException(
                    "Não é possível cancelar uma reserva finalizada.");

            await _reservaRepository.CancelarAsync(id);

            await SincronizarStatusDoQuarto(reserva.QuartoId);

            await PublicarEventoAsync(reserva, "Cancelada");
        }


        private async Task SincronizarStatusDoQuarto(long quartoId)
        {
            var quarto = await _quartoRepository.ObterPorIdAsync(quartoId);

            if (quarto is null)
            {
                return;
            }

            var reservas = await _reservaRepository.ObterPorQuartoAsync(quartoId);

            var temReservaAtiva = reservas.Any(r =>
                r.Status == StatusReserva.Pendente ||
                r.Status == StatusReserva.Confirmada);

            var novoStatus = temReservaAtiva
                ? StatusQuarto.Reservado
                : StatusQuarto.Disponivel;

            if (quarto.Status == novoStatus)
            {
                return;
            }

            quarto.Status = novoStatus;

            await _quartoRepository.AtualizarAsync(quarto);
        }
    }
}