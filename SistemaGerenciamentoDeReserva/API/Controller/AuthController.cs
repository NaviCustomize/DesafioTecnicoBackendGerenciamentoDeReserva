using Microsoft.AspNetCore.Mvc;
using SistemaGerenciamentoDeReserva.Application.DTOs.Login;
using SistemaGerenciamentoDeReserva.Application.Services;
using SistemaGerenciamentoDeReserva.Domain.Interfaces;

namespace SistemaGerenciamentoDeReserva.API.Controller
{
    [ApiController]
    [Route("auth")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(
            [FromBody] LoginRequestDto dto)
        {
            try
            {
                var resultado = await _authService.LoginAsync(
                    dto.Email,
                    dto.Senha);

                return Ok(resultado);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized("Email ou senha inválidos");
            }
        }
    }
}
