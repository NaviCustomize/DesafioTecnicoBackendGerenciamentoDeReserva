using SistemaGerenciamentoDeReserva.Domain.Entity;

namespace SistemaGerenciamentoDeReserva.Domain.Interfaces
{
    public interface INotificacaoRepository
    {
        Task<long> AdicionarAsync(Notificacao notificacao);

        Task<IEnumerable<Notificacao>> ObterRecentesAsync(int limite);
    }
}
