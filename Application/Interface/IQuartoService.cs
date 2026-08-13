using SistemaGerenciamentoDeReserva.Application.DTOs.Hotel;
using SistemaGerenciamentoDeReserva.Application.DTOs.Quarto;

namespace SistemaGerenciamentoDeReserva.Application.Interface
{
    public interface IQuartoService
    {
        Task<QuartoResponseDto> AdicionarQuarto(CriarQuartoDto dto);

        Task<QuartoResponseDto?> BuscarPorId(long id);

        Task<IEnumerable<QuartoResponseDto>> ListarQuarto();

        Task AtualizarQuarto(long id, AtualizarQuartoDto dto);

        Task DeletarQuarto(long id);
    }
}
