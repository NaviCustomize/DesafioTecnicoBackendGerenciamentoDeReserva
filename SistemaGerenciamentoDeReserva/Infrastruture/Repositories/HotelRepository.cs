using Dapper;
using SistemaGerenciamentoDeReserva.Domain.Entity;
using SistemaGerenciamentoDeReserva.Domain.Interfaces;
using System.Data;

namespace SistemaGerenciamentoDeReserva.Infrastruture.Repositories
{
    public class HotelRepository : IHotelRepository
    {
        private readonly IDbConnection _dbConnection;

        public HotelRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<Hotel?> ObterPorIdAsync(long id)
        {
            const string sql = """
            SELECT
                id,
                nome,
                localizacao,
                descricao
            FROM hoteis
            WHERE id = @Id;
            """;

            return await _dbConnection.QueryFirstOrDefaultAsync<Hotel>(
                sql,
                new { Id = id });
        }

        public async Task<IEnumerable<Hotel>> ObterTodosAsync()
        {
            const string sql = """
            SELECT
                id,
                nome,
                localizacao,
                descricao
            FROM hoteis
            ORDER BY id;
            """;

            return await _dbConnection.QueryAsync<Hotel>(sql);
        }

        public async Task<long> AdicionarAsync(Hotel hotel)
        {
            const string sql = """
            INSERT INTO hoteis
                (nome, localizacao, descricao)
            VALUES
                (@Nome, @Localizacao, @Descricao)
            RETURNING id;
            """;

            return await _dbConnection.ExecuteScalarAsync<long>(
                sql,
                hotel);
        }

        public async Task AtualizarAsync(Hotel hotel)
        {
            const string sql = """
            UPDATE hoteis
            SET
                nome = @Nome,
                localizacao = @Localizacao,
                descricao = @Descricao
            WHERE id = @Id;
            """;

            await _dbConnection.ExecuteAsync(sql, hotel);
        }

        public async Task DeletarAsync(long id)
        {
            const string sql = """
            DELETE FROM hoteis
            WHERE id = @Id;
            """;

            await _dbConnection.ExecuteAsync(
                sql,
                new { Id = id });
        }
    }
}
