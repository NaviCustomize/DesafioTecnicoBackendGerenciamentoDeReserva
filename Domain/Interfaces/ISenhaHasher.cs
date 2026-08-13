namespace SistemaGerenciamentoDeReserva.Domain.Interfaces
{
    public interface ISenhaHasher
    {
        string Hash(string senha);
        bool Verify(string senha, string senhaHash);
    }
}
