
using Hangfire.Dashboard;

namespace Helpi.Infrastructure.BackgroundJobs.Filters;

public class HangfireBasicAuthenticationFilter : IDashboardAuthorizationFilter
{
    public string? User { get; set; }
    public string? Pass { get; set; }

    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();
        var header = httpContext.Request.Headers["Authorization"].ToString();

        if (string.IsNullOrEmpty(header) || !header.StartsWith("Basic "))
        {
            httpContext.Response.Headers["WWW-Authenticate"] = "Basic realm=\"Hangfire Dashboard\"";
            return false;
        }

        var encodedAuth = header.Substring("Basic ".Length).Trim();
        var authBytes = Convert.FromBase64String(encodedAuth);
        var authString = System.Text.Encoding.UTF8.GetString(authBytes);
        var credentials = authString.Split(':');

        return credentials.Length == 2 &&
               credentials[0] == User &&
               credentials[1] == Pass;
    }

}