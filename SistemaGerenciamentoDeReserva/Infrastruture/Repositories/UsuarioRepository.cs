using Dapper;
using SistemaGerenciamentoDeReserva.Domain.Entity;
using SistemaGerenciamentoDeReserva.Domain.Interfaces;
using System.Data;

namespace SistemaGerenciamentoDeReserva.Infrastruture.Repositories
{
    public class UsuarioRepository : IUsuarioRepository
    {
        private readonly IDbConnection _dbConnection;

        public UsuarioRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<Usuario?> ObterPorIdAsync(long id)
        {
            const string sql =
                """
            SELECT id, nome, email, senha_hash AS SenhaHash,
            CASE WHEN role = 'Admin' THEN 1 ELSE 0 END AS ROLE
            FROM usuarios
            WHERE id = @Id;
            """;

            return await _dbConnection.QueryFirstOrDefaultAsync<Usuario>(sql, new { Id = id });
        }

        public async Task<IEnumerable<Usuario>> ObterTodosAsync()
        {
            const string sql =
                """
            SELECT id, nome, email, senha_hash AS SenhaHash,
            CASE WHEN role = 'Admin' THEN 1 ELSE 0 END AS ROLE
            FROM usuarios
            ORDER BY id;
            """;

            return await _dbConnection.QueryAsync<Usuario>(sql);
        }

        public async Task<long> AdicionarAsync(Usuario usuario)
        {
            const string sql =
                """
            INSERT INTO usuarios (nome, email, senha_hash, role)
            VALUES (@Nome, @Email, @SenhaHash, @Role)
            RETURNING id;
            """;

            return await _dbConnection.ExecuteScalarAsync<long>(sql, new
            {
                usuario.Nome,
                usuario.Email,
                usuario.SenhaHash,
                Role = usuario.Role.ToString()
            });
        }

        public async Task AtualizarAsync(Usuario usuario)
        {
            const string sql =
                """
            UPDATE usuarios SET
            nome = @Nome,
            email = @Email,
            senha_hash = @SenhaHash,
            role = @Role
            WHERE id = @Id;
            """;

            await _dbConnection.ExecuteAsync(sql, new
            {
                usuario.Id,
                usuario.Nome,
                usuario.Email,
                usuario.SenhaHash,
                Role = usuario.Role.ToString()
            });
        }

        public async Task DeletarAsync(long id)
        {
            const string sql =
                """
            DELETE FROM usuarios
            WHERE id = @Id;
            """;

            await _dbConnection.ExecuteAsync(sql, new { Id = id });
        }

        public async Task<Usuario?> ObterPorEmailAsync(string email)
        {
            const string sql =
                """
            SELECT id, nome, email, senha_hash AS SenhaHash,
            CASE WHEN role = 'Admin' THEN 1 ELSE 0 END AS Role
            FROM usuarios
            WHERE email = @Email;
            """;

            return await _dbConnection.QueryFirstOrDefaultAsync<Usuario>(sql, new { Email = email });
        }
    }
}
