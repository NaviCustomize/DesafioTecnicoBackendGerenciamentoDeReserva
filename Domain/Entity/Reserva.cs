using SistemaGerenciamentoDeReserva.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaGerenciamentoDeReserva.Domain.Entity
{
    public class Reserva
    {
        public long Id { get; set; }
        public DateTime DataCheckIn { get; set; }
        public DateTime DataCheckOut { get; set; }
        public StatusReserva Status { get; set; }

        public long UsuarioId { get; set; }

        public long HotelId { get; set; }

        public Reserva() { }

        public Reserva(DateTime dataCheckIn, DateTime dataCheckOut, StatusReserva status, long usuarioId, long hotelId)
        {
            DataCheckIn = dataCheckIn;
            DataCheckOut = dataCheckOut;
            Status = status;
            UsuarioId = usuarioId;
            HotelId = hotelId;
        }
    }
}
