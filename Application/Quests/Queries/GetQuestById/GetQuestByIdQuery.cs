using Application.Common.Interfaces;
using Application.Quests.Dtos;
using Domain.Enums;

namespace Application.Quests.Queries.GetQuestById
{
    public record GetQuestByIdQuery(int QuestId, QuestTypeEnum QuestType, int UserProfileId) : IQuery<QuestDetailsDto?>, ICurrentUserQuestCommand;
}
