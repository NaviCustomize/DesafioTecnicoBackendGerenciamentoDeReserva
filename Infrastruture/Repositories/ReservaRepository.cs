using Dapper;
using SistemaGerenciamentoDeReserva.Domain.Entity;
using SistemaGerenciamentoDeReserva.Domain.Interfaces;
using System.Data;
using System.Data.Common;

namespace SistemaGerenciamentoDeReserva.Infrastruture.Repositories
{
    public class ReservaRepository : IReservaRepository
    {
        private readonly IDbConnection _dbConnection;

        public ReservaRepository (IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }
        async Task IReservaRepository.AdicionarAsync(Reserva reserva)
        {
            var sql = "INSERT INTO Reservas (DataCheckIn, DataCheckOut, Status, UsuarioId, HotelId) VALUES (@DataCheckIn, @DataCheckOut, @Status, @UsuarioId, @HotelId)";
            await _dbConnection.ExecuteAsync(sql,reserva);
        }

        async Task IReservaRepository.AtualizarAsync(Reserva reserva)
        {
            var sql = "update Reservas set DataCheckIn = @DataCheckIn,  DataCheckOut = @DataCheckOut, Status = @Status, UsuarioId = @UsuarioId, HotelId = @HotelId where Id = @Id ";
            await _dbConnection.ExecuteAsync(sql, reserva);
        }

        async Task IReservaRepository.DeletarAsync(long id)
        {
            var sql = "delete from Reservas where Id = @Id";
            await _dbConnection.ExecuteAsync(sql, new {Id = id});
        }

        async Task<Reserva?> IReservaRepository.ObterPorIdAsync(long id)
        {
            var sql = "Select * from Reservas where Id = @Id";
            return await _dbConnection.QueryFirstOrDefaultAsync<Reserva?>(sql, new {Id = id});
        }

        async Task<IEnumerable<Reserva>> IReservaRepository.ObterTodasAsync()
        {
            var sql = "Select * from Reservas";
            return (await _dbConnection.QueryAsync<Reserva>(sql)).ToList();
        }
    }
}
