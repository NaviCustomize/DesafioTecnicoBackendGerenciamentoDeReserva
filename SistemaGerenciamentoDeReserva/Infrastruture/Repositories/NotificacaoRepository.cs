using Dapper;
using SistemaGerenciamentoDeReserva.Domain.Entity;
using SistemaGerenciamentoDeReserva.Domain.Interfaces;
using System.Data;

namespace SistemaGerenciamentoDeReserva.Infrastruture.Repositories
{
    public class NotificacaoRepository : INotificacaoRepository
    {
        private readonly IDbConnection _dbConnection;

        public NotificacaoRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<long> AdicionarAsync(Notificacao notificacao)
        {
            const string sql = """
            INSERT INTO notificacoes
                (reserva_id, usuario_id, quarto_id, tipo_evento,
                 hospede, hospede_email, hotel, quarto_numero,
                 data_checkin, data_checkout, ocorrido_em)
            VALUES
                (@ReservaId, @UsuarioId, @QuartoId, @TipoEvento,
                 @Hospede, @HospedeEmail, @Hotel, @QuartoNumero,
                 @DataCheckIn, @DataCheckOut, @OcorridoEm)
            RETURNING id;
            """;

            return await _dbConnection.ExecuteScalarAsync<long>(sql, notificacao);
        }

        public async Task<IEnumerable<Notificacao>> ObterRecentesAsync(int limite)
        {
            const string sql = """
            SELECT
                id, reserva_id AS ReservaId, usuario_id AS UsuarioId,
                quarto_id AS QuartoId, tipo_evento AS TipoEvento,
                hospede, hospede_email AS HospedeEmail, hotel,
                quarto_numero AS QuartoNumero,
                data_checkin AS DataCheckIn, data_checkout AS DataCheckOut,
                ocorrido_em AS OcorridoEm, processado_em AS ProcessadoEm
            FROM notificacoes
            ORDER BY processado_em DESC
            LIMIT @Limite;
            """;

            return await _dbConnection.QueryAsync<Notificacao>(sql, new { Limite = limite });
        }
    }
}
