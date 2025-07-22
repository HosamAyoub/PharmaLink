using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text.Json;

namespace PharmaLink_API.Middleware
{
    public class GlobalExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;

        public GlobalExceptionHandlingMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlingMiddleware> logger)
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
                _logger.LogError(ex, "An unhandled exception occurred: {Message}", ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            
            var response = new
            {
                success = false,
                message = "An error occurred while processing your request.",
                details = exception.Message,
                timestamp = DateTime.UtcNow
            };

            switch (exception)
            {
                case ArgumentNullException:
                case ArgumentException:
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response = new
                    {
                        success = false,
                        message = "Invalid request parameters.",
                        details = exception.Message,
                        timestamp = DateTime.UtcNow
                    };
                    break;

                case UnauthorizedAccessException:
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    response = new
                    {
                        success = false,
                        message = "Unauthorized access.",
                        details = exception.Message,
                        timestamp = DateTime.UtcNow
                    };
                    break;

                case InvalidOperationException:
                    context.Response.StatusCode = (int)HttpStatusCode.Conflict;
                    response = new
                    {
                        success = false,
                        message = "Operation cannot be completed.",
                        details = exception.Message,
                        timestamp = DateTime.UtcNow
                    };
                    break;

                case KeyNotFoundException:
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    response = new
                    {
                        success = false,
                        message = "Requested resource not found.",
                        details = exception.Message,
                        timestamp = DateTime.UtcNow
                    };
                    break;

                case TimeoutException:
                    context.Response.StatusCode = (int)HttpStatusCode.RequestTimeout;
                    response = new
                    {
                        success = false,
                        message = "Request timeout.",
                        details = exception.Message,
                        timestamp = DateTime.UtcNow
                    };
                    break;

                default:
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    break;
            }

            var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(jsonResponse);
        }
    }
}