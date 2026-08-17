using Dapper;
using SistemaGerenciamentoDeReserva.Domain.Entity;
using SistemaGerenciamentoDeReserva.Infrastruture.Repositories;

namespace SistemaGerenciamentoDeReserva.Tests.Infrastructure
{
    public class HotelRepositoryTests : RepositoryTestBase
    {
        private static Hotel NovoHotel() => new()
        {
            Nome = TextoUnico("Hotel"),
            Localizacao = "Petrópolis",
            Descricao = "Descrição inicial"
        };

        [Fact]
        public async Task AdicionarAsync_DeveRetornarIdGerado()
        {
            using var conexao = CriarConexao();
            var repositorio = new HotelRepository(conexao);

            var id = await repositorio.AdicionarAsync(NovoHotel());

            Assert.True(id > 0);
        }

        [Fact]
        public async Task ObterPorIdAsync_DeveRetornarHotelCadastrado()
        {
            using var conexao = CriarConexao();
            var repositorio = new HotelRepository(conexao);

            var hotel = NovoHotel();
            var id = await repositorio.AdicionarAsync(hotel);

            var encontrado = await repositorio.ObterPorIdAsync(id);

            Assert.NotNull(encontrado);
            Assert.Equal(hotel.Nome, encontrado!.Nome);
            Assert.Equal(hotel.Localizacao, encontrado.Localizacao);
            Assert.Equal(hotel.Descricao, encontrado.Descricao);
        }

        [Fact]
        public async Task ObterPorIdAsync_DeveRetornarNuloQuandoNaoExiste()
        {
            using var conexao = CriarConexao();
            var repositorio = new HotelRepository(conexao);

            var encontrado = await repositorio.ObterPorIdAsync(-1);

            Assert.Null(encontrado);
        }

        [Fact]
        public async Task ObterTodosAsync_DeveConterHotelCadastrado()
        {
            using var conexao = CriarConexao();
            var repositorio = new HotelRepository(conexao);

            var id = await repositorio.AdicionarAsync(NovoHotel());

            var todos = await repositorio.ObterTodosAsync();

            Assert.Contains(todos, h => h.Id == id);
        }

        [Fact]
        public async Task AtualizarAsync_DevePersistirNovosDados()
        {
            using var conexao = CriarConexao();
            var repositorio = new HotelRepository(conexao);

            var id = await repositorio.AdicionarAsync(NovoHotel());

            var novoNome = TextoUnico("Hotel Renomeado");

            await repositorio.AtualizarAsync(new Hotel
            {
                Id = id,
                Nome = novoNome,
                Localizacao = "Teresópolis",
                Descricao = "Descrição atualizada"
            });

            var atualizado = await repositorio.ObterPorIdAsync(id);

            Assert.NotNull(atualizado);
            Assert.Equal(novoNome, atualizado!.Nome);
            Assert.Equal("Teresópolis", atualizado.Localizacao);
            Assert.Equal("Descrição atualizada", atualizado.Descricao);
        }

        [Fact]
        public async Task DeletarAsync_DeveRemoverDasConsultas()
        {
            using var conexao = CriarConexao();
            var repositorio = new HotelRepository(conexao);

            var id = await repositorio.AdicionarAsync(NovoHotel());

            await repositorio.DeletarAsync(id);

            Assert.Null(await repositorio.ObterPorIdAsync(id));
            Assert.DoesNotContain(await repositorio.ObterTodosAsync(), h => h.Id == id);
        }

        [Fact]
        public async Task DeletarAsync_NaoDeveApagarLinhaDoBanco()
        {
            using var conexao = CriarConexao();
            var repositorio = new HotelRepository(conexao);

            var id = await repositorio.AdicionarAsync(NovoHotel());
            await repositorio.DeletarAsync(id);

            // A exclusão é lógica: a linha continua existindo, com excluido_em preenchido.
            var excluidoEm = await conexao.ExecuteScalarAsync<DateTime?>(
                "SELECT excluido_em FROM hoteis WHERE id = @Id;",
                new { Id = id });

            Assert.NotNull(excluidoEm);
        }
    }
}
