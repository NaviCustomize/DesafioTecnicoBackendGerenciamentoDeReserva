using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaGerenciamentoDeReserva.Application.DTOs.Notificacao;
using SistemaGerenciamentoDeReserva.Domain.Interfaces;

namespace SistemaGerenciamentoDeReserva.API.Controller
{
    [ApiController]
    [Route("notificacoes")]
    [Authorize(Roles = "Admin")]
    public class NotificacaoController : ControllerBase
    {
        private const int LimitePadrao = 50;

        private readonly INotificacaoRepository _notificacaoRepository;

        public NotificacaoController(INotificacaoRepository notificacaoRepository)
        {
            _notificacaoRepository = notificacaoRepository;
        }

        [HttpGet]
        public async Task<IActionResult> ObterRecentes([FromQuery] int limite = LimitePadrao)
        {
            var quantidade = Math.Clamp(limite, 1, 200);

            var notificacoes = await _notificacaoRepository.ObterRecentesAsync(quantidade);

            return Ok(notificacoes.Select(n => new NotificacaoResponseDto(
                n.Id,
                n.ReservaId,
                n.TipoEvento,
                n.Hospede,
                n.HospedeEmail,
                n.Hotel,
                n.QuartoNumero,
                n.DataCheckIn,
                n.DataCheckOut,
                n.ProcessadoEm)));
        }
    }
}
