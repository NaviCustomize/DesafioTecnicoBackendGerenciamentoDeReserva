using SistemaGerenciamentoDeReserva.Domain.Enums;

namespace SistemaGerenciamentoDeReserva.Application.DTOs.Quarto
{
    public record QuartoResponseDto(long Id, long HotelId, int Numero, TipoQuarto Tipo, decimal PrecoPorNoite, StatusQuarto Status);
}
