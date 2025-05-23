﻿using Domain;
using Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace MyLambdaApi.Filters
{
    public class QuestLabelAuthorizationFilter : IAsyncAuthorizationFilter
    {
        private readonly IQuestLabelRepository _questLabelRepository;

        public QuestLabelAuthorizationFilter(IQuestLabelRepository questLabelRepository)
        {
            _questLabelRepository = questLabelRepository;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            int? accountId = GetAccountIdFromContext(context);
            int? labelId = GetLabelIdFromContext(context);

            if (accountId is null || labelId is null)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            bool isOwner = await _questLabelRepository.IsLabelOwnedByUserAsync(labelId.Value, accountId.Value);

            if (!isOwner)
                context.Result = new ForbidResult();
        }

        private static int? GetLabelIdFromContext(AuthorizationFilterContext context)
        {
            if (context.RouteData.Values.TryGetValue("id", out object? idObj) &&
                idObj is string idString &&
                int.TryParse(idString, out int labelId))
            {
                return labelId;
            }

            return null;
        }

        private static int? GetAccountIdFromContext(AuthorizationFilterContext context)
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
