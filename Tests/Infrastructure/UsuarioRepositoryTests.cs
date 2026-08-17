using SistemaGerenciamentoDeReserva.Domain.Entity;
using SistemaGerenciamentoDeReserva.Domain.Enums;
using SistemaGerenciamentoDeReserva.Infrastruture.Repositories;

namespace SistemaGerenciamentoDeReserva.Tests.Infrastructure
{
    public class UsuarioRepositoryTests : RepositoryTestBase
    {
        private static Usuario NovoUsuario(RoleUsuario role = RoleUsuario.User) => new()
        {
            Nome = TextoUnico("Nome"),
            Sobrenome = "Sobrenome",
            Email = EmailUnico("usuario"),
            SenhaHash = BCrypt.Net.BCrypt.HashPassword("Senha@123"),
            Role = role
        };

        [Fact]
        public async Task AdicionarAsync_DeveRetornarIdGerado()
        {
            using var conexao = CriarConexao();
            var repositorio = new UsuarioRepository(conexao);

            var id = await repositorio.AdicionarAsync(NovoUsuario());

            Assert.True(id > 0);
        }

        [Fact]
        public async Task ObterPorIdAsync_DevePreservarNomeCompletoEPapel()
        {
            using var conexao = CriarConexao();
            var repositorio = new UsuarioRepository(conexao);

            var usuario = NovoUsuario(RoleUsuario.Admin);
            var id = await repositorio.AdicionarAsync(usuario);

            var encontrado = await repositorio.ObterPorIdAsync(id);

            Assert.NotNull(encontrado);
            Assert.Equal(usuario.Nome, encontrado!.Nome);
            Assert.Equal("Sobrenome", encontrado.Sobrenome);
            Assert.Equal($"{usuario.Nome} Sobrenome", encontrado.NomeCompleto);
            Assert.Equal(RoleUsuario.Admin, encontrado.Role);
            Assert.True(encontrado.Ativo);
        }

        [Fact]
        public async Task ObterPorEmailAsync_DeveRetornarUsuarioComHashDaSenha()
        {
            using var conexao = CriarConexao();
            var repositorio = new UsuarioRepository(conexao);

            var usuario = NovoUsuario();
            await repositorio.AdicionarAsync(usuario);

            var encontrado = await repositorio.ObterPorEmailAsync(usuario.Email);

            Assert.NotNull(encontrado);
            Assert.Equal(usuario.Email, encontrado!.Email);
            Assert.True(BCrypt.Net.BCrypt.Verify("Senha@123", encontrado.SenhaHash));
        }

        [Fact]
        public async Task ObterTodosAsync_DeveConterUsuarioAtivo()
        {
            using var conexao = CriarConexao();
            var repositorio = new UsuarioRepository(conexao);

            var id = await repositorio.AdicionarAsync(NovoUsuario());

            Assert.Contains(await repositorio.ObterTodosAsync(), u => u.Id == id);
        }

        [Fact]
        public async Task AtualizarAsync_DevePersistirNomeEEmail()
        {
            using var conexao = CriarConexao();
            var repositorio = new UsuarioRepository(conexao);

            var usuario = NovoUsuario();
            var id = await repositorio.AdicionarAsync(usuario);

            usuario.Id = id;
            usuario.Nome = TextoUnico("Renomeado");
            usuario.Email = EmailUnico("renomeado");

            await repositorio.AtualizarAsync(usuario);

            var atualizado = await repositorio.ObterPorIdAsync(id);

            Assert.NotNull(atualizado);
            Assert.Equal(usuario.Nome, atualizado!.Nome);
            Assert.Equal(usuario.Email, atualizado.Email);
        }

        [Fact]
        public async Task DeletarAsync_DeveRemoverDasConsultasDeAtivos()
        {
            using var conexao = CriarConexao();
            var repositorio = new UsuarioRepository(conexao);

            var id = await repositorio.AdicionarAsync(NovoUsuario());

            await repositorio.DeletarAsync(id);

            Assert.Null(await repositorio.ObterPorIdAsync(id));
            Assert.DoesNotContain(await repositorio.ObterTodosAsync(), u => u.Id == id);
        }

        [Fact]
        public async Task DeletarAsync_DeveImpedirQueUsuarioExcluidoFacaLogin()
        {
            using var conexao = CriarConexao();
            var repositorio = new UsuarioRepository(conexao);

            var usuario = NovoUsuario();
            var id = await repositorio.AdicionarAsync(usuario);

            await repositorio.DeletarAsync(id);

            // A busca por e-mail é o caminho do login: precisa filtrar os excluídos,
            // senão uma conta encerrada continuaria autenticando.
            Assert.Null(await repositorio.ObterPorEmailAsync(usuario.Email));
        }

        [Fact]
        public async Task ObterTodosIncluindoInativosAsync_DeveConterUsuarioExcluido()
        {
            using var conexao = CriarConexao();
            var repositorio = new UsuarioRepository(conexao);

            var id = await repositorio.AdicionarAsync(NovoUsuario());
            await repositorio.DeletarAsync(id);

            var todos = await repositorio.ObterTodosIncluindoInativosAsync();

            var excluido = Assert.Single(todos, u => u.Id == id);
            Assert.False(excluido.Ativo);
            Assert.NotNull(excluido.ExcluidoEm);
        }

        [Fact]
        public async Task ObterPorIdIncluindoInativoAsync_DeveEncontrarUsuarioExcluido()
        {
            using var conexao = CriarConexao();
            var repositorio = new UsuarioRepository(conexao);

            var id = await repositorio.AdicionarAsync(NovoUsuario());
            await repositorio.DeletarAsync(id);

            var encontrado = await repositorio.ObterPorIdIncluindoInativoAsync(id);

            Assert.NotNull(encontrado);
            Assert.False(encontrado!.Ativo);
        }

        [Fact]
        public async Task ReativarAsync_DeveDevolverUsuarioParaAsConsultas()
        {
            using var conexao = CriarConexao();
            var repositorio = new UsuarioRepository(conexao);

            var usuario = NovoUsuario();
            var id = await repositorio.AdicionarAsync(usuario);

            await repositorio.DeletarAsync(id);
            await repositorio.ReativarAsync(id);

            var reativado = await repositorio.ObterPorIdAsync(id);

            Assert.NotNull(reativado);
            Assert.True(reativado!.Ativo);
            Assert.NotNull(await repositorio.ObterPorEmailAsync(usuario.Email));
        }
    }
}
