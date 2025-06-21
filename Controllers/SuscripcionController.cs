using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using webapi.Services;
using System.Security.Claims;
using System.Threading.Tasks;

namespace webapi.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class SuscripcionController : ControllerBase
    {
        private readonly ISuscripcionService _suscripcionService;

        public SuscripcionController(ISuscripcionService suscripcionService)
        {
            _suscripcionService = suscripcionService;
        }

        [HttpGet("actual")]
        public async Task<IActionResult> GetSuscripcionActual()
        {
            var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (usuarioIdClaim == null || !int.TryParse(usuarioIdClaim, out int usuarioId))
            {
                return Unauthorized("Token inválido o usuario no autorizado.");
            }

            var suscripcion = await _suscripcionService.GetByUserId(usuarioId);
            if (suscripcion == null)
            {
                return NotFound("No se encontró una suscripción para este usuario.");
            }

            return Ok(suscripcion);
        }
    }
}