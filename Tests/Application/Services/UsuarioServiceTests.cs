using Moq;
using SistemaGerenciamentoDeReserva.Application.DTOs.Usuario;
using SistemaGerenciamentoDeReserva.Application.Services;
using SistemaGerenciamentoDeReserva.Domain.Entity;
using SistemaGerenciamentoDeReserva.Domain.Enums;
using SistemaGerenciamentoDeReserva.Domain.Interfaces;

namespace SistemaGerenciamentoDeReserva.Tests.Application.Services
{
    public class UsuarioServiceTests
    {
        private readonly Mock<IUsuarioRepository> _usuarioRepositoryMock;
        private readonly Mock<ISenhaHasher> _senhaHasherMock;
        private readonly UsuarioService _service;

        public UsuarioServiceTests()
        {
            _usuarioRepositoryMock = new Mock<IUsuarioRepository>();
            _senhaHasherMock = new Mock<ISenhaHasher>();

            _service = new UsuarioService(
                _usuarioRepositoryMock.Object,
                _senhaHasherMock.Object);
        }

        [Fact]
        public async Task AdicionarUsuario_EmailJaExiste_LancaInvalidOperationException()
        {
            var dto = new CriarUsuarioDto("João", "joao@email.com", "senha123");

            _usuarioRepositoryMock
                .Setup(r => r.ObterPorEmailAsync(dto.Email))
                .ReturnsAsync(new Usuario("João", dto.Email, "hash-existente", RoleUsuario.User));

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.AdicionarUsuario(dto));

            _usuarioRepositoryMock.Verify(
                r => r.AdicionarAsync(It.IsAny<Usuario>()),
                Times.Never);
        }

        [Fact]
        public async Task AdicionarUsuario_CenarioValido_HasheiaSenhaERetornaDto()
        {
            var dto = new CriarUsuarioDto("João", "joao@email.com", "senha123");

            _usuarioRepositoryMock
                .Setup(r => r.ObterPorEmailAsync(dto.Email))
                .ReturnsAsync((Usuario?)null);

            _senhaHasherMock
                .Setup(h => h.Hash(dto.Senha))
                .Returns("senha-hasheada");

            _usuarioRepositoryMock
                .Setup(r => r.AdicionarAsync(It.IsAny<Usuario>()))
                .ReturnsAsync(1L);

            var resultado = await _service.AdicionarUsuario(dto);

            Assert.Equal(1L, resultado.Id);
            Assert.Equal(dto.Nome, resultado.Nome);
            Assert.Equal(dto.Email, resultado.Email);

            _usuarioRepositoryMock.Verify(
                r => r.AdicionarAsync(It.Is<Usuario>(
                    u => u.SenhaHash == "senha-hasheada")),
                Times.Once);
        }

        [Fact]
        public async Task BuscarPorId_UsuarioInexistente_RetornaNull()
        {
            _usuarioRepositoryMock
                .Setup(r => r.ObterPorIdAsync(1))
                .ReturnsAsync((Usuario?)null);

            var resultado = await _service.BuscarPorId(1);

            Assert.Null(resultado);
        }

        [Fact]
        public async Task AtualizarUsuario_UsuarioInexistente_LancaKeyNotFoundException()
        {
            var dto = new AtualizarUsuarioDto("João", "joao@email.com");

            _usuarioRepositoryMock
                .Setup(r => r.ObterPorIdAsync(1))
                .ReturnsAsync((Usuario?)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _service.AtualizarUsuario(1, dto));
        }

        [Fact]
        public async Task AtualizarUsuario_EmailJaUsadoPorOutroUsuario_LancaInvalidOperationException()
        {
            var dto = new AtualizarUsuarioDto("João", "outro@email.com");

            var usuario = new Usuario("João", "joao@email.com", "hash", RoleUsuario.User) { Id = 1 };
            var outroUsuario = new Usuario("Maria", dto.Email, "hash", RoleUsuario.User) { Id = 2 };

            _usuarioRepositoryMock
                .Setup(r => r.ObterPorIdAsync(1))
                .ReturnsAsync(usuario);

            _usuarioRepositoryMock
                .Setup(r => r.ObterPorEmailAsync(dto.Email))
                .ReturnsAsync(outroUsuario);

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.AtualizarUsuario(1, dto));
        }

        [Fact]
        public async Task AtualizarUsuario_CenarioValido_AtualizaNomeEEmail()
        {
            var dto = new AtualizarUsuarioDto("João Atualizado", "joao.novo@email.com");

            var usuario = new Usuario("João", "joao@email.com", "hash", RoleUsuario.User) { Id = 1 };

            _usuarioRepositoryMock
                .Setup(r => r.ObterPorIdAsync(1))
                .ReturnsAsync(usuario);

            _usuarioRepositoryMock
                .Setup(r => r.ObterPorEmailAsync(dto.Email))
                .ReturnsAsync((Usuario?)null);

            await _service.AtualizarUsuario(1, dto);

            _usuarioRepositoryMock.Verify(
                r => r.AtualizarAsync(It.Is<Usuario>(
                    u => u.Nome == dto.Nome && u.Email == dto.Email)),
                Times.Once);
        }

        [Fact]
        public async Task DeletarUsuario_UsuarioInexistente_LancaKeyNotFoundException()
        {
            _usuarioRepositoryMock
                .Setup(r => r.ObterPorIdAsync(1))
                .ReturnsAsync((Usuario?)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _service.DeletarUsuario(1));
        }

        [Fact]
        public async Task DeletarUsuario_CenarioValido_ChamaRepositorio()
        {
            var usuario = new Usuario("João", "joao@email.com", "hash", RoleUsuario.User) { Id = 1 };

            _usuarioRepositoryMock
                .Setup(r => r.ObterPorIdAsync(1))
                .ReturnsAsync(usuario);

            await _service.DeletarUsuario(1);

            _usuarioRepositoryMock.Verify(r => r.DeletarAsync(1), Times.Once);
        }
    }
}
