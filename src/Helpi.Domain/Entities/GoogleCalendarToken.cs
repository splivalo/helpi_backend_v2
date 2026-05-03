namespace Helpi.Domain.Entities;

public class GoogleCalendarToken
{
    public int Id { get; set; }
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime TokenExpiry { get; set; }
    public string ConnectedEmail { get; set; } = string.Empty;
    public DateTime ConnectedAt { get; set; } = DateTime.UtcNow;
    /// <summary>The ID of the dedicated "Helpi" Google Calendar created on connect.</summary>
    public string CalendarId { get; set; } = "primary";
    /// <summary>Language code (hr/en) used for generating event text.</summary>
    public string Language { get; set; } = "hr";
}
