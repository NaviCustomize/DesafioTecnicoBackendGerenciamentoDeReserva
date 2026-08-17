using Microsoft.Extensions.Configuration;
using Npgsql;
using SistemaGerenciamentoDeReserva.Domain.Entity;
using SistemaGerenciamentoDeReserva.Domain.Enums;
using SistemaGerenciamentoDeReserva.Infrastruture.Repositories;

namespace SistemaGerenciamentoDeReserva.Tests.Infrastructure
{
    /// <summary>
    /// Base dos testes de repositório. Cada teste abre a própria conexão com o banco de
    /// teste e usa dados únicos, então a ordem de execução não importa e não há limpeza
    /// entre os testes.
    /// </summary>
    public abstract class RepositoryTestBase
    {
        private static readonly string ConnectionString = CarregarConnectionString();

        private static string CarregarConnectionString()
        {
            var configuracao = new ConfigurationBuilder()
                .AddUserSecrets<RepositoryTestBase>(optional: true)
                .AddEnvironmentVariables()
                .Build();

            return configuracao.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException(
                    "ConnectionStrings:DefaultConnection não configurada. " +
                    "Rode: dotnet user-secrets set \"ConnectionStrings:DefaultConnection\" \"...\" " +
                    "na pasta do projeto de testes, apontando para Gerenciamento_reserva_test.");
        }

        protected static NpgsqlConnection CriarConexao()
        {
            var conexao = new NpgsqlConnection(ConnectionString);
            conexao.Open();
            return conexao;
        }

        protected static string TextoUnico(string prefixo) => $"{prefixo}-{Guid.NewGuid():N}";

        protected static string EmailUnico(string prefixo) => $"{prefixo}.{Guid.NewGuid():N}@repositorio.test";

        protected static int NumeroUnico() => Random.Shared.Next(100_000, 999_999);

        /// <summary>Data futura, com o horário padrão de check-in do hotel.</summary>
        protected static DateTime CheckIn(int daqui) => DateTime.Today.AddDays(daqui).AddHours(14);

        /// <summary>Data futura, com o horário padrão de check-out do hotel.</summary>
        protected static DateTime CheckOut(int daqui) => DateTime.Today.AddDays(daqui).AddHours(12);

        protected static async Task<long> SemearUsuarioAsync(RoleUsuario role = RoleUsuario.User)
        {
            using var conexao = CriarConexao();

            return await new UsuarioRepository(conexao).AdicionarAsync(new Usuario
            {
                Nome = TextoUnico("Usuario"),
                Sobrenome = "Teste",
                Email = EmailUnico("usuario"),
                SenhaHash = BCrypt.Net.BCrypt.HashPassword("Senha@123"),
                Role = role
            });
        }

        protected static async Task<long> SemearHotelAsync()
        {
            using var conexao = CriarConexao();

            return await new HotelRepository(conexao).AdicionarAsync(new Hotel
            {
                Nome = TextoUnico("Hotel"),
                Localizacao = "Petrópolis",
                Descricao = "Semeado por teste de repositório"
            });
        }

        protected static async Task<long> SemearQuartoAsync(long? hotelId = null)
        {
            var hotel = hotelId ?? await SemearHotelAsync();

            using var conexao = CriarConexao();

            return await new QuartoRepository(conexao).AdicionarAsync(new Quarto
            {
                HotelId = hotel,
                Numero = NumeroUnico(),
                Tipo = TipoQuarto.Standard,
                PrecoPorNoite = 250m,
                Status = StatusQuarto.Disponivel
            });
        }
    }
}
