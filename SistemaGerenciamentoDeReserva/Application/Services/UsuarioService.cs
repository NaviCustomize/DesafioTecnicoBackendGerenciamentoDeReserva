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

            var usuario = new Usuario
            {
                Nome = dto.Nome,
                Email = dto.Email,
                SenhaHash = _senhaHasher.Hash(dto.Senha)
            };

            var id = await _usuarioRepository.AdicionarAsync(usuario);

            return new UsuarioResponseDto(
                id,
                usuario.Nome,
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

            usuario.Nome = dto.Nome;
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
    }
}
