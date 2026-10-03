using Application.Quests.Dtos;
using Domain.Models;

namespace Application.Quests
{
    public interface IQuestMapper
    {
        /// <summary>
        /// Projects a quest for the client. Takes the user's local <c>today</c> because completion and the
        /// current period are derived rather than stored — there is no flag to read.
        /// </summary>
        QuestDetailsDto MapToDto(Quest quest, DateOnly today, bool includeLegacyType = false);
    }
}
