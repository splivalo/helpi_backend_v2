
using System.Net;
using Helpi.Domain.Exceptions;

namespace Helpi.WebApi.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred. Details: {Details}", ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            // ✅ Unwrap DomainException to get more details
            if (exception is DomainException domainException)
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest; // 400 for domain-specific errors

                var domainResponse = new
                {
                    error = "A business rule violation occurred.",
                    message = domainException.Message,
                    innerMessage = domainException.InnerException?.Message,
                    Details = domainException.InnerException?.InnerException?.ToString(),
                };

                return context.Response.WriteAsJsonAsync(domainResponse);
            }




            var response = new
            {
                error = "An unexpected error occurred.",
                details = exception.Message,
                stackTrace = exception.StackTrace, /// TODO ⚠️  Remove in production for security,
                source = exception.Source
            };

            return context.Response.WriteAsJsonAsync(response);
        }
    }
}