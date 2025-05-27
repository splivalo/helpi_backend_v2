
namespace Helpi.Domain.ValueObjects
{
    public class PaymentResult
    {
        public bool Success { get; private set; }
        public string? PaymentIntentId { get; private set; }
        public string? Status { get; private set; }
        public string? ErrorMessage { get; private set; }
        public string? StripeErrorCode { get; private set; }
        public bool RequiresAction { get; private set; }
        public bool IsProcessing { get; private set; }
        public string? ClientSecret { get; private set; }


        public static PaymentResult SuccessResult(string paymentIntentId, string status) => new()
        {
            Success = true,
            PaymentIntentId = paymentIntentId,
            Status = status
        };

        public static PaymentResult RequiresActionResult(string paymentIntentId, string clientSecret, string status) => new()
        {
            RequiresAction = true,
            PaymentIntentId = paymentIntentId,
            ClientSecret = clientSecret,
            Status = status
        };


        public static PaymentResult Failed(string errorMessage, string? stripeErrorCode = null) => new()
        {
            Success = false,
            ErrorMessage = errorMessage,
            StripeErrorCode = stripeErrorCode
        };


        public static PaymentResult ProcessingResult(string paymentIntentId, string status) => new()
        {
            Success = false,
            IsProcessing = true,
            PaymentIntentId = paymentIntentId,
            Status = status
        };
    }

}