using Application.Common.Interfaces;
using Application.Quests.Dtos;
using Domain.Enums;

namespace Application.Quests.Commands.CreateQuest
{
    public abstract record CreateQuestCommand
    {
        public string Title { get; init; } = null!;
        public string? Description { get; init; } = null;
        public DateTime? StartDate { get; init; } = null;
        public DateTime? EndDate { get; init; } = null;
        public string? Emoji { get; init; } = null;
        public string? Priority { get; init; } = null;
        public string? Difficulty { get; init; } = null;
        public TimeOnly? ScheduledTime { get; init; } = null;
        public HashSet<int> Labels { get; init; } = [];
        public int UserProfileId { get; init; }
        public QuestTypeEnum QuestType { get; init; }
    }

    public record CreateOneTimeQuestCommand : CreateQuestCommand, ICommand<OneTimeQuestDetailsDto>;
    public record CreateDailyQuestCommand : CreateQuestCommand, ICommand<DailyQuestDetailsDto>;
    public record CreateWeeklyQuestCommand : CreateQuestCommand, ICommand<WeeklyQuestDetailsDto>
    {
        public HashSet<string> Weekdays { get; init; } = [];
    }
    public record CreateMonthlyQuestCommand : CreateQuestCommand, ICommand<MonthlyQuestDetailsDto>
    {
        public int StartDay { get; init; }
        public int EndDay { get; init; }
    }
    public record CreateSeasonalQuestCommand : CreateQuestCommand, ICommand<SeasonalQuestDetailsDto>
    {
        public string Season { get; init; } = null!;
    }
}