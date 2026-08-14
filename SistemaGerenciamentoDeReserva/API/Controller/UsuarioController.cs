using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaGerenciamentoDeReserva.Application.DTOs.Usuario;
using SistemaGerenciamentoDeReserva.Application.Interface;

namespace SistemaGerenciamentoDeReserva.API.Controller
{
    [ApiController]
    [Route("usuarios")]
    [Authorize]
    public class UsuarioController : ControllerBase
    {
        private readonly IUsuarioService _usuarioService;

        public UsuarioController(IUsuarioService usuarioService)
        {
            _usuarioService = usuarioService;
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Criar(
            [FromBody] CriarUsuarioDto dto)
        {
            try
            {
                var usuario = await _usuarioService.AdicionarUsuario(dto);

                return CreatedAtAction(nameof(ObterPorId),new { id = usuario.Id },usuario);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> ObterTodos()
        {
            var usuarios = await _usuarioService.ListarUsuarios();

            return Ok(usuarios);
        }

        [HttpGet("{id:long}")]
        public async Task<IActionResult> ObterPorId(long id)
        {
            var usuario = await _usuarioService.BuscarPorId(id);

            if (usuario is null)
                return NotFound("Usuário não encontrado.");

            return Ok(usuario);
        }

        [HttpPut("{id:long}")]
        public async Task<IActionResult> Atualizar(
            long id,
            [FromBody] AtualizarUsuarioDto dto)
        {
            try
            {
                await _usuarioService.AtualizarUsuario(id, dto);

                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
        }

        [HttpDelete("{id:long}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Deletar(long id)
        {
            try
            {
                await _usuarioService.DeletarUsuario(id);

                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}
