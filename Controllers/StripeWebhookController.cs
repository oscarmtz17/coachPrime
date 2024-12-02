using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.BillingPortal;
using webapi.Services;

namespace webapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StripeWebhookController : ControllerBase
    {
        private readonly ISuscripcionService _suscripcionService;
        private readonly IConfiguration _configuration;

        public StripeWebhookController(ISuscripcionService suscripcionService, IConfiguration configuration)
        {
            _suscripcionService = suscripcionService;
            _configuration = configuration;
        }

        [HttpPost]
        public async Task<IActionResult> HandleStripeWebhook()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

            try
            {
                var stripeEvent = EventUtility.ConstructEvent(
                    json,
                    Request.Headers["Stripe-Signature"],
                    _configuration["Stripe:WebhookSecret"] // Configura este valor en appsettings.json
                );

                // Manejar diferentes tipos de eventos
                if (stripeEvent.Type == "checkout.session.completed")
                {
                    var session = stripeEvent.Data.Object as Stripe.Checkout.Session;

                    if (session != null && session.Metadata != null)
                    {
                        var userId = session.Metadata["userId"];
                        var subscriptionId = session.Metadata["subscriptionId"];

                        // Actualizar la suscripción
                        var subscription = await _suscripcionService.GetById(int.Parse(subscriptionId));
                        if (subscription != null)
                        {
                            subscription.EstadoSuscripcionId = 2; // Activa
                            subscription.FechaInicio = DateTime.Now;
                            subscription.FechaFin = DateTime.Now.AddMonths(1); // Configura la duración real aquí
                            await _suscripcionService.Save(subscription);
                        }
                    }

                    return Ok();
                }

                // Si el tipo de evento no es manejado
                return BadRequest("Evento no manejado.");
            }
            catch (StripeException e)
            {
                return BadRequest($"Webhook Error: {e.Message}");
            }
            catch (Exception ex)
            {
                // Captura de cualquier otro error no relacionado con Stripe
                return BadRequest($"Error inesperado: {ex.Message}");
            }
        }

    }
}
