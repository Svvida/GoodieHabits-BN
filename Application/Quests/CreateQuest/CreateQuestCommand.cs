using Application.Quests.Dtos;
using Domain.Enum;
using MediatR;

namespace Application.Quests.CreateQuest
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
        public int AccountId { get; init; }
        public QuestTypeEnum QuestType { get; init; }
    }

    public record CreateOneTimeQuestCommand : CreateQuestCommand, IRequest<OneTimeQuestDetailsDto>;
    public record CreateDailyQuestCommand : CreateQuestCommand, IRequest<DailyQuestDetailsDto>;
    public record CreateWeeklyQuestCommand : CreateQuestCommand, IRequest<WeeklyQuestDetailsDto>
    {
        public HashSet<string> Weekdays { get; init; } = [];
    }
    public record CreateMonthlyQuestCommand : CreateQuestCommand, IRequest<MonthlyQuestDetailsDto>
    {
        public int StartDay { get; init; }
        public int EndDay { get; init; }
    }
    public record CreateSeasonalQuestCommand : CreateQuestCommand, IRequest<SeasonalQuestDetailsDto>
    {
        public string Season { get; init; } = null!;
    }
}