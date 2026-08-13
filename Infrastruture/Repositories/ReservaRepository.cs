using Dapper;
using SistemaGerenciamentoDeReserva.Domain.Entity;
using SistemaGerenciamentoDeReserva.Domain.Interfaces;
using System.Data;

namespace SistemaGerenciamentoDeReserva.Infrastruture.Repositories
{
    public class ReservaRepository : IReservaRepository
    {
        private readonly IDbConnection _dbConnection;

        public ReservaRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<Reserva?> ObterPorIdAsync(long id)
        {
            const string sql = """
            SELECT
                id,data_checkin AS DataCheckIn,data_checkout AS DataCheckOut,
                status,usuario_id AS UsuarioId,quarto_id AS QuartoId
            FROM reservas
            WHERE id = @Id;
            """;

            return await _dbConnection.QueryFirstOrDefaultAsync<Reserva>(sql,new { Id = id });
        }

        public async Task<IEnumerable<Reserva>> ObterTodosAsync()
        {
            const string sql = """
            SELECT
                id,data_checkin AS DataCheckIn,data_checkout AS DataCheckOut,
                status,usuario_id AS UsuarioId,quarto_id AS QuartoId
            FROM reservas
            ORDER BY id;
            """;

            return await _dbConnection.QueryAsync<Reserva>(sql);
        }

        public async Task<IEnumerable<Reserva>> ObterPorUsuarioAsync(long usuarioId)
        {
            const string sql = """
            SELECT
                id,data_checkin AS DataCheckIn,data_checkout AS DataCheckOut,
                status,usuario_id AS UsuarioId,quarto_id AS QuartoId
            FROM reservas
            WHERE usuario_id = @UsuarioId
            ORDER BY data_checkin DESC;
            """;

            return await _dbConnection.QueryAsync<Reserva>(sql,new { UsuarioId = usuarioId });
        }

        public async Task<bool> ExisteConflitoAsync(
            long quartoId,
            DateTime dataCheckIn,
            DateTime dataCheckOut,
            long? reservaId = null)
        {
            const string sql = """
            SELECT EXISTS (
                SELECT 1
                FROM reservas
                WHERE quarto_id = @QuartoId
                  AND status NOT IN (2)
                  AND data_checkin < @DataCheckOut
                  AND data_checkout > @DataCheckIn
                  AND (@ReservaId IS NULL OR id <> @ReservaId)
            );
            """;

            return await _dbConnection.ExecuteScalarAsync<bool>(sql,
                new
                {
                    QuartoId = quartoId,
                    DataCheckIn = dataCheckIn,
                    DataCheckOut = dataCheckOut,
                    ReservaId = reservaId
                });
        }

        public async Task<long> AdicionarAsync(Reserva reserva)
        {
            const string sql = """
            INSERT INTO reservas
                (
                    data_checkin,
                    data_checkout,
                    status,
                    usuario_id,
                    quarto_id
                )
            VALUES
                (
                    @DataCheckIn,
                    @DataCheckOut,
                    @Status,
                    @UsuarioId,
                    @QuartoId
                )
            RETURNING id;
            """;

            return await _dbConnection.ExecuteScalarAsync<long>(sql,reserva);
        }

        public async Task AtualizarAsync(Reserva reserva)
        {
            const string sql = """
            UPDATE reservas
            SET
                data_checkin = @DataCheckIn,
                data_checkout = @DataCheckOut
            WHERE id = @Id;
            """;

            await _dbConnection.ExecuteAsync(sql,reserva);
        }

        public async Task DeletarAsync(long id)
        {
            const string sql = """
            DELETE FROM reservas
            WHERE id = @Id;
            """;

            await _dbConnection.ExecuteAsync(sql,new { Id = id });
        }
    }
}
