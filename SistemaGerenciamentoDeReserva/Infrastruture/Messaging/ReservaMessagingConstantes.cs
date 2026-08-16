namespace SistemaGerenciamentoDeReserva.Infrastruture.Messaging
{
    internal static class ReservaMessagingConstantes
    {
        public const string Exchange = "reservas.exchange";
        public const string Queue = "reservas.notificacoes";
        public const string RoutingKeyPattern = "reserva.*";

        public const string DeadLetterExchange = "reservas.dlx";
        public const string DeadLetterQueue = "reservas.notificacoes.dlq";
        public const string DeadLetterRoutingKey = "reserva.falha";

        public static string RoutingKeyPara(string tipoEvento) =>
            $"reserva.{tipoEvento.ToLowerInvariant()}";
    }
}
