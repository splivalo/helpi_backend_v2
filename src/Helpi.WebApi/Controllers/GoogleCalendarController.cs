using Helpi.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Helpi.WebApi.Controllers;

[ApiController]
[Route("api/google-calendar")]
[Authorize]
public class GoogleCalendarController : ControllerBase
{
    private readonly IGoogleCalendarService _calendarService;

    public GoogleCalendarController(IGoogleCalendarService calendarService)
    {
        _calendarService = calendarService;
    }

    /// <summary>Returns the Google OAuth2 URL the admin should open in a browser.</summary>
    [HttpGet("connect-url")]
    public async Task<IActionResult> GetConnectUrl([FromQuery] string? lang = "hr")
    {
        var language = lang is "hr" or "en" ? lang : "hr";
        // Embed language in state so it survives the OAuth roundtrip
        var state = $"{Guid.NewGuid():N}|{language}";
        var url = await _calendarService.GetAuthUrlAsync(state);
        return Ok(new { url });
    }

    /// <summary>
    /// OAuth2 callback — called by Google after the admin grants access.
    /// Exchanges the code for tokens, then serves a self-closing success page.
    /// </summary>
    [HttpGet("callback")]
    [AllowAnonymous]
    public async Task<IActionResult> Callback(
        [FromQuery] string? code,
        [FromQuery] string? error,
        [FromQuery] string? state)
    {
        // Decode language from state (format: "{guid}|{lang}") — needed for both success and error pages
        var language = "hr";
        if (!string.IsNullOrEmpty(state))
        {
            var parts = state.Split('|');
            if (parts.Length >= 2 && parts[1] is "hr" or "en")
                language = parts[1];
        }

        if (!string.IsNullOrEmpty(error) || string.IsNullOrEmpty(code))
        {
            return Content(ErrorPage(error ?? "access_denied", language), "text/html");
        }

        var success = await _calendarService.HandleCallbackAsync(code, language);

        return Content(
            success ? SuccessPage(language) : ErrorPage("token_exchange_failed", language),
            "text/html");
    }

    /// <summary>Returns the current connection status.</summary>
    [HttpGet("status")]
    public async Task<IActionResult> GetStatus()
    {
        var status = await _calendarService.GetStatusAsync();
        return Ok(status);
    }

    /// <summary>Disconnects from Google Calendar and removes the stored tokens.</summary>
    [HttpDelete("disconnect")]
    public async Task<IActionResult> Disconnect()
    {
        await _calendarService.DisconnectAsync();
        return Ok();
    }

    // ── HTML helpers ──

    private static string SuccessPage(string language = "hr")
    {
        var title = language == "en" ? "Helpi \u2013 Calendar" : "Helpi \u2013 Kalendar";
        var heading = language == "en" ? "\u2705 Google Calendar connected!" : "\u2705 Google Calendar povezan!";
        var body = language == "en" ? "You can close this window and continue in the admin." : "Mo\u017ee\u0161 zatvoriti ovaj prozor i nastaviti u adminu.";
        var closing = language == "en" ? "Window closes automatically..." : "Prozor se zatvara automatski...";
        return $$"""
        <!DOCTYPE html>
        <html lang="{{language}}">
        <head><meta charset="utf-8"><title>{{title}}</title>
        <style>body{font-family:sans-serif;display:flex;justify-content:center;align-items:center;height:100vh;margin:0;background:#f4f6fa;}
        .card{background:#fff;padding:40px 60px;border-radius:16px;box-shadow:0 4px 24px rgba(0,0,0,.1);text-align:center;}
        h2{color:#2e7d32;} p{color:#555;} small{color:#999;}</style></head>
        <body>
          <div class="card">
            <h2>{{heading}}</h2>
            <p>{{body}}</p>
            <small>{{closing}}</small>
          </div>
          <script>setTimeout(() => window.close(), 3000);</script>
        </body></html>
        """;
    }

    private static string ErrorPage(string reason, string language = "hr")
    {
        var title = language == "en" ? "Helpi \u2013 Error" : "Helpi \u2013 Gre\u0161ka";
        var heading = language == "en" ? "&#10060; Connection error" : "&#10060; Gre\u0161ka pri povezivanju";
        var reasonLabel = language == "en" ? "Reason" : "Razlog";
        var retry = language == "en" ? "Close this window and try again." : "Zatvori prozor i poku\u0161aj ponovno.";
        return $$"""
        <!DOCTYPE html>
        <html lang="{{language}}">
        <head><meta charset="utf-8"><title>{{title}}</title>
        <style>body{font-family:sans-serif;display:flex;justify-content:center;align-items:center;height:100vh;margin:0;background:#f4f6fa;}
        .card{background:#fff;padding:40px 60px;border-radius:16px;box-shadow:0 4px 24px rgba(0,0,0,.1);text-align:center;}
        h2{color:#c62828;} p{color:#555;}</style></head>
        <body>
          <div class="card">
            <h2>{{heading}}</h2>
            <p>{{reasonLabel}}: <code>{{reason}}</code></p>
            <p>{{retry}}</p>
          </div>
        </body></html>
        """;
    }
}
