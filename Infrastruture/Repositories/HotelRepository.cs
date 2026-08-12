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

        public async Task<IEnumerable<Hotel>> ObterTodosAsync()
        {
            var sql = "select * from Hoteis";
            var resultado = await _dbConnection.QueryAsync<Hotel>(sql);
            return resultado.ToList();
        }

        public async Task<Hotel?> ObterPorIdAsync(long id)
        {
            var sql = "select * from Hoteis where Id = @Id";
            return await _dbConnection.QueryFirstOrDefaultAsync<Hotel?>(sql, new { Id = id });
        }

        public async Task AdicionarAsync(Hotel hotel)
        {
            var sql = "insert into Hoteis (Nome, Localizacao, PrecoPorNoite, QtdQuartos, Descricao) values (@Nome, @Localizacao, @PrecoPorNoite, @QtdQuartos, @Descricao)";
            await _dbConnection.ExecuteAsync(sql, hotel);
        }

        public async Task AtualizarAsync(Hotel hotel)
        {
            var sql = "update Hoteis set Nome = @Nome, Localizacao = @Localizacao, PrecoPorNoite = @PrecoPorNoite, QtdQuartos = @QtdQuartos, Descricao = @Descricao where Id = @Id";
            await _dbConnection.ExecuteAsync(sql, hotel);
        }

        public async Task DeletarAsync(long id)
        {
            var sql = "delete from Hoteis where Id = @Id";
            await _dbConnection.ExecuteAsync(sql, new { Id = id });
        }
    }
}
