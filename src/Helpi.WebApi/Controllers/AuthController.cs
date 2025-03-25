using Helpi.Application.DTOs.Auth;
using Microsoft.AspNetCore.Mvc;
using Helpi.Application.Services;


[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{

    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {

        _authService = authService;
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
            message = result.Message
        });
    }
}
