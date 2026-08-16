using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaGerenciamentoDeReserva.Application.DTOs.Usuario;
using SistemaGerenciamentoDeReserva.Application.Interface;
using System.Security.Claims;

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
            catch (ArgumentException ex)
            {
                return BadRequest(new { mensagem = ex.Message });
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
            var usuarios = await _usuarioService.ListarUsuarios();

            return Ok(usuarios);
        }

        [HttpGet("gestao")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ObterParaGestao()
        {
            var usuarios = await _usuarioService.ListarUsuariosParaAdmin();

            return Ok(usuarios);
        }

        [HttpPatch("{id:long}/reativar")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Reativar(long id)
        {
            try
            {
                await _usuarioService.ReativarUsuario(id);

                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { mensagem = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { mensagem = ex.Message });
            }
        }

        [HttpGet("{id:long}")]
        public async Task<IActionResult> ObterPorId(long id)
        {
            if (!PodeAcessarUsuario(id))
            {
                return StatusCode(403, new
                {
                    mensagem = "Você só pode consultar os seus próprios dados."
                });
            }

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
            if (!PodeAcessarUsuario(id))
            {
                return StatusCode(403, new
                {
                    mensagem = "Você só pode alterar os seus próprios dados."
                });
            }

            try
            {
                await _usuarioService.AtualizarUsuario(id, dto);

                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { mensagem = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
        }

        [HttpPut("{id:long}/senha")]
        public async Task<IActionResult> AlterarSenha(
            long id,
            [FromBody] AlterarSenhaDto dto)
        {
            if (!EhOProprioUsuario(id))
            {
                return StatusCode(403, new
                {
                    mensagem = "Você só pode alterar a sua própria senha."
                });
            }

            try
            {
                await _usuarioService.AlterarSenha(id, dto);

                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { mensagem = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { mensagem = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { mensagem = ex.Message });
            }
        }

        [HttpPost("{id:long}/encerrar-conta")]
        public async Task<IActionResult> EncerrarPropriaConta(
            long id,
            [FromBody] ConfirmarSenhaDto dto)
        {
            if (!EhOProprioUsuario(id))
            {
                return StatusCode(403, new
                {
                    mensagem = "Você só pode encerrar a sua própria conta."
                });
            }

            try
            {
                await _usuarioService.EncerrarPropriaConta(id, dto);

                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { mensagem = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { mensagem = ex.Message });
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

        private bool EhOProprioUsuario(long id)
        {
            var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            return long.TryParse(claim, out var usuarioId) && usuarioId == id;
        }

        private bool PodeAcessarUsuario(long id)
        {
            if (User.IsInRole("Admin"))
            {
                return true;
            }

            var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            return long.TryParse(claim, out var usuarioId) && usuarioId == id;
        }
    }
}
