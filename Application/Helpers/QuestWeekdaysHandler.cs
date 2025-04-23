using Application.Dtos.Quests.WeeklyQuest;
using Application.Interfaces;
using Domain.Enum;
using Domain.Interfaces.Quests;
using Domain.Models;

namespace Application.Helpers
{
    public class QuestWeekdaysHandler : IQuestWeekdaysHandler
    {
        private readonly IQuestRepository _questRepository;

        public QuestWeekdaysHandler(IQuestRepository questRepository)
        {
            _questRepository = questRepository;
        }

        public Quest HandleUpdateWeekdays(Quest quest, UpdateWeeklyQuestDto updateDto)
        {
            HashSet<WeekdayEnum> newWeekdaysHashSet = updateDto.Weekdays
                .Select(wd => Enum.Parse<WeekdayEnum>(wd))
                .ToHashSet();

            HashSet<WeekdayEnum> existingWeekdaysHashSet = quest.WeeklyQuest_Days
                .Select(wqd => wqd.Weekday)
                .ToHashSet();

            var weekdaysToAdd = newWeekdaysHashSet.Except(existingWeekdaysHashSet)
                .Select(day => new WeeklyQuest_Day
                {
                    QuestId = quest.Id,
                    Weekday = day
                })
                .ToList();

            var weekdaysToRemove = quest.WeeklyQuest_Days
                .Where(wqd => !newWeekdaysHashSet.Contains(wqd.Weekday))
                .ToList();

            if (weekdaysToRemove.Count != 0)
                _questRepository.RemoveQuestWeekdays(weekdaysToRemove);

            if (weekdaysToAdd.Count != 0)
                _questRepository.AddQuestWeekdays(weekdaysToAdd);

            return quest;
        }
    }
}
