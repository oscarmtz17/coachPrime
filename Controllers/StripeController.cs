using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Stripe;
using Stripe.Checkout;

[Route("api/[controller]")]
[ApiController]
public class StripeController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public StripeController(IConfiguration configuration)
    {
        _configuration = configuration;
        StripeConfiguration.ApiKey = _configuration["Stripe:SecretKey"];
    }

    [HttpPost("create-checkout-session")]
    public IActionResult CreateCheckoutSession()
    {
        var domain = "http://localhost:3000"; // Cambia al dominio de tu frontend

        var options = new SessionCreateOptions
        {
            PaymentMethodTypes = new List<string> { "card" },
            LineItems = new List<SessionLineItemOptions>
        {
            new SessionLineItemOptions
            {
                Price = "price_1QQKUQBZAdKqouiVDK0jLr25", // Reemplaza con el Price ID del producto recurrente
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

}
