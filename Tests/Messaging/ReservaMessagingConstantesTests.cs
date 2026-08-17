using SistemaGerenciamentoDeReserva.Infrastruture.Messaging;

namespace SistemaGerenciamentoDeReserva.Tests.Messaging
{
    public class ReservaMessagingConstantesTests
    {
        [Theory]
        [InlineData("Confirmada", "reserva.confirmada")]
        [InlineData("Atualizada", "reserva.atualizada")]
        [InlineData("Cancelada", "reserva.cancelada")]
        [InlineData("Lembrete", "reserva.lembrete")]
        public void RoutingKeyPara_DeveGerarChaveEmMinusculasComPrefixo(string tipoEvento, string esperado)
        {
            Assert.Equal(esperado, ReservaMessagingConstantes.RoutingKeyPara(tipoEvento));
        }

        [Theory]
        [InlineData("Confirmada")]
        [InlineData("Atualizada")]
        [InlineData("Cancelada")]
        [InlineData("Lembrete")]
        public void RoutingKeyPara_DeveCasarComOBindingDaFila(string tipoEvento)
        {
            var chave = ReservaMessagingConstantes.RoutingKeyPara(tipoEvento);
            var padrao = ReservaMessagingConstantes.RoutingKeyPattern;

            var prefixo = padrao[..padrao.IndexOf('*')];

            Assert.StartsWith(prefixo, chave);
            Assert.Equal(2, chave.Split('.').Length);
        }

        [Fact]
        public void DeadLetter_DeveUsarExchangeEFilaProprias()
        {
            Assert.NotEqual(ReservaMessagingConstantes.Exchange, ReservaMessagingConstantes.DeadLetterExchange);
            Assert.NotEqual(ReservaMessagingConstantes.Queue, ReservaMessagingConstantes.DeadLetterQueue);
        }

        [Fact]
        public void DeadLetterRoutingKey_NaoDeveCasarComOBindingDaFilaPrincipal()
        {
            var chaveDlq = ReservaMessagingConstantes.DeadLetterRoutingKey;

            Assert.NotEqual(ReservaMessagingConstantes.Exchange, ReservaMessagingConstantes.DeadLetterExchange);
            Assert.DoesNotContain(chaveDlq, new[]
            {
                ReservaMessagingConstantes.RoutingKeyPara("Confirmada"),
                ReservaMessagingConstantes.RoutingKeyPara("Atualizada"),
                ReservaMessagingConstantes.RoutingKeyPara("Cancelada"),
                ReservaMessagingConstantes.RoutingKeyPara("Lembrete")
            });
        }
    }
}
