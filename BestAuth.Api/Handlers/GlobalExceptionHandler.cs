using BestAuth.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using System.Net;

namespace BestAuth.Api.Handlers
{
    public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger = logger;

        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken ct)
        {
            var (statusCode, message) = GetExceptionDetails(exception);

            _logger.LogError(exception, message);

            httpContext.Response.StatusCode = (int)statusCode;
            await httpContext.Response.WriteAsync(message, ct);

            return true;
        }

        private (HttpStatusCode statusCode, string message) GetExceptionDetails(Exception ex)
        {
            return ex switch
            {
                LoginFailedException => (HttpStatusCode.Unauthorized, ex.Message),
                UserExistsException => (HttpStatusCode.Conflict, ex.Message),
                RegistrationFailedException => (HttpStatusCode.BadRequest, ex.Message),
                RefreshTokenException => (HttpStatusCode.Unauthorized, ex.Message),
                RequestNotFoundException => (HttpStatusCode.NotFound, ex.Message),
                _ => (HttpStatusCode.InternalServerError, ex.Message),
            };
        }
    }
}
