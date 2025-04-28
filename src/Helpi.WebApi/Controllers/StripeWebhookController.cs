using Helpi.Application.Interfaces;
using Helpi.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;

namespace Helpi.WebApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class StripeWebhookController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IOrderRepository _orderRepository;
        private readonly ILogger<StripeWebhookController> _logger;

        public StripeWebhookController(
            IConfiguration configuration,
            IOrderRepository orderRepository,
            ILogger<StripeWebhookController> logger)
        {
            _configuration = configuration;
            _orderRepository = orderRepository;
            _logger = logger;
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
                    _configuration["Stripe:WebhookSecret"]
                );

                // Handle specific events
                switch (stripeEvent.Type)
                {
                    case "payment_intent.succeeded":
                        var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
                        await HandlePaymentIntentSucceeded(paymentIntent);
                        break;
                    case "payment_intent.payment_failed":
                        var failedPaymentIntent = stripeEvent.Data.Object as PaymentIntent;
                        await HandlePaymentIntentFailed(failedPaymentIntent);
                        break;
                    case "payment_method.updated":
                    case "payment_method.attached":
                    case "payment_method.detached":
                        var paymentMethod = (PaymentMethod)stripeEvent.Data.Object;
                        // await _paymentService.HandlePaymentMethodWebhookAsync(paymentMethod.Id);
                        break;
                }

                return Ok();
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Error processing Stripe webhook");
                return BadRequest();
            }
        }

        private async Task HandlePaymentIntentSucceeded(PaymentIntent paymentIntent)
        {
            // if (paymentIntent.Metadata.TryGetValue("OrderId", out string orderIdStr) &&
            //     Guid.TryParse(orderIdStr, out Guid orderId))
            // {
            //     var order = await _orderRepository.GetByIdAsync(orderId);
            //     if (order != null)
            //     {
            //         order.PaymentStatus = PaymentStatus.Paid;
            //         order.PaymentDate = DateTime.UtcNow;
            //         order.PaymentReference = paymentIntent.Id;

            //         await _orderRepository.UpdateAsync(order);
            //         _logger.LogInformation("Payment succeeded for order {OrderId}", orderId);
            //     }
            // }
        }

        private async Task HandlePaymentIntentFailed(PaymentIntent paymentIntent)
        {
            // if (paymentIntent.Metadata.TryGetValue("OrderId", out string orderIdStr) &&
            //     Guid.TryParse(orderIdStr, out Guid orderId))
            // {
            //     var order = await _orderRepository.GetByIdAsync(orderId);
            //     if (order != null)
            //     {
            //         order.PaymentStatus = PaymentStatus.Failed;
            //         order.PaymentFailureReason = paymentIntent.LastPaymentError?.Message;

            //         await _orderRepository.UpdateAsync(order);
            //         _logger.LogWarning("Payment failed for order {OrderId}: {Reason}",
            //             orderId, paymentIntent.LastPaymentError?.Message);
            //     }
            // }
        }
    }
}