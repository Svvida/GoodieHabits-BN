using Application.Common.Interfaces;
using Application.Quests.Dtos;

namespace Application.Quests.Queries.GetQuestById
{
    public record GetQuestByIdQuery(int QuestId, int UserProfileId) : IQuery<QuestDetailsDto?>, ICurrentUserQuestCommand;
}
