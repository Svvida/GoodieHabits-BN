using Application.Dtos.MonthlyQuest;
using Application.Interfaces.Common;

namespace Application.Interfaces
{
    public interface IMonthlyQuestService : IBaseQuestService<
        GetMonthlyQuestDto,
        CreateMonthlyQuestDto,
        UpdateMonthlyQuestDto,
        PatchMonthlyQuestDto>
    {
    }
}
