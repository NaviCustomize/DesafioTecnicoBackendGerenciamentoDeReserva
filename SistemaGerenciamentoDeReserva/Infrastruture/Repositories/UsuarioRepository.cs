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
            SELECT id, nome, sobrenome, email, senha_hash AS SenhaHash,
            CASE WHEN role = 'Admin' THEN 1 ELSE 0 END AS ROLE
            FROM usuarios
            WHERE id = @Id AND excluido_em IS NULL;
            """;

            return await _dbConnection.QueryFirstOrDefaultAsync<Usuario>(sql, new { Id = id });
        }

        public async Task<IEnumerable<Usuario>> ObterTodosAsync()
        {
            const string sql =
                """
            SELECT id, nome, sobrenome, email, senha_hash AS SenhaHash,
            CASE WHEN role = 'Admin' THEN 1 ELSE 0 END AS ROLE
            FROM usuarios
            WHERE excluido_em IS NULL
            ORDER BY id;
            """;

            return await _dbConnection.QueryAsync<Usuario>(sql);
        }

        public async Task<IEnumerable<Usuario>> ObterTodosIncluindoInativosAsync()
        {
            const string sql =
                """
            SELECT id, nome, sobrenome, email, senha_hash AS SenhaHash,
            CASE WHEN role = 'Admin' THEN 1 ELSE 0 END AS Role,
            excluido_em AS ExcluidoEm
            FROM usuarios
            ORDER BY id;
            """;

            return await _dbConnection.QueryAsync<Usuario>(sql);
        }

        public async Task<Usuario?> ObterPorIdIncluindoInativoAsync(long id)
        {
            const string sql =
                """
            SELECT id, nome, sobrenome, email, senha_hash AS SenhaHash,
            CASE WHEN role = 'Admin' THEN 1 ELSE 0 END AS Role,
            excluido_em AS ExcluidoEm
            FROM usuarios
            WHERE id = @Id;
            """;

            return await _dbConnection.QueryFirstOrDefaultAsync<Usuario>(sql, new { Id = id });
        }

        public async Task ReativarAsync(long id)
        {
            const string sql =
                """
            UPDATE usuarios
            SET excluido_em = NULL,
                atualizado_em = NOW()
            WHERE id = @Id;
            """;

            await _dbConnection.ExecuteAsync(sql, new { Id = id });
        }

        public async Task<long> AdicionarAsync(Usuario usuario)
        {
            const string sql =
                """
            INSERT INTO usuarios (nome, sobrenome, email, senha_hash, role)
            VALUES (@Nome, @Sobrenome, @Email, @SenhaHash, @Role)
            RETURNING id;
            """;

            return await _dbConnection.ExecuteScalarAsync<long>(sql, new
            {
                usuario.Nome,
                usuario.Sobrenome,
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
            sobrenome = @Sobrenome,
            email = @Email,
            senha_hash = @SenhaHash,
            role = @Role,
            atualizado_em = NOW()
            WHERE id = @Id;
            """;

            await _dbConnection.ExecuteAsync(sql, new
            {
                usuario.Id,
                usuario.Nome,
                usuario.Sobrenome,
                usuario.Email,
                usuario.SenhaHash,
                Role = usuario.Role.ToString()
            });
        }


        public async Task DeletarAsync(long id)
        {
            const string sql =
                """
            UPDATE usuarios
            SET excluido_em = NOW()
            WHERE id = @Id AND excluido_em IS NULL;
            """;

            await _dbConnection.ExecuteAsync(sql, new { Id = id });
        }

        public async Task<Usuario?> ObterPorEmailAsync(string email)
        {
            const string sql =
                """
            SELECT id, nome, sobrenome, email, senha_hash AS SenhaHash,
            CASE WHEN role = 'Admin' THEN 1 ELSE 0 END AS Role
            FROM usuarios
            WHERE email = @Email AND excluido_em IS NULL;
            """;

            return await _dbConnection.QueryFirstOrDefaultAsync<Usuario>(sql, new { Email = email });
        }
    }
}
