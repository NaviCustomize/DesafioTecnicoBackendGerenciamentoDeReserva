using System.Net.Sockets;

namespace SistemaGerenciamentoDeReserva.Tests.Messaging
{
    internal static class RabbitMqDisponivel
    {
        internal const string HostName = "localhost";
        internal const int Port = 5672;
        internal const string UserName = "guest";
        internal const string Password = "guest";

        private static readonly Lazy<bool> Sonda = new(Verificar, isThreadSafe: true);

        internal static bool Sim => Sonda.Value;

        private static bool Verificar()
        {
            try
            {
                using var cliente = new TcpClient();

                return cliente.ConnectAsync(HostName, Port).Wait(TimeSpan.FromSeconds(2))
                    && cliente.Connected;
            }
            catch
            {
                return false;
            }
        }
    }

    public sealed class FatoComRabbitMqAttribute : FactAttribute
    {
        public FatoComRabbitMqAttribute()
        {
            if (!RabbitMqDisponivel.Sim)
            {
                Skip = $"RabbitMQ não respondeu em {RabbitMqDisponivel.HostName}:{RabbitMqDisponivel.Port}. " +
                       "Suba com 'docker compose up -d' para executar este teste.";
            }
        }
    }
}
