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
        private readonly IStripeService _stripeService;

        public SuscripcionController(ISuscripcionService suscripcionService, IStripeService stripeService)
        {
            _suscripcionService = suscripcionService;
            _stripeService = stripeService;
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

        [HttpPost("cancelar")]
        public async Task<IActionResult> CancelarSuscripcion([FromBody] CancelSuscripcionRequest request)
        {
            var usuarioIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (usuarioIdClaim == null || !int.TryParse(usuarioIdClaim, out int usuarioId))
                return Unauthorized("Token inválido o usuario no autorizado.");

            var suscripcion = await _suscripcionService.GetById(request.SuscripcionId);
            if (suscripcion == null || suscripcion.UsuarioId != usuarioId)
                return NotFound("No se encontró la suscripción para este usuario.");

            // Cancela en Stripe si corresponde
            if (!string.IsNullOrEmpty(suscripcion.StripeSubscriptionId))
                await _stripeService.CancelStripeSubscriptionAsync(suscripcion.StripeSubscriptionId);

            // Cancela en la base de datos
            await _suscripcionService.Cancel(suscripcion.SuscripcionId);

            return Ok(new { success = true });
        }

        public class CancelSuscripcionRequest
        {
            public int SuscripcionId { get; set; }
        }
    }
}