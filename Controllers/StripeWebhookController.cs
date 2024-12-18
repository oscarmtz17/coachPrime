using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Stripe;
using Stripe.BillingPortal;
using webapi.Services;
using Stripe.Checkout;


namespace webapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StripeWebhookController : ControllerBase
    {
        private readonly ISuscripcionService _suscripcionService;
        private readonly IConfiguration _configuration;
        private readonly EmailService _emailService;

        private readonly CoachPrimeContext _context;

        public StripeWebhookController(ISuscripcionService suscripcionService, IConfiguration configuration, EmailService emailService, CoachPrimeContext context)
        {
            _suscripcionService = suscripcionService;
            _configuration = configuration;
            _emailService = emailService;
            _context = context;
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
                    var sessionService = new Stripe.Checkout.SessionService();

                    // Convertir el objeto stripeEvent.Data.Object a Stripe.Checkout.Session
                    var sessionObject = stripeEvent.Data.Object as Stripe.Checkout.Session;

                    if (sessionObject == null || string.IsNullOrEmpty(sessionObject.Id))
                    {
                        return BadRequest("Session object is null or does not contain a valid ID.");
                    }

                    var session = sessionService.Get(
                        sessionObject.Id,
                        new SessionGetOptions
                        {
                            Expand = new List<string> { "subscription" }
                        });

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
                    // Enviar correo de confirmación de pago
                    var user = await _suscripcionService.GetUsuarioById(subscription.UsuarioId);
                    if (user == null)
                    {
                        return BadRequest($"No se encontró un usuario con el ID {subscription.UsuarioId}.");
                    }
                    var userEmail = user.Email;
                    var userName = $"{user.Nombre} {user.Apellido}";

                    var plan = await _suscripcionService.GetPlanById(subscription.PlanId);
                    if (plan == null)
                    {
                        return BadRequest($"No se encontró un plan con el ID {subscription.PlanId}.");
                    }
                    var planName = plan.Nombre;


                    // Enviar correo de confirmación de pago
                    if (subscription.FechaInicio != null && subscription.FechaFin.HasValue)
                    {
                        await _emailService.SendPaymentConfirmationEmail(
                            userEmail,
                            userName,
                            planName,
                            subscription.FechaInicio,
                            subscription.FechaFin.Value
                        );
                    }
                    else
                    {
                        // Manejar el caso en que las fechas sean null (opcional)
                        Console.WriteLine("Las fechas de inicio o fin no están definidas.");
                    }



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
