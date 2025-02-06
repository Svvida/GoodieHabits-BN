using Application.Dtos.DailyQuest;
using Application.Interfaces.Common;

namespace Application.Interfaces
{
    public interface IDailyQuestService : IBaseQuestService<
        DailyQuestDto,
        CreateDailyQuestDto,
        UpdateDailyQuestDto,
        PatchDailyQuestDto>
    {
    }
}
