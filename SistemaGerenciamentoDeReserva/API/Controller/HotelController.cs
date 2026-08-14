using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaGerenciamentoDeReserva.Application.DTOs.Hotel;
using SistemaGerenciamentoDeReserva.Application.Interface;

namespace SistemaGerenciamentoDeReserva.API.Controller
{
    [ApiController]
    [Route("hoteis")]
    public class HotelController : ControllerBase
    {
        private readonly IHotelService _hotelService;

        public HotelController(IHotelService hotelService)
        {
            _hotelService = hotelService;
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Criar(
            [FromBody] CriarHotelDto dto)
        {
            var hotel = await _hotelService.AdicionarHotel(dto);

            return CreatedAtAction(nameof(ObterPorId),new { id = hotel.Id },hotel);
        }

        [HttpGet]
        public async Task<IActionResult> ObterTodos()
        {
            var hoteis = await _hotelService.ListarHotel();

            return Ok(hoteis);
        }

        [HttpGet("{id:long}")]
        public async Task<IActionResult> ObterPorId(long id)
        {
            var hotel = await _hotelService.BuscarPorId(id);

            if (hotel is null)
                return NotFound("Hotel não encontrado.");

            return Ok(hotel);
        }

        [HttpPut("{id:long}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Atualizar(
            long id,
            [FromBody] AtualizarHotelDto dto)
        {
            try
            {
                await _hotelService.AtualizarHotel(id, dto);

                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpDelete("{id:long}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Deletar(long id)
        {
            try
            {
                await _hotelService.DeletarHotel(id);

                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}
