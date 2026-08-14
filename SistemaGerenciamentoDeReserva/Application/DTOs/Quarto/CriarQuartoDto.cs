using SistemaGerenciamentoDeReserva.Domain.Enums;

namespace SistemaGerenciamentoDeReserva.Application.DTOs.Quarto
{
    public record CriarQuartoDto(long HotelId, int Numero, TipoQuarto Tipo, decimal PrecoPorNoite);
}
