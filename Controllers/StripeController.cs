using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Stripe;
using Stripe.Checkout;
using System.Security.Claims;
using webapi.Services;

[Route("api/[controller]")]
[ApiController]
public class StripeController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly IUsuarioService _usuarioService; // Asumimos que existe un servicio de usuario
    private readonly IStripeService _stripeService;

    public StripeController(IConfiguration configuration, IUsuarioService usuarioService, IStripeService stripeService)
    {
        _configuration = configuration;
        _usuarioService = usuarioService;
        _stripeService = stripeService;
        StripeConfiguration.ApiKey = _configuration["Stripe:SecretKey"];
    }

    [HttpPost("create-checkout-session")]
    public IActionResult CreateCheckoutSession([FromBody] CheckoutRequest request)
    {
        try
        {
            var priceId = _stripeService.GetStripePriceId(request.PlanId);
            var domain = "http://localhost:3000"; // Cambia al dominio de tu frontend

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        Price = priceId,
                        Quantity = 1,
                    },
                },
                Mode = "subscription", // Usa modo suscripción
                SuccessUrl = $"{domain}/success",
                CancelUrl = $"{domain}/cancel",
            };

            var service = new SessionService();
            var session = service.Create(options);

            return Ok(new { url = session.Url });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // Endpoint para que un usuario existente actualice su plan
    [HttpPost("create-checkout-session-for-upgrade")]
    public async Task<IActionResult> CreateCheckoutSessionForUpgrade([FromBody] UpgradeRequest request)
    {
        var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (usuarioIdClaim == null || !int.TryParse(usuarioIdClaim, out int usuarioId))
        {
            return Unauthorized("Token inválido o usuario no autorizado.");
        }

        var user = await _usuarioService.GetByIdAsync(usuarioId);
        if (user == null)
        {
            return NotFound("Usuario no encontrado.");
        }

        try
        {
            var priceId = _stripeService.GetStripePriceId(request.PlanId);

            var domain = "http://localhost:3000";

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions { Price = priceId, Quantity = 1 }
                },
                Mode = "subscription",
                SuccessUrl = $"{domain}/success?session_id={{CHECKOUT_SESSION_ID}}",
                CancelUrl = $"{domain}/cancel",
                CustomerEmail = user.Email,
                Metadata = new Dictionary<string, string>
                {
                    { "userId", usuarioId.ToString() },
                    { "planId", request.PlanId.ToString() }
                }
            };

            var service = new SessionService();
            var session = await service.CreateAsync(options);

            return Ok(new { url = session.Url });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}

public class UpgradeRequest
{
    public int PlanId { get; set; }
}

public class CheckoutRequest
{
    public int PlanId { get; set; }
}
