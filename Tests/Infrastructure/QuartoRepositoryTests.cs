using SistemaGerenciamentoDeReserva.Domain.Entity;
using SistemaGerenciamentoDeReserva.Domain.Enums;
using SistemaGerenciamentoDeReserva.Infrastruture.Repositories;

namespace SistemaGerenciamentoDeReserva.Tests.Infrastructure
{
    public class QuartoRepositoryTests : RepositoryTestBase
    {
        private static Quarto NovoQuarto(long hotelId) => new()
        {
            HotelId = hotelId,
            Numero = NumeroUnico(),
            Tipo = TipoQuarto.Luxo,
            PrecoPorNoite = 480.50m,
            Status = StatusQuarto.Disponivel
        };

        [Fact]
        public async Task AdicionarAsync_DeveRetornarIdGerado()
        {
            var hotelId = await SemearHotelAsync();

            using var conexao = CriarConexao();
            var repositorio = new QuartoRepository(conexao);

            var id = await repositorio.AdicionarAsync(NovoQuarto(hotelId));

            Assert.True(id > 0);
        }

        [Fact]
        public async Task ObterPorIdAsync_DevePreservarTipoPrecoEStatus()
        {
            var hotelId = await SemearHotelAsync();

            using var conexao = CriarConexao();
            var repositorio = new QuartoRepository(conexao);

            var quarto = NovoQuarto(hotelId);
            var id = await repositorio.AdicionarAsync(quarto);

            var encontrado = await repositorio.ObterPorIdAsync(id);

            Assert.NotNull(encontrado);
            Assert.Equal(hotelId, encontrado!.HotelId);
            Assert.Equal(quarto.Numero, encontrado.Numero);
            Assert.Equal(TipoQuarto.Luxo, encontrado.Tipo);
            Assert.Equal(480.50m, encontrado.PrecoPorNoite);
            Assert.Equal(StatusQuarto.Disponivel, encontrado.Status);
        }

        [Fact]
        public async Task ObterTodosAsync_DeveConterQuartoCadastrado()
        {
            var hotelId = await SemearHotelAsync();

            using var conexao = CriarConexao();
            var repositorio = new QuartoRepository(conexao);

            var id = await repositorio.AdicionarAsync(NovoQuarto(hotelId));

            var todos = await repositorio.ObterTodosAsync();

            Assert.Contains(todos, q => q.Id == id);
        }

        [Fact]
        public async Task AtualizarAsync_DevePersistirPrecoEStatus()
        {
            var hotelId = await SemearHotelAsync();

            using var conexao = CriarConexao();
            var repositorio = new QuartoRepository(conexao);

            var quarto = NovoQuarto(hotelId);
            var id = await repositorio.AdicionarAsync(quarto);

            quarto.Id = id;
            quarto.PrecoPorNoite = 999.90m;
            quarto.Status = StatusQuarto.Reservado;
            quarto.Tipo = TipoQuarto.SuiteMaster;

            await repositorio.AtualizarAsync(quarto);

            var atualizado = await repositorio.ObterPorIdAsync(id);

            Assert.NotNull(atualizado);
            Assert.Equal(999.90m, atualizado!.PrecoPorNoite);
            Assert.Equal(StatusQuarto.Reservado, atualizado.Status);
            Assert.Equal(TipoQuarto.SuiteMaster, atualizado.Tipo);
        }

        [Fact]
        public async Task DeletarAsync_DeveRemoverDasConsultas()
        {
            var hotelId = await SemearHotelAsync();

            using var conexao = CriarConexao();
            var repositorio = new QuartoRepository(conexao);

            var id = await repositorio.AdicionarAsync(NovoQuarto(hotelId));

            await repositorio.DeletarAsync(id);

            Assert.Null(await repositorio.ObterPorIdAsync(id));
            Assert.DoesNotContain(await repositorio.ObterTodosAsync(), q => q.Id == id);
        }
    }
}
