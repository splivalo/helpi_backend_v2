
using System.Net;
using Helpi.Domain.Exceptions;


namespace Helpi.WebApi.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;
        private readonly IWebHostEnvironment _env;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger,
          IWebHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
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

        private Task HandleExceptionAsync(HttpContext context, Exception exception)
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
                stackTrace = _env.IsDevelopment() ? exception.StackTrace : null,
                source = exception.Source
            };

            return context.Response.WriteAsJsonAsync(response);
        }
    }
}