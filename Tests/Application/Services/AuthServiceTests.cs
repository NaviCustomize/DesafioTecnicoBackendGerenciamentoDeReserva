using Moq;
using SistemaGerenciamentoDeReserva.Application.Services;
using SistemaGerenciamentoDeReserva.Domain.Entity;
using SistemaGerenciamentoDeReserva.Domain.Enums;
using SistemaGerenciamentoDeReserva.Domain.Interfaces;

namespace SistemaGerenciamentoDeReserva.Tests.Application.Services
{
    public class AuthServiceTests
    {
        private readonly Mock<IUsuarioRepository> _usuarioRepositoryMock;
        private readonly Mock<ISenhaHasher> _senhaHasherMock;
        private readonly Mock<IJwtService> _jwtServiceMock;
        private readonly AuthService _service;

        public AuthServiceTests()
        {
            _usuarioRepositoryMock = new Mock<IUsuarioRepository>();
            _senhaHasherMock = new Mock<ISenhaHasher>();
            _jwtServiceMock = new Mock<IJwtService>();

            _service = new AuthService(
                _usuarioRepositoryMock.Object,
                _senhaHasherMock.Object,
                _jwtServiceMock.Object);
        }

        [Fact]
        public async Task LoginAsync_UsuarioInexistente_LancaUnauthorizedAccessException()
        {
            _usuarioRepositoryMock
                .Setup(r => r.ObterPorEmailAsync("joao@email.com"))
                .ReturnsAsync((Usuario?)null);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => _service.LoginAsync("joao@email.com", "senha123"));
        }

        [Fact]
        public async Task LoginAsync_SenhaInvalida_LancaUnauthorizedAccessException()
        {
            var usuario = new Usuario("João", "joao@email.com", "hash-correto", RoleUsuario.User);

            _usuarioRepositoryMock
                .Setup(r => r.ObterPorEmailAsync(usuario.Email))
                .ReturnsAsync(usuario);

            _senhaHasherMock
                .Setup(h => h.Verify("senha-errada", usuario.SenhaHash))
                .Returns(false);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => _service.LoginAsync(usuario.Email, "senha-errada"));

            _jwtServiceMock.Verify(
                j => j.GerarToken(It.IsAny<Usuario>()),
                Times.Never);
        }

        [Fact]
        public async Task LoginAsync_CenarioValido_RetornaTokenGeradoPeloJwtService()
        {
            var usuario = new Usuario("João", "joao@email.com", "hash-correto", RoleUsuario.User);

            _usuarioRepositoryMock
                .Setup(r => r.ObterPorEmailAsync(usuario.Email))
                .ReturnsAsync(usuario);

            _senhaHasherMock
                .Setup(h => h.Verify("senha123", usuario.SenhaHash))
                .Returns(true);

            _jwtServiceMock
                .Setup(j => j.GerarToken(usuario))
                .Returns("token-jwt-gerado");

            var resultado = await _service.LoginAsync(usuario.Email, "senha123");

            Assert.Equal("token-jwt-gerado", resultado.Token);
            Assert.Equal(usuario.Nome, resultado.Nome);
        }
    }
}
