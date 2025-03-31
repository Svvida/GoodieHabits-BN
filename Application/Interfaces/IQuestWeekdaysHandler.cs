using Application.Dtos.Quests.WeeklyQuest;
using Domain.Models;

namespace Application.Interfaces
{
    public interface IQuestWeekdaysHandler
    {
        Quest HandleUpdateWeekdays(Quest quest, UpdateWeeklyQuestDto updateDto);
    }
}
