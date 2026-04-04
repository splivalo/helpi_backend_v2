using Helpi.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Helpi.WebApi.Filters;

/// <summary>
/// Automatically broadcasts an "EntityChanged" SignalR event after any
/// successful POST / PUT / PATCH / DELETE action, so all connected
/// clients (admin, senior-app, student-app) can refresh their data.
/// </summary>
public class EntityChangedBroadcastFilter : IAsyncActionFilter
{
    private readonly ISignalRNotificationService _signalR;

    public EntityChangedBroadcastFilter(ISignalRNotificationService signalR)
    {
        _signalR = signalR;
    }

    private static readonly HashSet<string> MutationMethods =
        new(StringComparer.OrdinalIgnoreCase) { "POST", "PUT", "PATCH", "DELETE" };

    // Controllers that should NOT trigger an entity broadcast.
    private static readonly HashSet<string> ExcludedControllers =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "Auth",
            "SignalR",
            "Dashboard",
            "Health",
            "PricingConfiguration", // already uses SettingsChanged
        };

    public async Task OnActionExecutionAsync(
        ActionExecutingContext context,
        ActionExecutionDelegate next)
    {
        var result = await next();

        // Skip if the action threw an exception
        if (result.Exception != null && !result.ExceptionHandled) return;

        // Only broadcast for mutation HTTP methods
        if (!MutationMethods.Contains(context.HttpContext.Request.Method)) return;

        // Skip excluded controllers
        var controller = context.RouteData.Values["controller"]?.ToString();
        if (string.IsNullOrEmpty(controller) || ExcludedControllers.Contains(controller)) return;

        // Only broadcast on 2xx status codes
        var statusCode = result.Result switch
        {
            ObjectResult obj => obj.StatusCode ?? 200,
            StatusCodeResult sc => sc.StatusCode,
            _ => 200
        };

        if (statusCode < 200 || statusCode >= 300) return;

        await _signalR.BroadcastEntityChangedAsync(controller);
    }
}
