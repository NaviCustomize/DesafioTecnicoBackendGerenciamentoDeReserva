using System.Net;
using System.Net.Http.Json;
using SistemaGerenciamentoDeReserva.Application.DTOs.Quarto;
using SistemaGerenciamentoDeReserva.Domain.Enums;

namespace SistemaGerenciamentoDeReserva.Tests.Integration
{
    public class QuartoControllerIntegrationTests : IntegrationTestBase
    {
        public QuartoControllerIntegrationTests(CustomWebApplicationFactory factory) : base(factory)
        {
        }

        private async Task AutenticarComoAdminAsync()
        {
            var email = EmailUnico("admin-quartos");
            await SeedUsuarioAsync("Administrador", email, "Senha123!", "Admin");
            AutenticarComo(await LoginAsync(email, "Senha123!"));
        }

        [Fact]
        public async Task ObterTodos_SemTokenDeAutenticacao_Retorna200()
        {
            var response = await Client.GetAsync("/quartos");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Criar_SemTokenDeAutenticacao_Retorna401()
        {
            var dto = new CriarQuartoDto(HotelId: 1, Numero: 101, Tipo: TipoQuarto.Standard, PrecoPorNoite: 200m);

            var response = await Client.PostAsJsonAsync("/quartos", dto);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Criar_ComUsuarioSemPapelAdmin_Retorna403()
        {
            var email = EmailUnico("usuario-comum");
            await SeedUsuarioAsync("Usuário Comum", email, "Senha123!", "User");
            AutenticarComo(await LoginAsync(email, "Senha123!"));

            var dto = new CriarQuartoDto(HotelId: 1, Numero: 101, Tipo: TipoQuarto.Standard, PrecoPorNoite: 200m);

            var response = await Client.PostAsJsonAsync("/quartos", dto);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task Criar_HotelInexistente_ComTokenAdmin_Retorna404()
        {
            await AutenticarComoAdminAsync();

            var dto = new CriarQuartoDto(HotelId: 999999999, Numero: 101, Tipo: TipoQuarto.Standard, PrecoPorNoite: 200m);

            var response = await Client.PostAsJsonAsync("/quartos", dto);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Criar_ComTokenAdmin_Retorna201ComStatusDisponivel()
        {
            var idHotel = await SeedHotelAsync(TextoUnico("Hotel"), "São Paulo");
            await AutenticarComoAdminAsync();

            var dto = new CriarQuartoDto(idHotel, Numero: 205, Tipo: TipoQuarto.Luxo, PrecoPorNoite: 350m);

            var response = await Client.PostAsJsonAsync("/quartos", dto);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var criado = await response.Content.ReadFromJsonAsync<QuartoResponseDto>(JsonOptions);
            Assert.Equal(StatusQuarto.Disponivel, criado!.Status);
            Assert.Equal(dto.Numero, criado.Numero);
        }

        [Fact]
        public async Task Deletar_QuartoInexistente_ComTokenAdmin_Retorna404()
        {
            await AutenticarComoAdminAsync();

            var response = await Client.DeleteAsync("/quartos/999999999");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Deletar_ComTokenAdmin_Retorna204ERemoveDoBanco()
        {
            var idHotel = await SeedHotelAsync(TextoUnico("Hotel"), "São Paulo");
            var idQuarto = await SeedQuartoAsync(idHotel, 101, TipoQuarto.Standard, 150m, StatusQuarto.Disponivel);
            await AutenticarComoAdminAsync();

            var response = await Client.DeleteAsync($"/quartos/{idQuarto}");

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            var busca = await Client.GetAsync($"/quartos/{idQuarto}");
            Assert.Equal(HttpStatusCode.NotFound, busca.StatusCode);
        }

        [Fact]
        public async Task Deletar_QuartoComReservas_Retorna204ESomeDaListagem()
        {
            var idHotel = await SeedHotelAsync(TextoUnico("Hotel"), "Curitiba");
            var idQuarto = await SeedQuartoAsync(idHotel, 707, TipoQuarto.Luxo, 400m, StatusQuarto.Disponivel);

            var idDono = await SeedUsuarioAsync("Dono", EmailUnico("dono-quarto-del"), "Senha123!", "User");
            await SeedReservaAsync(
                idDono, idQuarto,
                new DateTime(2026, 10, 1), new DateTime(2026, 10, 4),
                StatusReserva.Confirmada);

            await AutenticarComoAdminAsync();

            var response = await Client.DeleteAsync($"/quartos/{idQuarto}");

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            var busca = await Client.GetAsync($"/quartos/{idQuarto}");
            Assert.Equal(HttpStatusCode.NotFound, busca.StatusCode);

            var lista = await Client.GetFromJsonAsync<List<QuartoResponseDto>>("/quartos", JsonOptions);
            Assert.DoesNotContain(lista!, q => q.Id == idQuarto);
        }
    }
}
