using Npgsql;
using System.Net;

namespace SistemaGerenciamentoDeReserva.API.Middleware
{
    public class TratamentoDeErroMiddleware
    {
        private const string ViolacaoDeChaveEstrangeira = "23503";

        private const string ViolacaoDeRestricao = "23001";

        private const string ViolacaoDeUnicidade = "23505";

        private readonly RequestDelegate _next;
        private readonly ILogger<TratamentoDeErroMiddleware> _logger;

        public TratamentoDeErroMiddleware(
            RequestDelegate next,
            ILogger<TratamentoDeErroMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (PostgresException ex)
            {
                _logger.LogError(ex, "Erro do banco em {Caminho}", context.Request.Path);

                var (status, mensagem) = TraduzirErroDeBanco(ex);

                await ResponderAsync(context, status, mensagem);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro não tratado em {Caminho}", context.Request.Path);

                await ResponderAsync(
                    context,
                    HttpStatusCode.InternalServerError,
                    "Ocorreu um erro inesperado. Tente novamente.");
            }
        }

        private static (HttpStatusCode, string) TraduzirErroDeBanco(PostgresException ex) =>
            ex.SqlState switch
            {
                ViolacaoDeRestricao or ViolacaoDeChaveEstrangeira => (
                    HttpStatusCode.Conflict,
                    "Este registro está vinculado a outros e não pode ser removido."),

                ViolacaoDeUnicidade => (
                    HttpStatusCode.Conflict,
                    "Já existe um registro com esses dados."),

                _ => (
                    HttpStatusCode.InternalServerError,
                    "Não foi possível concluir a operação no banco de dados."),
            };

        private static async Task ResponderAsync(
            HttpContext context,
            HttpStatusCode status,
            string mensagem)
        {
            if (context.Response.HasStarted)
            {
                return;
            }

            context.Response.Clear();
            context.Response.StatusCode = (int)status;
            context.Response.ContentType = "application/json";

            await context.Response.WriteAsJsonAsync(new
            {
                status = (int)status,
                mensagem
            });
        }
    }
}
