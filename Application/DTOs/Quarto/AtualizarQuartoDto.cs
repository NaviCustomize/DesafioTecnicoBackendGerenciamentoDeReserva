using SistemaGerenciamentoDeReserva.Domain.Enums;

namespace SistemaGerenciamentoDeReserva.Application.DTOs.Quarto
{
    public record AtualizarQuartoDto(int Numero, TipoQuarto Tipo, decimal PrecoPorNoite);
}
