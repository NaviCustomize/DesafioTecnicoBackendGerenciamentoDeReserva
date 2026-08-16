using System.Net;
using System.Net.Http.Json;
using SistemaGerenciamentoDeReserva.Application.DTOs.Usuario;
using SistemaGerenciamentoDeReserva.Application.DTOs.Login;

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
            var dto = new CriarUsuarioDto("Novo Usuário", "Da Silva", EmailUnico("sem-token"), "Senha123!");

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
            await SeedUsuarioAsync("Autor", emailAutor, "Senha123!", "Admin");
            AutenticarComo(await LoginAsync(emailAutor, "Senha123!"));

            var dto = new CriarUsuarioDto("Usuário Criado", "Da Silva", EmailUnico("criado"), "Senha123!");

            var response = await Client.PostAsJsonAsync("/usuarios", dto);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var criado = await response.Content.ReadFromJsonAsync<UsuarioResponseDto>(JsonOptions);
            Assert.Equal(dto.Nome, criado!.Nome);
            Assert.Equal(dto.Email, criado.Email);

            var busca = await Client.GetAsync($"/usuarios/{criado.Id}");
            Assert.Equal(HttpStatusCode.OK, busca.StatusCode);
        }

        [Fact]
        public async Task Atualizar_OsPropriosDados_Retorna204EAtualizaDados()
        {
            var emailAutor = EmailUnico("autor-atualizar");
            var idAutor = await SeedUsuarioAsync("Nome Antigo", emailAutor, "Senha123!", "User");
            AutenticarComo(await LoginAsync(emailAutor, "Senha123!"));

            var novoEmail = EmailUnico("atualizado");
            var dto = new AtualizarUsuarioDto("Nome Novo", "Sobrenome Novo", novoEmail);

            var response = await Client.PutAsJsonAsync($"/usuarios/{idAutor}", dto);

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            var busca = await Client.GetAsync($"/usuarios/{idAutor}");
            var usuarioAtualizado = await busca.Content.ReadFromJsonAsync<UsuarioResponseDto>(JsonOptions);

            Assert.Equal("Nome Novo", usuarioAtualizado!.Nome);
            Assert.Equal(novoEmail, usuarioAtualizado.Email);
        }

        [Fact]
        public async Task Atualizar_DadosDeOutroUsuario_Retorna403()
        {
            var emailAutor = EmailUnico("autor-invasor");
            await SeedUsuarioAsync("Autor", emailAutor, "Senha123!", "User");
            AutenticarComo(await LoginAsync(emailAutor, "Senha123!"));

            var emailAlvo = EmailUnico("alvo-protegido");
            var idAlvo = await SeedUsuarioAsync("Nome Original", emailAlvo, "Senha123!", "User");

            var dto = new AtualizarUsuarioDto("Nome Invadido", "Sobrenome Invadido", EmailUnico("email-invadido"));

            var response = await Client.PutAsJsonAsync($"/usuarios/{idAlvo}", dto);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);

            AutenticarComo(await LoginAsync(emailAlvo, "Senha123!"));
            var busca = await Client.GetAsync($"/usuarios/{idAlvo}");
            var alvo = await busca.Content.ReadFromJsonAsync<UsuarioResponseDto>(JsonOptions);

            Assert.Equal("Nome Original", alvo!.Nome);
            Assert.Equal(emailAlvo, alvo.Email);
        }

        [Fact]
        public async Task ObterPorId_DeOutroUsuario_Retorna403()
        {
            var emailAutor = EmailUnico("autor-bisbilhoteiro");
            await SeedUsuarioAsync("Autor", emailAutor, "Senha123!", "User");
            AutenticarComo(await LoginAsync(emailAutor, "Senha123!"));

            var idAlvo = await SeedUsuarioAsync("Alvo", EmailUnico("alvo-get"), "Senha123!", "User");

            var response = await Client.GetAsync($"/usuarios/{idAlvo}");

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task ObterTodos_ComUsuarioSemPapelAdmin_Retorna403()
        {
            var emailAutor = EmailUnico("comum-lista");
            await SeedUsuarioAsync("Usuário Comum", emailAutor, "Senha123!", "User");
            AutenticarComo(await LoginAsync(emailAutor, "Senha123!"));

            var response = await Client.GetAsync("/usuarios");

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task ObterTodos_ComPapelAdmin_Retorna200()
        {
            var emailAdmin = EmailUnico("admin-lista");
            await SeedUsuarioAsync("Administrador", emailAdmin, "Senha123!", "Admin");
            AutenticarComo(await LoginAsync(emailAdmin, "Senha123!"));

            var response = await Client.GetAsync("/usuarios");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Criar_SemSobrenome_Retorna400()
        {
            var dto = new CriarUsuarioDto("Só Nome", "   ", EmailUnico("sem-sobrenome"), "Senha123!");

            var response = await Client.PostAsJsonAsync("/usuarios", dto);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Criar_ComSobrenome_PersisteOSobrenome()
        {
            var dto = new CriarUsuarioDto("Ana", "Ribeiro", EmailUnico("com-sobrenome"), "Senha123!");

            var response = await Client.PostAsJsonAsync("/usuarios", dto);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var criado = await response.Content.ReadFromJsonAsync<UsuarioResponseDto>(JsonOptions);

            Assert.Equal("Ana", criado!.Nome);
            Assert.Equal("Ribeiro", criado.Sobrenome);

            var login = await Client.PostAsJsonAsync(
                "/auth/login",
                new { email = dto.Email, senha = "Senha123!" });

            var corpo = await login.Content.ReadFromJsonAsync<LoginResponseDto>(JsonOptions);

            Assert.Equal("Ribeiro", corpo!.Sobrenome);
        }

        [Fact]
        public async Task Gestao_ListaInclusiveContasInativas()
        {
            var emailAdmin = EmailUnico("admin-gestao");
            await SeedUsuarioAsync("Administrador", emailAdmin, "Senha123!", "Admin");

            var emailInativo = EmailUnico("sera-inativado");
            var idInativo = await SeedUsuarioAsync("Inativo", emailInativo, "Senha123!", "User");

            AutenticarComo(await LoginAsync(emailAdmin, "Senha123!"));
            await Client.DeleteAsync($"/usuarios/{idInativo}");

            var listaComum = await Client.GetFromJsonAsync<List<UsuarioResponseDto>>("/usuarios", JsonOptions);
            Assert.DoesNotContain(listaComum!, u => u.Id == idInativo);

            var gestao = await Client.GetFromJsonAsync<List<UsuarioAdminResponseDto>>("/usuarios/gestao", JsonOptions);
            var alvo = Assert.Single(gestao!, u => u.Id == idInativo);

            Assert.False(alvo.Ativo);
            Assert.NotNull(alvo.InativoDesde);
        }

        [Fact]
        public async Task Reativar_ContaInativa_DevolveOAcesso()
        {
            var emailAdmin = EmailUnico("admin-reativa");
            await SeedUsuarioAsync("Administrador", emailAdmin, "Senha123!", "Admin");

            var emailAlvo = EmailUnico("volta-ativo");
            var idAlvo = await SeedUsuarioAsync("Alvo", emailAlvo, "Senha123!", "User");

            AutenticarComo(await LoginAsync(emailAdmin, "Senha123!"));
            await Client.DeleteAsync($"/usuarios/{idAlvo}");

            var loginBloqueado = await Client.PostAsJsonAsync(
                "/auth/login",
                new { email = emailAlvo, senha = "Senha123!" });
            Assert.Equal(HttpStatusCode.Unauthorized, loginBloqueado.StatusCode);

            var reativacao = await Client.PatchAsync($"/usuarios/{idAlvo}/reativar", content: null);
            Assert.Equal(HttpStatusCode.NoContent, reativacao.StatusCode);

            var loginLiberado = await Client.PostAsJsonAsync(
                "/auth/login",
                new { email = emailAlvo, senha = "Senha123!" });
            Assert.Equal(HttpStatusCode.OK, loginLiberado.StatusCode);
        }

        [Fact]
        public async Task Reativar_ContaJaAtiva_Retorna409()
        {
            var emailAdmin = EmailUnico("admin-reativa-ativa");
            await SeedUsuarioAsync("Administrador", emailAdmin, "Senha123!", "Admin");
            AutenticarComo(await LoginAsync(emailAdmin, "Senha123!"));

            var idAtivo = await SeedUsuarioAsync("Ativo", EmailUnico("ja-ativo"), "Senha123!", "User");

            var response = await Client.PatchAsync($"/usuarios/{idAtivo}/reativar", content: null);

            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        }

        [Fact]
        public async Task Reativar_ComUsuarioSemPapelAdmin_Retorna403()
        {
            var email = EmailUnico("comum-reativa");
            await SeedUsuarioAsync("Comum", email, "Senha123!", "User");
            AutenticarComo(await LoginAsync(email, "Senha123!"));

            var idAlvo = await SeedUsuarioAsync("Alvo", EmailUnico("alvo-reativa"), "Senha123!", "User");

            var response = await Client.PatchAsync($"/usuarios/{idAlvo}/reativar", content: null);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task AlterarSenha_ComSenhaAtualCorreta_Retorna204ETrocaOLogin()
        {
            var email = EmailUnico("troca-senha");
            var id = await SeedUsuarioAsync("Usuário", email, "Senha123!", "User");
            AutenticarComo(await LoginAsync(email, "Senha123!"));

            var response = await Client.PutAsJsonAsync(
                $"/usuarios/{id}/senha",
                new AlterarSenhaDto("Senha123!", "NovaSenha456!"));

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            var comSenhaAntiga = await Client.PostAsJsonAsync(
                "/auth/login",
                new { email, senha = "Senha123!" });
            Assert.Equal(HttpStatusCode.Unauthorized, comSenhaAntiga.StatusCode);

            var comSenhaNova = await Client.PostAsJsonAsync(
                "/auth/login",
                new { email, senha = "NovaSenha456!" });
            Assert.Equal(HttpStatusCode.OK, comSenhaNova.StatusCode);
        }

        [Fact]
        public async Task AlterarSenha_ComSenhaAtualErrada_Retorna403()
        {
            var email = EmailUnico("senha-errada");
            var id = await SeedUsuarioAsync("Usuário", email, "Senha123!", "User");
            AutenticarComo(await LoginAsync(email, "Senha123!"));

            var response = await Client.PutAsJsonAsync(
                $"/usuarios/{id}/senha",
                new AlterarSenhaDto("SenhaQualquer!", "NovaSenha456!"));

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task AlterarSenha_DeOutroUsuarioMesmoSendoAdmin_Retorna403()
        {
            var emailAdmin = EmailUnico("admin-troca-senha");
            await SeedUsuarioAsync("Administrador", emailAdmin, "Senha123!", "Admin");
            AutenticarComo(await LoginAsync(emailAdmin, "Senha123!"));

            var idAlvo = await SeedUsuarioAsync("Alvo", EmailUnico("alvo-senha"), "Senha123!", "User");

            var response = await Client.PutAsJsonAsync(
                $"/usuarios/{idAlvo}/senha",
                new AlterarSenhaDto("Senha123!", "NovaSenha456!"));

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task AlterarSenha_NovaSenhaCurta_Retorna400()
        {
            var email = EmailUnico("senha-curta");
            var id = await SeedUsuarioAsync("Usuário", email, "Senha123!", "User");
            AutenticarComo(await LoginAsync(email, "Senha123!"));

            var response = await Client.PutAsJsonAsync(
                $"/usuarios/{id}/senha",
                new AlterarSenhaDto("Senha123!", "123"));

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task EncerrarPropriaConta_ComSenhaCorreta_Retorna204EBloqueiaLogin()
        {
            var email = EmailUnico("encerra-conta");
            var id = await SeedUsuarioAsync("Usuário", email, "Senha123!", "User");
            AutenticarComo(await LoginAsync(email, "Senha123!"));

            var response = await Client.PostAsJsonAsync(
                $"/usuarios/{id}/encerrar-conta",
                new ConfirmarSenhaDto("Senha123!"));

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            var login = await Client.PostAsJsonAsync(
                "/auth/login",
                new { email, senha = "Senha123!" });

            Assert.Equal(HttpStatusCode.Unauthorized, login.StatusCode);
        }

        [Fact]
        public async Task EncerrarPropriaConta_ComSenhaErrada_Retorna403EMantemAConta()
        {
            var email = EmailUnico("encerra-senha-errada");
            var id = await SeedUsuarioAsync("Usuário", email, "Senha123!", "User");
            AutenticarComo(await LoginAsync(email, "Senha123!"));

            var response = await Client.PostAsJsonAsync(
                $"/usuarios/{id}/encerrar-conta",
                new ConfirmarSenhaDto("SenhaErrada!"));

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);

            var login = await Client.PostAsJsonAsync(
                "/auth/login",
                new { email, senha = "Senha123!" });

            Assert.Equal(HttpStatusCode.OK, login.StatusCode);
        }

        [Fact]
        public async Task EncerrarConta_DeOutroUsuario_Retorna403()
        {
            var email = EmailUnico("encerra-alheia");
            await SeedUsuarioAsync("Usuário", email, "Senha123!", "User");
            AutenticarComo(await LoginAsync(email, "Senha123!"));

            var idAlvo = await SeedUsuarioAsync("Alvo", EmailUnico("alvo-encerrar"), "Senha123!", "User");

            var response = await Client.PostAsJsonAsync(
                $"/usuarios/{idAlvo}/encerrar-conta",
                new ConfirmarSenhaDto("Senha123!"));

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
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
