using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaGerenciamentoDeReserva.Application.DTOs.Reserva;
using SistemaGerenciamentoDeReserva.Application.Interface;
using System.Security.Claims;

namespace SistemaGerenciamentoDeReserva.API.Controller
{
    [ApiController]
    [Route("reservas")]
    [Authorize]
    public class ReservaController : ControllerBase
    {
        private readonly IReservaService _reservaService;

        public ReservaController(
            IReservaService reservaService)
        {
            _reservaService = reservaService;
        }

        [HttpPost]
        public async Task<IActionResult> Criar(
            [FromBody] CriarReservaDto dto)
        {
            try
            {
                var usuarioId = ObterUsuarioId();

                var reserva = await _reservaService.AdicionarReserva(dto, usuarioId);

                return CreatedAtAction(nameof(ObterPorId),new { id = reserva.Id },reserva);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
        }


        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ObterTodos()
        {
            var reservas = await _reservaService.ListarReserva();

            return Ok(reservas);
        }

        [HttpGet("{id:long}")]
        public async Task<IActionResult> ObterPorId(long id)
        {
            var reserva = await _reservaService.BuscarPorId(id);

            if (reserva is null)
                return NotFound("Reserva não encontrada.");


            if (!User.IsInRole("Admin") && reserva.UsuarioId != ObterUsuarioId())
            {
                return StatusCode(403, new
                {
                    mensagem = "Você só pode consultar as suas próprias reservas."
                });
            }

            return Ok(reserva);
        }

        [HttpGet("minhas")]
        public async Task<IActionResult> MinhasReservas()
        {
            var usuarioId = ObterUsuarioId();

            var reservas = await _reservaService.ListarReservasPorUsuario(usuarioId);

            return Ok(reservas);
        }

        [HttpGet("minhas/historico")]
        public async Task<IActionResult> Historico()
        {
            var usuarioId = ObterUsuarioId();

            var reservas = await _reservaService.ListarHistoricoPorUsuario(usuarioId);

            return Ok(reservas);
        }

        [HttpPut("{id:long}")]
        public async Task<IActionResult> Atualizar(
            long id,
            [FromBody] AtualizarReservaDto dto)
        {
            try
            {
                var usuarioId = ObterUsuarioId();

                await _reservaService.AtualizarReserva(id, dto, usuarioId);

                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
        }

        [HttpDelete("{id:long}")]
        public async Task<IActionResult> Deletar(long id)
        {
            try
            {
                var usuarioId = ObterUsuarioId();

                await _reservaService.DeletarReserva(id, usuarioId);

                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        private long ObterUsuarioId()
        {
            var claim = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (!long.TryParse(claim, out var usuarioId))
            {
                throw new UnauthorizedAccessException(
                    "Usuário não identificado.");
            }

            return usuarioId;
        }

        [HttpPatch("{id:long}/cancelar")]
        public async Task<IActionResult> Cancelar(long id)
        {
            try
            {
                var usuarioId = ObterUsuarioId();

                await _reservaService.CancelarReserva(id, usuarioId);

                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new
                {
                    mensagem = ex.Message
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new
                {
                    mensagem = ex.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new
                {
                    mensagem = ex.Message
                });
            }
        }
    }
}
