using Application.Dtos.WeeklyQuest;
using Application.Interfaces.Common;

namespace Application.Interfaces
{
    public interface IWeeklyQuestService : IBaseQuestService<
        WeeklyQuestDto,
        CreateWeeklyQuestDto,
        UpdateWeeklyQuestDto,
        PatchWeeklyQuestDto>
    {
    }
}
