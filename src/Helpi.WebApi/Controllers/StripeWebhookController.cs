using Helpi.Application.Interfaces;
using Helpi.Application.Interfaces.Services;
using Helpi.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;

namespace Helpi.WebApi.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class StripeWebhookController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IPaymentService _paymentService;
        private readonly ILogger<StripeWebhookController> _logger;

        public StripeWebhookController(
            IConfiguration configuration,
            IPaymentService paymentService,
            ILogger<StripeWebhookController> logger)
        {
            _configuration = configuration;
            _paymentService = paymentService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> HandleStripeWebhook()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

            try
            {

                var secret = Environment.GetEnvironmentVariable("Stripe:WebhookSecret")
                                ?? _configuration["Stripe:WebhookSecret"]
                                ?? throw new ArgumentNullException("Stripe:WebhookSecret");

                var stripeEvent = EventUtility.ConstructEvent(
                    json,
                    Request.Headers["Stripe-Signature"],
                    secret
                );

                _logger.LogInformation(stripeEvent.ToJson());



                // Handle specific events
                switch (stripeEvent.Type)
                {

                    case "payment_method.updated":
                    case "payment_method.attached":
                    case "payment_method.detached":
                        var paymentMethod = (PaymentMethod)stripeEvent.Data.Object;
                        // await _paymentService.HandlePaymentMethodWebhookAsync(paymentMethod.Id);
                        break;
                    case "charge.refunded":
                        var refund = stripeEvent.Data.Object as Charge;
                        await _paymentService.HandlePaymentRefund(
                            refund.PaymentIntentId,
                            refund.Id,
                            refund.AmountRefunded,
                            "by admin"
                        );
                        break;
                }

                return Ok();
            }
            catch (StripeException ex)
            {
                _logger.LogInformation(ex, "Error processing Stripe webhook");
                return BadRequest();
            }
        }

    }
}