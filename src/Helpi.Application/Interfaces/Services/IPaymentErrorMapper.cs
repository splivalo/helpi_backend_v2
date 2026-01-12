using Stripe;

namespace Helpi.Application.Interfaces.Services;

public interface IPaymentErrorMapper
{
    string GetLocalizedErrorMessage(StripeException ex, string language = "hr");
}
