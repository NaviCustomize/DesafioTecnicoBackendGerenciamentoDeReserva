using SistemaGerenciamentoDeReserva.Domain.Enums;

namespace SistemaGerenciamentoDeReserva.Domain.Entity
{
    public class Quarto
    {
        public long Id { get; set; }
        public long HotelId { get; set; }
        public int Numero { get; set; }
        public TipoQuarto Tipo { get; set; }
        public  decimal PrecoPorNoite { get; set; }
        public StatusQuarto Status { get; set; }

        public Quarto() { }
        
        public Quarto(long hotelId, int numero, TipoQuarto tipo, decimal precoPorNoite, StatusQuarto status)
        {
            HotelId = hotelId;
            Numero = numero;
            Tipo = tipo;
            PrecoPorNoite = precoPorNoite;
            Status = status;
        }
    }
}
