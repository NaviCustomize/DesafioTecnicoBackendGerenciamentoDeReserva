using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using SistemaGerenciamentoDeReserva.Application.DTOs.Login;
using SistemaGerenciamentoDeReserva.Domain.Entity;
using SistemaGerenciamentoDeReserva.Domain.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SistemaGerenciamentoDeReserva.Application.Services
{
    public class AuthService
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly ISenhaHasher _senhaHasher;
        private readonly IJwtService _jwtService;

        public AuthService(
            IUsuarioRepository usuarioRepository,
            ISenhaHasher senhaHasher,
            IJwtService jwtService)
        {
            _usuarioRepository = usuarioRepository;
            _senhaHasher = senhaHasher;
            _jwtService = jwtService;
        }

        public async Task<LoginResponseDto> LoginAsync(string email,string senha)
        {
            var usuario = await _usuarioRepository.ObterPorEmailAsync(email);

            if (usuario is null)
                throw new UnauthorizedAccessException("Email ou senha inválidos.");

            var senhaValida = _senhaHasher.Verify(senha,usuario.SenhaHash);

            if (!senhaValida)
                throw new UnauthorizedAccessException(
                    "Email ou senha inválidos.");

            var token = _jwtService.GerarToken(usuario);


            return new LoginResponseDto(
                token, usuario.Id, usuario.Nome, usuario.Sobrenome, usuario.Role);
        }
    }
}
