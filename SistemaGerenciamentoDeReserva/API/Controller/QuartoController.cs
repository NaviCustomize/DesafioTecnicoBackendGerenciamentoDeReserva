using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaGerenciamentoDeReserva.Application.DTOs.Quarto;
using SistemaGerenciamentoDeReserva.Application.Interface;

namespace SistemaGerenciamentoDeReserva.API.Controller
{
    [ApiController]
    [Route("quartos")]
    public class QuartoController : ControllerBase
    {
        private readonly IQuartoService _quartoService;

        public QuartoController(IQuartoService quartoService)
        {
            _quartoService = quartoService;
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Criar(
            [FromBody] CriarQuartoDto dto)
        {
            try
            {
                var quarto = await _quartoService.AdicionarQuarto(dto);

                return CreatedAtAction(nameof(ObterPorId),new { id = quarto.Id },quarto);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> ObterTodos()
        {
            var quartos = await _quartoService.ListarQuarto();

            return Ok(quartos);
        }

        [HttpGet("{id:long}")]
        public async Task<IActionResult> ObterPorId(long id)
        {
            var quarto = await _quartoService.BuscarPorId(id);

            if (quarto is null)
                return NotFound("Quarto não encontrado.");

            return Ok(quarto);
        }

        [HttpPut("{id:long}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Atualizar(
            long id,
            [FromBody] AtualizarQuartoDto dto)
        {
            try
            {
                await _quartoService.AtualizarQuarto(id, dto);

                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpDelete("{id:long}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Deletar(long id)
        {
            try
            {
                await _quartoService.DeletarQuarto(id);

                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}
