using System.Net;
using System.Net.Http.Json;
using SistemaGerenciamentoDeReserva.Application.DTOs.Usuario;

namespace SistemaGerenciamentoDeReserva.Tests.Integration
{
    public class UsuarioControllerIntegrationTests : IntegrationTestBase
    {
        public UsuarioControllerIntegrationTests(CustomWebApplicationFactory factory) : base(factory)
        {
        }

        

        [Fact]
        public async Task Login_CredenciaisValidas_Retorna200ComToken()
        {
            var email = EmailUnico("login-ok");
            await SeedUsuarioAsync("Usuário Teste", email, "Senha123!", "User");

            var response = await Client.PostAsJsonAsync(
                "/auth/login",
                new { email, senha = "Senha123!" });

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Login_SenhaIncorreta_Retorna401()
        {
            var email = EmailUnico("login-senha-errada");
            await SeedUsuarioAsync("Usuário Teste", email, "Senha123!", "User");

            var response = await Client.PostAsJsonAsync(
                "/auth/login",
                new { email, senha = "SenhaErrada!" });

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Login_EmailInexistente_Retorna401()
        {
            var response = await Client.PostAsJsonAsync(
                "/auth/login",
                new { email = EmailUnico("nao-existe"), senha = "Senha123!" });

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        

        [Fact]
        public async Task Criar_SemTokenDeAutenticacao_Retorna201()
        {
            var dto = new CriarUsuarioDto("Novo Usuário", EmailUnico("sem-token"), "Senha123!");

            var response = await Client.PostAsJsonAsync("/usuarios", dto);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        [Fact]
        public async Task ObterTodos_SemTokenDeAutenticacao_Retorna401()
        {
            var response = await Client.GetAsync("/usuarios");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        

        [Fact]
        public async Task Criar_ComTokenValido_Retorna201EPersisteNoBanco()
        {
            var emailAutor = EmailUnico("autor-criar");
            await SeedUsuarioAsync("Autor", emailAutor, "Senha123!", "User");
            AutenticarComo(await LoginAsync(emailAutor, "Senha123!"));

            var dto = new CriarUsuarioDto("Usuário Criado", EmailUnico("criado"), "Senha123!");

            var response = await Client.PostAsJsonAsync("/usuarios", dto);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var criado = await response.Content.ReadFromJsonAsync<UsuarioResponseDto>(JsonOptions);
            Assert.Equal(dto.Nome, criado!.Nome);
            Assert.Equal(dto.Email, criado.Email);

            var busca = await Client.GetAsync($"/usuarios/{criado.Id}");
            Assert.Equal(HttpStatusCode.OK, busca.StatusCode);
        }

        [Fact]
        public async Task Atualizar_UsuarioExistente_Retorna204EAtualizaDados()
        {
            var emailAutor = EmailUnico("autor-atualizar");
            await SeedUsuarioAsync("Autor", emailAutor, "Senha123!", "User");
            AutenticarComo(await LoginAsync(emailAutor, "Senha123!"));

            var idAlvo = await SeedUsuarioAsync("Nome Antigo", EmailUnico("alvo-atualizar"), "Senha123!", "User");

            var novoEmail = EmailUnico("atualizado");
            var dto = new AtualizarUsuarioDto("Nome Novo", novoEmail);

            var response = await Client.PutAsJsonAsync($"/usuarios/{idAlvo}", dto);

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            var busca = await Client.GetAsync($"/usuarios/{idAlvo}");
            var usuarioAtualizado = await busca.Content.ReadFromJsonAsync<UsuarioResponseDto>(JsonOptions);

            Assert.Equal("Nome Novo", usuarioAtualizado!.Nome);
            Assert.Equal(novoEmail, usuarioAtualizado.Email);
        }

        [Fact]
        public async Task Deletar_ComUsuarioSemPapelAdmin_Retorna403()
        {
            var emailAutor = EmailUnico("nao-admin");
            await SeedUsuarioAsync("Usuário Comum", emailAutor, "Senha123!", "User");
            AutenticarComo(await LoginAsync(emailAutor, "Senha123!"));

            var idAlvo = await SeedUsuarioAsync("Alvo", EmailUnico("alvo-delete-403"), "Senha123!", "User");

            var response = await Client.DeleteAsync($"/usuarios/{idAlvo}");

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task Deletar_ComUsuarioAdmin_Retorna204ERemoveDoBanco()
        {
            var emailAdmin = EmailUnico("admin");
            await SeedUsuarioAsync("Administrador", emailAdmin, "Senha123!", "Admin");
            AutenticarComo(await LoginAsync(emailAdmin, "Senha123!"));

            var idAlvo = await SeedUsuarioAsync("Alvo", EmailUnico("alvo-delete-204"), "Senha123!", "User");

            var response = await Client.DeleteAsync($"/usuarios/{idAlvo}");

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            var busca = await Client.GetAsync($"/usuarios/{idAlvo}");
            Assert.Equal(HttpStatusCode.NotFound, busca.StatusCode);
        }
    }
}
