using Helpi.Application.DTOs;

namespace Helpi.Application.Interfaces.Services;

/// <summary>
/// Manages the admin's Google Calendar integration.
/// All "Try" methods swallow exceptions — calendar failures never break business logic.
/// </summary>
public interface IGoogleCalendarService
{
    Task<bool> IsConnectedAsync();

    /// <summary>Returns the Google OAuth2 authorisation URL the admin must visit.</summary>
    Task<string> GetAuthUrlAsync(string state);

    /// <summary>Exchanges the authorisation code for tokens and stores them. Returns true on success.</summary>
    Task<bool> HandleCallbackAsync(string code, string language);

    Task<GoogleCalendarStatusDto> GetStatusAsync();

    Task DisconnectAsync();

    /// <summary>Creates a Calendar event for the given job instance and stores the eventId back on the entity.</summary>
    Task TryCreateEventAndSaveAsync(int jobInstanceId);

    /// <summary>Updates the Calendar event for the given job instance (reads eventId from the entity).</summary>
    Task TryUpdateEventAsync(int jobInstanceId);

    /// <summary>Deletes a Calendar event by its Google event id.</summary>
    Task TryDeleteEventAsync(string eventId);

    /// <summary>
    /// Syncs upcoming instances for the current month to Google Calendar.
    /// When <paramref name="fullResync"/> is true, clears ALL event IDs first and re-creates every event
    /// (used after connect/reconnect). When false, only syncs instances that don't yet have an event
    /// (used by nightly Hangfire job).
    /// </summary>
    Task SyncUpcomingInstancesAsync(bool fullResync = false);
}
