using SistemaGerenciamentoDeReserva.Domain.Entity;

namespace SistemaGerenciamentoDeReserva.Domain.Interfaces
{
    public interface IJwtService
    {
        string GerarToken(Usuario usuario);
    }
}
