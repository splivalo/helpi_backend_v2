using Helpi.Application.DTOs.Auth;
using Microsoft.AspNetCore.Mvc;
using Helpi.Application.Services;
using System.Security.Claims;



[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{

    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {

        _authService = authService;
    }

    [HttpPost("register/admin")]
    public async Task<IActionResult> RegisterAdmin(AdminRegisterDto dto)
    {
        var result = await _authService.RegisterAdmin(dto);


        if (!result.Success)
        {
            return BadRequest(new { message = result.Message });
        }

        return Ok(new
        {
            message = result.Message
        });
    }
    [HttpPost("register/student")]
    public async Task<IActionResult> RegisterStudent(StudentRegisterDto dto)
    {



        var result = await _authService.RegisterStudent(dto);


        if (!result.Success)
        {
            return BadRequest(new { message = result.Message });
        }

        return Ok(new
        {
            message = result.Message
        });
    }



    [HttpPost("register/customer")]
    public async Task<IActionResult> RegisterCustomer(CustomerRegisterDto dto)
    {



        var result = await _authService.RegisterCustomer(dto);


        if (!result.Success)
        {
            return BadRequest(new { message = result.Message });
        }


        return Ok(new
        {
            message = result.Message
        });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {

        var result = await _authService.Login(dto);

        if (!result.Success)
        {
            return Unauthorized(new { message = result.Message });
        }

        return Ok(new
        {
            token = result.Token,
            userId = result.UserId,
            userType = result.UserType,
            firebaseToken = result.firebaseToken,
            message = result.Message
        });
    }


    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword(ChangePasswordDto dto)


    {

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            throw new InvalidOperationException("User not loggedin.");
        }

        var tokenResponseDto = await _authService.ChangePassword(userId, dto);

        return Ok(tokenResponseDto);
    }


}
