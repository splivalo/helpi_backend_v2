namespace Helpi.Application.DTOs.Auth
{
    public class AdminResetPasswordDto
    {
        public int UserId { get; set; }
        public string NewPassword { get; set; } = string.Empty;
    }
}
