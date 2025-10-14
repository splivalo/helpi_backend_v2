namespace Helpi.Application.DTOs.Auth;


public class ForgotPasswordDto
{
    public string Email { get; set; } = string.Empty;
}

public class VerifyResetCodeDto
{
    public string Email { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}
