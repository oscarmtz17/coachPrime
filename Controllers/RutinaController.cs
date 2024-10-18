using Microsoft.AspNetCore.Mvc;
using webapi.Models;  // Asegúrate de que los DTOs están en este namespace
using webapi.Services;
using System.Threading.Tasks;

namespace webapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RutinaController : ControllerBase
    {
        private readonly IRutinaService _rutinaService;

        public RutinaController(IRutinaService rutinaService)
        {
            _rutinaService = rutinaService;
        }

        // POST: api/rutina
        [HttpPost]
        public async Task<IActionResult> CreateRutina([FromBody] CreateRutinaRequest request)
        {
            // Validamos si el request es válido
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Llamamos al servicio para crear la rutina
            var result = await _rutinaService.CreateRutinaAsync(request);
            
            if (result)
            {
                return Ok("Rutina creada exitosamente.");
            }
            else
            {
                return BadRequest("Hubo un error al crear la rutina.");
            }
        }

        // GET: api/rutina/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetRutinaById(int id)
        {
            var rutina = await _rutinaService.GetRutinaByIdAsync(id);
            
            if (rutina != null)
            {
                return Ok(rutina);
            }

            return NotFound("Rutina no encontrada.");
        }

        // PUT: api/rutina/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRutina(int id, [FromBody] CreateRutinaRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _rutinaService.UpdateRutinaAsync(id, request);
            
            if (result)
            {
                return Ok("Rutina actualizada exitosamente.");
            }
            else
            {
                return NotFound("Rutina no encontrada.");
            }
        }

        // DELETE: api/rutina/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRutina(int id)
        {
            var result = await _rutinaService.DeleteRutinaAsync(id);
            
            if (result)
            {
                return Ok("Rutina eliminada exitosamente.");
            }

            return NotFound("Rutina no encontrada.");
        }
    }
}
