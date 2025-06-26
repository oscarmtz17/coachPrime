using System;
using System.Threading.Tasks;
using Stripe;

namespace webapi.Services
{
    public interface IStripeService
    {
        string GetStripePriceId(int planId);
        Task CancelStripeSubscriptionAsync(string stripeSubscriptionId);
    }

    public class StripeService : IStripeService
    {
        public string GetStripePriceId(int planId)
        {
            return planId switch
            {
                3 => "price_1QQKUQBZAdKqouiVDK0jLr25", // Premium Mensual
                4 => "price_1QR97hBZAdKqouiVKf5WRxMl", // Premium Anual
                _ => throw new ArgumentException("Plan no válido o no requiere pago.", nameof(planId)),
            };
        }

        public async Task CancelStripeSubscriptionAsync(string stripeSubscriptionId)
        {
            var service = new SubscriptionService();
            await service.CancelAsync(stripeSubscriptionId, null);
        }
    }
}