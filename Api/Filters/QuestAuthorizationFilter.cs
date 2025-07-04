﻿using Domain;
using Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Api.Filters
{
    public class QuestAuthorizationFilter : IAsyncAuthorizationFilter
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<QuestAuthorizationFilter> _logger;

        public QuestAuthorizationFilter(IUnitOfWork unitOfWork, ILogger<QuestAuthorizationFilter> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            _logger.LogDebug("QuestAuthorizationFilter.OnAuthorizationAsync is being invoked!");
            int? accountId = GetAccountIdFromContext(context);
            int? questId = GetQuestIdFromContext(context);

            if (accountId is null || questId is null)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            bool isOwner = await _unitOfWork.Quests.IsQuestOwnedByUserAsync(questId.Value, accountId.Value);

            if (!isOwner)
            {
                _logger.LogWarning("User {AccountId} is not the owner of Quest {QuestId}", accountId, questId);
                context.Result = new ForbidResult();
            }
        }

        private int? GetQuestIdFromContext(AuthorizationFilterContext context)
        {
            if (context.RouteData.Values.TryGetValue("id", out object? idObj) &&
                idObj is string idString &&
                int.TryParse(idString, out int questId))
            {
                return questId;
            }

            _logger.LogWarning("Invalid or missing Quest ID in route");
            return null;
        }

        private int? GetAccountIdFromContext(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;

            if (user.Identity?.IsAuthenticated != true)
            {
                _logger.LogInformation("Unauthenticated user attempted to access a quest");
                return null;
            }

            if (int.TryParse(user.FindFirst(JwtClaimTypes.AccountId)?.Value, out int accountId))
                return accountId;

            _logger.LogError("Invalida account ID in user claims");
            return null;
        }
    }
}
