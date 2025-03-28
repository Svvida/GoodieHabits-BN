using Domain;
using Domain.Interfaces;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Api.Filters
{
    public class TimeZoneUpdateFilter : IAsyncActionFilter
    {
        private readonly IAccountRepository _accountRepository;
        private readonly ILogger<TimeZoneUpdateFilter> _logger;

        public TimeZoneUpdateFilter(IAccountRepository accountRepository, ILogger<TimeZoneUpdateFilter> logger)
        {
            _accountRepository = accountRepository;
            _logger = logger;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var httpContext = context.HttpContext;
            var userId = GetAccountIdFromContext(context);

            if (userId is not null && httpContext.Request.Headers.TryGetValue("x-time-zone", out var timeZoneValue))
            {
                var user = await _accountRepository.GetByIdAsync(userId.Value);

                if (user is not null && user.TimeZone != timeZoneValue)
                {
                    user.TimeZone = timeZoneValue;
                    await _accountRepository.UpdateAsync(user);
                    _logger.LogInformation("Updated timezone for user {UserId} to {TimeZone}.", userId, timeZoneValue);
                }
            }

            await next();
        }

        private static int? GetAccountIdFromContext(ActionExecutingContext context)
        {
            var user = context.HttpContext.User;

            if (user.Identity?.IsAuthenticated != true)
                return null;

            if (int.TryParse(user.FindFirst(JwtClaimTypes.AccountId)?.Value, out int accountId))
                return accountId;

            return null;
        }
    }
}
