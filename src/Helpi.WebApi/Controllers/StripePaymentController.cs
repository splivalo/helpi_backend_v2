using System.Security.Claims;
using Helpi.Application.DTOs;
using Helpi.Application.Interfaces;
using Helpi.Application.Services;
using Helpi.Domain.Entities;
using Helpi.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Helpi.WebApi.Controllers;

[Authorize]
[ApiController]
[Route("api/stripe-payment")]
public class PaymentController : ControllerBase
{
    private readonly StripePaymentService _stripeService;
    private readonly IUserRepository _userRepository;
    private readonly IPaymentProfileRepository _paymentProfileRepository;
    private readonly ILogger<PaymentController> _logger;

    public PaymentController(
        StripePaymentService stripeService,
        IUserRepository userRepository,
        IPaymentProfileRepository paymentProfileRepository,
        ILogger<PaymentController> logger)
    {
        _stripeService = stripeService;
        _userRepository = userRepository;
        _paymentProfileRepository = paymentProfileRepository;
        _logger = logger;
    }

    [HttpPost("setup-intent")]
    public async Task<ActionResult> CreateSetupIntent()
    {
        try
        {
            // Get current user from the token claims
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var userId = int.Parse(userIdString!);

            var user = await _userRepository.GetByIdAsync(userId);

            if (user == null)
            {
                return NotFound("User not found");
            }

            var clientSecret = await _stripeService.CreateSetupIntent(user);


            return Ok(new { ClientSecret = clientSecret });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating setup intent");
            return StatusCode(500, "An error occurred while setting up payment");
        }
    }

    [HttpPost("save-payment-method")]
    public async Task<ActionResult> SavePaymentMethod([FromBody] SaveStripePaymentMethodDto request)
    {
        try
        {
            // Get current user from the token claims
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = await _userRepository.GetByIdAsync(int.Parse(userId!));

            if (user == null)
            {
                return NotFound("User not found");
            }

            var stripePaymentProfile = await _paymentProfileRepository.GetStipePaymentByUserIdAsync(user.Id);

            if (stripePaymentProfile == null)
            {
                return BadRequest("User does not have a payment profile");
            }

            var paymentMethodId = await _stripeService.SavePaymentMethodAsync(
                                                        stripePaymentProfile.StripeCustomerId!,
                                                        request.PaymentMethodId);

            return Ok(new { PaymentMethodId = paymentMethodId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving payment method");
            return StatusCode(500, "An error occurred while saving payment method");
        }
    }

    [HttpGet("payment-methods")]
    public async Task<ActionResult> GetPaymentMethods()
    {
        try
        {
            // Get current user from the token claims
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userId = int.Parse(userIdString!);
            var user = await _userRepository.GetByIdAsync(userId);

            if (user == null)
            {
                return NotFound("User not found");
            }

            var stripePaymentProfile = await _paymentProfileRepository.GetStipePaymentByUserIdAsync(user.Id);
            if (stripePaymentProfile == null)
            {
                return Ok(new List<PaymentMethodDto>());
            }

            var paymentMethods = await _stripeService.GetSavedPaymentMethodsAsync(stripePaymentProfile.StripeCustomerId!);
            return Ok(paymentMethods);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving payment methods");
            return StatusCode(500, "An error occurred while retrieving payment methods");
        }
    }

}
