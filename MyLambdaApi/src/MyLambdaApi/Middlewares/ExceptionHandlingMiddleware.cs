using Domain.Exceptions;
using Microsoft.IdentityModel.Tokens;

namespace MyLambdaApi.Middlewares
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(
            RequestDelegate next,
            ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (AppException ex) // Catch known exceptions
            {
                _logger.LogError("Caught AppException: {ExceptionType} - {Message}", ex.GetType().Name, ex.Message);
                context.Response.StatusCode = ex.StatusCode;
                await HandleExceptionAsync(context, ex);
            }
            catch (SecurityTokenException ex)
            {
                _logger.LogError("Caught SecurityTokenException: {ExceptionType} - {Message}", ex.GetType().Name, ex.Message);
                context.Response.StatusCode = 401;
                await HandleExceptionAsync(context, ex);
            }
            catch (Exception ex) // Catch unexpected exceptions
            {
                var exceptionDetails = new
                {
                    ExceptionType = ex.GetType().Name,
                    ex.Message,
                    ex.StackTrace,
                    ex.Source,
                    TargetSite = ex.TargetSite?.Name,
                    InnerException = ex.InnerException?.Message
                };

                _logger.LogError("Caught unhandled exception: {@ExceptionDetails}", exceptionDetails);
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await HandleExceptionAsync(context, new Exception(ex.Message));
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var response = new
            {
                message = exception.Message,
                statusCode = context.Response.StatusCode,
                timestamp = DateTime.UtcNow
            };

            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(response);
        }
    }
}
