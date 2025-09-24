using System.Security.Claims;
using Helpi.Application.DTOs;
using Helpi.Application.Interfaces;
using Helpi.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Helpi.WebApi.Controllers;

[Authorize]
[ApiController]
[Route("api/stripe-payment")]
public class StripePaymentController : ControllerBase
{
    private readonly IStripePaymentService _stripeService;
    private readonly IUserRepository _userRepository;
    private readonly IPaymentProfileRepository _paymentProfileRepository;
    private readonly ILogger<StripePaymentController> _logger;

    public StripePaymentController(
        IStripePaymentService stripeService,
        IUserRepository userRepository,
        IPaymentProfileRepository paymentProfileRepository,
        ILogger<StripePaymentController> logger)
    {
        _stripeService = stripeService;
        _userRepository = userRepository;
        _paymentProfileRepository = paymentProfileRepository;
        _logger = logger;
    }

    [HttpPost("setup-intent")]
    public async Task<ActionResult<StripeSetupIntentResponseDto>> CreateSetupIntent()
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

            var clientSecret = await _stripeService.CreateSetupIntentAsync(user);


            return Ok(new StripeSetupIntentResponseDto { ClientSecret = clientSecret });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating setup intent");
            return StatusCode(500, "An error occurred while setting up payment");
        }
    }


    /// <summary>
    /// Save to local DB
    /// Note: system automaticall listens to stripe and syncs .. no probalbly no need for this
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    // [HttpPost("save-payment-method")]
    // public async Task<ActionResult> SavePaymentMethod([FromBody] SaveStripePaymentMethodDto request)
    // {
    //     try
    //     {
    //         // Get current user from the token claims
    //         var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    //         var user = await _userRepository.GetByIdAsync(int.Parse(userId!));

    //         if (user == null)
    //         {
    //             return NotFound("User not found");
    //         }

    //         var stripePaymentProfile = await _paymentProfileRepository.GetStipePaymentByUserIdAsync(user.Id);

    //         if (stripePaymentProfile == null)
    //         {
    //             return BadRequest("User does not have a payment profile");
    //         }

    //         var paymentMethodId = await _stripeService.SavePaymentMethodAsync(
    //                                                     stripePaymentProfile.StripeCustomerId!,
    //                                                     request.PaymentMethodId);

    //         return Ok(new { PaymentMethodId = paymentMethodId });
    //     }
    //     catch (Exception ex)
    //     {
    //         _logger.LogError(ex, "Error saving payment method");
    //         return StatusCode(500, "An error occurred while saving payment method");
    //     }
    // }

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
