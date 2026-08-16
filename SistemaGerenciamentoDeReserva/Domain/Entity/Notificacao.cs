namespace SistemaGerenciamentoDeReserva.Domain.Entity
{
    public class Notificacao
    {
        public long Id { get; set; }
        public long ReservaId { get; set; }
        public long UsuarioId { get; set; }
        public long QuartoId { get; set; }
        public string TipoEvento { get; set; } = string.Empty;
        public string Hospede { get; set; } = string.Empty;
        public string HospedeEmail { get; set; } = string.Empty;
        public string Hotel { get; set; } = string.Empty;
        public int QuartoNumero { get; set; }
        public DateTime DataCheckIn { get; set; }
        public DateTime DataCheckOut { get; set; }
        public DateTime OcorridoEm { get; set; }
        public DateTime ProcessadoEm { get; set; }
    }
}
