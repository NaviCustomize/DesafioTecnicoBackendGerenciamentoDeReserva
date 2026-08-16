using Dapper;
using SistemaGerenciamentoDeReserva.Domain.Entity;
using SistemaGerenciamentoDeReserva.Domain.Interfaces;
using System.Data;

namespace SistemaGerenciamentoDeReserva.Infrastruture.Repositories
{
    public class QuartoRepository : IQuartoRepository
    {
        private readonly IDbConnection _dbConnection;

        public QuartoRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<Quarto?> ObterPorIdAsync(long id)
        {
            const string sql = """
            SELECT
                id, hotel_id AS HotelId, numero, tipo,
                preco_por_noite AS PrecoPorNoite,
                status
            FROM quartos
            WHERE id = @Id AND excluido_em IS NULL;
            """;

            return await _dbConnection.QueryFirstOrDefaultAsync<Quarto>(sql, new { Id = id });
        }

        public async Task<IEnumerable<Quarto>> ObterTodosAsync()
        {
            const string sql = """
            SELECT
                id, hotel_id AS HotelId, numero, tipo,
                preco_por_noite AS PrecoPorNoite,
                status
            FROM quartos
            WHERE excluido_em IS NULL
            ORDER BY id;
            """;

            return await _dbConnection.QueryAsync<Quarto>(sql);
        }

        public async Task<long> AdicionarAsync(Quarto quarto)
        {
            const string sql = """
            INSERT INTO quartos
                (
                    hotel_id,
                    numero,
                    tipo,
                    preco_por_noite,
                    status
                )
            VALUES
                (
                    @HotelId,
                    @Numero,
                    @Tipo,
                    @PrecoPorNoite,
                    @Status
                )
            RETURNING id;
            """;

            return await _dbConnection.ExecuteScalarAsync<long>(sql, quarto);
        }

        public async Task AtualizarAsync(Quarto quarto)
        {
            const string sql = """
            UPDATE quartos
            SET
                hotel_id = @HotelId,
                numero = @Numero,
                tipo = @Tipo,
                preco_por_noite = @PrecoPorNoite,
                status = @Status,
                atualizado_em = NOW()
            WHERE id = @Id;
            """;

            await _dbConnection.ExecuteAsync(sql, quarto);
        }


        public async Task DeletarAsync(long id)
        {
            const string sql = """
            UPDATE quartos
            SET excluido_em = NOW()
            WHERE id = @Id AND excluido_em IS NULL;
            """;

            await _dbConnection.ExecuteAsync(sql, new { Id = id });
        }
    }
}
