using Microsoft.AspNetCore.Identity;
using SistemaGerenciamentoDeReserva.Application.DTOs.Usuario;
using SistemaGerenciamentoDeReserva.Application.Interface;
using SistemaGerenciamentoDeReserva.Domain.Entity;
using SistemaGerenciamentoDeReserva.Domain.Interfaces;

namespace SistemaGerenciamentoDeReserva.Application.Services
{
    public class UsuarioService : IUsuarioService
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly ISenhaHasher _senhaHasher;

        public UsuarioService(IUsuarioRepository usuarioRepository,ISenhaHasher senhaHasher)
        {
            _usuarioRepository = usuarioRepository;
            _senhaHasher = senhaHasher;
        }

        public async Task<UsuarioResponseDto> AdicionarUsuario(
            CriarUsuarioDto dto)
        {
            var usuarioExistente =
            await _usuarioRepository.ObterPorEmailAsync(dto.Email);

            if (usuarioExistente is not null)
            {
                throw new InvalidOperationException(
                    "Já existe um usuário com esse email.");
            }

            if (string.IsNullOrWhiteSpace(dto.Sobrenome))
            {
                throw new ArgumentException("O sobrenome é obrigatório.");
            }

            var usuario = new Usuario
            {
                Nome = dto.Nome.Trim(),
                Sobrenome = dto.Sobrenome.Trim(),
                Email = dto.Email,
                SenhaHash = _senhaHasher.Hash(dto.Senha)
            };

            var id = await _usuarioRepository.AdicionarAsync(usuario);

            return new UsuarioResponseDto(
                id,
                usuario.Nome,
                usuario.Sobrenome,
                usuario.Email,
                usuario.Role
            );
        }

        public async Task<UsuarioResponseDto?> BuscarPorId(long id)
        {
            var usuario = await _usuarioRepository.ObterPorIdAsync(id);

            if (usuario is null)
                return null;

            return new UsuarioResponseDto(
                usuario.Id,
                usuario.Nome,
                usuario.Sobrenome,
                usuario.Email,
                usuario.Role
            );
        }

        public async Task<IEnumerable<UsuarioResponseDto>> ListarUsuarios()
        {
            var usuarios = await _usuarioRepository.ObterTodosAsync();

            return usuarios.Select(usuario =>
                new UsuarioResponseDto(
                    usuario.Id,
                    usuario.Nome,
                    usuario.Sobrenome,
                    usuario.Email,
                    usuario.Role
                ));
        }

        public async Task AtualizarUsuario(
            long id,
            AtualizarUsuarioDto dto)
        {
            var usuario = await _usuarioRepository.ObterPorIdAsync(id);

            if (usuario is null)
            {
                throw new KeyNotFoundException(
                    "Usuário não encontrado.");
            }

            var usuarioComEmail =
                await _usuarioRepository.ObterPorEmailAsync(dto.Email);

            if (usuarioComEmail is not null &&
                usuarioComEmail.Id != id)
            {
                throw new InvalidOperationException(
                    "Já existe um usuário com esse email.");
            }

            if (string.IsNullOrWhiteSpace(dto.Sobrenome))
            {
                throw new ArgumentException("O sobrenome é obrigatório.");
            }

            usuario.Nome = dto.Nome.Trim();
            usuario.Sobrenome = dto.Sobrenome.Trim();
            usuario.Email = dto.Email;

            await _usuarioRepository.AtualizarAsync(usuario);
        }

        public async Task DeletarUsuario(long id)
        {
            var usuario = await _usuarioRepository.ObterPorIdAsync(id);

            if (usuario is null)
            {
                throw new KeyNotFoundException(
                    "Usuário não encontrado.");
            }

            await _usuarioRepository.DeletarAsync(id);
        }

        public async Task<IEnumerable<UsuarioAdminResponseDto>> ListarUsuariosParaAdmin()
        {
            var usuarios = await _usuarioRepository.ObterTodosIncluindoInativosAsync();

            return usuarios.Select(u => new UsuarioAdminResponseDto(
                u.Id,
                u.Nome,
                u.Sobrenome,
                u.Email,
                u.Role,
                u.Ativo,
                u.ExcluidoEm));
        }

        public async Task ReativarUsuario(long id)
        {
            var usuario = await _usuarioRepository.ObterPorIdIncluindoInativoAsync(id);

            if (usuario is null)
            {
                throw new KeyNotFoundException("Usuário não encontrado.");
            }

            if (usuario.Ativo)
            {
                throw new InvalidOperationException("Esta conta já está ativa.");
            }


            var comMesmoEmail = await _usuarioRepository.ObterPorEmailAsync(usuario.Email);

            if (comMesmoEmail is not null && comMesmoEmail.Id != id)
            {
                throw new InvalidOperationException(
                    "Já existe uma conta ativa com esse e-mail.");
            }

            await _usuarioRepository.ReativarAsync(id);
        }

        public async Task AlterarSenha(long id, AlterarSenhaDto dto)
        {
            var usuario = await _usuarioRepository.ObterPorIdAsync(id);

            if (usuario is null)
            {
                throw new KeyNotFoundException("Usuário não encontrado.");
            }

            if (!_senhaHasher.Verify(dto.SenhaAtual, usuario.SenhaHash))
            {
                throw new UnauthorizedAccessException("A senha atual está incorreta.");
            }

            if (string.IsNullOrWhiteSpace(dto.NovaSenha) || dto.NovaSenha.Length < 6)
            {
                throw new ArgumentException(
                    "A nova senha precisa ter no mínimo 6 caracteres.");
            }

            if (_senhaHasher.Verify(dto.NovaSenha, usuario.SenhaHash))
            {
                throw new ArgumentException(
                    "A nova senha precisa ser diferente da atual.");
            }

            usuario.SenhaHash = _senhaHasher.Hash(dto.NovaSenha);

            await _usuarioRepository.AtualizarAsync(usuario);
        }

        public async Task EncerrarPropriaConta(long id, ConfirmarSenhaDto dto)
        {
            var usuario = await _usuarioRepository.ObterPorIdAsync(id);

            if (usuario is null)
            {
                throw new KeyNotFoundException("Usuário não encontrado.");
            }


            if (!_senhaHasher.Verify(dto.Senha, usuario.SenhaHash))
            {
                throw new UnauthorizedAccessException("A senha informada está incorreta.");
            }

            await _usuarioRepository.DeletarAsync(id);
        }
    }
}
