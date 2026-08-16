using System.Net;
using System.Net.Http.Json;
using SistemaGerenciamentoDeReserva.Application.DTOs.Reserva;
using SistemaGerenciamentoDeReserva.Application.DTOs.Quarto;
using SistemaGerenciamentoDeReserva.Domain.Enums;

namespace SistemaGerenciamentoDeReserva.Tests.Integration
{
    public class ReservaControllerIntegrationTests : IntegrationTestBase
    {
        public ReservaControllerIntegrationTests(CustomWebApplicationFactory factory) : base(factory)
        {
        }

        private async Task<(long HotelId, long QuartoId)> SeedHotelComQuartoAsync(StatusQuarto status = StatusQuarto.Disponivel)
        {
            var idHotel = await SeedHotelAsync(TextoUnico("Hotel"), "São Paulo");
            var idQuarto = await SeedQuartoAsync(idHotel, 101, TipoQuarto.Standard, 200m, status);
            return (idHotel, idQuarto);
        }

        private async Task<QuartoResponseDto> ObterQuartoAsync(long id)
        {
            var resposta = await Client.GetAsync($"/quartos/{id}");
            resposta.EnsureSuccessStatusCode();

            return (await resposta.Content.ReadFromJsonAsync<QuartoResponseDto>(JsonOptions))!;
        }

        [Fact]
        public async Task Criar_SemTokenDeAutenticacao_Retorna401()
        {
            var dto = new CriarReservaDto(1, new DateTime(2026, 3, 10), new DateTime(2026, 3, 12));

            var response = await Client.PostAsJsonAsync("/reservas", dto);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Criar_CenarioValido_Retorna201EPersisteComoConfirmada()
        {
            var (_, idQuarto) = await SeedHotelComQuartoAsync();

            var emailUsuario = EmailUnico("hospede-criar");
            await SeedUsuarioAsync("Hóspede", emailUsuario, "Senha123!", "User");
            AutenticarComo(await LoginAsync(emailUsuario, "Senha123!"));

            var dto = new CriarReservaDto(idQuarto, new DateTime(2026, 4, 10), new DateTime(2026, 4, 12));

            var response = await Client.PostAsJsonAsync("/reservas", dto);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var criada = await response.Content.ReadFromJsonAsync<ReservaResponseDto>(JsonOptions);
            Assert.Equal(StatusReserva.Confirmada, criada!.Status);
            Assert.Equal(idQuarto, criada.QuartoId);
        }

        [Fact]
        public async Task Criar_ConflitoDePeriodoComReservaExistente_Retorna409()
        {
            var (_, idQuarto) = await SeedHotelComQuartoAsync();

            var emailDono = EmailUnico("dono-original");
            var idDono = await SeedUsuarioAsync("Dono Original", emailDono, "Senha123!", "User");
            await SeedReservaAsync(
                idDono, idQuarto,
                new DateTime(2026, 5, 10), new DateTime(2026, 5, 20),
                StatusReserva.Confirmada);

            var emailUsuario = EmailUnico("hospede-conflito");
            await SeedUsuarioAsync("Hóspede", emailUsuario, "Senha123!", "User");
            AutenticarComo(await LoginAsync(emailUsuario, "Senha123!"));

            var dto = new CriarReservaDto(idQuarto, new DateTime(2026, 5, 15), new DateTime(2026, 5, 25));

            var response = await Client.PostAsJsonAsync("/reservas", dto);

            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        }

        [Fact]
        public async Task MinhasReservas_RetornaSomenteReservasDoUsuarioLogado()
        {
            var (_, idQuarto) = await SeedHotelComQuartoAsync();

            var emailA = EmailUnico("usuario-a");
            var idUsuarioA = await SeedUsuarioAsync("Usuário A", emailA, "Senha123!", "User");
            var idReservaA = await SeedReservaAsync(
                idUsuarioA, idQuarto,
                new DateTime(2026, 6, 1), new DateTime(2026, 6, 3),
                StatusReserva.Confirmada);

            var emailB = EmailUnico("usuario-b");
            var idUsuarioB = await SeedUsuarioAsync("Usuário B", emailB, "Senha123!", "User");
            var idReservaB = await SeedReservaAsync(
                idUsuarioB, idQuarto,
                new DateTime(2026, 6, 10), new DateTime(2026, 6, 12),
                StatusReserva.Confirmada);

            AutenticarComo(await LoginAsync(emailA, "Senha123!"));

            var response = await Client.GetAsync("/reservas/minhas");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var reservas = await response.Content.ReadFromJsonAsync<List<ReservaResponseDto>>(JsonOptions);

            Assert.Contains(reservas!, r => r.Id == idReservaA);
            Assert.DoesNotContain(reservas!, r => r.Id == idReservaB);
        }

        [Fact]
        public async Task Cancelar_ReservaDeOutroUsuario_Retorna403()
        {
            var (_, idQuarto) = await SeedHotelComQuartoAsync();

            var emailDono = EmailUnico("dono-cancelar");
            var idDono = await SeedUsuarioAsync("Dono", emailDono, "Senha123!", "User");
            var idReserva = await SeedReservaAsync(
                idDono, idQuarto,
                new DateTime(2026, 7, 1), new DateTime(2026, 7, 3),
                StatusReserva.Confirmada);

            var emailIntruso = EmailUnico("intruso");
            await SeedUsuarioAsync("Intruso", emailIntruso, "Senha123!", "User");
            AutenticarComo(await LoginAsync(emailIntruso, "Senha123!"));

            var response = await Client.PatchAsync($"/reservas/{idReserva}/cancelar", content: null);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task Cancelar_CenarioValido_Retorna204EAtualizaStatusParaCancelada()
        {
            var (_, idQuarto) = await SeedHotelComQuartoAsync();

            var emailDono = EmailUnico("dono-cancelar-ok");
            var idDono = await SeedUsuarioAsync("Dono", emailDono, "Senha123!", "User");
            var idReserva = await SeedReservaAsync(
                idDono, idQuarto,
                new DateTime(2026, 8, 1), new DateTime(2026, 8, 3),
                StatusReserva.Confirmada);

            AutenticarComo(await LoginAsync(emailDono, "Senha123!"));

            var response = await Client.PatchAsync($"/reservas/{idReserva}/cancelar", content: null);

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            var busca = await Client.GetAsync($"/reservas/{idReserva}");
            var reserva = await busca.Content.ReadFromJsonAsync<ReservaResponseDto>(JsonOptions);

            Assert.Equal(StatusReserva.Cancelada, reserva!.Status);
        }

        [Fact]
        public async Task ObterPorId_ReservaDeOutroUsuario_Retorna403()
        {
            var (_, idQuarto) = await SeedHotelComQuartoAsync();

            var idDono = await SeedUsuarioAsync("Dono", EmailUnico("dono-get-403"), "Senha123!", "User");
            var idReserva = await SeedReservaAsync(
                idDono, idQuarto,
                new DateTime(2026, 9, 1), new DateTime(2026, 9, 4),
                StatusReserva.Confirmada);

            var emailIntruso = EmailUnico("intruso-get");
            await SeedUsuarioAsync("Intruso", emailIntruso, "Senha123!", "User");
            AutenticarComo(await LoginAsync(emailIntruso, "Senha123!"));

            var response = await Client.GetAsync($"/reservas/{idReserva}");

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task ObterTodos_ComUsuarioSemPapelAdmin_Retorna403()
        {
            var email = EmailUnico("comum-todas-reservas");
            await SeedUsuarioAsync("Usuário Comum", email, "Senha123!", "User");
            AutenticarComo(await LoginAsync(email, "Senha123!"));

            var response = await Client.GetAsync("/reservas");

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task ObterTodos_ComPapelAdmin_Retorna200()
        {
            var email = EmailUnico("admin-todas-reservas");
            await SeedUsuarioAsync("Administrador", email, "Senha123!", "Admin");
            AutenticarComo(await LoginAsync(email, "Senha123!"));

            var response = await Client.GetAsync("/reservas");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Criar_QuartoDisponivel_MarcaQuartoComoReservado()
        {
            var (_, idQuarto) = await SeedHotelComQuartoAsync();

            var email = EmailUnico("hospede-status");
            await SeedUsuarioAsync("Hóspede", email, "Senha123!", "User");
            AutenticarComo(await LoginAsync(email, "Senha123!"));

            var dto = new CriarReservaDto(idQuarto, new DateTime(2026, 5, 10), new DateTime(2026, 5, 12));

            var response = await Client.PostAsJsonAsync("/reservas", dto);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var quarto = await ObterQuartoAsync(idQuarto);

            Assert.Equal(StatusQuarto.Reservado, quarto.Status);
        }

        [Fact]
        public async Task Cancelar_UnicaReserva_DevolveQuartoParaDisponivel()
        {
            var (_, idQuarto) = await SeedHotelComQuartoAsync();

            var email = EmailUnico("hospede-status-cancela");
            await SeedUsuarioAsync("Hóspede", email, "Senha123!", "User");
            AutenticarComo(await LoginAsync(email, "Senha123!"));

            var criacao = await Client.PostAsJsonAsync(
                "/reservas",
                new CriarReservaDto(idQuarto, new DateTime(2026, 5, 20), new DateTime(2026, 5, 22)));

            var criada = await criacao.Content.ReadFromJsonAsync<ReservaResponseDto>(JsonOptions);

            await Client.PatchAsync($"/reservas/{criada!.Id}/cancelar", content: null);

            var quarto = await ObterQuartoAsync(idQuarto);

            Assert.Equal(StatusQuarto.Disponivel, quarto.Status);
        }

        [Fact]
        public async Task Criar_SegundoPeriodoNoMesmoQuarto_Retorna201()
        {
            var (_, idQuarto) = await SeedHotelComQuartoAsync();

            var email = EmailUnico("hospede-dois-periodos");
            await SeedUsuarioAsync("Hóspede", email, "Senha123!", "User");
            AutenticarComo(await LoginAsync(email, "Senha123!"));

            var primeira = await Client.PostAsJsonAsync(
                "/reservas",
                new CriarReservaDto(idQuarto, new DateTime(2026, 6, 1), new DateTime(2026, 6, 5)));

            Assert.Equal(HttpStatusCode.Created, primeira.StatusCode);

            var segunda = await Client.PostAsJsonAsync(
                "/reservas",
                new CriarReservaDto(idQuarto, new DateTime(2026, 7, 1), new DateTime(2026, 7, 5)));

            Assert.Equal(HttpStatusCode.Created, segunda.StatusCode);
        }

        [Fact]
        public async Task Criar_AplicaHorarioPadraoDoHotel()
        {
            var (_, idQuarto) = await SeedHotelComQuartoAsync();

            var email = EmailUnico("hospede-horario");
            await SeedUsuarioAsync("Hóspede", email, "Senha123!", "User");
            AutenticarComo(await LoginAsync(email, "Senha123!"));

            var response = await Client.PostAsJsonAsync(
                "/reservas",
                new CriarReservaDto(idQuarto, new DateTime(2027, 1, 10), new DateTime(2027, 1, 14)));

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var criada = await response.Content.ReadFromJsonAsync<ReservaResponseDto>(JsonOptions);

            Assert.Equal(new DateTime(2027, 1, 10, 14, 0, 0), criada!.DataCheckIn);
            Assert.Equal(new DateTime(2027, 1, 14, 12, 0, 0), criada.DataCheckOut);
        }

        [Fact]
        public async Task Criar_CheckInNoDiaDoCheckOutAnterior_Retorna201()
        {
            var (_, idQuarto) = await SeedHotelComQuartoAsync();

            var email = EmailUnico("hospede-virada");
            await SeedUsuarioAsync("Hóspede", email, "Senha123!", "User");
            AutenticarComo(await LoginAsync(email, "Senha123!"));

            var primeira = await Client.PostAsJsonAsync(
                "/reservas",
                new CriarReservaDto(idQuarto, new DateTime(2027, 2, 1), new DateTime(2027, 2, 5)));
            Assert.Equal(HttpStatusCode.Created, primeira.StatusCode);

            var segunda = await Client.PostAsJsonAsync(
                "/reservas",
                new CriarReservaDto(idQuarto, new DateTime(2027, 2, 5), new DateTime(2027, 2, 8)));

            Assert.Equal(HttpStatusCode.Created, segunda.StatusCode);
        }
    }
}
