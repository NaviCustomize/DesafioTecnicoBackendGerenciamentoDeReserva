using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using SistemaGerenciamentoDeReserva.Application.Interface;
using SistemaGerenciamentoDeReserva.Infrastruture.Messaging;
using System.Linq;

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

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IReservaNotificacaoPublisher>();
                services.AddSingleton<IReservaNotificacaoPublisher, NoOpReservaNotificacaoPublisher>();

                var descriptor = services.SingleOrDefault(d =>
                    d.ServiceType == typeof(IHostedService) &&
                    d.ImplementationType == typeof(ReservaNotificacaoConsumer));

                if (descriptor is not null)
                    services.Remove(descriptor);
            });
        }
    }
}
