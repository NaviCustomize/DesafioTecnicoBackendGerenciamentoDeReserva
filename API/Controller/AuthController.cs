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
                var token = await _authService.LoginAsync(
                    dto.Email,
                    dto.Senha);

                return Ok(new
                {
                    token
                });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized("Email ou senha inválidos");
            }
        }
    }
}
