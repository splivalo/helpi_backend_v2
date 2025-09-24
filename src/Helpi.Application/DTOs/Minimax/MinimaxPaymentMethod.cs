

namespace Helpi.Application.DTOs.Minimax;

public class MinimaxPaymentMethod
{
    public int PaymentMethodId { get; set; }
    public string? Name { get; set; }
    public string? Type { get; set; }
    public string? Usage { get; set; }
    public string? Default { get; set; }
}
