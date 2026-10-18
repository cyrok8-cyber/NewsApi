using System.Net;
using System.Text.Json;
using NewsApi.Exceptions;

namespace NewsApi.Exceptions
{
    // Ловит любое необработанное исключение во всём приложении
    // и превращает его в единообразный JSON-ответ по правилам REST
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
                await _next(context); // пропускаем запрос дальше по цепочке
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var (statusCode, message) = exception switch
            {
                NotFoundException => (HttpStatusCode.NotFound, exception.Message),
                ConflictException => (HttpStatusCode.Conflict, exception.Message),
                UnauthorizedAccessException => (HttpStatusCode.Forbidden, exception.Message),
                AuthenticationFailedException => (HttpStatusCode.Unauthorized, exception.Message),
                BannedWordException => (HttpStatusCode.BadRequest, exception.Message),
                ArgumentException => (HttpStatusCode.BadRequest, exception.Message),

                _ => (HttpStatusCode.InternalServerError, "Внутренняя ошибка сервера")
            };

            if (statusCode == HttpStatusCode.InternalServerError)
                _logger.LogError(exception, "Необработанное исключение");

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;

            var result = JsonSerializer.Serialize(new
            {
                status = (int)statusCode,
                message
            });

            await context.Response.WriteAsync(result);
        }
    }
}