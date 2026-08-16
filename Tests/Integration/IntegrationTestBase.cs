using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using SistemaGerenciamentoDeReserva.Domain.Enums;

namespace SistemaGerenciamentoDeReserva.Tests.Integration
{
    public abstract class IntegrationTestBase : IClassFixture<CustomWebApplicationFactory>
    {
        protected static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() }
        };

        protected readonly HttpClient Client;
        private readonly string _connectionString;

        protected IntegrationTestBase(CustomWebApplicationFactory factory)
        {
            Client = factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                BaseAddress = new Uri("https://localhost")
            });

            _connectionString = factory.Services
                .GetRequiredService<IConfiguration>()
                .GetConnectionString("DefaultConnection")!;
        }

        protected void AutenticarComo(string token)
        {
            Client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }

        protected static string EmailUnico(string prefixo) =>
            $"{prefixo}.{Guid.NewGuid():N}@integration.test";

        protected static string TextoUnico(string prefixo) =>
            $"{prefixo}-{Guid.NewGuid():N}";

        protected async Task<long> SeedUsuarioAsync(
            string nome, string email, string senha, string role, string sobrenome = "Sobrenome")
        {
            const string sql = """
                INSERT INTO usuarios (nome, sobrenome, email, senha_hash, role)
                VALUES (@Nome, @Sobrenome, @Email, @SenhaHash, @Role)
                RETURNING id;
                """;

            return await ExecuteScalarAsync(sql,
                new NpgsqlParameter("Nome", nome),
                new NpgsqlParameter("Sobrenome", sobrenome),
                new NpgsqlParameter("Email", email),
                new NpgsqlParameter("SenhaHash", BCrypt.Net.BCrypt.HashPassword(senha)),
                new NpgsqlParameter("Role", role));
        }

        protected async Task<long> SeedHotelAsync(string nome, string localizacao)
        {
            const string sql = """
                INSERT INTO hoteis (nome, localizacao, descricao)
                VALUES (@Nome, @Localizacao, NULL)
                RETURNING id;
                """;

            return await ExecuteScalarAsync(sql,
                new NpgsqlParameter("Nome", nome),
                new NpgsqlParameter("Localizacao", localizacao));
        }

        protected async Task<long> SeedQuartoAsync(
            long hotelId, int numero, TipoQuarto tipo, decimal precoPorNoite, StatusQuarto status)
        {
            const string sql = """
                INSERT INTO quartos (hotel_id, numero, tipo, preco_por_noite, status)
                VALUES (@HotelId, @Numero, @Tipo, @Preco, @Status)
                RETURNING id;
                """;

            return await ExecuteScalarAsync(sql,
                new NpgsqlParameter("HotelId", hotelId),
                new NpgsqlParameter("Numero", numero),
                new NpgsqlParameter("Tipo", (int)tipo),
                new NpgsqlParameter("Preco", precoPorNoite),
                new NpgsqlParameter("Status", (int)status));
        }

        protected async Task<long> SeedReservaAsync(
            long usuarioId, long quartoId, DateTime checkIn, DateTime checkOut, StatusReserva status)
        {
            const string sql = """
                INSERT INTO reservas (data_checkin, data_checkout, status, usuario_id, quarto_id)
                VALUES (@CheckIn, @CheckOut, @Status, @UsuarioId, @QuartoId)
                RETURNING id;
                """;

            return await ExecuteScalarAsync(sql,
                new NpgsqlParameter("CheckIn", checkIn),
                new NpgsqlParameter("CheckOut", checkOut),
                new NpgsqlParameter("Status", (int)status),
                new NpgsqlParameter("UsuarioId", usuarioId),
                new NpgsqlParameter("QuartoId", quartoId));
        }

        private async Task<long> ExecuteScalarAsync(string sql, params NpgsqlParameter[] parameters)
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddRange(parameters);

            return Convert.ToInt64(await command.ExecuteScalarAsync());
        }

        protected async Task<string> LoginAsync(string email, string senha)
        {
            var response = await Client.PostAsJsonAsync("/auth/login", new { email, senha });
            response.EnsureSuccessStatusCode();

            var body = await response.Content.ReadFromJsonAsync<TokenResponse>();
            return body!.Token;
        }

        private record TokenResponse(string Token);
    }
}
