using System.Net;
using System.Net.Http.Json;
using SistemaGerenciamentoDeReserva.Application.DTOs.Hotel;

namespace SistemaGerenciamentoDeReserva.Tests.Integration
{
    public class HotelControllerIntegrationTests : IntegrationTestBase
    {
        public HotelControllerIntegrationTests(CustomWebApplicationFactory factory) : base(factory)
        {
        }

        private async Task AutenticarComoAdminAsync()
        {
            var email = EmailUnico("admin-hoteis");
            await SeedUsuarioAsync("Administrador", email, "Senha123!", "Admin");
            AutenticarComo(await LoginAsync(email, "Senha123!"));
        }

        [Fact]
        public async Task ObterTodos_SemTokenDeAutenticacao_Retorna200()
        {
            var response = await Client.GetAsync("/hoteis");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Criar_SemTokenDeAutenticacao_Retorna401()
        {
            var dto = new CriarHotelDto(TextoUnico("Hotel"), "São Paulo", null);

            var response = await Client.PostAsJsonAsync("/hoteis", dto);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Criar_ComUsuarioSemPapelAdmin_Retorna403()
        {
            var email = EmailUnico("usuario-comum");
            await SeedUsuarioAsync("Usuário Comum", email, "Senha123!", "User");
            AutenticarComo(await LoginAsync(email, "Senha123!"));

            var dto = new CriarHotelDto(TextoUnico("Hotel"), "São Paulo", null);

            var response = await Client.PostAsJsonAsync("/hoteis", dto);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task Criar_ComTokenAdmin_Retorna201EPersisteNoBanco()
        {
            await AutenticarComoAdminAsync();

            var dto = new CriarHotelDto(TextoUnico("Hotel Central"), "São Paulo", "Perto do centro");

            var response = await Client.PostAsJsonAsync("/hoteis", dto);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var criado = await response.Content.ReadFromJsonAsync<HotelResponseDto>(JsonOptions);
            Assert.Equal(dto.Nome, criado!.Nome);

            var busca = await Client.GetAsync($"/hoteis/{criado.Id}");
            Assert.Equal(HttpStatusCode.OK, busca.StatusCode);
        }

        [Fact]
        public async Task ObterPorId_HotelInexistente_Retorna404()
        {
            var response = await Client.GetAsync("/hoteis/999999999");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Atualizar_ComTokenAdmin_Retorna204EAtualizaDados()
        {
            var idHotel = await SeedHotelAsync(TextoUnico("Hotel Antigo"), "Rio de Janeiro");
            await AutenticarComoAdminAsync();

            var novoNome = TextoUnico("Hotel Renovado");
            var dto = new AtualizarHotelDto(novoNome, "Belo Horizonte", "Descrição nova");

            var response = await Client.PutAsJsonAsync($"/hoteis/{idHotel}", dto);

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            var busca = await Client.GetAsync($"/hoteis/{idHotel}");
            var atualizado = await busca.Content.ReadFromJsonAsync<HotelResponseDto>(JsonOptions);

            Assert.Equal(novoNome, atualizado!.Nome);
            Assert.Equal("Belo Horizonte", atualizado.Localizacao);
        }

        [Fact]
        public async Task Deletar_ComTokenAdmin_Retorna204ERemoveDoBanco()
        {
            var idHotel = await SeedHotelAsync(TextoUnico("Hotel a Remover"), "Salvador");
            await AutenticarComoAdminAsync();

            var response = await Client.DeleteAsync($"/hoteis/{idHotel}");

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            var busca = await Client.GetAsync($"/hoteis/{idHotel}");
            Assert.Equal(HttpStatusCode.NotFound, busca.StatusCode);
        }
    }
}
