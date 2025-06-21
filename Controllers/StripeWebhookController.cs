using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Stripe;
using Stripe.Checkout;
using webapi.Services;
using Stripe.BillingPortal;
using webapi.Models;


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

            if (session?.Metadata == null || !session.Metadata.ContainsKey("userId"))
            {
                Console.WriteLine("Metadata is missing or invalid (userId is required).");
                return;
            }

            var userId = int.Parse(session.Metadata["userId"]);
            var planId = int.Parse(session.Metadata["planId"]); // Asumimos que siempre enviaremos el planId
            Suscripcion subscription;

            // Diferenciar entre un registro nuevo y una actualización
            if (session.Metadata.ContainsKey("subscriptionId"))
            {
                // Es un registro nuevo, la suscripción ya existe en estado "pendiente"
                var subscriptionId = int.Parse(session.Metadata["subscriptionId"]);
                subscription = await _suscripcionService.GetById(subscriptionId);
            }
            else
            {
                // Es una actualización de plan, buscamos la suscripción existente del usuario
                subscription = await _suscripcionService.GetByUserId(userId);
                if (subscription == null)
                {
                    // Si no existe, la creamos (caso de un usuario que nunca tuvo registro de suscripción)
                    subscription = new Suscripcion
                    {
                        UsuarioId = userId,
                    };
                    // No llamamos a Save aquí, lo hacemos en el bloque común de abajo
                }
            }

            if (subscription != null)
            {
                subscription.PlanId = planId; // Actualizar al nuevo plan
                subscription.EstadoSuscripcionId = 2; // Activa
                subscription.FechaInicio = DateTime.Now;
                subscription.FechaFin = planId == 4 ? DateTime.Now.AddYears(1) : DateTime.Now.AddMonths(1);
                subscription.StripeSubscriptionId = session.Subscription?.Id;

                // Log para depuración
                var logData = JsonConvert.SerializeObject(subscription, Formatting.Indented);
                Console.WriteLine("--- [WEBHOOK] Datos de suscripción a actualizar ---");
                Console.WriteLine(logData);
                Console.WriteLine("-------------------------------------------------");

                await _suscripcionService.Update(subscription);

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
                    await _suscripcionService.Update(subscription);

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
            if (invoice?.SubscriptionId == null)
            {
                Console.WriteLine("--- [WEBHOOK] Invoice o SubscriptionId es null en HandlePaymentSucceeded ---");
                return;
            }

            // Para obtener los metadatos, necesitamos la sesión de la suscripción, no la factura.
            var subscriptionService = new SubscriptionService();
            var subscriptionStripe = await subscriptionService.GetAsync(invoice.SubscriptionId);

            if (subscriptionStripe?.Metadata == null || !subscriptionStripe.Metadata.ContainsKey("userId"))
            {
                Console.WriteLine("--- [WEBHOOK] Metadata es null o no contiene userId en HandlePaymentSucceeded ---");
                return;
            }

            var userId = int.Parse(subscriptionStripe.Metadata["userId"]);
            var planId = int.Parse(subscriptionStripe.Metadata["planId"]);

            var subscriptionDb = await _suscripcionService.GetByUserId(userId);
            if (subscriptionDb == null)
            {
                subscriptionDb = new Suscripcion { UsuarioId = userId };
            }

            subscriptionDb.PlanId = planId;
            subscriptionDb.EstadoSuscripcionId = 2; // Activa
            subscriptionDb.FechaInicio = DateTime.Now;
            subscriptionDb.FechaFin = planId == 4 ? DateTime.Now.AddYears(1) : DateTime.Now.AddMonths(1);
            subscriptionDb.StripeSubscriptionId = invoice.SubscriptionId;

            // Log para depuración
            var logData = JsonConvert.SerializeObject(subscriptionDb, Formatting.Indented);
            Console.WriteLine("--- [WEBHOOK] Datos de suscripción a actualizar desde HandlePaymentSucceeded ---");
            Console.WriteLine(logData);
            Console.WriteLine("-------------------------------------------------------------------------");

            await _suscripcionService.Update(subscriptionDb);
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

                    await _suscripcionService.Update(suscripcion);

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
