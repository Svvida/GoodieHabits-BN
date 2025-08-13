using Application.Common.Interfaces;
using Application.Quests.Dtos;
using Domain.Enums;
using MediatR;

namespace Application.Quests.Queries.GetQuestById
{
    public record GetQuestByIdQuery(int QuestId, QuestTypeEnum QuestType, int AccountId) : IRequest<QuestDetailsDto?>, ICurrentUserQuestCommand;
}
