using Microsoft.AspNetCore.Mvc;
using SistemaGerenciamentoDeReserva.Application.DTOs;
using SistemaGerenciamentoDeReserva.Application.Services;
using SistemaGerenciamentoDeReserva.Domain.Interfaces;

namespace SistemaGerenciamentoDeReserva.API.Controller
{
    [ApiController]
    [Route("auth")]
    public class AuthController : ControllerBase
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly AuthService _authService;

        public AuthController(AuthService authService, IUsuarioRepository usuarioRepository)
        {
            _authService = authService;
            _usuarioRepository = usuarioRepository;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
        {
            var usuario = await _usuarioRepository.BuscarPorEmail(dto.Email);

            if (usuario == null || usuario.Senha != dto.Senha)
                return Unauthorized("Email ou senha inválidos");

            var token = _authService.GerarToken(usuario);

            return Ok(new
            {
                token,
                usuarioId = usuario.Id,
                nome = usuario.Nome
            });
        }
    }
}
