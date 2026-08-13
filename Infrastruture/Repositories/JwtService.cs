using Microsoft.IdentityModel.Tokens;
using SistemaGerenciamentoDeReserva.Domain.Entity;
using SistemaGerenciamentoDeReserva.Domain.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SistemaGerenciamentoDeReserva.Infrastruture.Repositories
{
    public class JwtService : IJwtService
    {
        private readonly IConfiguration _configuration;

        public JwtService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GerarToken(Usuario usuario)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            var secretKey = _configuration["JwtSettings:Secret"];
            var key = Encoding.ASCII.GetBytes(secretKey!);

            var expireMinutes = Convert.ToDouble(
                _configuration["JwtSettings:ExpireMinutes"] ?? "60");

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                new Claim(
                    ClaimTypes.NameIdentifier,
                    usuario.Id.ToString()),

                new Claim(
                    ClaimTypes.Email,
                    usuario.Email),
                new Claim(
                    ClaimTypes.Role,
                    usuario.Role.ToString())
            }),

                Expires = DateTime.UtcNow.AddMinutes(expireMinutes),

                Issuer = _configuration["JwtSettings:Issuer"],
                Audience = _configuration["JwtSettings:Audience"],

                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}
