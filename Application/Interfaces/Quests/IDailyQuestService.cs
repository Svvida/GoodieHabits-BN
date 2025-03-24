using Application.Dtos.Quests.DailyQuest;
using Application.Interfaces.Common;

namespace Application.Interfaces.Quests
{
    public interface IDailyQuestService : IBaseQuestService<
        GetDailyQuestDto,
        CreateDailyQuestDto,
        UpdateDailyQuestDto,
        DailyQuestCompletionPatchDto>
    {
    }
}
