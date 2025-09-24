using System.ComponentModel.DataAnnotations;

namespace Helpi.Application.DTOs.Order;

public class OrderServiceCreateDto
{
    [Required]
    [Range(1, int.MaxValue)]
    public int ServiceId { get; set; }
}

public class OrderServiceDto
{
    public int OrderId { get; set; }
    public int ServiceId { get; set; }
    public ServiceDto Service { get; set; } = null!;
}