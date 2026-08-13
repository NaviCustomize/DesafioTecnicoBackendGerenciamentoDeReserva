using SistemaGerenciamentoDeReserva.Domain.Interfaces;

namespace SistemaGerenciamentoDeReserva.Infrastruture.Repositories
{
    public class SenhaHasher : ISenhaHasher
    {
        public string Hash(string senha)
        {
            return BCrypt.Net.BCrypt.HashPassword(senha);
        }

        public bool Verify(string senha, string senhaHash)
        {
            return BCrypt.Net.BCrypt.Verify(senha, senhaHash);
        }
    }
}
