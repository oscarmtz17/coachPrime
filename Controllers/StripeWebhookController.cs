using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Stripe;
using Stripe.Checkout;
using webapi.Services;
using Stripe.BillingPortal;


namespace webapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StripeWebhookController : ControllerBase
    {
        private readonly ISuscripcionService _suscripcionService;
        private readonly IConfiguration _configuration;
        private readonly EmailService _emailService;


        public StripeWebhookController(ISuscripcionService suscripcionService, IConfiguration configuration, EmailService emailService)
        {
            _suscripcionService = suscripcionService;
            _configuration = configuration;
            _emailService = emailService;
        }

        [HttpPost]
        public async Task<IActionResult> HandleStripeWebhook()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

            try
            {
                var stripeEvent = JsonConvert.DeserializeObject<Event>(json);

                // Manejar los diferentes tipos de eventos de Stripe
                switch (stripeEvent.Type)
                {
                    case "checkout.session.completed":
                        await HandleCheckoutSessionCompleted(stripeEvent);
                        break;

                    case "invoice.payment_failed":
                        await HandlePaymentFailed(stripeEvent);
                        break;

                    case "invoice.payment_succeeded":
                        await HandlePaymentSucceeded(stripeEvent);
                        break;

                    case "customer.subscription.deleted":
                        await HandleSubscriptionDeleted(stripeEvent);
                        break;

                    default:
                        Console.WriteLine($"Evento no manejado: {stripeEvent.Type}");
                        break;
                }

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

        private async Task HandleCheckoutSessionCompleted(Event stripeEvent)
        {
            var sessionService = new Stripe.Checkout.SessionService();
            var sessionObject = stripeEvent.Data.Object as Stripe.Checkout.Session;

            if (sessionObject == null || string.IsNullOrEmpty(sessionObject.Id))
            {
                Console.WriteLine("Session object is null or invalid.");
                return;
            }

            var session = sessionService.Get(
                sessionObject.Id,
                new SessionGetOptions { Expand = new List<string> { "subscription" } }
            );

            if (session?.Metadata == null || !session.Metadata.ContainsKey("userId") || !session.Metadata.ContainsKey("subscriptionId"))
            {
                Console.WriteLine("Metadata is missing or invalid.");
                return;
            }

            var userId = session.Metadata["userId"];
            var subscriptionId = session.Metadata["subscriptionId"];

            var subscription = await _suscripcionService.GetById(int.Parse(subscriptionId));
            if (subscription != null)
            {
                subscription.EstadoSuscripcionId = 2; // Activa
                subscription.FechaInicio = DateTime.Now;
                subscription.FechaFin = DateTime.Now.AddMonths(1);
                subscription.StripeSubscriptionId = session.Subscription?.Id;

                _suscripcionService.Update(subscription);

                // Enviar correo de confirmación
                var user = await _suscripcionService.GetUsuarioById(subscription.UsuarioId);
                var plan = await _suscripcionService.GetPlanById(subscription.PlanId);

                if (user != null && plan != null)
                {
                    await _emailService.SendPaymentConfirmationEmail(
                        user.Email, $"{user.Nombre} {user.Apellido}",
                        plan.Nombre, subscription.FechaInicio, subscription.FechaFin.Value
                    );
                }
            }
        }

        private async Task HandlePaymentFailed(Event stripeEvent)
        {
            var invoice = stripeEvent.Data.Object as Invoice;
            var stripeSubscriptionId = invoice?.SubscriptionId;

            if (!string.IsNullOrEmpty(stripeSubscriptionId))
            {
                var subscription = await _suscripcionService.GetByStripeId(stripeSubscriptionId);

                if (subscription != null)
                {
                    subscription.EstadoSuscripcionId = 5; // Suspendida
                    _suscripcionService.Update(subscription);

                    var user = await _suscripcionService.GetUsuarioById(subscription.UsuarioId);
                    /*                     if (user != null)
                                        {
                                            await _emailService.SendPaymentFailedEmail(user.Email, user.Nombre);
                                        } */
                }
            }
        }

        private async Task HandlePaymentSucceeded(Event stripeEvent)
        {
            var invoice = stripeEvent.Data.Object as Invoice;
            var stripeSubscriptionId = invoice?.SubscriptionId;

            if (!string.IsNullOrEmpty(stripeSubscriptionId))
            {
                var subscription = await _suscripcionService.GetByStripeId(stripeSubscriptionId);

                if (subscription != null)
                {
                    subscription.EstadoSuscripcionId = 6; // Reactivada
                    subscription.FechaInicio = DateTime.Now;
                    subscription.FechaFin = DateTime.Now.AddMonths(1);

                    _suscripcionService.Update(subscription);

                    var user = await _suscripcionService.GetUsuarioById(subscription.UsuarioId);
                    if (user != null)
                    {
                        await _emailService.SendPaymentConfirmationEmail(
                            user.Email, user.Nombre, "Reactivación", subscription.FechaInicio, subscription.FechaFin.Value
                        );
                    }
                }
            }
        }

        private async Task HandleSubscriptionDeleted(Event stripeEvent)
        {
            var subscription = stripeEvent.Data.Object as Subscription;
            var stripeSubscriptionId = subscription?.Id;

            if (!string.IsNullOrEmpty(stripeSubscriptionId))
            {
                var suscripcion = await _suscripcionService.GetByStripeId(stripeSubscriptionId);

                if (suscripcion != null)
                {
                    suscripcion.EstadoSuscripcionId = subscription.CanceledAt.HasValue ? 4 : 3; // Cancelada o Expirada
                    suscripcion.FechaCancelacion = DateTime.Now;

                    _suscripcionService.Update(suscripcion);

                    var user = await _suscripcionService.GetUsuarioById(suscripcion.UsuarioId);
                    /*                     if (user != null)
                                        {
                                            await _emailService.SendSubscriptionCancelledEmail(user.Email, user.Nombre);
                                        } */
                }
            }
        }
    }
}
