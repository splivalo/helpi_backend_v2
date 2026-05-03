namespace Helpi.Application.DTOs;

public class GoogleCalendarStatusDto
{
    public bool IsConnected { get; set; }
    public string? ConnectedEmail { get; set; }
    public DateTime? ConnectedAt { get; set; }
}
