using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
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
                // var stripeEvent = EventUtility.ConstructEvent(
                //     json,
                //     Request.Headers["Stripe-Signature"],
                //     _configuration["Stripe:WebhookSecret"]
                // );
                var stripeEvent = JsonConvert.DeserializeObject<Event>(json);


                if (stripeEvent.Type == "checkout.session.completed")
                {
                    var session = stripeEvent.Data.Object as Stripe.Checkout.Session;

                    if (session == null)
                    {
                        return BadRequest("Session data is null.");
                    }

                    if (session.Metadata == null || !session.Metadata.ContainsKey("userId") || !session.Metadata.ContainsKey("subscriptionId"))
                    {
                        return BadRequest("Metadata is missing or invalid.");
                    }

                    var userId = session.Metadata["userId"];
                    var subscriptionId = session.Metadata["subscriptionId"];

                    if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(subscriptionId))
                    {
                        return BadRequest("UserId or SubscriptionId is null or empty.");
                    }
                    // Actualizar la suscripción en la base de datos
                    var subscription = await _suscripcionService.GetById(int.Parse(subscriptionId));
                    if (subscription == null)
                    {
                        return BadRequest($"No se encontró la suscripción con ID {subscriptionId}.");
                    }

                    subscription.EstadoSuscripcionId = 2; // Activa
                    subscription.FechaInicio = DateTime.Now;
                    subscription.FechaFin = DateTime.Now.AddMonths(1);
                    subscription.StripeSubscriptionId = session.Subscription?.Id;

                    _suscripcionService.Update(subscription);

                    return Ok("Subscription updated successfully.");

                }


                Console.WriteLine($"Evento no manejado: {stripeEvent.Type}");
                return Ok();
            }
            catch (StripeException e)
            {
                Console.WriteLine($"Stripe Webhook Error: {e.Message}");
                return BadRequest($"Stripe Webhook Error: {e.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected Error: {ex.Message}");
                return BadRequest($"Unexpected Error: {ex.Message}");
            }
        }




    }
}
