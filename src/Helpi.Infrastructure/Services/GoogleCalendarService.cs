using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Helpi.Application.DTOs;
using Helpi.Application.Interfaces.Services;
using Helpi.Domain.Entities;
using Helpi.Domain.Enums;
using Helpi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Helpi.Infrastructure.Services;

public class GoogleCalendarService : IGoogleCalendarService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<GoogleCalendarService> _logger;

    private string ClientId => _configuration["GoogleCalendar:ClientId"] ?? string.Empty;
    private string ClientSecret => _configuration["GoogleCalendar:ClientSecret"] ?? string.Empty;
    private string RedirectUri => _configuration["GoogleCalendar:RedirectUri"]
        ?? "http://localhost:5142/api/google-calendar/callback";

    private static readonly string[] CalendarScopes =
    [
        CalendarService.Scope.Calendar,
        "email"
    ];

    public GoogleCalendarService(
        IServiceScopeFactory scopeFactory,
        IConfiguration configuration,
        ILogger<GoogleCalendarService> logger)
    {
        _scopeFactory = scopeFactory;
        _configuration = configuration;
        _logger = logger;
    }

    // ─────────────────────────────────────────────────────────────
    //  OAuth
    // ─────────────────────────────────────────────────────────────

    public Task<bool> IsConnectedAsync()
    {
        using var scope = _scopeFactory.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return ctx.GoogleCalendarTokens.AnyAsync();
    }

    public Task<string> GetAuthUrlAsync(string state)
    {
        var flow = CreateFlow();
        var request = (Google.Apis.Auth.OAuth2.Requests.GoogleAuthorizationCodeRequestUrl)
            flow.CreateAuthorizationCodeRequest(RedirectUri);
        request.State = state;
        // force offline access so we always get a refresh token
        request.AccessType = "offline";
        // approval_prompt is deprecated — use prompt=consent for reliable consent screen
        var url = request.Build().AbsoluteUri;
        url += "&prompt=consent%20select_account";
        return Task.FromResult(url);
    }

    public async Task<bool> HandleCallbackAsync(string code, string language)
    {
        try
        {
            var flow = CreateFlow();
            var tokenResponse = await flow.ExchangeCodeForTokenAsync(
                "admin", code, RedirectUri, CancellationToken.None);

            using var scope = _scopeFactory.CreateScope();
            var ctx = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var existing = await ctx.GoogleCalendarTokens.FindAsync(1);
            var isReconnect = existing != null;
            if (existing == null)
            {
                existing = new GoogleCalendarToken { Id = 1 };
                ctx.GoogleCalendarTokens.Add(existing);
            }

            existing.AccessToken = tokenResponse.AccessToken ?? string.Empty;
            // RefreshToken may be null on subsequent authorisations — keep previous if so
            if (!string.IsNullOrEmpty(tokenResponse.RefreshToken))
                existing.RefreshToken = tokenResponse.RefreshToken;
            existing.TokenExpiry = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresInSeconds ?? 3600);
            existing.ConnectedAt = DateTime.UtcNow;
            existing.Language = language is "hr" or "en" ? language : "hr";

            // Resolve connected account email
            existing.ConnectedEmail = await ResolveEmailAsync(tokenResponse.AccessToken) ?? "Google";

            // Save first so CreateCalendarServiceAsync can read the token
            await ctx.SaveChangesAsync();

            if (isReconnect)
            {
                // Reconnect (possibly with different language) — delete old Helpi calendar
                // so we start completely clean.
                try
                {
                    var (calSvc, oldCalId, _) = await CreateCalendarServiceAsync();
                    if (calSvc != null && !string.IsNullOrEmpty(oldCalId) && oldCalId != "primary")
                        await calSvc.Calendars.Delete(oldCalId).ExecuteAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Could not delete old Helpi calendar on reconnect");
                }
            }

            // Create a fresh "Helpi" calendar
            existing.CalendarId = await EnsureHelpiCalendarAsync() ?? "primary";
            await ctx.SaveChangesAsync();

            // Full sync — awaited, not fire-and-forget, to prevent race conditions.
            // fullResync: true clears ALL event IDs atomically before inserting events.
            await SyncUpcomingInstancesAsync(fullResync: true);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Google Calendar OAuth callback failed");
            return false;
        }
    }

    public async Task<GoogleCalendarStatusDto> GetStatusAsync()
    {
        using var scope = _scopeFactory.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var token = await ctx.GoogleCalendarTokens.FindAsync(1);
        if (token == null)
            return new GoogleCalendarStatusDto { IsConnected = false };

        return new GoogleCalendarStatusDto
        {
            IsConnected = true,
            ConnectedEmail = token.ConnectedEmail,
            ConnectedAt = token.ConnectedAt
        };
    }

    public async Task DisconnectAsync()
    {
        // 1. Delete the dedicated Helpi calendar from Google (removes all events at once)
        try
        {
            var (calSvc, calendarId, _) = await CreateCalendarServiceAsync();
            if (calSvc != null && !string.IsNullOrEmpty(calendarId) && calendarId != "primary")
                await calSvc.Calendars.Delete(calendarId).ExecuteAsync();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not delete Helpi Google Calendar on disconnect");
        }

        using var scope = _scopeFactory.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // 2. Clear stored event IDs so a future reconnect starts fresh
        await ctx.JobInstances
            .Where(j => j.GoogleCalendarEventId != null)
            .ExecuteUpdateAsync(s => s.SetProperty(j => j.GoogleCalendarEventId, (string?)null));

        // 3. Remove the token
        var token = await ctx.GoogleCalendarTokens.FindAsync(1);
        if (token != null)
        {
            ctx.GoogleCalendarTokens.Remove(token);
            await ctx.SaveChangesAsync();
        }
    }

    // ─────────────────────────────────────────────────────────────
    //  Event CRUD (all "Try" — failures never break business logic)
    // ─────────────────────────────────────────────────────────────

    public async Task TryCreateEventAndSaveAsync(int jobInstanceId)
    {
        try
        {
            var (calSvc, calendarId, language) = await CreateCalendarServiceAsync();
            if (calSvc == null) return;

            using var scope = _scopeFactory.CreateScope();
            var ctx = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var instance = await LoadInstanceWithIncludesAsync(ctx, jobInstanceId);
            if (instance == null) return;

            var ev = BuildCalendarEvent(instance, language);
            var created = await calSvc.Events.Insert(ev, calendarId).ExecuteAsync();

            instance.GoogleCalendarEventId = created.Id;
            await ctx.SaveChangesAsync();

            _logger.LogInformation("📅 Calendar event created for JobInstance {Id}: {EventId}",
                jobInstanceId, created.Id);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not create Google Calendar event for JobInstance {Id}", jobInstanceId);
        }
    }

    public async Task TryUpdateEventAsync(int jobInstanceId)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var ctx = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var instance = await LoadInstanceWithIncludesAsync(ctx, jobInstanceId);
            if (instance?.GoogleCalendarEventId == null) return;

            var (calSvc, calendarId, language) = await CreateCalendarServiceAsync();
            if (calSvc == null) return;

            var ev = BuildCalendarEvent(instance, language);
            await calSvc.Events.Update(ev, calendarId, instance.GoogleCalendarEventId).ExecuteAsync();

            _logger.LogInformation("📅 Calendar event updated for JobInstance {Id}: {EventId}",
                jobInstanceId, instance.GoogleCalendarEventId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not update Google Calendar event for JobInstance {Id}", jobInstanceId);
        }
    }

    public async Task TryDeleteEventAsync(string eventId)
    {
        try
        {
            var (calSvc, calendarId, _) = await CreateCalendarServiceAsync();
            if (calSvc == null) return;

            await calSvc.Events.Delete(calendarId, eventId).ExecuteAsync();

            _logger.LogInformation("📅 Calendar event deleted: {EventId}", eventId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not delete Google Calendar event {EventId}", eventId);
        }
    }

    public async Task SyncUpcomingInstancesAsync(bool fullResync = false)
    {
        try
        {
            var (calSvc, calendarId, language) = await CreateCalendarServiceAsync();
            if (calSvc == null) return;

            using var scope = _scopeFactory.CreateScope();
            var ctx = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // Full resync: clear ALL event IDs first so every instance is re-created.
            // This runs in the same scope — no race condition possible.
            if (fullResync)
            {
                await ctx.JobInstances
                    .Where(j => j.GoogleCalendarEventId != null)
                    .ExecuteUpdateAsync(s =>
                        s.SetProperty(j => j.GoogleCalendarEventId, (string?)null));
                _logger.LogInformation("📅 Full resync — cleared all existing event IDs.");
            }

            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var endOfMonth = new DateOnly(today.Year, today.Month,
                DateTime.DaysInMonth(today.Year, today.Month));

            var instances = await ctx.JobInstances
                .Include(j => j.Senior).ThenInclude(s => s.Contact)
                .Include(j => j.Order).ThenInclude(o => o.OrderServices).ThenInclude(os => os.Service)
                .Include(j => j.ScheduleAssignment).ThenInclude(a => a!.Student).ThenInclude(s => s.Contact)
                .Where(j =>
                    j.Status == JobInstanceStatus.Upcoming &&
                    j.ScheduledDate >= today &&
                    j.ScheduledDate <= endOfMonth &&
                    j.GoogleCalendarEventId == null)
                .ToListAsync();

            _logger.LogInformation(
                "📅 Syncing {Count} job instances to Google Calendar (lang={Lang}, calendar={CalId})...",
                instances.Count, language, calendarId);

            var created = 0;
            var failed = 0;
            foreach (var instance in instances)
            {
                try
                {
                    var ev = BuildCalendarEvent(instance, language);
                    var result = await calSvc.Events.Insert(ev, calendarId).ExecuteAsync();
                    instance.GoogleCalendarEventId = result.Id;
                    created++;
                }
                catch (Exception ex)
                {
                    failed++;
                    _logger.LogWarning(ex,
                        "Skipping CalendarSync for JobInstance {Id} (Senior={Senior})",
                        instance.Id, instance.Senior?.Contact?.FullName ?? "?");
                }
            }

            await ctx.SaveChangesAsync();
            _logger.LogInformation(
                "📅 Google Calendar sync complete: {Created} created, {Failed} failed out of {Total}.",
                created, failed, instances.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Google Calendar SyncUpcomingInstances failed");
        }
    }

    // ─────────────────────────────────────────────────────────────
    //  Internals
    // ─────────────────────────────────────────────────────────────

    private IAuthorizationCodeFlow CreateFlow()
    {
        return new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
        {
            ClientSecrets = new ClientSecrets
            {
                ClientId = ClientId,
                ClientSecret = ClientSecret
            },
            Scopes = CalendarScopes
        });
    }

    private async Task<(CalendarService? Service, string CalendarId, string Language)> CreateCalendarServiceAsync()
    {
        using var scope = _scopeFactory.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var token = await ctx.GoogleCalendarTokens.FindAsync(1);
        if (token == null) return (null, "primary", "hr");

        var tokenResponse = new TokenResponse
        {
            AccessToken = token.AccessToken,
            RefreshToken = token.RefreshToken,
            ExpiresInSeconds = Math.Max(0, (long)(token.TokenExpiry - DateTime.UtcNow).TotalSeconds)
        };

        var flow = CreateFlow();
        var credential = new UserCredential(flow, "admin", tokenResponse);

        var service = new CalendarService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credential,
            ApplicationName = "Helpi Admin"
        });

        return (service, token.CalendarId ?? "primary", token.Language ?? "hr");
    }

    /// <summary>
    /// Finds an existing "Helpi" calendar or creates a new one.
    /// Returns the calendar ID, or null on failure.
    /// </summary>
    private async Task<string?> EnsureHelpiCalendarAsync()
    {
        try
        {
            var (calSvc, _, _) = await CreateCalendarServiceAsync();
            if (calSvc == null) return null;

            // Check if a calendar named "Helpi" already exists
            var list = await calSvc.CalendarList.List().ExecuteAsync();
            var existing = list.Items?.FirstOrDefault(c => c.Summary == "Helpi");
            if (existing != null)
            {
                _logger.LogInformation("📅 Found existing Helpi calendar: {Id}", existing.Id);
                return existing.Id;
            }

            // Create a new calendar
            var newCal = new Google.Apis.Calendar.v3.Data.Calendar
            {
                Summary = "Helpi",
                TimeZone = "Europe/Zagreb"
            };
            var created = await calSvc.Calendars.Insert(newCal).ExecuteAsync();
            _logger.LogInformation("📅 Created Helpi calendar: {Id}", created.Id);
            return created.Id;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not create Helpi calendar, falling back to primary");
            return null;
        }
    }

    private static async Task<JobInstance?> LoadInstanceWithIncludesAsync(AppDbContext ctx, int jobInstanceId)
    {
        return await ctx.JobInstances
            .Include(j => j.Senior).ThenInclude(s => s.Contact)
            .Include(j => j.Order).ThenInclude(o => o.OrderServices).ThenInclude(os => os.Service)
            .Include(j => j.ScheduleAssignment).ThenInclude(a => a!.Student).ThenInclude(s => s.Contact)
            .FirstOrDefaultAsync(j => j.Id == jobInstanceId);
    }

    private static Event BuildCalendarEvent(JobInstance instance, string language = "hr")
    {
        var seniorName = instance.Senior?.Contact?.FullName ?? "Senior";
        var seniorAddress = instance.Senior?.Contact?.FullAddress ?? string.Empty;
        var seniorNotes = instance.Notes ?? string.Empty;

        var studentName = instance.ScheduleAssignment?.Student?.Contact?.FullName ?? "N/A";

        var services = instance.Order?.OrderServices?
            .Select(os => os.Service?.GetName(language) ?? string.Empty)
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .ToList() ?? [];

        var na = language == "en" ? "N/A" : "N/A";
        var servicesText = services.Count > 0 ? string.Join(", ", services) : na;

        string description;
        if (language == "en")
        {
            description = $"Student attending: {studentName}\nServices: {servicesText}";
            if (!string.IsNullOrWhiteSpace(seniorNotes))
                description += $"\nNote: {seniorNotes}";
        }
        else
        {
            description = $"Dolazi student: {studentName}\nUsluge: {servicesText}";
            if (!string.IsNullOrWhiteSpace(seniorNotes))
                description += $"\nNapomena: {seniorNotes}";
        }

        // Build DateTime from DateOnly + TimeOnly
        var startDt = instance.ScheduledDate.ToDateTime(instance.StartTime);
        var endDt = instance.ScheduledDate.ToDateTime(instance.EndTime);

        return new Event
        {
            Summary = seniorName,
            Description = description,
            Location = seniorAddress,
            Start = new EventDateTime { DateTimeDateTimeOffset = startDt, TimeZone = "Europe/Zagreb" },
            End = new EventDateTime { DateTimeDateTimeOffset = endDt, TimeZone = "Europe/Zagreb" }
        };
    }

    /// <summary>
    /// Calls the Google token-info endpoint to discover the connected account email.
    /// Returns null if it fails — the caller stores "Google" as fallback.
    /// </summary>
    private async Task<string?> ResolveEmailAsync(string? accessToken)
    {
        if (string.IsNullOrEmpty(accessToken)) return null;
        try
        {
            using var http = new HttpClient();
            var resp = await http.GetAsync(
                $"https://www.googleapis.com/oauth2/v1/userinfo?access_token={accessToken}");
            if (!resp.IsSuccessStatusCode) return null;
            var json = await resp.Content.ReadAsStringAsync();
            using var doc = System.Text.Json.JsonDocument.Parse(json);
            return doc.RootElement.TryGetProperty("email", out var emailElem)
                ? emailElem.GetString()
                : null;
        }
        catch
        {
            return null;
        }
    }
}
