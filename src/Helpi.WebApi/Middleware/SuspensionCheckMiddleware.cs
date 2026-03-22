using System.Net;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Helpi.Domain.Entities;

namespace Helpi.WebApi.Middleware;

public class SuspensionCheckMiddleware
{
    private readonly RequestDelegate _next;

    private static readonly HashSet<string> _excludedPaths = new(StringComparer.OrdinalIgnoreCase)
    {
        "/api/suspensions",
        "/api/auth/login",
        "/api/auth/register",
        "/api/auth/refresh-token",
        "/api/auth/check-email",
    };

    public SuspensionCheckMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Skip for unauthenticated requests (handled by [Authorize])
        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim))
        {
            await _next(context);
            return;
        }

        // Skip for excluded paths
        var path = context.Request.Path.Value ?? "";
        if (_excludedPaths.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
        {
            await _next(context);
            return;
        }

        // Skip for Admin users
        if (context.User.IsInRole("Admin"))
        {
            await _next(context);
            return;
        }

        // Check suspension status
        if (int.TryParse(userIdClaim, out var userId))
        {
            var userManager = context.RequestServices.GetRequiredService<UserManager<User>>();
            var user = await userManager.FindByIdAsync(userId.ToString());

            if (user != null && user.IsSuspended)
            {
                context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                context.Response.ContentType = "application/json";

                var response = new
                {
                    error = "AccountSuspended",
                    message = "Your account has been suspended.",
                    reason = user.SuspensionReason,
                    suspendedAt = user.SuspendedAt,
                };

                await context.Response.WriteAsync(
                    JsonSerializer.Serialize(response, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    })
                );
                return;
            }
        }

        await _next(context);
    }
}
