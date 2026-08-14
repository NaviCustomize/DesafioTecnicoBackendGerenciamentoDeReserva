using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace SistemaGerenciamentoDeReserva.Tests.Integration
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        public CustomWebApplicationFactory()
        {
            var secrets = new ConfigurationBuilder()
                .AddUserSecrets<CustomWebApplicationFactory>(optional: true)
                .Build();

            foreach (var (key, value) in secrets.AsEnumerable(makePathsRelative: false))
            {
                if (value is not null)
                {
                    Environment.SetEnvironmentVariable(key.Replace(":", "__"), value);
                }
            }
        }
    }
}
