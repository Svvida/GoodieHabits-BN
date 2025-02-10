using Application.Dtos.WeeklyQuest;
using Application.Interfaces.Common;

namespace Application.Interfaces
{
    public interface IWeeklyQuestService : IBaseQuestService<
        GetWeeklyQuestDto,
        CreateWeeklyQuestDto,
        UpdateWeeklyQuestDto,
        PatchWeeklyQuestDto>
    {
    }
}
