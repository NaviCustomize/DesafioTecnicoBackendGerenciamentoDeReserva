using SistemaGerenciamentoDeReserva.Application.DTOs.Reserva;
using SistemaGerenciamentoDeReserva.Application.Interface;
using SistemaGerenciamentoDeReserva.Domain.Entity;
using SistemaGerenciamentoDeReserva.Domain.Enums;
using SistemaGerenciamentoDeReserva.Domain.Interfaces;

namespace SistemaGerenciamentoDeReserva.Application.Services
{
    public class ReservaService : IReservaService
    {
        private readonly IReservaRepository _reservaRepository;
        private readonly IQuartoRepository _quartoRepository;

        public ReservaService(
            IReservaRepository reservaRepository,
            IQuartoRepository quartoRepository)
        {
            _reservaRepository = reservaRepository;
            _quartoRepository = quartoRepository;
        }

        public async Task<ReservaResponseDto> AdicionarReserva(
            CriarReservaDto dto,
            long usuarioId)
        {
            if (dto.DataCheckIn >= dto.DataCheckOut)
            {
                throw new ArgumentException(
                    "A data de check-in deve ser anterior à data de check-out.");
            }

            var quarto = await _quartoRepository.ObterPorIdAsync(dto.QuartoId);

            if (quarto is null)
            {
                throw new KeyNotFoundException(
                    "Quarto não encontrado.");
            }

            if (quarto.Status != StatusQuarto.Disponivel)
            {
                throw new InvalidOperationException(
                    "O quarto não está disponível.");
            }

            var conflito = await _reservaRepository.ExisteConflitoAsync(dto.QuartoId,dto.DataCheckIn,dto.DataCheckOut);

            if (conflito)
            {
                throw new InvalidOperationException(
                    "Já existe uma reserva para este quarto neste período.");
            }

            var reserva = new Reserva
            {
                UsuarioId = usuarioId,
                QuartoId = dto.QuartoId,
                DataCheckIn = dto.DataCheckIn,
                DataCheckOut = dto.DataCheckOut,
                Status = StatusReserva.Confirmada
            };

            var id = await _reservaRepository.AdicionarAsync(reserva);

            reserva.Id = id;

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
            if (dto.DataCheckIn >= dto.DataCheckOut)
            {
                throw new ArgumentException(
                    "A data de check-in deve ser anterior à data de check-out.");
            }

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
                    dto.DataCheckIn,
                    dto.DataCheckOut,
                    reserva.Id);

            if (conflito)
            {
                throw new InvalidOperationException(
                    "Já existe uma reserva para este quarto neste período.");
            }

            reserva.DataCheckIn = dto.DataCheckIn;
            reserva.DataCheckOut = dto.DataCheckOut;

            await _reservaRepository.AtualizarAsync(reserva);
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
        }
    }
}