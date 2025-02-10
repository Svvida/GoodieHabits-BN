using Application.Dtos.DailyQuest;
using Application.Interfaces.Common;

namespace Application.Interfaces
{
    public interface IDailyQuestService : IBaseQuestService<
        GetDailyQuestDto,
        CreateDailyQuestDto,
        UpdateDailyQuestDto,
        PatchDailyQuestDto>
    {
    }
}
