using Application.Common.Exceptions;
using Domain.Exceptions;
using Microsoft.IdentityModel.Tokens;

namespace Api.Middlewares
{
    public class ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        public async Task Invoke(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch (ValidationException ex) // Catch all validation exceptions{
            {
                logger.LogDebug("Validation failed: {Message}", ex.Message);
                context.Response.StatusCode = 400;

                var response = new
                {
                    title = ex.Message,
                    statusCode = ex.StatusCode,
                    errors = ex.Errors
                };

                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(response);
            }
            catch (AppException ex) // Catch known exceptions
            {
                logger.LogError("Caught AppException: {ExceptionType} - {Message}", ex.GetType().Name, ex.Message);
                context.Response.StatusCode = ex.StatusCode;
                await HandleExceptionAsync(context, ex);
            }
            catch (SecurityTokenException ex)
            {
                logger.LogError("Caught SecurityTokenException: {ExceptionType} - {Message}", ex.GetType().Name, ex.Message);
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

                logger.LogError("Caught unhandled exception: {@ExceptionDetails}", exceptionDetails);
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
