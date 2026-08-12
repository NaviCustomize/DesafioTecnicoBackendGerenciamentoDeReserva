using Dapper;
using SistemaGerenciamentoDeReserva.Domain.Entity;
using SistemaGerenciamentoDeReserva.Domain.Interfaces;
using System.Data;
using System.Data.Common;

namespace SistemaGerenciamentoDeReserva.Infrastruture.Repositories
{
    public class UsuarioRepository : IUsuarioRepository
    {
        private readonly IDbConnection _dbConnection;

        public UsuarioRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }
        //lembrar de nao colocar como public por conta do IUsuarioRepository
        async Task<Usuario> IUsuarioRepository.BuscarPorId(long id)
        {
            var sql = "select * from Usuarios where Id = @Id";
            return await _dbConnection.QueryFirstOrDefaultAsync<Usuario>(sql, new { Id = id });
        }

        async Task<List<Usuario>> IUsuarioRepository.ListarUsuarios()
        {
            var sql = " select * from Usuarios ";
            return (await _dbConnection.QueryAsync<Usuario>(sql)).ToList();
        }

        async Task IUsuarioRepository.Adicionar(Usuario usuario)
        {
            var sql = " insert into Usuarios (Nome, Email, Senha) values (@Nome, @Email, @Senha) ";
            await _dbConnection.ExecuteAsync(sql, usuario);
        }

        async Task IUsuarioRepository.Atualizar(Usuario usuario)
        {
            var sql = "update Usuarios set Nome = @Nome, Email = @Email, Senha = @Senha where Id = @Id" ;
            await _dbConnection.ExecuteAsync(sql, usuario);
        }

        async Task IUsuarioRepository.Deletar(long id)
        {
            var sql = "delete from Usuarios where Id = @Id";
            await _dbConnection.ExecuteAsync(sql, new { Id = id });
        }

        async Task<Usuario> IUsuarioRepository.BuscarPorEmail(string email)
        {
            var sql = "select * from Usuarios where Email = @Email";
            return await _dbConnection.QueryFirstOrDefaultAsync<Usuario>(sql, new { Email = email });
        }
    }
}
