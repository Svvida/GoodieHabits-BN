using Application.Dtos.Quests.WeeklyQuest;
using Application.Interfaces.Common;

namespace Application.Interfaces.Quests
{
    public interface IWeeklyQuestService : IBaseQuestService<
        GetWeeklyQuestDto,
        CreateWeeklyQuestDto,
        UpdateWeeklyQuestDto,
        PatchWeeklyQuestDto>
    {
    }
}
