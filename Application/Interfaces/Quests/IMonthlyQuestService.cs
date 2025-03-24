using Application.Dtos.Quests.MonthlyQuest;
using Application.Interfaces.Common;

namespace Application.Interfaces.Quests
{
    public interface IMonthlyQuestService : IBaseQuestService<
        GetMonthlyQuestDto,
        CreateMonthlyQuestDto,
        UpdateMonthlyQuestDto,
        MonthlyQuestCompletionPatchDto>
    {
    }
}
